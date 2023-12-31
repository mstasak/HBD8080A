﻿using HBD8080A.Contracts.Services;
using HBD8080A.ViewModels;

using Microsoft.UI.Xaml;

namespace HBD8080A.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        //_navigationService.NavigateTo(typeof(MainViewModel).FullName!, args.Arguments);
        _navigationService.NavigateTo(typeof(FrontPanelViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
