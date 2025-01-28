namespace TiredDoctorManhattan.Wasm.Services;

public class UpdateAlertService : IUpdateAlertService
{
    public UpdateAlertService() => Instance = this;

    public static UpdateAlertService? Instance { get; private set; }

    public event Action? OnUpdateAvailable;

    void IUpdateAlertService.ShowUpdateMessage() => ShowUpdateMessage();

    [JSInvokable(nameof(ShowUpdateMessage))]
    public static void ShowUpdateMessage() => Instance?.OnUpdateAvailable?.Invoke();
}
