namespace IdempotencyDemo.API.Endpoints.Configuration
{
	public static class EnpointsRegisterer
	{
		public static void RegisterEndpoints(this WebApplication app)
		{

			static bool isAssignableToIAppEnpoints(Type t) =>
				!t.IsAbstract
				&& !t.IsInterface
				&& typeof(IEndpoints).IsAssignableFrom(t);

			var type = typeof(IEndpoints);
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(isAssignableToIAppEnpoints);

			List<string> failedImplementations = [];
			foreach (var t in types)
			{
				var ctor = t.GetConstructor(Type.EmptyTypes);

				if (ctor is null)
				{
					failedImplementations.Add($"{t.Name}: IAppEndpoints implementing classes must present an empty constructor");
					continue;
				}

				var appEndpoints = (IEndpoints)Activator.CreateInstance(t)!;
				appEndpoints.RegisterEndpoints(app);
			}

			if (failedImplementations.Count > 0)
			{
				throw new EnpointsRegisterFailedException("Some endpoints failed to register", failedImplementations);
			}
		}
	}

	public class EnpointsRegisterFailedException(string message, IEnumerable<string> failingImplementations) : Exception(message)
	{
		public IEnumerable<string> FailingImplementations { get; set; } = failingImplementations;
	}
}
