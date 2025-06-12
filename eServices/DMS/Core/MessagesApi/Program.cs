using eServices.Dms.Core.MessagesApi.Services;
using eServices.Dms.Core.MessagesRepository;
using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDmsApiSwaggerGen("DmsMessagesApi");

var dmsMessagesConnectionString = builder.Configuration.GetConnectionString("DmsMessages")
	?? throw new InvalidOperationException("Missing connection string: DmsMessages");
builder.Services.AddDbContext<eHubTransactionsContext>(options
	=> options.UseSqlServer(dmsMessagesConnectionString,
		providerOptions => providerOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IDmsMessagesRepository_V0_1, DmsMessagesRepository_V0_1>();

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
	options.AddPolicy(AppRoles.SendMessage, policy => { policy.RequireRole(AppRoles.SendMessage); });
	options.AddPolicy(AppRoles.ReceiveMessage, policy => { policy.RequireRole(AppRoles.ReceiveMessage); });
	if (!builder.Environment.IsDevelopment())
		options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapWtgHealthChecks();

var messages = app.NewVersionedApi()
	.MapGroup("messages")
	.MapMessagesEndpoints();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.Run();
