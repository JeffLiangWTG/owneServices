using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public static class HttpContextExtensions
	{
		const string DB_Context = "Batch_DbContext";

		public static void SetContext(this HttpContext context, IStagingRepository repository)
		{
			Argument.NotNull(context, nameof(context));
			Argument.NotNull(repository, nameof(repository));

			context.Items[DB_Context] = repository;
		}

		public static IStagingRepository GetContext(this HttpContext context)
		{
			Argument.NotNull(context, nameof(context));

			if (context.Items.TryGetValue(DB_Context, out var dbContext))
			{
				var result = (IStagingRepository)dbContext;
				return result;
			}
			else
			{
				var repository = context.RequestServices?.GetRequiredService<IStagingRepository>();
				SetContext(context, repository);
				return repository;
			}
		}
	}
}
