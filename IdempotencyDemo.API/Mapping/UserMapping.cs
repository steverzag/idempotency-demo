using IdempotencyDemo.API.Data.Models;
using IdempotencyDemo.API.DTOs;

namespace IdempotencyDemo.API.Mapping
{
	public static class UserMapping
	{
		public static UserDTO ToDTO(this User user)
		{
			return new UserDTO
			(
				user.Id,
				user.FirstName,
				user.LastName,
				user.Email,
				user.CreatedAt
			);
		}

		public static User ToEntity(this CreateUserRequest request)
		{
			return new User
			{
				FirstName = request.FirstName,
				LastName = request.LastName,
				Email = request.Email
			};
		}

		public static User ToEntity(this UpdateUserRequest request, User user)
		{
			user.FirstName = request.FirstName;
			user.LastName = request.LastName;
			user.Email = request.Email;

			return user;
		}
	}
}
