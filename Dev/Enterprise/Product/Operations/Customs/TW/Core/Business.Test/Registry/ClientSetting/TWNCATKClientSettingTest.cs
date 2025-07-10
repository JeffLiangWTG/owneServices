using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWNCATKClientSetting))]
	sealed class TWNCATKClientSettingTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return BizObj;
		}

		new TWNCATKClientSetting BizObj => (TWNCATKClientSetting)base.BizObj;
		[ExpectNoExceptions]
		public void TestSetCustomDefaultValuesCore()
		{
			var setting = new TWNCATKClientSetting();
			NUnit.Framework.Assert.That(setting.RunningIntervalInSeconds, NUnit.Framework.Is.EqualTo(60).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMachineNameChangesEHubClientId()
		{
			NUnit.Framework.Assert.That(BizObj.EHubClientID, NUnit.Framework.Is.EqualTo(ZString.Empty), "EHubClientId should be null for a new Object.");
			NUnit.Framework.Assert.That(clientSetting.EHubClientID, NUnit.Framework.Is.EqualTo(company.LicenceKeyIdentifier + "_TCA").Using(CustomComparers.TypeComparison), "EHubClientId should have been set.");
			NUnit.Framework.Assert.That(clientSetting.EHubClientStatus, NUnit.Framework.Is.EqualTo("OK").Using(CustomComparers.TypeComparison), "EHubClientId should have been set.");
			BizObj.MachineName = "";
			NUnit.Framework.Assert.That(BizObj.EHubClientID.IsEmpty, NUnit.Framework.Is.True, "EHubClientId should have been cleared.");
			NUnit.Framework.Assert.That(BizObj.EHubClientStatus.IsEmpty, NUnit.Framework.Is.True, "EHubClientStatus should have been cleared.");
		}

		[ExpectNoExceptions]
		public void TestShouldRegisterEHubClient()
		{
			NUnit.Framework.Assert.That(clientSetting.ShouldRegisterEHubClient, NUnit.Framework.Is.True, "ShouldRegisterEHubClient is true");
			clientSetting.MachineName = ZString.Empty;
			clientSetting.SendToFolder = ZString.Empty;
			NUnit.Framework.Assert.That(!clientSetting.ShouldRegisterEHubClient, NUnit.Framework.Is.True, "ShouldRegisterEHubClient is false");
		}

		[ExpectNoExceptions]
		public void TestShouldUnregisterEHubClient()
		{
			var registryItem = TWCustomsDataRegistry.Instance.TWNCATKClientSetting;
			registryItem.Options &= RegistryOptions.NotCached;
			var factory = registryItem.Factory;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "MXE";
			factory.Save();
			RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
			registryItem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, clientSetting);
			registryItem.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, clientSetting);
			registryItem.OnAllValuesSaved();
			NUnit.Framework.Assert.That(!clientSetting.ShouldUnregisterEHubClient, NUnit.Framework.Is.True, "ShouldUnregisterEHubClient is false");
			clientSetting = registryItem.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			clientSetting.MachineName = ZString.Empty;
			clientSetting.SendToFolder = ZString.Empty;
			registryItem.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, clientSetting);
			registryItem.OnAllValuesSaved();
			NUnit.Framework.Assert.That(clientSetting.ShouldUnregisterEHubClient, NUnit.Framework.Is.True, "ShouldUnregisterEHubClient is true");
		}

		public void TestAllEmpty()
		{
			BizObj.SendToFolder = ZString.Empty;
			BizObj.MachineName = ZString.Empty;
			BizObj.RunningIntervalInSeconds = 0;
			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.MachineNameInfo);
			AssertHasErrors(BizObj.SendToFolderInfo);
			AssertHasErrors(BizObj.RunningIntervalInSecondsInfo);
			BizObj.SendToFolder = @"D:\Folders\ArchiveFolder\";
			BizObj.MachineName = "Machine 1";
			BizObj.RunningIntervalInSeconds = 15;
		}

		public void TestValidateMachineName()
		{
			BizObj.SendToFolder = @"D:\Folders\ArchiveFolder\";
			BizObj.Validation.ValidateMachineName();
			AssertHasErrorContaining(BizObj.MachineNameInfo, MandatoryValidation.MustBeEntered);
			BizObj.MachineName = "111";
			AssertNoErrorContaining(BizObj.MachineNameInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateSendToFolder()
		{
			BizObj.MachineName = "Machine 1";
			BizObj.Validation.ValidateSendToFolder();
			AssertHasErrorContaining(BizObj.SendToFolderInfo, MandatoryValidation.MustBeEntered);
			BizObj.SendToFolder = "Folder";
			AssertNoErrorContaining(BizObj.SendToFolderInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(BizObj.SendToFolderInfo, "is not a local folder path.");
			BizObj.SendToFolder = @"D:\222.**^<>";
			AssertHasErrorContaining(BizObj.SendToFolderInfo, "is not a local folder path.");
			BizObj.SendToFolder = @"D:\Folders\SendToFolder\";
			AssertNoErrorContaining(BizObj.SendToFolderInfo, "is not a local folder path.");
		}

		public void TestRunningInterval()
		{
			BizObj.MachineName = "Machine 1";
			BizObj.RunningIntervalInSeconds = ZInt.Zero;
			BizObj.Validation.ValidateRunningIntervalInSeconds();
			AssertHasErrorContaining(BizObj.RunningIntervalInSecondsInfo, MandatoryValidation.MustBeEntered);
			BizObj.RunningIntervalInSeconds = 13;
			AssertNoErrorContaining(BizObj.RunningIntervalInSecondsInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(BizObj.RunningIntervalInSecondsInfo, "Running Interval cannot be smaller than 15 seconds.");
			BizObj.RunningIntervalInSeconds = 15;
			AssertNoErrorContaining(BizObj.RunningIntervalInSecondsInfo, "Running Interval cannot be smaller than 15 seconds.");
		}

		public void TestValidateMachineNameWithCompanyPK()
		{
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "TW2";
			company2.GC_RN_NKCountryCode = "TW";
			company2.Branches.AddNew().GB_Code = "TW2";
			Factory.Save();
			var registryItem = TWCustomsDataRegistry.Instance.TWNCATKClientSetting;
			registryItem.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new TWNCATKClientSetting()
			{ MachineName = "Machine Name", SendToFolder = @"D:\Folders\SendFolder", RunningIntervalInSeconds = 15, });
			BizObj.MachineName = "Machine Name";
			BizObj.SendToFolder = @"D:\Folders\SendFolder";
			BizObj.RunningIntervalInSeconds = 15;
			registryItem.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, BizObj);
			registryItem.DataType.Validate(registryItem, BizObj, company2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertHasError(BizObj.MachineNameInfo, "There is already a company which has a same Machine Name.");
		}

		[ExpectNoExceptions]
		public void TestDoNotChangeMachineNameValueToUpper()
		{
			var setting = new TWNCATKClientSetting();
			setting.MachineName = "AaBbC";
			NUnit.Framework.Assert.That(setting.MachineName, NUnit.Framework.Is.EqualTo("AaBbC").Using(CustomComparers.TypeComparison));
		}

		GlbCompany company;
		TWNCATKClientSetting clientSetting;
		protected override void SetUp()
		{
			base.SetUp();
			company = Factory.New<GlbCompany>();
			company.GC_Code = "TW1";
			company.GC_RN_NKCountryCode = CountryCodes.Taiwan;
			company.Branches.AddNew().GB_Code = "TW1";
			Factory.Save();
			clientSetting = new TWNCATKClientSetting(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			clientSetting.MachineName = "Machine Name";
			clientSetting.SendToFolder = @"D:\Folders\SendFolder";
			clientSetting.RunningIntervalInSeconds = 15;
		}
	}
}
