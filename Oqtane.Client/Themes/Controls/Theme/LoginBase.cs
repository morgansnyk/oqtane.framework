using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Enums;
using Oqtane.Providers;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Themes.Controls
{
    public class LoginBase : ThemeControlBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IUserService UserService { get; set; }
        [Inject] public IJSRuntime jsRuntime { get; set; }
        [Inject] public IServiceProvider ServiceProvider { get; set; }
        [Inject] public SiteState SiteState { get; set; }
        [Inject] public ILogService LoggingService { get; set; }

        protected void LoginUser()
        {
            var returnurl = PageState.Alias.Path;
            if (PageState.Page.Path != "/")
            {
                returnurl += "/" + PageState.Page.Path;
            }
            NavigationManager.NavigateTo(NavigateUrl("login", "?returnurl=" + returnurl));
        }

        protected async Task LogoutUser()
        {
            await UserService.LogoutUserAsync(PageState.User);
            await LoggingService.Log(PageState.Alias, PageState.Page.PageId, null, PageState.User.UserId, GetType().AssemblyQualifiedName, "Logout", LogFunction.Security, LogLevel.Information, null, "User Logout For Username {Username}", PageState.User.Username);
            PageState.User = null;

            var url = PageState.Alias.Path + "/" + PageState.Page.Path;
            if (!UserSecurity.IsAuthorized(PageState.User, PermissionNames.View, PageState.Page.Permissions))
            {
                url = PageState.Alias.Path;
            }            

            if (PageState.Runtime == Shared.Runtime.Server)
            {
                // server-side Blazor needs to redirect to the Logout page
                NavigationManager.NavigateTo(Utilities.TenantUrl(PageState.Alias, "/pages/logout/") + "?returnurl=" + WebUtility.UrlEncode(url), true);
            }
            else
            {
                // client-side Blazor
                var authstateprovider = (IdentityAuthenticationStateProvider)ServiceProvider.GetService(typeof(IdentityAuthenticationStateProvider));
                authstateprovider.NotifyAuthenticationChanged();
                NavigationManager.NavigateTo(NavigateUrl(url, true));
            }
        }
    }
}
