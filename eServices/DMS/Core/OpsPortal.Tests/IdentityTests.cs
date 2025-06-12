using System.Linq;
using eServices.Dms.Core.OpsPortal.Services;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Identity = eServices.Dms.Core.OpsPortal.Components.Pages.Identity;

namespace eServices.Dms.Core.OpsPortal.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class IdentityTests : BunitContext
{
	[Test]
	public void IdentityIndexListsApps()
	{
		var userManagerLogger = new FakeLogger<UserManager<AppUser>>();
		var userStore = new Mock<IUserStore<AppUser>>();
		var userManager = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, userManagerLogger);
		Services.AddSingleton(userManager.Object);

		userManager.Setup(x => x.Users).Returns(new[] { new AppUser { Id = "1", UserName = "TestUser" } }.AsQueryable());
		userManager.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(new[] { "TestRole" });

		this.SetupQuickGrid();

		var cut = Render<Identity.Index>();

		cut.Find("table").MarkupMatches("""
			<table theme="default" aria-rowcount="2"  class="quickgrid table "  >
			  <thead >
			    <tr >
			      <th class="col-justify-start " aria-sort="none" scope="col" >
			        <div class="col-header-content" >
			          <div class="col-title" >
			            <div class="col-title-text" >ID</div>
			          </div>
			        </div>
			      </th>
			      <th class="col-justify-start " aria-sort="none" scope="col" >
			        <div class="col-header-content" >
			          <div class="col-title" >
			            <div class="col-title-text" >Claims</div>
			          </div>
			        </div>
			      </th>
			      <th class="col-justify-start " aria-sort="none" scope="col" >
			        <div class="col-header-content" >
			          <div class="col-title" >
			            <div class="col-title-text" >Locked Out?</div>
			          </div>
			        </div>
			      </th>
			      <th class="col-justify-start " aria-sort="none" scope="col" >
			        <div class="col-header-content" >
			          <div class="col-title" >
			            <div class="col-title-text" ></div>
			          </div>
			        </div>
			      </th>
			    </tr>
			  </thead>
			  <tbody >
			    <tr aria-rowindex="2" >
			      <td class="col-justify-start " >TestUser</td>
			      <td class="col-justify-start " >TestRole<br>
			      </td>
			      <td class="col-justify-start " >False</td>
			      <td class="col-justify-start " >
			        <a href="identity/edit/TestUser">Edit</a>
			        |
			        <a href="identity/details/TestUser">Details</a>
			        |
			        <a href="identity/delete/TestUser">Delete</a>
			      </td>
			    </tr>
			  </tbody>
			</table>
			""");
	}

	[Test]
	public void IdentityCreateCreatesNewApp()
	{
		var userManagerLogger = new FakeLogger<UserManager<AppUser>>();
		var userStore = new Mock<IUserStore<AppUser>>();
		var userManager = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, userManagerLogger);
		Services.AddSingleton(userManager.Object);

		var roleManagerLogger = new FakeLogger<RoleManager<IdentityRole>>();
		var roleStore = new Mock<IRoleStore<IdentityRole>>();
		var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null!, null!, null!, roleManagerLogger);
		Services.AddSingleton(roleManager.Object);

		var configuration = new ConfigurationBuilder().Build();
		var appAccessService = new Mock<AppAccessService>(configuration);
		appAccessService.Setup(x => x.GetAccessKey("P@ssw0rd")).Returns("XXXXXX");
		Services.AddSingleton(appAccessService.Object);

		userManager.Setup(x => x.CreateAsync(It.Is<AppUser>(appUser => appUser.UserName == "APP" && appUser.Email == "dms@example.org"), "P@ssw0rd"))
			.ReturnsAsync(IdentityResult.Success);

		var cut = Render<Identity.Create>();
		cut.Find("#id").Change("APP");
		cut.Find("#password").Change("P@ssw0rd");
		cut.Find("#confirm-password").Change("P@ssw0rd");
		cut.Find("#email").Change("dms@example.org");
		cut.Find("#role-send-message").Change(true);
		cut.Find("#role-receive-message").Change(true);
		cut.Find("#role-read-config").Change(true);
		cut.Find("#role-write-storage").Change(true);
		cut.Find("#submit").Click();

		Assert.That(Services.GetRequiredService<NavigationManager>().Uri, Is.EqualTo("http://localhost/identity/details/APP"));
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.SendMessage}:APP"), Times.Once);
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.ReceiveMessage}:APP"), Times.Once);
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.ReadConfig}:APP"), Times.Once);
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.WriteStorage}:APP"), Times.Once);
	}

	[Test]
	public void IdentityDetailsDisplaysApp()
	{
		var userManagerLogger = new FakeLogger<UserManager<AppUser>>();
		var userStore = new Mock<IUserStore<AppUser>>();
		var userManager = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, userManagerLogger);
		Services.AddSingleton(userManager.Object);

		var configuration = new ConfigurationBuilder().Build();
		var appAccessService = new Mock<AppAccessService>(configuration);
		appAccessService.Setup(x => x.GetBasicAuthHeader("APP", "XXXXXX")).Returns("Basic QVBQOlhYWFhYWA==");
		Services.AddSingleton(appAccessService.Object);

		userManager.Setup(x => x.FindByNameAsync("APP")).ReturnsAsync(new AppUser { UserName = "APP", Email = "dms@example.org", AccessKey = "XXXXXX" });

		var cut = Render<Identity.Details>(parameters => parameters.Add(p => p.Id, "APP"));

		cut.FindAll("dl").MarkupMatches("""
			<dl class="row">
			  <dt class="col-sm-2">ID</dt>
			  <dd class="col-sm-10">APP</dd>
			</dl>
			<dl class="row">
			  <dt class="col-sm-2">Claims</dt>
			  <dd class="col-sm-10"></dd>
			</dl>
			<dl class="row">
			  <dt class="col-sm-2">Contact</dt>
			  <dd class="col-sm-10">dms@example.org</dd>
			</dl>
			<dl class="row">
			  <dt class="col-sm-2">Locked Out?</dt>
			  <dd class="col-sm-10">False</dd>
			</dl>
			<dl class="row">
			  <dt class="col-sm-2">Access Failed Count</dt>
			  <dd class="col-sm-10">0</dd>
			</dl>
			<dl class="row">
			  <dt class="col-sm-2">Authorization Header</dt>
			  <dd class="col-sm-10">Basic QVBQOlhYWFhYWA==</dd>
			</dl>
			""");
		cut.FindAll("a").MarkupMatches("""
			<a href="/identity/edit/APP">Edit</a>
			<a href="/identity">Back to List</a>
			""");
	}

	[Test]
	public void IdentityDetailsReturnsNotFound()
	{
		var userManagerLogger = new FakeLogger<UserManager<AppUser>>();
		var userStore = new Mock<IUserStore<AppUser>>();
		var userManager = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, userManagerLogger);
		Services.AddSingleton(userManager.Object);

		var configuration = new ConfigurationBuilder().Build();
		var appAccessService = new Mock<AppAccessService>(configuration);
		Services.AddSingleton(appAccessService.Object);

		var cut = Render<Identity.Details>(parameters => parameters.Add(p => p.Id, "APP"));

		Assert.That(Services.GetRequiredService<NavigationManager>().Uri, Is.EqualTo("http://localhost/notfound"));
	}

	[Test]
	public void IdentityEditUpdatesApp()
	{
		var userManagerLogger = new FakeLogger<UserManager<AppUser>>();
		var userStore = new Mock<IUserStore<AppUser>>();
		var userManager = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, userManagerLogger);
		Services.AddSingleton(userManager.Object);

		var roleManagerLogger = new FakeLogger<RoleManager<IdentityRole>>();
		var roleStore = new Mock<IRoleStore<IdentityRole>>();
		var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null!, null!, null!, roleManagerLogger);
		Services.AddSingleton(roleManager.Object);

		var configuration = new ConfigurationBuilder().Build();
		var appAccessService = new Mock<AppAccessService>(configuration);
		appAccessService.Setup(x => x.GetAccessKey("dr0wss@P")).Returns("XXXXXX");
		Services.AddSingleton(appAccessService.Object);

		var user = new AppUser { UserName = "APP", Email = "dms@example.org", AccessKey = "XXXXXX" };
		userManager.Setup(x => x.FindByNameAsync("APP")).ReturnsAsync(user);
		userManager.Setup(x => x.SetUserNameAsync(user, "AAA")).ReturnsAsync(IdentityResult.Success);
		userManager.Setup(x => x.SetEmailAsync(user, "aaa@example.org")).ReturnsAsync(IdentityResult.Success);
		userManager.Setup(x => x.RemovePasswordAsync(user)).ReturnsAsync(IdentityResult.Success);
		userManager.Setup(x => x.AddPasswordAsync(user, "dr0wss@P")).ReturnsAsync(IdentityResult.Success);
		userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

		var cut = Render<Identity.Edit>(parameters => parameters.Add(p => p.Id, "APP"));

		cut.Find("#id").Change("AAA");
		cut.Find("#password").Change("dr0wss@P");
		cut.Find("#confirm-password").Change("dr0wss@P");
		cut.Find("#email").Change("aaa@example.org");
		cut.Find("#role-send-message").Change(true);
		cut.Find("#role-receive-message").Change(true);
		cut.Find("#role-read-config").Change(true);
		cut.Find("#role-write-storage").Change(true);
		cut.Find("#submit").Click();

		Assert.That(Services.GetRequiredService<NavigationManager>().Uri, Is.EqualTo("http://localhost/identity/details/AAA"));
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.SendMessage}:APP"), Times.Once);
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.ReceiveMessage}:APP"), Times.Once);
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.ReadConfig}:APP"), Times.Once);
		userManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), $"{AppRoles.WriteStorage}:APP"), Times.Once);
	}

	[Test]
	public void IdentityDeleteRemovesApp()
	{
		var userManagerLogger = new FakeLogger<UserManager<AppUser>>();
		var userStore = new Mock<IUserStore<AppUser>>();
		var userManager = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, userManagerLogger);
		Services.AddSingleton(userManager.Object);

		var configuration = new ConfigurationBuilder().Build();
		Services.AddSingleton(configuration);

		var user = new AppUser { UserName = "APP", Email = "dms@example.org", AccessKey = "XXXXXX" };
		userManager.Setup(x => x.FindByNameAsync("APP")).ReturnsAsync(user);
		userManager.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(new[] { "TestRole" });
		userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

		var cut = Render<Identity.Delete>(parameters => parameters.Add(p => p.Id, "APP"));

		cut.Find("#delete").Click();

		Assert.That(Services.GetRequiredService<NavigationManager>().Uri, Is.EqualTo("http://localhost/identity"));
	}
}
