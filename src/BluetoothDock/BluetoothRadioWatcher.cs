using Windows.Devices.Radios;

namespace BluetoothDock;

sealed class BluetoothRadioWatcher : IDisposable
{
    private readonly Control _ui;
    private readonly Action _onChanged;
    private Radio? _radio;
    private volatile bool _isOn = true;
    private bool _disposed;

    public BluetoothRadioWatcher(Control ui, Action onChanged)
    {
        _ui = ui;
        _onChanged = onChanged;
        Attach();
    }

    public bool IsOn => _isOn;

    /// <summary>
    /// Live WinRT query. Used on left-click so a missed event cannot block connect.
    /// Updates the cache so the icon matches.
    /// </summary>
    public bool RefreshFromOs()
    {
        bool on = BluetoothAudioService.IsBluetoothRadioOn();
        _isOn = on;
        return on;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Radio? radio = _radio;
        _radio = null;
        if (radio is null)
            return;

        try
        {
            radio.StateChanged -= OnStateChanged;
        }
        catch
        {
        }
    }

    private void Attach()
    {
        try
        {
            IReadOnlyList<Radio> radios = Task.Run(() => Radio.GetRadiosAsync().AsTask())
                .GetAwaiter()
                .GetResult();

            Radio? bluetooth = radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
            if (bluetooth is null)
            {
                _isOn = true;
                return;
            }

            _radio = bluetooth;
            _isOn = bluetooth.State == RadioState.On;
            _radio.StateChanged += OnStateChanged;
        }
        catch
        {
            _isOn = true;
        }
    }

    private void OnStateChanged(Radio sender, object args)
    {
        if (_disposed)
            return;

        try
        {
            _isOn = sender.State == RadioState.On;
        }
        catch
        {
            _isOn = true;
        }

        Notify();
    }

    private void Notify()
    {
        if (_disposed || !_ui.IsHandleCreated)
            return;

        try
        {
            _ui.BeginInvoke(_onChanged);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
