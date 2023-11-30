using System.Text;
using HBD8080A.Hardware.Computer;
using HBD8080A.Services;
using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace HBD8080A.Views;

public sealed partial class MemoryPage : Page
{
    public MemoryViewModel ViewModel
    {
        get;
    }
    public ComputerSystemService Computer {
        get;
    }
    public Cpu8080A Cpu {
        get;
    }

    public MemoryPage()
    {
        ViewModel = App.GetService<MemoryViewModel>();
        Computer = App.GetService<ComputerSystemService>();
        Cpu = Computer.Cpu;
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        RefreshMemoryView();
    }

    private void RefreshMemoryView() {
        StringBuilder sb = new();
        var memory = Cpu.Memory;
        for (var i = 0; i < 65536; i += 16) {
            var sHex = "";
            var sText = "";
            sHex += $"{i:X4}: ";
            for (var j = 0; j < 16; j++) {
                sHex += $"{memory[i + j]:X2} ";
                var ch = memory[i + j];
                if (ch < 32) {
                    sText += ".";
                } else {
                    sText += (char)ch;
                }
            }

            sb.Append(sHex + "   " + sText + "\n");
        }

        txtMemory.Text = sb.ToString();
    }

    private void RefreshBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        //MVVM be damned (sometimes)!  This is easier than setting up observable viewmodel property bindings.
        //At least for RAD prototype work.
        PCBox.Text = $"{Cpu.PC:X4}";
        SPBox.Text = $"{Cpu.A:X4}";
        ABox.Text = $"{Cpu.A:X2}";
        BBox.Text = $"{Cpu.B:X2}";
        CBox.Text = $"{Cpu.C:X2}";
        DBox.Text = $"{Cpu.D:X2}";
        EBox.Text = $"{Cpu.E:X2}";
        HBox.Text = $"{Cpu.H:X2}";
        LBox.Text = $"{Cpu.L:X2}";
        MBox.Text = $"{Cpu.M:X2}";
        FlagBox.Text = $"{Cpu.A:X2}";
        ABox.Text = $"{Cpu.A:X2}";
        RefreshMemoryView();
    }

    private void StepBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }

    private void RunBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }

    private void StopBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }
}


/**
 * Potential enhancements:
 * Control data format (hex+text, hex readonly, text readonly, unicode?, disassembly pane, octab, binary, some kind of bitmap, etc)
 * Bookmarks
 * Goto Address
 * Symbol pane to show values associated with nearby addresses or referenced addresses?
 * edit capability
 * highlighting memory state (ROM/RAM, Paged, uninitialized, loaded, read from, written to, recently readonly, recently written?)
 * marking and copy to clipboard?
 * 
 **/ 