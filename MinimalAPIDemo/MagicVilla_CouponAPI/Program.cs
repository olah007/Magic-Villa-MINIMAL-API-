using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast")
//.WithOpenApi();

app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
{
    APIResponse response = new();
    _logger.Log(LogLevel.Information, "Getting all Coupons");

    response.Result = CouponStore.couponList;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCoupons").Produces<APIResponse>(200);

app.MapGet("/api/coupon/{id:int}", (ILogger < Program > _logger, int id) =>
{
    APIResponse response = new();
    response.Result = CouponStore.couponList.FirstOrDefault(x => x.Id == id);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCoupon").Produces<APIResponse>(200);

app.MapPost("/api/coupon", async ([FromBody] CouponCreateDTO coupon_C_DTO, IMapper _mapper, IValidator<CouponCreateDTO> _validation) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
    
    //var validationResult = _validation.ValidateAsync(couponCreateDTO).GetAwaiter().GetResult();
    var validationResult = await _validation.ValidateAsync(coupon_C_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }
    if (CouponStore.couponList.FirstOrDefault(x => x.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already Exists.");
        return Results.BadRequest(response);
    }
    
    Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);
    coupon.Id = CouponStore.couponList.OrderBy(x => x.Id).ToList()[^1].Id + 1;
    CouponStore.couponList.Add(coupon);
    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    response.Result = couponDTO;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.Created;
    return Results.Ok(response);
    //return Results.CreatedAtRoute("GetCoupon", new { id = couponDTO.Id }, couponDTO);
    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
    //return Results.Ok(coupon);
}).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

app.MapPut("api/coupon", async ([FromBody] CouponUpdateDTO coupon_U_DTO, IMapper _mapper, IValidator<CouponUpdateDTO> _validation) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    var validationResult = await _validation.ValidateAsync(coupon_U_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }
    
    Coupon couponFromStore = CouponStore.couponList.FirstOrDefault(u => u.Id == coupon_U_DTO.Id);
    if(couponFromStore != null)
    {
        couponFromStore.IsActive = coupon_U_DTO.IsActive;
        couponFromStore.Name = coupon_U_DTO.Name;
        couponFromStore.Percent = coupon_U_DTO.Percent;
        couponFromStore.LastUpdated = DateTime.Now;
    }
    else
    {
        response.ErrorMessages.Add("Invalid id");
        return Results.BadRequest(response);
    }
    
    response.Result = _mapper.Map<CouponDTO>(couponFromStore);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("UpdateCoupon").Accepts<CouponUpdateDTO>("application/json").Produces<APIResponse>(200).Produces(400);

app.MapDelete("api/coupon/{id:int}", (int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    Coupon couponFromStore = CouponStore.couponList.FirstOrDefault(u => u.Id == id);
    if (couponFromStore != null)
        CouponStore.couponList.Remove(couponFromStore);
    else
    {
        response.ErrorMessages.Add("Invalid id");
        return Results.BadRequest(response);
    }

    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.NoContent;
    return Results.Ok(response);
});

app.UseHttpsRedirection();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
