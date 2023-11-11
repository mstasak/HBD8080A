namespace HBD8080A.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
    
    /// <summary>
    /// As part of shutdown, close and modeless accessory windows (CPU View,
    /// AM View, Log view, etc).
    /// </summary>
    public void CloseOtherWindows();
}
