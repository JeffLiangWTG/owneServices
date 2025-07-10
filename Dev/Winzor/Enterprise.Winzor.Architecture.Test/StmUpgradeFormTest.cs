using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
namespace Enterprise.Winzor.Architecture.Test;

public class StmUpgradeFormTest
{
	[Test]
	public async Task UpgradeProgressOpensWhenInitiatingFileUpload()
	{
		StmUpgradeFormForWinzorTest form = null;

		using var ctx = new EnterpriseTestContext();
		var windowShown = new TaskCompletionSource();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new StmUpgradeFormForWinzorTest(GetContainer());
			form.GetFileToImportDialogResult = DialogResult.OK;
			form.FileToImport = TempFile.NewWithExtension("edp").Filename;
			form.OnStartProgressForm += (_, _) => windowShown.SetResult();

			return form;
		});

		Assert.That(await windowShown.Task.WithTimeout(TimeSpan.FromSeconds(5)), Is.False);

		ClickUploadFileButton(rendered);

		Assert.That(await windowShown.Task.WithTimeout(TimeSpan.FromSeconds(5)), Is.True);
	}

	#region Helper Methods

	StmUpgradeCollectionContainer GetContainer()
	{
		var factory = new BusinessObjectFactory();
		var staff = new StmUpgradeCollectionContainer(factory);

		var upgrade1 = factory.NewWithValidTestData<StmUpgrade>();
		upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
		upgrade1.SZ_ExeVersionDate = new ZDateTime(DateTime.Now.Year + 5, 1, 12, 12, 12, 12);
		upgrade1.VersionNumber = new VersionNumber(99, 1, 2, 3);

		var upgrade2 = factory.NewWithValidTestData<StmUpgrade>();
		upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
		upgrade2.SZ_ExeVersionDate = new ZDateTime(DateTime.Now.Year + 5, 12, 13, 15, 12, 11);
		upgrade2.VersionNumber = new VersionNumber(99, 7, 8, 9);

		return staff;
	}

	void ClickUploadFileButton(IRenderedComponent<ControlProxyComponent> rendered)
	{
		var upgradeButton = rendered.FindAll("button").FirstOrDefault(x => x.TextContent == "Import From File");
		Assert.That(upgradeButton, Is.Not.Null, "Could not find Import From File button.");

		upgradeButton.Click();
	}

	#endregion

	#region Test Classes

	class StmUpgradeFormForWinzorTest : StmUpgradeFormForTest
	{
		public StmUpgradeFormForWinzorTest(StmUpgradeCollectionContainer dataSource, Mock<UpgradeManager> upgradeManagerMock = null)
			: base(dataSource, upgradeManagerMock)
		{
		}

		#nullable enable
		public event EventHandler? OnStartProgressForm;
		#nullable disable

		protected override void StartProgressForm()
		{
			OnStartProgressForm?.Invoke(this, EventArgs.Empty);
			base.StartProgressForm();
		}
	}

	#endregion
}
