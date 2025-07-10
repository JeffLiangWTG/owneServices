using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(TWNCATKClientSettingRegistryItemEditor))]
	public sealed class TWNCATKClientSettingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new TWNCATKClientSettingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TWNCATKClientSettingRegistryItemUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TWNCATKClientSettingRegistryItemUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TWNCATKClientSettingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { CreateNewValue() };
		}

		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 436, 247);
				AssertEquals("should have the correct width", 436, control.Width);
				AssertEquals("should have the correct height", 180, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, control.Anchor);
			}
		}

		TWNCATKClientSetting CreateNewValue()
		{
			return new TWNCATKClientSetting { MachineName = "Machine Name", SendToFolder = @"D:\Folders\SendFolder", RunningIntervalInSeconds = 60 };
		}

		public void TestSystemCreateTimeShouldBeChangeAfterUpdate()
		{
			var registryItem = TWCustomsDataRegistry.Instance.TWNCATKClientSetting;
			registryItem.Options &= RegistryOptions.NotCached;
			var factory = registryItem.Factory;
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DTW";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "MXE";
			factory.Save();
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
			var setting = new TWNCATKClientSetting(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), factory);
			setting.MachineName = "Machine1";
			setting.SendToFolder = @"D:\Folders\SendFolder";
			setting.RunningIntervalInSeconds = 120;

			var regItem = new RegistryItemTagForTest(TWCustomsDataRegistry.Instance.TWNCATKClientSetting)
			{
				CompanyPKForTest = company.PK.ToGuid(),
				BranchPKForTest = Guid.Empty,
				DepartmentPKForTest = Guid.Empty,
				NewValue = setting,
				IsChanged = true,
				HasValue = true
			};

			regItem.SaveAllValues();

			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub")
			{ OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + " DESC" };
			var interchange = factory.LoadTop1<EDIInterchange>(interchangeQuery);

			var systemCreateTimeUtc = interchange.EI_SystemCreateTimeUtc;
			interchange.Delete();

			setting.MachineName = "Machine2";

			regItem.NewValue = setting;
			regItem.IsChanged = true;
			regItem.HasValue = true;

			regItem.SaveAllValues();

			interchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotEquals("EI_SystemCreateTimeUtc", systemCreateTimeUtc, interchange.EI_SystemCreateTimeUtc);
		}
	}
}
