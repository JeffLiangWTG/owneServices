using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ReferenceDataUpdateService.Web
{
	public class Program
	{
		static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var app = builder.Build();
			app.UseDefaultFiles();
			app.UseStaticFiles();
			app.Use(async (context, next) =>
			{
				context.Request.Path = new PathString(@"/index.html");
				await next();
			});
			app.UseStaticFiles();
			app.Run();
		}
	}
}
