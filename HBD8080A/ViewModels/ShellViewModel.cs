using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HBD8080A.Contracts.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

namespace HBD8080A.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;

    public ICommand MenuFileExitCommand
    {
        get;
    }

    public ICommand MenuViewsTerminalCommand
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
    public ICommand MenuViewsMemoryCommand
    {
        get;
    }
    public ICommand MenuViewsRegistersCommand
    {
        get;
    }
    public ICommand MenuViewsDisAsmCommand
    {
        get;
    }

    //public ICommand MenuViewsFrontPanelCommand
    //{
    //    get;
    //}

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
        MenuViewsTerminalCommand = new RelayCommand(OnMenuViewsTerminal);
        MenuViewsTestbedCommand = new RelayCommand(OnMenuViewsTestbed);
        //MenuViewsFrontPanelCommand = new RelayCommand(OnMenuViewsFrontPanel);
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
        MenuViewsStatusCommand = new RelayCommand(OnMenuViewsStatus);
        MenuViewsMemoryCommand = new RelayCommand(OnMenuViewsMemory);
        MenuViewsRegistersCommand = new RelayCommand(OnMenuViewsRegisters);
        MenuViewsDisAsmCommand = new RelayCommand(OnMenuViewsDisAsm);
        MenuViewsMainCommand = new RelayCommand(OnMenuViewsMain);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    private void OnMenuFileExit() => Application.Current.Exit();

    private void OnMenuViewsTerminal() => NavigationService.NavigateTo(typeof(TerminalViewModel).FullName!);

    private void OnMenuViewsTestbed() => NavigationService.NavigateTo(typeof(TestbedViewModel).FullName!);
    //private void OnMenuViewsFrontPanel() => NavigationService.NavigateTo(typeof(FrontPanelViewModel).FullName!);

    private void OnMenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    private void OnMenuViewsMemory() => NavigationService.NavigateTo(typeof(MemoryViewModel).FullName!);
    private void OnMenuViewsRegisters() => NavigationService.NavigateTo(typeof(RegistersViewModel).FullName!);
    private void OnMenuViewsDisAsm() => NavigationService.NavigateTo(typeof(DisAsmViewModel).FullName!);

    private void OnMenuViewsStatus() => NavigationService.NavigateTo(typeof(StatusViewModel).FullName!);

    private void OnMenuViewsMain() => NavigationService.NavigateTo(typeof(FrontPanelViewModel).FullName!);
}
