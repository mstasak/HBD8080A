// See https://aka.ms/new-console-template for more information
using System.Reflection;
using Microsoft.VisualBasic;

public class XASMMain {
    static int Main(string[] args) {
        if (args.Length < 2) {
            PrintTitleAndUsage();
            return 1;
        }
        if (args.Length == 2) {
            PrintHelp();
            return 1;
        }
        Console.WriteLine("Hello, World!");
        return 0;
    }

    private static void PrintHelp() {
        Console.Error.WriteLine("""

            """);
    }

    private static void PrintTitleAndUsage() {
        var exeName = Assembly.GetExecutingAssembly().GetName().Name;
        var exeVer = Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"""
            {exeName} {exeVer}: a cross-assembler for compiling 8080 assembler code under Windows.
            Usage: {exeName} [options] filename [,filename...]
            For more info, use {exeName} -h or {exeName} --help
        """);
    }
}