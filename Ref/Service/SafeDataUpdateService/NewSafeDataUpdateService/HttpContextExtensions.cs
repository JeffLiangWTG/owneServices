using System;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class HttpContextExtensions
	{
		const string DB_Context = "Batch_DbContext";

		public static bool ShouldReportIssue(this HttpContext context)
		{
			Argument.NotNull(context, nameof(context));
			var strReportIssue = context.Request?.Headers?["ReportIssue"];
			if (strReportIssue.HasValue && !StringValues.IsNullOrEmpty(strReportIssue.Value))
			{
				return strReportIssue.Value.ToString() != "false";
			}
			return false;
		}

		public static void SetContext(this HttpContext context, IReferenceDataRepository repository, bool shared)
		{
			Argument.NotNull(context, nameof(context));
			Argument.NotNull(repository, nameof(repository));

			context.Items[DB_Context] = Tuple.Create(repository, shared);
		}

		public static Tuple<IReferenceDataRepository, bool> GetContext(this HttpContext context)
		{
			Argument.NotNull(context, nameof(context));
			if (context.Items.TryGetValue(DB_Context, out var dbContext))
			{
				var result = (Tuple<IReferenceDataRepository, bool>)dbContext;
				return result;
			}
			else
			{
				var repository = context.RequestServices?.GetRequiredService<IReferenceDataRepository>();
				SetContext(context, repository, false);
				return Tuple.Create(repository, false);
			}
		}
	}
}
