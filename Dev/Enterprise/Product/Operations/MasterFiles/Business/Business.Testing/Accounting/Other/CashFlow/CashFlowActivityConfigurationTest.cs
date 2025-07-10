using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowActivityConfiguration))]
	class CashFlowActivityConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			configuration = new CashFlowActivityConfiguration();
			configuration.Code = "XXX";
			configuration.EnglishDescription = "Undefined";
			configuration.ActivityType = "X";
			configurations = new CashFlowActivityConfigurationCollection();
			configurations.Add(configuration);
		}
		CashFlowActivityConfiguration configuration;
		CashFlowActivityConfigurationCollection configurations;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CashFlowActivityConfiguration item = new CashFlowActivityConfiguration();
			item.Code = "XXX";
			item.EnglishDescription = "Undefined";
			item.ActivityType = "X";
			return item;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected new CashFlowActivityConfiguration BizObj
		{
			get { return (CashFlowActivityConfiguration)base.BizObj; }
		}

		protected virtual CashFlowActivityConfigurationCollection GetAuthorisationSettingsCollection()
		{
			return new CashFlowActivityConfigurationCollection();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public void TestActivityDescription()
		{
			var registryitem = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration;
			var resKey = registryitem.GetKey(null, "test");

			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				configuration.SetCustomizedDataCaptionSource(registryitem);
				configuration.EnglishActivityDescription = "test";
				AssertEquals("测试", configuration.ActivityDescription);

				registryitem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations);
				var overrideValue = registryitem.Value;
				AssertEquals("test", overrideValue[0].EnglishActivityDescription);
			}
		}
	}
}
