using System.Diagnostics;
using System.Reflection;

namespace BluetoothDock;

sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private readonly Form _sync;
    private readonly System.Windows.Forms.Timer _debounce;
    private readonly AudioEndpointWatcher? _watcher;
    private readonly AppConfig _config;

    private bool _busy;
    private bool _busyConnecting;
    private CancellationTokenSource? _busyCts;

    public TrayApplicationContext()
    {
        _config = AppConfig.Load();
        Autostart.ApplyOnLaunch();

        _sync = new Form
        {
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000),
            Size = new Size(1, 1),
            Opacity = 0,
            ShowIcon = false
        };
        _ = _sync.Handle;

        _menu = new ContextMenuStrip();
        _menu.Opening += (_, _) => RebuildMenu();

        _notifyIcon = new NotifyIcon
        {
            Icon = TrayIcons.Disconnected,
            Visible = true,
            ContextMenuStrip = _menu,
            Text = Strings.AppName
        };
        _notifyIcon.MouseClick += OnTrayMouseClick;

        _debounce = new System.Windows.Forms.Timer { Interval = 250 };
        _debounce.Tick += (_, _) =>
        {
            _debounce.Stop();
            RefreshPresentation();
        };

        try
        {
            _watcher = new AudioEndpointWatcher(_sync, QueueRefresh);
        }
        catch
        {
            _watcher = null;
        }

        RefreshPresentation();
    }

    private void OnTrayMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        if (_busy)
            return;

        if (!BluetoothAudioService.IsBluetoothRadioOn())
        {
            ShowBalloon(Strings.BluetoothOff);
            return;
        }

        IReadOnlyList<BluetoothAudioDevice> devices = BluetoothAudioService.Enumerate();
        TryAutoSelect(devices);
        BluetoothAudioDevice? selected = FindSelected(devices);

        if (selected is null)
        {
            if (devices.Count == 0)
                ShowBalloon(Strings.NoDevices);
            else if (!string.IsNullOrWhiteSpace(_config.DeviceName))
                ShowBalloon($"{_config.DeviceName} — {Strings.NotAvailable}");

            ShowContextMenu();
            return;
        }

        _ = ToggleAsync(selected);
    }

    private async Task ToggleAsync(BluetoothAudioDevice selected)
    {
        _busyCts?.Cancel();
        _busyCts?.Dispose();
        _busyCts = new CancellationTokenSource();
        CancellationToken token = _busyCts.Token;

        bool connect = !selected.IsConnected;
        _busy = true;
        _busyConnecting = connect;
        RefreshPresentation();

        try
        {
            bool accepted = connect
                ? BluetoothAudioService.Connect(selected.ContainerId)
                : BluetoothAudioService.Disconnect(selected.ContainerId);

            bool reached = accepted && await WaitForConnectionStateAsync(selected.ContainerId, connect, token);
            if (!reached && !token.IsCancellationRequested)
                ShowBalloon(connect ? Strings.ConnectFailed : Strings.DisconnectFailed);
        }
        catch
        {
            if (!token.IsCancellationRequested)
                ShowBalloon(connect ? Strings.ConnectFailed : Strings.DisconnectFailed);
        }
        finally
        {
            _busy = false;
            RefreshPresentation();
        }
    }

    private static async Task<bool> WaitForConnectionStateAsync(Guid containerId, bool connected, CancellationToken token)
    {
        for (int i = 0; i < 40; i++)
        {
            token.ThrowIfCancellationRequested();
            BluetoothAudioDevice? current = Find(containerId);
            if (current is not null && current.IsConnected == connected)
                return true;

            await Task.Delay(200, token);
        }

        BluetoothAudioDevice? last = Find(containerId);
        return last is not null && last.IsConnected == connected;
    }

    private static BluetoothAudioDevice? Find(Guid containerId) =>
        BluetoothAudioService.Enumerate().FirstOrDefault(d => d.ContainerId == containerId);

    private void ShowContextMenu()
    {
        MethodInfo? method = typeof(NotifyIcon).GetMethod(
            "ShowContextMenu",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (method is not null)
        {
            method.Invoke(_notifyIcon, null);
            return;
        }

        _menu.Show(Cursor.Position);
    }

    private void RebuildMenu()
    {
        _menu.Items.Clear();

        IReadOnlyList<BluetoothAudioDevice> devices = BluetoothAudioService.Enumerate();
        TryAutoSelect(devices);

        if (devices.Count == 0)
        {
            _menu.Items.Add(new ToolStripMenuItem(Strings.NoDevices) { Enabled = false });
        }
        else
        {
            foreach (BluetoothAudioDevice device in devices)
            {
                string label = device.IsConnected
                    ? $"{device.Name}  · {Strings.Connected}"
                    : device.Name;

                var item = new ToolStripMenuItem(label)
                {
                    Checked = device.ContainerId == _config.ParsedContainerId,
                    Tag = device,
                    CheckOnClick = false
                };
                item.Click += OnDeviceChosen;
                _menu.Items.Add(item);
            }
        }

        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(new ToolStripMenuItem(Strings.BluetoothSettings, null, (_, _) => OpenBluetoothSettings()));
        _menu.Items.Add(new ToolStripMenuItem(Strings.StartWithWindows, null, (_, _) => ToggleAutostart())
        {
            Checked = Autostart.IsEnabled,
            CheckOnClick = false
        });
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(new ToolStripMenuItem(Strings.About, null, (_, _) => ShowAbout()));
        _menu.Items.Add(new ToolStripMenuItem(Strings.Exit, null, (_, _) => Quit()));
    }

    private void OnDeviceChosen(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem { Tag: BluetoothAudioDevice device })
            return;

        _config.ContainerId = device.ContainerId.ToString("D");
        _config.DeviceName = device.Name;
        _config.Save();
        RefreshPresentation();
    }

    private void TryAutoSelect(IReadOnlyList<BluetoothAudioDevice> devices)
    {
        if (_config.ParsedContainerId is not null || devices.Count != 1)
            return;

        BluetoothAudioDevice only = devices[0];
        _config.ContainerId = only.ContainerId.ToString("D");
        _config.DeviceName = only.Name;
        _config.Save();
    }

    private BluetoothAudioDevice? FindSelected(IReadOnlyList<BluetoothAudioDevice> devices)
    {
        Guid? id = _config.ParsedContainerId;
        if (id is null)
            return null;

        return devices.FirstOrDefault(d => d.ContainerId == id);
    }

    private void QueueRefresh()
    {
        _debounce.Stop();
        _debounce.Start();
    }

    private void RefreshPresentation()
    {
        if (_busy)
        {
            _notifyIcon.Icon = TrayIcons.Busy;
            string verb = _busyConnecting ? Strings.Connecting : Strings.Disconnecting;
            string name = _config.DeviceName ?? Strings.AppName;
            _notifyIcon.Text = TruncateTip($"{name} — {verb}");
            return;
        }

        if (!BluetoothAudioService.IsBluetoothRadioOn())
        {
            _notifyIcon.Icon = TrayIcons.Disconnected;
            _notifyIcon.Text = TruncateTip(Strings.BluetoothOff);
            return;
        }

        IReadOnlyList<BluetoothAudioDevice> devices = BluetoothAudioService.Enumerate();
        TryAutoSelect(devices);
        BluetoothAudioDevice? selected = FindSelected(devices);

        if (selected is null)
        {
            _notifyIcon.Icon = TrayIcons.Disconnected;
            if (!string.IsNullOrWhiteSpace(_config.DeviceName) && _config.ParsedContainerId is not null)
                _notifyIcon.Text = TruncateTip($"{_config.DeviceName} — {Strings.NotAvailable}");
            else if (devices.Count == 0)
                _notifyIcon.Text = TruncateTip(Strings.NoDevices);
            else
                _notifyIcon.Text = TruncateTip(Strings.SelectDevice);

            return;
        }

        _notifyIcon.Icon = selected.IsConnected ? TrayIcons.Connected : TrayIcons.Disconnected;
        string state = selected.IsConnected ? Strings.Connected : Strings.Disconnected;
        _notifyIcon.Text = TruncateTip($"{selected.Name} — {state}");
    }

    private void ShowBalloon(string text)
    {
        _notifyIcon.BalloonTipTitle = Strings.AppName;
        _notifyIcon.BalloonTipText = text;
        _notifyIcon.BalloonTipIcon = ToolTipIcon.None;
        _notifyIcon.ShowBalloonTip(4000);
    }

    private void ToggleAutostart()
    {
        try
        {
            if (Autostart.IsEnabled)
                Autostart.Disable();
            else
                Autostart.Enable();
        }
        catch
        {
            ShowBalloon(Strings.AutostartFailed);
        }
    }

    private void ShowAbout()
    {
        using var about = new AboutForm();
        about.ShowDialog();
    }

    private static void OpenBluetoothSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:bluetooth",
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private static string TruncateTip(string text)
    {
        const int max = 63;
        if (text.Length <= max)
            return text;

        return string.Concat(text.AsSpan(0, max - 1), "…");
    }

    private void Quit()
    {
        _busyCts?.Cancel();
        _debounce.Stop();
        _debounce.Dispose();
        _watcher?.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _sync.Dispose();
        Autostart.DeleteInstalledExeAfterThisProcessExits();
        ExitThread();
    }
}
