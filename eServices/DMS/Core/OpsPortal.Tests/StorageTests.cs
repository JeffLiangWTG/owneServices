using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using eServices.Dms.Core.OpsPortal.Services;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.Dms.Core.StorageRepository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Storage = eServices.Dms.Core.OpsPortal.Components.Pages.Storage;

namespace eServices.Dms.Core.OpsPortal.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class StorageTests : BunitContext
{
	[Test]
	public void StorageIndexListsStorageOwners()
	{
		Services.AddDbContextFactory<DmsStorageContext>(options => options.UseInMemoryDatabase("DmsStorage"));
		this.SetupQuickGrid();

		var context = Services.GetRequiredService<IDbContextFactory<DmsStorageContext>>().CreateDbContext();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
		context.DmsStorageCatalog.Add(new() { SC_Schema = "APP", SC_Name = "ViaDMS", SC_Type = "Document", SC_Version = "0.1", SC_ExpirationDays = 90, SC_CreateUser = "Me", SC_CreateTime = DateTime.Parse("2024-09-01 01:00") });
		context.DmsStorageCatalog.Add(new() { SC_Schema = "OTH", SC_Name = "ViaDMS", SC_Type = "Document", SC_Version = "0.1", SC_ExpirationDays = 90, SC_CreateUser = "Me", SC_CreateTime = DateTime.Parse("2024-09-01 02:00") });
		context.SaveChanges();

		var cut = Render<Storage.Index>();

		cut.FindAll("td").MarkupMatches("""
			<td class="col-justify-start " >APP</td>
			<td class="col-justify-start " >1</td>
			<td class="col-justify-start " >
			  <a href="storage/APP">Details</a>
			</td>
			<td class="col-justify-start " >OTH</td>
			<td class="col-justify-start " >1</td>
			<td class="col-justify-start " >
			  <a href="storage/OTH">Details</a>
			</td>
			""");
	}

	[Test]
	public void StorageIndexOwnerListsTables()
	{
		Services.AddDbContextFactory<DmsStorageContext>(options => options.UseInMemoryDatabase("DmsStorage"));
		this.SetupQuickGrid();

		var context = Services.GetRequiredService<IDbContextFactory<DmsStorageContext>>().CreateDbContext();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
		context.DmsStorageCatalog.Add(new() { SC_Schema = "APP", SC_Name = "ViaDMS", SC_Type = "Document", SC_Version = "0.1", SC_ExpirationDays = 90, SC_CreateUser = "Me", SC_CreateTime = DateTime.Parse("2024-09-01 01:00") });
		context.DmsStorageCatalog.Add(new() { SC_Schema = "OTH", SC_Name = "ViaDMS", SC_Type = "Document", SC_Version = "0.1", SC_ExpirationDays = 90, SC_CreateUser = "Me", SC_CreateTime = DateTime.Parse("2024-09-01 02:00") });
		context.SaveChanges();

		var cut = Render<Storage.IndexOwner>(parameters =>
		{
			parameters.Add(p => p.Owner, "APP");
		});

		cut.FindAll("td").MarkupMatches("""
			<td class="col-justify-start " >ViaDMS</td>
			<td class="col-justify-start " >Document</td>
			<td class="col-justify-start " >0.1</td>
			<td class="col-justify-start " >90</td>
			<td class="col-justify-start " >2024-09-01 01:00:00Z</td>
			<td class="col-justify-start " >Me</td>
			<td class="col-justify-start " >
			  <a href="storage/APP/edit/ViaDMS">Edit</a>
			  |
			  <a href="storage/APP/details/ViaDMS">Details</a>
			  |
			  <a href="storage/APP/delete/ViaDMS">Delete</a>
			</td>
			""");
	}

	[Test]
	public void StorageCreateCreatesNewTable()
	{
		Services.AddDbContextFactory<DmsStorageContext>(options => options.UseInMemoryDatabase("DmsStorage"));
		var storageRepository = new Mock<IDmsStorageRepository>();
		Services.AddSingleton(storageRepository.Object);

		var user = new AppUser() { UserName = "APP", AccessKey = "XXXXXX" };
		var userManagerLogger = new FakeLogger<UserManager<AppUser>>();
		var userStore = new Mock<IUserStore<AppUser>>();
		var userManager = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, userManagerLogger);
		userManager.SetupGet(x => x.Users).Returns(new List<AppUser>([]).AsQueryable());
		userManager.Setup(x => x.FindByNameAsync("APP")).ReturnsAsync(user);
		Services.AddSingleton(userManager.Object);

		var httpContextAccessor = new Mock<IHttpContextAccessor>();
		httpContextAccessor.Setup(x => x.HttpContext!.User.Identity!.Name).Returns("Me");
		Services.AddSingleton(httpContextAccessor.Object);

		var configuration = new ConfigurationBuilder().Build();
		var appAccessService = new Mock<AppAccessService>(configuration);
		appAccessService.Setup(x => x.GetSqlAccessKey("XXXXXX")).Returns("P@ssw0rd");
		Services.AddSingleton(appAccessService.Object);

		var cut = Render<Storage.Create>();
		cut.Find("#sc_schema").Change("APP");
		cut.Find("#sc_name").Change("ViaDMS");
		cut.Find("#submit").Click();

