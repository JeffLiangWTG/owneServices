using eServices.Dms.Core.ConfigApi.Services;
using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDmsApiSwaggerGen("DmsConfigApi");

var dmsConfigConnectionString = builder.Configuration.GetConnectionString("DmsConfig")
	?? throw new InvalidOperationException("Missing connection string: DmsConfig");
builder.Services.AddDbContext<eHubTransactionsContext>(options
	=> options.UseSqlServer(dmsConfigConnectionString,
		providerOptions => providerOptions.EnableRetryOnFailure()));

var dmsIdentityConnectionString = builder.Configuration.GetConnectionString("DmsIdentity")
	?? throw new InvalidOperationException("Missing connection string: DmsIdentity");
builder.Services.AddDbContext<IdentityDbContext<AppUser>>(options
	=> options.UseSqlServer(dmsIdentityConnectionString,
		providerOptions => providerOptions.EnableRetryOnFailure()));

builder.Services.AddIdentityCore<AppUser>()
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<IdentityDbContext<AppUser>>()
	.AddSignInManager();

builder.Services.AddAuthentication(BasicDefaults.AuthenticationScheme)
	.AddBasic()
	.AddIdentityCookies();
builder.Services.AddAuthorization(options =>
{
	if (!builder.Environment.IsDevelopment())
		options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapWtgHealthChecks();

app.NewVersionedApi()
	.MapGroup("registrations")
	.MapRegistrationsEndpoints();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.Run();
