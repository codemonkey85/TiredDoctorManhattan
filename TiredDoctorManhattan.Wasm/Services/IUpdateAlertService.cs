namespace TiredDoctorManhattan.Wasm.Services;

public interface IUpdateAlertService
{
    event Action? OnUpdateAvailable;

    void ShowUpdateMessage();
}