		var context = Services.GetRequiredService<IDbContextFactory<DmsStorageContext>>().CreateDbContext();
		Assert.That(Services.GetRequiredService<NavigationManager>().Uri, Is.EqualTo("http://localhost/storage/APP"));
		storageRepository.Verify(x => x.RegisterOwnerAsync("APP"), Times.Once);
		storageRepository.Verify(x => x.CreateTableAsync(It.Is<DmsStorageCatalog>(c => c.SC_Schema == "APP" && c.SC_Name == "ViaDMS")), Times.Once);
	}

	[Test]
	public void StorageDetailsDisplaysInfo()
	{
		Services.AddDbContextFactory<DmsStorageContext>(options => options.UseInMemoryDatabase("DmsStorage"));

		var context = Services.GetRequiredService<IDbContextFactory<DmsStorageContext>>().CreateDbContext();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
		context.DmsStorageCatalog.Add(new() { SC_Schema = "APP", SC_Name = "ViaDMS", SC_Type = "Document", SC_Version = "0.1", SC_ExpirationDays = 90, SC_CreateUser = "Me", SC_CreateTime = DateTime.Parse("2024-09-01 01:00") });
		context.SaveChanges();

		var cut = Render<Storage.Details>(parameters =>
		{
			parameters.Add(p => p.Owner, "APP");
			parameters.Add(p => p.TableName, "ViaDMS");
		});

		cut.MarkupMatches("""
			<h1>APP.ViaDMS</h1>
			<div>
			  <h4>Details</h4>
			  <hr>
			  <dl class="row">
			    <dt class="col-sm-2">Owner</dt>
			    <dd class="col-sm-10">APP</dd>
			  </dl>
			  <dl class="row">
			    <dt class="col-sm-2">Table Name</dt>
			    <dd class="col-sm-10">ViaDMS</dd>
			  </dl>
			  <dl class="row">
			    <dt class="col-sm-2">Type</dt>
			    <dd class="col-sm-10">Document</dd>
			  </dl>
			  <dl class="row">
			    <dt class="col-sm-2">Version</dt>
			    <dd class="col-sm-10">0.1</dd>
			  </dl>
			  <dl class="row">
			    <dt class="col-sm-2">Expiration Days</dt>
			    <dd class="col-sm-10">90</dd>
			  </dl>
			  <dl class="row">
			    <dt class="col-sm-2">Create Time</dt>
			    <dd class="col-sm-10">2024-09-01 01:00:00Z</dd>
			  </dl>
			  <dl class="row">
			    <dt class="col-sm-2">Create User</dt>
			    <dd class="col-sm-10">Me</dd>
			  </dl>
			  <div>
			    <a href="/storage/APP/edit/ViaDMS">Edit</a>
			    |
			    <a href="/storage/APP">Back to List</a>
			  </div>
			</div>
			""");
	}

	[Test]
	public void StorageEditUpdatesCatalog()
	{
		Services.AddDbContextFactory<DmsStorageContext>(options => options.UseInMemoryDatabase("DmsStorage"));

		var context = Services.GetRequiredService<IDbContextFactory<DmsStorageContext>>().CreateDbContext();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
		context.DmsStorageCatalog.Add(new() { SC_Schema = "APP", SC_Name = "ViaDMS", SC_Type = "Document", SC_Version = "0.1", SC_ExpirationDays = 90, SC_CreateUser = "Me", SC_CreateTime = DateTime.Parse("2024-09-01 01:00") });
		context.SaveChanges();

		var cut = Render<Storage.Edit>(parameters =>
		{
			parameters.Add(p => p.Owner, "APP");
			parameters.Add(p => p.TableName, "ViaDMS");
		});

		cut.Find("#sc_expirationdays").Change(30);
		cut.Find("#submit").Click();

		Assert.That(context.DmsStorageCatalog.AsNoTracking().First().SC_ExpirationDays, Is.EqualTo(30));
		Assert.That(Services.GetRequiredService<NavigationManager>().Uri, Is.EqualTo("http://localhost/storage/APP"));
	}

	[Test]
	public void StorageDeleteRemovesTable()
	{
		var httpContext = new Mock<HttpContext>();
		httpContext.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Me")])));
		var httpContextAccessor = new Mock<IHttpContextAccessor>();
		httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);
		Services.AddSingleton(httpContextAccessor.Object);

		Services.AddDbContextFactory<DmsStorageContext>(options => options.UseInMemoryDatabase("DmsStorage"));

		var context = Services.GetRequiredService<IDbContextFactory<DmsStorageContext>>().CreateDbContext();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
		context.DmsStorageCatalog.Add(new() { SC_Schema = "APP", SC_Name = "ViaDMS", SC_Type = "Document", SC_Version = "0.1", SC_ExpirationDays = 90, SC_CreateUser = "Me", SC_CreateTime = DateTime.Parse("2024-09-01 01:00") });
		context.SaveChanges();

		var cut = Render<Storage.Delete>(parameters =>
		{
			parameters.Add(p => p.Owner, "APP");
			parameters.Add(p => p.TableName, "ViaDMS");
		});

		cut.Find("#delete").Click();
		Assert.That(context.DmsStorageCatalog.AsNoTracking().First().SC_DeleteUser, Is.EqualTo("Me"));
		Assert.That(Services.GetRequiredService<NavigationManager>().Uri, Is.EqualTo("http://localhost/storage/APP"));
	}
}
