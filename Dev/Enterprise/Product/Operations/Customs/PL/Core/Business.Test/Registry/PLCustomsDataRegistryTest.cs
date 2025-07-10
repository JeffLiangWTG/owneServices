using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(PLCustomsDataRegistry))]
sealed class PLCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<PLCustomsDataRegistry>
{
	public void TestAllRegistryItemsHavePLCountryFilter()
	{
		CombineAssertions(() =>
		{
			foreach (IRegistryItem registryItem in AllItems)
			{
				Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Poland));
			}
		});
	}

	public void TestIsForProductivityWise()
	{
		AssertEquals(false, PLCustomsDataRegistry.Instance.IsForProductivityWise);
	}

	public void TestPUESCSendMaxRetryCountForTest()
	{
		AssertPUESCSendMaxRetryCount(DatabaseTypes.Codes.Test, PLRegistryDataConstants.PUESCSendMaxRetryCount);
	}

	public void TestPUESCSendMaxRetryCountForProd()
	{
		AssertPUESCSendMaxRetryCount(DatabaseTypes.Codes.Production, PLRegistryDataConstants.PUESCSendMaxRetryCount);
	}

	public void TestPUESCRequestTimeOutForTest()
	{
		AssertPUESCRequestTimeOut(DatabaseTypes.Codes.Test, PLRegistryDataConstants.PUESCWebServiceRequestTimeOut);
	}

	public void TestPUESCRequestTimeOutForProd()
	{
		AssertPUESCRequestTimeOut(DatabaseTypes.Codes.Production, PLRegistryDataConstants.PUESCWebServiceRequestTimeOut);
	}

	public void TestPUESCEmailChannelForTest()
	{
		AssertPUESCEmailChannel(DatabaseTypes.Codes.Test, PLRegistryDataConstants.PUESCEmailAddressTest);
	}

	public void TestPUESCEmailChannelForProd()
	{
		AssertPUESCEmailChannel(DatabaseTypes.Codes.Production, PLRegistryDataConstants.PUESCEmailAddressProd);
	}

	public void TestPCSEmailChannelForTest()
	{
		AssertPCSEmailChannel(DatabaseTypes.Codes.Test, string.Empty);
	}

	public void TestPCSEmailChannelForProd()
	{
		AssertPCSEmailChannel(DatabaseTypes.Codes.Production, PLRegistryDataConstants.PCSEmailAddressProd);
	}

	public void TestEAttachmentsEmailChannelForTest()
	{
		AssertEAttachmentsEmailChannel(DatabaseTypes.Codes.Test, string.Empty);
	}

	public void TestEAttachmentsEmailChannelForProd()
	{
		AssertEAttachmentsEmailChannel(DatabaseTypes.Codes.Production, PLRegistryDataConstants.EAttachmentsEmailAddressProd);
	}

	public void TestCommunicationEmailChannelEmailAddress()
	{
		AssertCommunicationEmailChannelEmailAddress(string.Empty);
	}

	public void TestDefaultCommunicationChannelNCTSP5WhenEmailAddressNotNullOrEmpty()
	{
		using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Odyssey"))
		{
			AssertDefaultCommunicationChannelNCTSP5(!PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value.IsNullOrEmpty());
		}
	}

	public void TestDefaultCommunicationChannelNCTSP5WhenEmailAddressNullOrEmpty()
	{
		using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
		{
			AssertDefaultCommunicationChannelNCTSP5(!PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value.IsNullOrEmpty());
		}
	}

	void AssertPUESCSendMaxRetryCount(string databaseType, int defaultValue)
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		registrationKey.DatabaseTypeForTest = databaseType;

		TestRegistryItem(ItemSet.PUESCSendMaxRetryCount,
			"PUESCWebServiceRequestMaxRetryCount",
			PLCustomsDataRegistry.Categories.Customs_PL_PUESC_WebService,
			"PUESC request max retry count",
			"This is the maximum attempts that will be made to communicate with PUESC Web Service to send messages",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			defaultValue);
	}

	void AssertPUESCRequestTimeOut(string databaseType, int defaultValue)
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		registrationKey.DatabaseTypeForTest = databaseType;

		TestRegistryItem(ItemSet.PUESCRequestTimeOut,
			"PUESCWebServiceRequestTimeOut",
			PLCustomsDataRegistry.Categories.Customs_PL_PUESC_WebService,
			"PCT Service timeout",
			"Automatic retry time for documents retrieval request (minutes)",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			defaultValue);
	}

	void AssertPUESCEmailChannel(string databaseType, string defaultValue)
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		registrationKey.DatabaseTypeForTest = databaseType;

		TestStringRegistryItem(ItemSet.PUESCEmailChannel,
			"PUESCEmailChannel",
			PLCustomsDataRegistry.Categories.Customs_PL_PUESC_EmailChannel,
			"PUESC Email Address",
			"This is the Default Email Address for PUESC Email Channel",
			RegistryStorageFlags.Company,
			TextEditorType.TextBox,
			RegistryOptions.Default,
			defaultValue,
			CharacterCase.Normal);
	}

	void AssertPCSEmailChannel(string databaseType, string defaultValue)
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		registrationKey.DatabaseTypeForTest = databaseType;

		TestStringRegistryItem(ItemSet.PCSEmailChannel,
			"PCSEmailChannel",
			PLCustomsDataRegistry.Categories.Customs_PL_PCS_EmailChannel,
			"PCS Email Address",
			"This is the Email Address for PCS Email Channel",
			RegistryStorageFlags.Company,
			TextEditorType.TextBox,
			RegistryOptions.Default,
			defaultValue,
			CharacterCase.Normal);
	}

	void AssertEAttachmentsEmailChannel(string databaseType, string defaultValue)
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		registrationKey.DatabaseTypeForTest = databaseType;

		TestStringRegistryItem(ItemSet.EAttachmentsEmailChannel,
			"EAttachmentsEmailChannel",
			PLCustomsDataRegistry.Categories.Customs_PL_EAttachments_EmailChannel,
			"E-Attachments Email Address",
			"This is the Email Address for E-Attachments email channel",
			RegistryStorageFlags.Company,
			TextEditorType.TextBox,
			RegistryOptions.Default,
			defaultValue,
			CharacterCase.Normal);
	}

	void AssertCommunicationEmailChannelEmailAddress(string defaultValue)
	{
		TestStringRegistryItem(ItemSet.CommunicationEmailChannelEmailAddress,
			"CommunicationEmailChannelEmailAddress",
			PLCustomsDataRegistry.Categories.Customs_PL_CommunicationEmailChannel,
			"Email Address",
			"This is the Email Address for Communication email channel",
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			TextEditorType.TextBox,
			RegistryOptions.IsValueMandatory,
			defaultValue,
			CharacterCase.Normal);
	}

	void AssertDefaultCommunicationChannelNCTSP5(bool defaultValueDependentEmailAddressHasValue)
	{
		TestGenericRegistryItem(ItemSet.DefaultCommunicationChannelNCTSP5,
			"DefaultCommunicationChannelNCTSP5",
			PLCustomsDataRegistry.Categories.Customs_PL_DefaultCommunicationChannel,
			"NCTS P5",
			"Enable this option to default the Communication Channel SEAP ID or Email Channel",
			RegistryStorageFlags.Company,
			RegistryOptions.Default);
		AssertEquals(defaultValueDependentEmailAddressHasValue, ItemSet.DefaultCommunicationChannelNCTSP5.DefaultValue.IsEmailChannel);
		AssertEquals(!defaultValueDependentEmailAddressHasValue, ItemSet.DefaultCommunicationChannelNCTSP5.DefaultValue.IsSeapID);
	}
}
