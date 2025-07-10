using System;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

public class CargoWiseAuthStateProviderTests
{
	[Test]
	[WithTransaction]
	public async Task DeveloperLoginsWithClientTokenAreAllowedAsync()
	{
		var optionsValue = new CargoWiseOptions
		{
			DbServerName = Db.ServerName,
			DatabaseName = Db.DatabaseName,
		};
		Db.InitializeDatabaseDetails(optionsValue.DbServerName, optionsValue.DatabaseName);
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());
		using var ctx = new EnterpriseTestContext();
		using (Db.DisposableActionForDbConnection())
		{
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				var clientToken =  GetStmAccessToken();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assume.That(clientToken, Is.Not.Empty);
				Assume.That(accessControl.TryPeek(clientToken, "BLC", out var _)); // The token should be registered before the test starts

				var cargoWiseAuthOptions = new CargoWiseAuthOptions()
				{
					ClientToken = clientToken,
				};

				// Act
				var loginResult = cargoWiseAuthStateProvider.Login(cargoWiseAuthStateProvider, cargoWiseAuthOptions);

				Assert.That(loginResult.IsSuccess, Is.True);
				Assert.That(!accessControl.TryPeek(clientToken, "BLC", out var _)); // The client token should have been consumed by the login process
			});
		}

		Assert.That(LoginDirector.Instance.AuthenticatedUser.IsOK);
		Assert.That(LoginDirector.Instance.LoggedInLocation, Is.True);
		Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK);
		Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.User.IsDeveloper, Is.True);
		Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.OK));
	}

	[Test]
	[WithTransaction]
	public async Task LoginsWithNoLocationAreAuthorizedAsync()
	{
		var optionsValue = new CargoWiseOptions
		{
			DbServerName = Db.ServerName,
			DatabaseName = Db.DatabaseName,
		};
		Db.InitializeDatabaseDetails(optionsValue.DbServerName, optionsValue.DatabaseName);
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());
		using var ctx = new EnterpriseTestContext();
		using (Db.DisposableActionForDbConnection())
		{
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				var clientToken = GetStmAccessToken_StaffWithNoLocation();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assume.That(clientToken, Is.Not.Empty);
				Assume.That(accessControl.TryPeek(clientToken, "BLC", out var _)); // The token should be registered before the test starts
				var cargoWiseAuthOptions = new CargoWiseAuthOptions()
				{
					ClientToken = clientToken,
				};

				// Act
				var loginResult = cargoWiseAuthStateProvider.Login(cargoWiseAuthStateProvider, cargoWiseAuthOptions);
				Assert.That(loginResult.IsSuccess, Is.False);
			});
		}

		Assert.That(LoginDirector.Instance.AuthenticatedUser.IsOK, Is.True);
		Assert.That(LoginDirector.Instance.LoggedInLocation, Is.False);
		Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK, Is.True);
	}

	static string GetStmAccessToken()
	{
		using (Db.DisposableActionForDbConnection())
		{
			var accessControl = new TokenizedAccessControl();
			var accessTokenInfo = new AccessTokenInfo(string.Empty, Guid.Parse("ABE1D8D8-A709-4BFA-88E3-53997AA925E2"), GlbStaffSchema.Constants.Prefix);
			return accessControl.CreateLimitedToken("BLC", accessTokenInfo, TimeSpan.FromMinutes(5), 1);
		}
	}

	static string GetStmAccessToken_StaffWithNoLocation()
	{
		using (Db.DisposableActionForDbConnection())
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Guid.Empty;
			staff.GS_GB_LastLogonBranch = Guid.Empty;
			factory.Save();

			var accessControl = new TokenizedAccessControl();
			var accessTokenInfo = new AccessTokenInfo(string.Empty, staff.PK.ToGuid(), GlbStaffSchema.Constants.Prefix);
			return accessControl.CreateLimitedToken("BLC", accessTokenInfo, TimeSpan.FromMinutes(5), 1);
		}
	}

	[Test]
	[WithTransaction]
	public async Task AnnonymousLoginsAreAllowedInDebugWhenFlagged()
	{
		using var ctx = new EnterpriseTestContext();
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());

		var cargoWiseAuthOptions = new CargoWiseAuthOptions()
		{
			AllowAnonymousDeveloperLogins = true,
		};

		await ctx.WinzorDispatcher.InvokeAsync(() => { cargoWiseAuthStateProvider.Login(cargoWiseAuthStateProvider, cargoWiseAuthOptions); });

		Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.IsOK);
		Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.User.IsDeveloper, Is.True);
		Assert.That(cargoWiseAuthStateProvider.AuthenticatedUser.State, Is.EqualTo(LoginAuthenticationInfo.Status.OK));
	}

	[Test]
	public async Task ClientTokenMustBeProvidedWhenAnonymousLoginDisallowedAsync()
	{
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());

		using var ctx = new EnterpriseTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var cargoWiseAuthOptions = new CargoWiseAuthOptions()
			{
				AllowAnonymousDeveloperLogins = false,
			};

			// Act
			var result = cargoWiseAuthStateProvider.Login(cargoWiseAuthStateProvider, cargoWiseAuthOptions);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.FailedMessage, Is.EqualTo("Anonymous login is disabled"));
		});
	}
}
