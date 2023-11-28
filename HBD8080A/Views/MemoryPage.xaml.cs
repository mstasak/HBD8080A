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

    public MemoryPage()
    {
        ViewModel = App.GetService<MemoryViewModel>();
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        StringBuilder sb = new();
        
        //for (int line = 1; line <= 1000; line++) {
        //    sb.Append($"Line {line,4}\n");   
        //}
        
        var Computer = App.GetService<ComputerSystemService>();
        var Cpu = Computer.Cpu;
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