﻿using System;
using System.Threading.Tasks;
using AuthClient.Models;
using AuthClient.RequestSettings.Inerfaces;
using AuthClient.ViewModels.AccountViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthClient.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IRequestService _communicationService;
        private readonly ILogger<AccountController> _logger;
        private const string CookieTokenKeyName = "token";

        public AccountController(IRequestService service, 
            ILogger<AccountController> logger)
        {
            _communicationService = service;
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await SetTokenCookie(model.Email, model.Password);

            _logger.LogInformation($"User was loged in with email: {model.Email}");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _communicationService.PostAsync("api/register", model, FormHeaders(JsonType));

            await SetTokenCookie(model.Email, model.Password);

            _logger.LogInformation($"User was registered with email: {model.Email}");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult LogOff()
        {
            Response.Cookies.Delete(CookieTokenKeyName);

            _logger.LogInformation("User was loged off");

            return RedirectToAction("Index", "Home");
        }

        private async Task SetTokenCookie(string email, string password)
        {
            var body = $"username={email}&password={password}";

            var token = await _communicationService.PostAsync<TokenApiModel, string>("token", body, FormHeaders(FormType));

            Response.Cookies.Append(CookieTokenKeyName, token.Token, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(token.ExpiresIn)
            });

            _logger.LogInformation($"User with username = {email} password = {password} got token");
        }
    }
}