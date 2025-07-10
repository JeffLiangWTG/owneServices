using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(NOCustomsDataRegistry))]
sealed class NOCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<NOCustomsDataRegistry>
{
	public void TestAllRegistryItemsHaveNOCountryFilter()
	{
		CombineAssertions(() =>
		{
			foreach (IRegistryItem registryItem in AllItems)
			{
				Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Norway));
			}
		});
	}

	public void TestIsForProductivityWise()
	{
		AssertEquals(false, NOCustomsDataRegistry.Instance.IsForProductivityWise);
	}

	public void TestCustomsNODIID()
	{
		TestGenericRegistryItem(ItemSet.CustomsNodiId,
			"CustomsNodiId",
			CustomsDataRegistry.Categories.Customs_Norway,
			"Customs NODI ID",
			"Customs Mailbox ID used for submitting messages.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForDevelopers);

		AssertType(typeof(NodiDataType), NOCustomsDataRegistry.Instance.CustomsNodiId.DataType);
	}

	public void TestEnableTestMessagesEnvProd()
	{
		AssertEnableTestMessages(DatabaseTypes.Codes.Production, RegistryOptions.IsHidden);
	}

	public void TestEnableTestMessagesEnvTest()
	{
		AssertEnableTestMessages(DatabaseTypes.Codes.Test, RegistryOptions.IsOnlyForDevelopers);
	}

	public void TestEnableTemporaryStorageRegister()
		=> TestRegistryItem(ItemSet.EnableTemporaryStorageRegister,
			"EnableTemporaryStorageRegister",
			NOCustomsDataRegistry.Categories.Customs_Norway_TemporaryStorage,
			"Enable Register",
			"Set to YES to Enable Temporary Storage - Register",
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false);

	public void TestIsTemporaryStorageRegisterEnabled()
	{
		var noTemporaryStorageRegistry = NOCustomsDataRegistry.Instance as Integration.Customs.NO.INOTemporaryStorageRegistry;
		AssertNotNull("NO Temporary Storage Registry Instance", noTemporaryStorageRegistry);

		CombineAssertions(() =>
		{
			AssertEquals("Registry Default Value", false, noTemporaryStorageRegistry.IsTemporaryStorageRegisterEnabled);
			SetRegistryAndAssertValueOfIsTemporaryStorageRegisterEnabled(true);
			SetRegistryAndAssertValueOfIsTemporaryStorageRegisterEnabled(false);
		});

		void SetRegistryAndAssertValueOfIsTemporaryStorageRegisterEnabled(bool registryValue)
		{
			using (NOCustomsDataRegistry.Instance.EnableTemporaryStorageRegister.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				AssertEquals($"When Registry Value is set as {registryValue}", registryValue, noTemporaryStorageRegistry.IsTemporaryStorageRegisterEnabled);
			}
		}
	}

	public void TestINOTemporaryStorageRegistryItem_ObjectFactoryConfiguration()
		=> AssertType<NOCustomsDataRegistry>("INOTemporaryStorageRegistry instance", ObjectFactory.Get<Integration.Customs.NO.INOTemporaryStorageRegistry>());

	public void TestCategories_Customs_Norway_TemporaryStorageCategory()
	{
		var multilingualString = NOCustomsDataRegistry.Categories.Customs_Norway_TemporaryStorage;
		AssertEquals("Registry Category Path as string", "Customs/Country or Region Specific/Norway/Temporary Storage", multilingualString.ToString());
	}

	public void TestSendImportErrorsTo()
	{
		var registryItem = NOCustomsDataRegistry.Instance.SendImportErrorsTo;
		CombineAssertions(() =>
		{
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<GroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"NOSendImportErrorsTo",
				NOCustomsDataRegistry.Categories.Customs_Norway_Import,
				"Send Import Errors To",
				"Send Import errors to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			AssertImportMessageDefaultValues(registryItem.DefaultValue);
		});
	}

	public void TestSendImportAcknowledgementsTo()
	{
		var registryItem = NOCustomsDataRegistry.Instance.SendImportAcknowledgementsTo;
		CombineAssertions(() =>
		{
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<GroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"NOSendImportAcknowledgementsTo",
				NOCustomsDataRegistry.Categories.Customs_Norway_Import,
				"Send Import Acknowledgements To",
				"Send Import acknowledgements to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			AssertImportMessageDefaultValues(registryItem.DefaultValue);
		});
	}

	void AssertImportMessageDefaultValues(GroupNotification defaultValue)
	{
		AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, defaultValue.SendMode);
		AssertEquals("Default Group", ZGuid.Empty, defaultValue.SendGroupPK);
	}

	public void TestSendExportErrorsTo()
	{
		var registryItem = NOCustomsDataRegistry.Instance.SendExportErrorsTo;
		CombineAssertions(() =>
		{
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<GroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"NOSendExportErrorsTo",
				NOCustomsDataRegistry.Categories.Customs_Norway_Export,
				"Send Export Errors To",
				"Send Export errors to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController);
			AssertExportMessageDefaultValues(registryItem.DefaultValue);
		});
	}

	public void TestSendExportAcknowledgementsTo()
	{
		var registryItem = NOCustomsDataRegistry.Instance.SendExportAcknowledgementsTo;
		CombineAssertions(() =>
		{
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<GroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"NOSendExportAcknowledgementsTo",
				NOCustomsDataRegistry.Categories.Customs_Norway_Export,
				"Send Export Acknowledgements To",
				"Send Export acknowledgements to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController);
			AssertExportMessageDefaultValues(registryItem.DefaultValue);
		});
	}

	void AssertExportMessageDefaultValues(GroupNotification defaultValue)
	{
		AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, defaultValue.SendMode);
		AssertEquals("Default Group", ZGuid.Empty, defaultValue.SendGroupPK);
	}

	void AssertEnableTestMessages(string databaseType, RegistryOptions defaultValue)
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		registrationKey.DatabaseTypeForTest = databaseType;

		TestRegistryItem(
			ItemSet.EnableTestMessages,
			"EnableTestMessages",
			CustomsDataRegistry.Categories.Customs_Norway,
			"Test Environment",
			"Enable test messages to Norwegian customs?",
			RegistryStorageFlags.Company,
			defaultValue,
			false
		);
	}

	public void TestFtpSettingsCustoms()
	{
		var registryItem = ItemSet.FTPSettingsCustoms;
		CombineAssertions(() =>
		{
			AssertEquals("Registry Type", typeof(FTPSettingsCustomsRegistryItem), registryItem.GetType());

			TestGenericRegistryItem(registryItem,
				"NOFTPSettingsCustoms",
				NOCustomsDataRegistry.Categories.Customs_Norway_FTPSettings,
				"Customs",
				"The FTP server settings for sending and receiving declaration entry messages to and from Norwegian customs.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController);

			AssertDefaultValues(registryItem.DefaultValue);
		});

		void AssertDefaultValues(FTPSettingsCustomsRegistry defaultValue)
		{
			AssertEquals("URL Address", "ftp.ec.evry.com", defaultValue.Url);
			AssertEquals("Port", "21", defaultValue.Port);
			AssertEquals("SendToCustomFolder", "in", defaultValue.SendToCustomFolder);
			AssertEquals("ReceiveFromCustomFolder", "out", defaultValue.ReceiveFromCustomFolder);
		}
	}

	public void TestFtpSettingsEMMADoc()
	{
		var registryItem = ItemSet.FTPSettingsEMMADoc;
		CombineAssertions(() =>
		{
			AssertEquals("Registry Type", typeof(FTPSettingsEMMADocRegistryItem), registryItem.GetType());

			TestGenericRegistryItem(registryItem,
			 "NOFTPSettingsEMMADoc",
			 NOCustomsDataRegistry.Categories.Customs_Norway_FTPSettings,
			 "EMMA Doc",
			 "The FTP server settings for sending declaration entry and attachments to EMMA Doc.",
			 RegistryStorageFlags.Company,
			 RegistryOptions.IsOnlyForController);

			AssertDefaultValues(registryItem.DefaultValue);
		});

		void AssertDefaultValues(FTPSettingsRegistry defaultValue)
		{
			AssertEquals("URL Address", "ftpedoc.emma.no", defaultValue.Url);
			AssertEquals("Port", "21", defaultValue.Port);
		}
	}
}
