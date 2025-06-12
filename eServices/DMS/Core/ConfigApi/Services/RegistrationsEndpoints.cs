using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace eServices.Dms.Core.ConfigApi.Services
{
	public static class RegistrationsEndpoints
	{
		public static RouteGroupBuilder MapRegistrationsEndpoints(this RouteGroupBuilder group)
		{
			group.HasApiVersion(DmsDefaults.DefaultApiVersion);

			group.MapGet("/{owner}/{type}/{id}", GetByTypeAndId);
			group.MapGet("/{owner}/{type}", GetByType);

			return group;
		}

		public static async Task<Results<Ok<RegistrationResult>, NotFound, UnauthorizedHttpResult>> GetByTypeAndId(
			eHubTransactionsContext dmsConfigContext,
			HttpContext context,
			string owner,
			string type,
			string id,
			string? qualifier)
		{
			if (!context.User.IsInRole($"{AppRoles.ReadConfig}:{owner}"))
				return TypedResults.Unauthorized();

			switch (owner)
			{
				case "USC":
					bool.TryParse(qualifier ?? "true", out bool isProduction);
					var registrations = await dmsConfigContext.eHubUSCustomsRegistry.Where(r
							=> r.ER_ApplicationCode == type
							   && r.ER_CC_ClientNavigation != null
							   && r.ER_CC_ClientNavigation.CC_ID == id
							   && r.ER_IsProduction == isProduction)
						.Select(r => new RegistrationItem(r.ER_CC_ClientNavigation!.CC_ID, r.ER_Name, r.ER_Value, r.ER_IsProduction)).ToListAsync();
					return (registrations.Count > 0 ? TypedResults.Ok(new RegistrationResult(owner, type, registrations)) : TypedResults.NotFound());
				default:
					return TypedResults.NotFound();
			}
		}

		public static async Task<Results<Ok<RegistrationResult>, NotFound, UnauthorizedHttpResult>> GetByType(
			eHubTransactionsContext dmsConfigContext,
			HttpContext context,
			string owner,
			string type,
			string value,
			string? qualifier)
		{
			if (!context.User.IsInRole($"{AppRoles.ReadConfig}:{owner}"))
				return TypedResults.Unauthorized();

			switch (owner)
			{
				case "USC":
					bool.TryParse(qualifier ?? "true", out bool isProduction);
					var registrations = await dmsConfigContext.eHubUSCustomsRegistry.Where(r
							=> r.ER_ApplicationCode == type && r.ER_Value == value && r.ER_IsProduction == isProduction)
						.Select(r => new RegistrationItem(r.ER_CC_ClientNavigation!.CC_ID, r.ER_Name, r.ER_Value, r.ER_IsProduction)).ToListAsync();
					return (registrations.Count > 0 ? TypedResults.Ok(new RegistrationResult(owner, type, registrations)) : TypedResults.NotFound());
				default:
					return TypedResults.NotFound();
			}
		}

		public record RegistrationResult(string owner, string type, IEnumerable<RegistrationItem> registrations);
		public record RegistrationItem(string ClientId, string TypeName, string Code, bool? IsProduction);
	}
}
