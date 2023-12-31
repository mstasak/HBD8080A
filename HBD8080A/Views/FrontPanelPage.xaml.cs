﻿using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace HBD8080A.Views;

public sealed partial class FrontPanelPage : Page
{
    public FrontPanelViewModel ViewModel
    {
        get;
    }

    public FrontPanelPage()
    {
        ViewModel = App.GetService<FrontPanelViewModel>();
        InitializeComponent();
    }

}
