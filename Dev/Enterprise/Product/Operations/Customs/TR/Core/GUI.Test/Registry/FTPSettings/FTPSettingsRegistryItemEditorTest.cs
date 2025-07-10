using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItemEditor))]
	sealed class FTPSettingsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new FTPSettingsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((FTPSettingsRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(FTPSettingsRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new FTPSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport);

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var fTPSettings = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			fTPSettings.FTPAddress = ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet;
			fTPSettings.Port = 21;
			fTPSettings.Inbox = "inbox";
			fTPSettings.Outbox = "outbox";

			Factory.Save();

			return new object[] { fTPSettings };
		}

		public void TestSystemCreateTimeShouldBeChangeAfterUpdate()
		{
			var setting = new FTPSettings();
			setting.ExportUnionUserCode = "ExportUnionUser";
			setting.ExportUnionUserPassword = "Password";

			var regItem = new RegistryItemTagForTest(TRCustomsDataRegistry.Instance.FTPSettings)
			{
				CompanyPKForTest = GlbCompany.CurrentCompany.PK.ToGuid(),
				BranchPKForTest = Guid.Empty,
				DepartmentPKForTest = Guid.Empty,
				NewValue = setting,
				IsChanged = true,
				HasValue = true
			};

			regItem.SaveAllValues();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "CustomsConfiguration");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var interchange = Factory.LoadTop1<EDIInterchange>(zQuery);

			var systemCreateTimeUtc = interchange.EI_SystemCreateTimeUtc;
			interchange.Delete();

			setting.ExportUnionUserCode = "Export77";

			regItem.NewValue = setting;
			regItem.IsChanged = true;
			regItem.HasValue = true;

			regItem.SaveAllValues();

			interchange = Factory.LoadTop1<EDIInterchange>(zQuery);
			AssertNotEquals("EI_SystemCreateTimeUtc", systemCreateTimeUtc, interchange.EI_SystemCreateTimeUtc);
		}
	}
}
