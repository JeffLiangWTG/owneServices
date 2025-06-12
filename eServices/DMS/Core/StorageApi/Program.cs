using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.Dms.Core.StorageApi.Services;
using eServices.Dms.Core.StorageRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
	serverOptions.AllowSynchronousIO = true;
});

builder.Services.AddDmsApiSwaggerGen("DmsStorageApi");

builder.Services.AddDbContext<DmsStorageContext>(options
	=> options.UseSqlServer(builder.Configuration.GetConnectionString("DmsStorage")
		?? throw new InvalidOperationException("Missing connection string: DmsStorage"),
		providerOptions => providerOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IDmsStorageRepository, DmsStorageRepository_V0_1>();

builder.Services.AddDbContext<IdentityDbContext<AppUser>>(options
	=> options.UseSqlServer(builder.Configuration.GetConnectionString("DmsIdentity")
		?? throw new InvalidOperationException("Missing connection string: DmsIdentity"),
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

var documents = app.NewVersionedApi().MapGroup("/{owner}/docs/{table-name}/{key}")
	.MapDocsEndpoints();

var objects = app.NewVersionedApi().MapGroup("/{owner}/blobs/{table-name}/{key}")
	.MapBlobsEndpoints();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.Run();
