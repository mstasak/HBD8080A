namespace MFComputer.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
    public void CloseOtherWindows();
}
