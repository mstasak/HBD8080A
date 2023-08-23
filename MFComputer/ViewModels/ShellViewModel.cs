using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MFComputer.Contracts.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

namespace MFComputer.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;

    public ICommand MenuFileExitCommand
    {
        get;
    }

    public ICommand MenuViewsTestbedCommand
    {
        get;
    }
    public ICommand MenuSettingsCommand
    {
        get;
    }

    public ICommand MenuViewsStatusCommand
    {
        get;
    }

    public ICommand MenuViewsMainCommand
    {
        get;
    }

    public INavigationService NavigationService
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public ShellViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;

        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuViewsTestbedCommand = new RelayCommand(OnMenuViewsTestbed);
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
        MenuViewsStatusCommand = new RelayCommand(OnMenuViewsStatus);
        MenuViewsMainCommand = new RelayCommand(OnMenuViewsMain);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    private void OnMenuFileExit() => Application.Current.Exit();

    private void OnMenuViewsTestbed() => NavigationService.NavigateTo(typeof(TestbedViewModel).FullName!);

    private void OnMenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);

    private void OnMenuViewsStatus() => NavigationService.NavigateTo(typeof(StatusViewModel).FullName!);

    private void OnMenuViewsMain() => NavigationService.NavigateTo(typeof(MainViewModel).FullName!);
}
