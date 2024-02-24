using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void ConfigureAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/login", Login)
                .WithName("Login").Accepts<LoginRequestDTO>("application/json").Produces<APIResponse>(200).Produces(400);

            app.MapPost("/api/register", Register)
                .WithName("Register").Accepts<RegistrationRequestDTO>("application/json").Produces<APIResponse>(200).Produces(400);
        }

        private async static Task<IResult> Login([FromBody] LoginRequestDTO model, IAuthRepository _authRepo)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
            var loginResponse = await _authRepo.Login(model);
            if(loginResponse == null)
            {
                response.ErrorMessages.Add("Username or Password is incorrect");
                return Results.BadRequest(response);
            }

            response.Result = loginResponse;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }

        private async static Task<IResult> Register([FromBody] RegistrationRequestDTO model, IAuthRepository _authRepo)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
            bool ifUserNameIsUnique = _authRepo.IsUniqueUser(model.UserName);
            if(!ifUserNameIsUnique)
            {
                response.ErrorMessages.Add("Username already exist");
                return Results.BadRequest(response);
            }

            var registerResponse = await _authRepo.Register(model);
            if (registerResponse == null || string.IsNullOrEmpty(registerResponse.UserName))
            {
                return Results.BadRequest(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
    }
}
