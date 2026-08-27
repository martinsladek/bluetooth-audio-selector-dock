namespace BluetoothDock;

sealed class AudioEndpointWatcher : IMMNotificationClient, IDisposable
{
    private readonly IMMDeviceEnumerator _enumerator;
    private readonly Control _ui;
    private readonly Action _onChanged;
    private bool _disposed;

    public AudioEndpointWatcher(Control ui, Action onChanged)
    {
        _ui = ui;
        _onChanged = onChanged;
        _enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();
        int hr = _enumerator.RegisterEndpointNotificationCallback(this);
        if (hr < 0)
            MarshalThrow(hr);
    }

    public int OnDeviceStateChanged(string deviceId, uint newState)
    {
        Notify();
        return 0;
    }

    public int OnDeviceAdded(string deviceId)
    {
        Notify();
        return 0;
    }

    public int OnDeviceRemoved(string deviceId)
    {
        Notify();
        return 0;
    }

    public int OnDefaultDeviceChanged(EDataFlow flow, ERole role, string defaultDeviceId)
    {
        Notify();
        return 0;
    }

    public int OnPropertyValueChanged(string deviceId, PropertyKey key)
    {
        Notify();
        return 0;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        try
        {
            _enumerator.UnregisterEndpointNotificationCallback(this);
        }
        catch
        {
        }

        ComRelease.Once(_enumerator);
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

    private static void MarshalThrow(int hr)
    {
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
    }
}
