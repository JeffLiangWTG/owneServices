using System.Text;
using eServices.Dms.Core.OpsPortal.Components;
using eServices.Dms.Core.OpsPortal.Services;
using eServices.Dms.Core.ServiceDefaults;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddWindowsService();

builder.Services.AddDmsApiSwaggerGen("DmsOpsPortal");

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("MessagesUser", policy
		=> policy.RequireRole(builder.Configuration["MessagesUserGroup"]
			?? throw new InvalidOperationException("Missing configuration setting: MessagesUserGroup")));
	options.AddPolicy("eServicesAdmins", policy
		=> policy.RequireRole(builder.Configuration["eServicesAdminsGroup"]
			?? throw new InvalidOperationException("Missing configuration setting: eServicesAdminsGroup")));
	if (!builder.Environment.IsEnvironment("Test"))
	{
		options.FallbackPolicy = options.DefaultPolicy;
	}
});

builder.Services.AddHttpClient("DmsMessagesApi", client =>
{
	client.BaseAddress = new("http://messagesapi");
	var auth = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{AppIds.DmsOpsPortal}:{builder.Configuration["DMSOPSPORTAL_PWD"]
			?? throw new InvalidOperationException("Missing configuration setting: DMSOPSPORTAL_PWD")}"))}";
	client.DefaultRequestHeaders.Add("Authorization", auth);
}).AddApiVersion(DmsDefaults.DefaultApiVersion);

var dmsIdentityConnectionString = builder.Configuration.GetConnectionString("DmsIdentity")
	?? throw new InvalidOperationException("Missing connection string: DmsIdentity");
builder.Services.AddDbContext<AppIdentityContext>(options =>
	options.UseSqlServer(dmsIdentityConnectionString,
		providerOptions => providerOptions.EnableRetryOnFailure()));

var dmsStorageConnectionString = builder.Configuration.GetConnectionString("DmsStorage")
	?? throw new InvalidOperationException("Missing connection string: DmsStorage");
builder.Services.AddDbContextFactory<DmsStorageContext>(options =>
	options.UseSqlServer(dmsStorageConnectionString,
		providerOptions => providerOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IDmsStorageRepository, DmsStorageRepository_V0_1>();

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<AppUser>()
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<AppIdentityContext>();

builder.Services.AddTransient<AppAccessService>();

#if DEBUG
builder.Services.AddSassCompiler();

if (builder.Environment.IsDevelopment())
{
	var dmsMessagesConnectionString = builder.Configuration.GetConnectionString("DmsMessages")
		?? throw new InvalidOperationException("Missing connection string: DmsMessages");
	builder.Services.AddDbContextFactory<eHubTransactionsContext>(options =>
		options.UseSqlServer(dmsMessagesConnectionString,
		providerOptions => providerOptions.EnableRetryOnFailure()));
}
else
{
	builder.Services.AddDbContextFactory<eHubTransactionsContext>(options => options.UseInMemoryDatabase("DmsStorage"));
}
#else
	builder.Services.AddDbContextFactory<eHubTransactionsContext>(options => options.UseInMemoryDatabase("DmsStorage"));
#endif

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoggingMiddleware>();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.NewVersionedApi().MapGroup("/messages/content")
	.MapMessagesEndpoints();

if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	scope.ServiceProvider.GetRequiredService<AppIdentityContext>().Database.EnsureCreated();
	scope.ServiceProvider.GetRequiredService<eHubTransactionsContext>().Database.EnsureCreated();
	scope.ServiceProvider.GetRequiredService<DmsStorageContext>().Database.EnsureCreated();

	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	await userManager.CreateAsync(new AppUser { UserName = AppIds.DmsGateway, AccessKey = "FOQ28QeWENV6zAEpaTdJxR+D6VdxeJqZ4zsYP+LnlT4=" }, "DM$phase1");
	await userManager.CreateAsync(new AppUser { UserName = AppIds.DmsOpsPortal, AccessKey = "FOQ28QeWENV6zAEpaTdJxR+D6VdxeJqZ4zsYP+LnlT4=" }, "DM$phase1");

	var usc = new AppUser { UserName = "USC", AccessKey = "BnidFVrJF6QHzXYd4AfyLLqaBfhWH3xGkcfINEd9qt4=" };
	await userManager.CreateAsync(usc, "DM$phase1");
	List<string> roles = [$"{AppRoles.SendMessage}:USC", $"{AppRoles.ReceiveMessage}:USC", $"{AppRoles.ReadConfig}:USC", $"{AppRoles.WriteStorage}:USC", $"{AppRoles.ReadStorage}:USC"];
	roles.ForEach(r => roleManager.CreateAsync(new(r)).Wait());
	await userManager.AddToRolesAsync(usc, roles);
}

app.Run();
