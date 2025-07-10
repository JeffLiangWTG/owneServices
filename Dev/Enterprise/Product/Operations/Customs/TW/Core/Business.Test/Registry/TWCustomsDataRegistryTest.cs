using System;
using CargoWise.Application;
using CargoWise.BrandManager;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.TWCustomsDataRegistry;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWCustomsDataRegistry))]
	sealed class TWCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<TWCustomsDataRegistry>
	{
		public void TestEnableNXM()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.EnableNX201_01, "EnableNX201_01", Categories.Customs_Taiwan, "Enable NX201_01", "Set this value to 'Yes' to enable NX201_01 related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
				TestRegistryItem(ItemSet.EnableNX201_07, "EnableNX201_07", Categories.Customs_Taiwan, "Enable NX201_07", "Set this value to 'Yes' to enable NX201_07 related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
				TestRegistryItem(ItemSet.EnableNX301, "EnableNX301", Categories.Customs_Taiwan, "Enable NX301", "Set this value to 'Yes' to enable NX301 related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
				TestRegistryItem(ItemSet.EnableNX301_AX, "EnableNX301_AX", Categories.Customs_Taiwan, "Enable NX301_AX", "Set this value to 'Yes' to enable NX301_AX related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
				TestRegistryItem(ItemSet.EnableNX301_DN, "EnableNX301_DN", Categories.Customs_Taiwan, "Enable NX301_DN", "Set this value to 'Yes' to enable NX301_DN related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
				TestRegistryItem(ItemSet.EnableNX401, "EnableNX401", Categories.Customs_Taiwan, "Enable NX401", "Set this value to 'Yes' to enable NX401 related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
				TestRegistryItem(ItemSet.EnableNX601, "EnableNX601", Categories.Customs_Taiwan, "Enable NX601", "Set this value to 'Yes' to enable NX601 related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
				TestRegistryItem(ItemSet.EnableNX603, "EnableNX603", Categories.Customs_Taiwan, "Enable NX603", "Set this value to 'Yes' to enable NX603 related functionalities.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
			});
		}

		public void TestEnableCustomsDeclarationPackingList()
		{
			TestRegistryItem(ItemSet.EnableCustomsDeclarationPackingList, "EnableCustomsDeclarationPackingList", Categories.Customs_Taiwan, "Enable Customs Declaration Packing List", "Set this to 'Yes' to show the menu item under Customs Declaration > Brokerage > Create Packing List, and hide the tabs under Customs Declaration > Packing and Customs Declaration > Invoice Lines > Packages in the supported countries.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, true);
		}

		public void TestTestMode()
		{
			TestRegistryItem(ItemSet.TWIsTestMode, "TWIsTestMode", Categories.Customs_Taiwan, "Is Test Mode", "Should TW customs messages (for Declaration and Transhipment Declaration) be sent in testing mode?", RegistryStorageFlags.Company, RegistryOptions.Default, ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Test);
		}

		[ExpectNoExceptions]
		public void TestIsTestLicence()
		{
			var registry = new TWCustomsDataRegistry();
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			NUnit.Framework.Assert.That(!registry.IsTestLicence, NUnit.Framework.Is.True);
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			NUnit.Framework.Assert.That(registry.IsTestLicence, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsTestMode()
		{
			Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			NUnit.Framework.Assert.That(IsTestMode, NUnit.Framework.Is.True);
			Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			NUnit.Framework.Assert.That(!IsTestMode, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestTranshipmentDescriptionCustomization()
		{
			var importitem = ItemSet.TranshipmentDescriptionCustomization;
			NUnit.Framework.Assert.That(importitem.Name, NUnit.Framework.Is.EqualTo("TranshipmentDescriptionCustomization"), "Name");
			NUnit.Framework.Assert.That(importitem.Category, NUnit.Framework.Is.EqualTo(Categories.Customs_Taiwan).Using(CustomComparers.TypeComparison), "Category");
			NUnit.Framework.Assert.That(importitem.Caption, NUnit.Framework.Is.EqualTo("Transhipment Description Customization"), "Caption");
			NUnit.Framework.Assert.That(importitem.Hint, NUnit.Framework.Is.EqualTo($"Select one or more of the following check boxes to define what reference information will be used to describe an transhipment when it appears in the Favorites or Recent Items areas on the main {BrandingFactory.Instance.ProductName} form and the Recent Items on the Transhipment module."), "Hint");
			NUnit.Framework.Assert.That(importitem.Storage, NUnit.Framework.Is.EqualTo(RegistryStorageFlags.Company), "Flags");
			NUnit.Framework.Assert.That(((CodeDescriptionBoolRegistryEditorInfo)importitem.EditorInfo).BoolColumnCaption, NUnit.Framework.Is.EqualTo("Select the data you wish to use to describe an transhipment"), "Bool Caption");
			NUnit.Framework.Assert.That(importitem.DefaultValue.Count, NUnit.Framework.Is.EqualTo(5), "Default Value");
			AssertCodeDescriptionBool("JNO", "Job Number", true, importitem.DefaultValue[0]);
			AssertCodeDescriptionBool("MBL", "Master Bill", true, importitem.DefaultValue[1]);
			AssertCodeDescriptionBool("HBL", "House Bill", true, importitem.DefaultValue[2]);
			AssertCodeDescriptionBool("ENT", "Entry Number", false, importitem.DefaultValue[3]);
			AssertCodeDescriptionBool("IMP", "Importer Code", false, importitem.DefaultValue[4]);
		}

		[ExpectNoExceptions]
		void AssertCodeDescriptionBool(string code, string description, bool sysdefined, CodeDescriptionBool codeDescriptionBool)
		{
			NUnit.Framework.Assert.That(codeDescriptionBool.Code, NUnit.Framework.Is.EqualTo(code).Using(CustomComparers.TypeComparison), "Code");
			NUnit.Framework.Assert.That(codeDescriptionBool.Description, NUnit.Framework.Is.EqualTo(description).Using(CustomComparers.TypeComparison), "Description");
			NUnit.Framework.Assert.That(codeDescriptionBool.Bool, NUnit.Framework.Is.EqualTo(sysdefined).Using(CustomComparers.TypeComparison), "Bool");
		}

		public void TestCusBrokerageBoxNumber()
		{
			TestGenericRegistryItem(ItemSet.CusBrokerageBoxNumber, "CusBrokerageBoxNumber", Categories.Customs_Taiwan, "Default Brokerage Box Number", "The customs Brokerage Box Number.", RegistryStorageFlags.Company);
		}

		public void TestCusGoodsLocation()
		{
			TestGenericRegistryItem(ItemSet.CusGoodsLocation, "CusGoodsLocation", Categories.Customs_Taiwan, "Default Goods Location", "Default Goods Location.", RegistryStorageFlags.Company);
		}

		public void TestTWNCATKClientSetting()
		{
			TestGenericRegistryItem(ItemSet.TWNCATKClientSetting, "TWNCATKClientSetting", Categories.Customs_Taiwan, "NCATK Message Sending Configuration", "The settings are for TW NCATK Client Application which is a standalone tool installed on the client’s local machine. The tool sends and receives messages through the Remote Printing Client software.", RegistryStorageFlags.Company);
		}

		public void TestCusBrokerStaff()
		{
			TestGenericRegistryItem(ItemSet.CusBrokerStaff, "CusBrokerStaff", Categories.Customs_Taiwan, "Default Broker Staff", "Default Broker Staff.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment);
		}

		public void TestCusCustomsOffice()
		{
			TestGenericRegistryItem(ItemSet.CusCustomsOffice, "CusCustomsOffice", Categories.Customs_Taiwan, "Default Customs Office", "Default Customs Office.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment);
		}

		[ExpectNoExceptions]
		public void TestITWCustomsRegistry_CustomsPackingListEnable()
		{
			NUnit.Framework.Assert.That(((Integration.Customs.TW.ITWCustomsRegistry)ItemSet).CustomsPackingListEnable, NUnit.Framework.Is.EqualTo(ItemSet.EnableCustomsDeclarationPackingList).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestITWCustomsRegistry_EnableBriefCustomsDeclaration()
		{
			NUnit.Framework.Assert.That(((Integration.Customs.TW.ITWCustomsRegistry)ItemSet).EnableBriefCustomsDeclaration, NUnit.Framework.Is.EqualTo(ItemSet.EnableBriefCustomsDeclaration).Using(CustomComparers.TypeComparison));
		}

		public void TestEnableBriefCustomsDeclaration()
		{
			TestRegistryItem(
				ItemSet.EnableBriefCustomsDeclaration,
				"TWENABLEBRIEFCUSDECL",
				Categories.Customs_Taiwan,
				"Enable Brief Customs Declaration",
				"Set this value to 'Yes' to enable Taiwan Brief Customs Declaration related functionalities.",
				RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestAlwaysCalculatePackQtyFromPackNumber()
		{
			TestRegistryItem(
				ItemSet.AlwaysCalculatePackQtyFromPackNumber,
				"ALWAYSCALCPACKQTYFROMPACKNO",
				Categories.Customs_Taiwan,
				"Always Calculate Pack Quantity From Pack Number",
				"When the value is 'Yes', the 'Calculate Pack Qty from Pack #' menu item under Packing List > Action will be clicked by default.",
				RegistryStorageFlags.Branch, RegistryOptions.Default,
				false);
		}

		public void TestAlwaysCalculateAdditionalTax()
		{
			TestRegistryItem(
				ItemSet.AlwaysCalculateAdditionalTax,
				"AlwaysCalculateAdditionalTax",
				Categories.Customs_Taiwan,
				"Always Calculate Additional Tax",
				"Set to 'Yes' to allow automatically calculating the additional taxes when the selected tariff has Customs Regulations T, T*, B, B*, L*, C. In other words, if the goods do not need to declare the corresponding additional taxes, they need to be deleted manually.",
				RegistryStorageFlags.Company, RegistryOptions.Default,
				false);
		}

		public void TestValidateEntryNumber()
		{
			TestRegistryItem(
				ItemSet.ValidateEntryNumber,
				"ValidateEntryNumber",
				Categories.Customs_Taiwan,
				"Validate Entry Number",
				"Display customs error when Entry number is not allocated.",
				RegistryStorageFlags.Company, RegistryOptions.Default,
				false);
		}

		public void TestDefaultPrintingGoodsLocationDescription()
		{
			TestRegistryItem(
				ItemSet.DefaultPrintingGoodsLocationDescription,
				"DefaultPrintingGoodsLocationDescription",
				Categories.Customs_Taiwan,
				"Default Printing Goods Location Description",
				"Set to 'Yes' to print Goods Location description on Customs Declaration documents.",
				RegistryStorageFlags.Company, RegistryOptions.Default,
				false);
		}
	}
}
