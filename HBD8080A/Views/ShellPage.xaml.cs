using HBD8080A.Contracts.Services;
using HBD8080A.Hardware.Computer;
using HBD8080A.Helpers;
using HBD8080A.Services;
using HBD8080A.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation.Metadata;
using Windows.System;

namespace HBD8080A.Views;

public sealed partial class ShellPage : Page {
    public ShellViewModel ViewModel {
        get;
    }

    public ShellPage(ShellViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        ShellMenuBarSettingsButton.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(ShellMenuBarSettingsButton_PointerPressed), true);
        ShellMenuBarSettingsButton.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(ShellMenuBarSettingsButton_PointerReleased), true);
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args) {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) {
        ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerPressedEvent, (PointerEventHandler)ShellMenuBarSettingsButton_PointerPressed);
        ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)ShellMenuBarSettingsButton_PointerReleased);
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null) {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue) {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void ShellMenuBarSettingsButton_PointerEntered(object sender, PointerRoutedEventArgs e) {
        AnimatedIcon.SetState((UIElement)sender, "PointerOver");
    }

    private void ShellMenuBarSettingsButton_PointerPressed(object sender, PointerRoutedEventArgs e) {
        AnimatedIcon.SetState((UIElement)sender, "Pressed");
    }

    private void ShellMenuBarSettingsButton_PointerReleased(object sender, PointerRoutedEventArgs e) {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void ShellMenuBarSettingsButton_PointerExited(object sender, PointerRoutedEventArgs e) {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void Load_Click(object sender, RoutedEventArgs e) {

    }

    private void Monitor_Click(object sender, RoutedEventArgs e) {

    }

    private void CommandBar_Loaded(object sender, RoutedEventArgs e) {
        //if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.AppBarButton", "LabelPosition")) {
        //    BtnLoad.LabelPosition = CommandBarLabelPosition.Collapsed;
        //}
    }

    private void GraphicsTerm_Click(object sender, RoutedEventArgs e) {

    }

    private void SCS_Click(object sender, RoutedEventArgs e) {

    }

    private void PATB_Click(object sender, RoutedEventArgs e) {
        //load file from static location
        //var memory = File.ReadAllBytes("f:\\dev\\8080\\TinyB\\PaloAlto\\patb.bin");
        var memory = HBD8080A.Resources.i8080Programs.paloaltotinybasic;
        if (memory != null) {
            var Computer = App.GetService<ComputerSystemService>();
            var Cpu = Computer.Cpu;
            //stop cpu
            PrepareToLoad(Cpu);
            //reset
            //load memory
            for (var i = 0; i < memory.Count(); i++) {
                Cpu.Memory[i] = memory[i];
            }
            //start cpu
            CpuState(Cpu, Cpu8080A.CpuState.Running);
        }
    }

    private void PrepareToLoad(Cpu8080A cpu) {
        //if off, turn on
        if (cpu.CurrentState == Cpu8080A.CpuState.Off) {
            CpuState(cpu, Cpu8080A.CpuState.On);
        }
        //if running then stop
        if (cpu.CurrentState == Cpu8080A.CpuState.Running || cpu.CurrentState == Cpu8080A.CpuState.Halt) {
            CpuState(cpu, Cpu8080A.CpuState.Stopped);
        }
        //if on then reset
        if (cpu.CurrentState == Cpu8080A.CpuState.On) {
            CpuState(cpu, Cpu8080A.CpuState.Reset);
        }

        //if reset wait for not reset
    }

    private void CpuState(Cpu8080A cpu, Cpu8080A.CpuState newState) {
        cpu.RequestedState = newState;
        //when done, this will reset to unchanged
        var elapsed = 0;
        do {
            if (cpu.RequestedState == newState) {
                Thread.Sleep(1000);
                elapsed++;
            } else {
                break;
            }
        } while (elapsed < 1000);
    }

    private void TTY_Click(object sender, RoutedEventArgs e) {
        var Terminal = App.GetService<DumbTerminalService>();
        Terminal.IsOn = this.BtnTTY.IsChecked ?? false;

    }
}
