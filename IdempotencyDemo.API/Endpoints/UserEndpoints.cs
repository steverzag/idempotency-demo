
using Microsoft.EntityFrameworkCore;
using IdempotencyDemo.API.Data;
using IdempotencyDemo.API.Data.Models;
using IdempotencyDemo.API.DTOs;
using IdempotencyDemo.API.Endpoints.Configuration;
using IdempotencyDemo.API.Services;
using IdempotencyDemo.API.Filters;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace IdempotencyDemo.API.Endpoints
{
	public class UserEndpoints : IEndpoints
	{
		public void RegisterEndpoints(IEndpointRouteBuilder builder)
		{
			var group = builder
				.MapGroup("/users")
				.WithTags("Users")
				.AddFluentValidationAutoValidation();

			group.MapGet("/", GetAllUsers);
			group.MapGet("/{id}", GetUserById).WithName("UserById");
			group.MapPost("/", CreateUser)
				.AddEndpointFilter<IdempotentBasicFilter>();//In Memory and basic indepotency implementation
				//.AddEndpointFilter<IdempotentCachedFilter>();//Cached indepotency implementation
			group.MapPut("/", UpdateUser);
			group.MapDelete("/{id}", DeleteUser);
		}

		private async static Task<IResult> GetAllUsers(UserService userService)
		{
			var users = await userService.GetAllUsersAsync();
			return Results.Ok(users);
		}

		private async static Task<IResult> GetUserById(int id, UserService userService)
		{
			var user = await userService.GetUserByIdAsync(id);
			if(user is null)
			{
				return Results.NotFound("user not found");
			}

			return Results.Ok(user);
		}

		private async static Task<IResult> CreateUser(CreateUserRequest request, UserService userService)
		{
			var userId = await userService.CreateUserAsync(request);
			return Results.CreatedAtRoute("UserById", routeValues: new { id = userId });
		}

		private async static Task<IResult> UpdateUser(UpdateUserRequest request, UserService userService)
		{
			var user = await userService.UpdateUserAsync(request);
			return Results.Ok(user);
		}

		private async static Task<IResult> DeleteUser(int id, UserService userService)
		{
			await userService.DeleteUserAsync(id);
			return Results.NoContent();
		}
	}
}
