using Microsoft.AspNetCore.Components;
using System;
using Tangy_Models;
using TangyWeb_Client.Service.IService;

namespace TangyWeb_Client.Pages.Authentication
{
    public partial class Register
    {
        private SignUpRequestDTO SignUpRequest = new();
        public bool IsProcessing { get; set; } = false;
        public bool ShowRegistrationErrors { get; set; } = false;
        public IEnumerable<string> Errors { get; set; }

        [Inject]
        IAuthenticationService _authService { get; set; }

        [Inject]
        NavigationManager _navigationManager { get; set; }


        private async Task RegisterUser()
        {
            ShowRegistrationErrors = false;
            IsProcessing = true;
            var result = await _authService.RegisterUser(SignUpRequest);
            if (result.IsRegistrationSuccessful)
            {
                // registration successful
                _navigationManager.NavigateTo("/login");
            }
            else
            {
                //registration failed
                Errors = result.Errors;
                ShowRegistrationErrors = true;
            }

            IsProcessing = false;
        }
    }
}
