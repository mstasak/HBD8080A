﻿using HBD8080A.Activation;
using HBD8080A.Contracts.Services;
using HBD8080A.Core.Contracts.Services;
using HBD8080A.Core.Services;
using HBD8080A.Helpers;
using HBD8080A.Models;
using HBD8080A.Services;
using HBD8080A.ViewModels;
using HBD8080A.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HBD8080A;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();
    public static Dictionary<string, WindowEx> OtherWindows {
        get;
    } = new Dictionary<string, WindowEx>();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton(ComputerSystemService.Instance);
            {
                _ = DumbTerminalService.Instance;
            }
            services.AddSingleton(DumbTerminalService.Instance);
            services.AddSingleton(DisplayAdapterService.Instance);
            //services.AddSingleton(FrontPanelDataService.Instance);

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            //Emulator
            services.AddSingleton<Hardware.Computer.Cpu8080A>();

            // Views and ViewModels
            services.AddSingleton<FrontPanelViewModel>();
            services.AddTransient<TerminalViewModel>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            //services.AddTransient<MainPage>();
            services.AddTransient<MainViewModel>();
            services.AddSingleton<FrontPanelPage>();
            services.AddTransient<StatusPage>();
            services.AddTransient<StatusViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MemoryPage>();
            services.AddTransient<MemoryViewModel>();
            services.AddSingleton<DisplayAdapterPage>();
            services.AddTransient<DisplayAdapterViewModel>();
            services.AddTransient<RegistersPage>();
            services.AddTransient<RegistersViewModel>();
            services.AddTransient<DisAsmPage>();
            services.AddTransient<DisAsmViewModel>();
            services.AddSingleton<TestbedPage>();
            services.AddSingleton<TestbedViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    /// <summary>
    /// App startup code.
    /// Populate the main window shell with an instance of MainPage.
    /// Also create a second window and populate it with the Front Panel page.
    /// May enhance to remember window sizes and positions, and perhaps
    /// restore state of other windows (cpu view, ram view, registers, etc)
    /// </summary>
    /// <param name="args"></param>
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);

        //var fpWindow = new FrontPanelWindow();
        //OtherWindows.Add("Front Panel", fpWindow);
        //var fpFrame = new Frame();
        //fpWindow.Content = fpFrame;
        //var frontPanel = GetService<FrontPanelPage>();
        //fpFrame.Content = frontPanel;
        //fpWindow.Show();
    }
}
