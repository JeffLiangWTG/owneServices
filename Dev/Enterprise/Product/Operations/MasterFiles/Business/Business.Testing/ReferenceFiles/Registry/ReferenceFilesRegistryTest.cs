using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReferenceFilesDataRegistry))]
	sealed class ReferenceFilesRegistryTest : RegistryItemSetTestCaseWithFactory<ReferenceFilesDataRegistry>
	{
		public void TestComplianceListDefaults()
		{
			TestGenericRegistryItem(ItemSet.ComplianceListDefaults, "ComplianceListDefaults", "Reference Files", "Compliance List Defaults", "This registry determines the default exclusion settings for newly created Compliance List records.", RegistryStorageFlags.System);

			var expectedDefaultDescriptions = new List<(ZString, string, string)>()
			{
				("ADV", "Advisories", "N"),
				("ENF", "Enforcement Orders", "Y"),
				("EXC", "Exclusion List", "N"),
				("FIN", "Financial Sanctions", "N"),
				("LAW", "Law Enforcement", "Y"),
				("SAN", "Sanctions Ownership", "N"),
				("TRA", "Trade Restrictions", "N"),
				("VIS", "Visa/Travel Ban", "N"),
				("WAC", "War Crimes", "N"),
				("WAN", "Warnings", "Y")
			};

			AssertContainsExactElementsInAnyOrder("all Compliance List Defaults", expectedDefaultDescriptions, ItemSet.ComplianceListDefaults.DefaultValue.Cast<CodeDescriptionBool>().Select(x => (x.Code, x.Description.ToString(), x.Bool.ToString())));
		}

		public void TestEquipmentCertificateTypes()
		{
			AssertEquals("Name", "EquipmentCertificateTypes", ItemSet.EquipmentCertificateTypes.Name);
			AssertEquals("Category", "Reference Files", ItemSet.EquipmentCertificateTypes.Category);
			AssertEquals("Caption", "Equipment Certificate Types", ItemSet.EquipmentCertificateTypes.Caption);
			AssertEquals("Hint", "Types of certificates/licenses/registrations/services applicable to Equipment.", ItemSet.EquipmentCertificateTypes.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EquipmentCertificateTypes.Storage);

			AssertEquals(true, ItemSet.EquipmentCertificateTypes.Value.Count >= 1);
			var item1 = ItemSet.EquipmentCertificateTypes.Value.Find(EquipmentCertificateTypeList.Codes.Service);
			AssertCertificateType(EquipmentCertificateTypeList.Codes.Service, EquipmentCertificateTypeList.Descriptions.Service, false, false, false, AlertTypeList.Codes.NoAlert, item1);

			var collection = new CertificateTypeCollection();
			var type = collection.AddNew();
			type.SetupValues("AAA", "A desc", true, true, false, AlertTypeList.Codes.NoAlert);
			type = collection.AddNew();
			type.SetupValues("BBB", "B desc", false, false, false, AlertTypeList.Codes.NoAlert);

			ItemSet.EquipmentCertificateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(2, ItemSet.EquipmentCertificateTypes.Value.Count);
			AssertCertificateType("AAA", "A desc", true, true, false, AlertTypeList.Codes.NoAlert, ItemSet.EquipmentCertificateTypes.Value[0]);
			AssertCertificateType("BBB", "B desc", false, false, false, AlertTypeList.Codes.NoAlert, ItemSet.EquipmentCertificateTypes.Value[1]);
		}

		public void TestPrintOuterBreakdownPackingDetailsOnBOL()
		{
			TestRegistryItem(ItemSet.ShowPersonalEffects,
				"ShowPersonalEffects",
				"Reference Files",
				"Show Personal Effects",
				"Enable this option to show the 'Is Personal Effects' option on Commodity.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestEnableShippingLineReferenceFile()
		{
			TestRegistryItem(ItemSet.EnableShippingLineReferenceFile,
				"EnableShippingLineReferenceFile",
				"Reference Files",
				"Enable Shipping Line Reference File",
				"Set option to 'Yes' will enable Shipping Line Reference File",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				true);
		}

		#region Implementation

		void AssertCertificateType(ZString code, ZString description, ZBool isMandatory, ZBool isUnique, ZBool isSystem, ZString alertType, CertificateType type)
		{
			AssertEquals("Code", code, type.Code);
			AssertEquals("Description", description, type.Description);
			AssertEquals("IsMandatory", isMandatory, type.IsMandatory);
			AssertEquals("IsUnique", isUnique, type.IsUnique);
			AssertEquals("IsSystem", isSystem, type.IsSystem);
			AssertEquals("AlertType", alertType, type.AlertType);
		}

		#endregion
	}
}
