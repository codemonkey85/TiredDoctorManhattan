namespace TiredDoctorManhattan.Wasm.Services;

public class UpdateAlertService : IUpdateAlertService
{
    public static UpdateAlertService? Instance { get; private set; }

    public event Action? OnUpdateAvailable;

    public UpdateAlertService() => Instance = this;

    void IUpdateAlertService.ShowUpdateMessage() => ShowUpdateMessage();

    [JSInvokable(nameof(ShowUpdateMessage))]
    public static void ShowUpdateMessage() => Instance?.OnUpdateAvailable?.Invoke();
}
