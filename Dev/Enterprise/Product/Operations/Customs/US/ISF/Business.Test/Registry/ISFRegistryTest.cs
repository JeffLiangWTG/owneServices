using System;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.DataRegistry.Business.Testing
{
	[TestedType(typeof(ISFRegistry))]
	sealed class ISFRegistryTest : RegistryItemSetTestCaseWithFactory<ISFRegistry>
	{
		public void TestImporterSecurityFilingNumberCustomisation()
		{
			TestGenericRegistryItem(ItemSet.ImporterSecurityFilingNumberCustomisation, "ImporterSecurityFilingNumberCustomisation", ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling, "Importer Security Filing Number Customization", "Override this value to customize how Importer Security Filing numbers are formatted", RegistryStorageFlags.All);
			BillCustomisationRegistryDataType dataType = (BillCustomisationRegistryDataType)ItemSet.ImporterSecurityFilingNumberCustomisation.DataType;
			AssertEquals("GeneratedNumberName", "Importer Security Filing Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 20, dataType.MaxLength);
			AssertEquals("7", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.TransportMode]);
		}

		public void TestImporterSecurityFilingNumberCustomisation_SerializeDefaultValue()
		{
			var item = ItemSet.ImporterSecurityFilingNumberCustomisation;
			var dataType = item.DataType;

			var original = item.DefaultValue;
			var deserialized = (BillOfLadingNumberCustomisation)dataType.Deserialise(dataType.Serialise(original));

			CombineAssertions(() =>
			{
				Assert("Items should be seen as equal", dataType.ValuesAreEqual(original, deserialized));
				AssertEquals("Should have the same number of elements", original.Elements.Count, deserialized.Elements.Count);
				AssertEquals("Should have the same number of unfiltered elements", original.UnFilteredElements.Count, deserialized.UnFilteredElements.Count);
			});
		}

		public void TestImporterSecurityFilingMessagesGroup()
		{
			TestRegistryItem(ItemSet.ImporterSecurityFilingMessagesGroup,
				"ImporterSecurityFilingMessagesGroup",
				ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
				"Messaging Group",
				"Group to receive Importer Security Filing messaging notifications",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.CannotCallParameterlessValueGetter,
				RegistryFindBoxCollection.GlbGroup,
				Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestImporterSecurityFilingNoOfHTSDigits()
		{
			TestGenericRegistryItem(ItemSet.ImporterSecurityFilingNoOfHTSDigits,
				"ImporterSecurityFilingNoOfHTSDigits",
				ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
				"No. Of HTS Digits",
				"The default number of HTS digits to be reported to Customs.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter,
				NumberOfHarmonizedDigitsList.Codes.Ten);
		}

		public void TestImporterSecurityFilingShouldMergeLine()
		{
			TestRegistryItem(ItemSet.ImporterSecurityFilingShouldMergeLine,
				"ImporterSecurityFilingShouldMergeLine",
				ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
				"Merge Lines?",
				"If ticked, the system will merge the lines by default.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter,
				false);
		}

		public void TestImporterSecurityFilingShouldReportContainerToCustoms()
		{
			TestRegistryItem(ItemSet.ImporterSecurityFilingShouldReportContainerToCustoms,
				"ImporterSecurityFilingShouldReportContainerToCustoms",
				ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
				"Report Containers?",
				"If ticked, the system will report the container details to Customs by default.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter,
				true);
		}

		public void TestImporterSecurityFilingXMLImportNotificationGroup()
		{
			TestRegistryItem(ItemSet.ImporterSecurityFilingXMLImportNotificationGroup,
				"ImporterSecurityFilingXMLImportNotificationGroup",
				RawDataRegistry.Categories.Notification,
				"ISF XML Import Notification Group",
				"The staff group that will be notified about the Result of Importer Security Filing XML Import.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.CannotCallParameterlessValueGetter,
				RegistryFindBoxCollection.GlbGroup,
				Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestImporterSecurityFilingXMLImportNotificationGroup_ProductivityWiseEnabled()
		{
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			TestRegistryItem(ItemSet.ImporterSecurityFilingXMLImportNotificationGroup,
				"ImporterSecurityFilingXMLImportNotificationGroup",
				RawDataRegistry.Categories.Notification,
				"ISF XML Import Notification Group",
				"The staff group that will be notified about the Result of Importer Security Filing XML Import.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.IsHidden,
				RegistryFindBoxCollection.GlbGroup,
				Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestImporterSecurityFilingDataImportDirectory()
		{
			TestRegistryItem(ItemSet.ImporterSecurityFilingDataImportDirectory, "ImporterSecurityFilingDataImportDirectory", SystemDataRegistry.Categories.System_DataImportSettings + "/Importer Security Filing", "Folder to scan for ISF XML files", "The folder specified here should contain any Importer Security Filing XML files that are to be automatically imported.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, TextEditorType.DirectoryBrowser, "");
		}

		public void TestISFUSRestrictISFJobs()
		{
			TestRegistryItem(ItemSet.ISFUSRestrictISFJobs,
				"ISFUSRestrictISFJobs",
				ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
				"Restrict ISF jobs to the Home Branch",
				"This registry setting will restrict ISF jobs from being saved if the branch on the ISF job is in a different company than the user’s home branch company.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				false);
		}

		public void TestImporterSecurityFilingMessageUsageReportDate()
		{
			TestGenericRegistryItem(ItemSet.ImporterSecurityFilingMessageUsageReportDate,
				"ImporterSecurityFilingMessageUsageReportDate",
				ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
				"Next Message Usage Report Date",
				"The date that the next Message Usage Report should be run.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers);
			AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short, ((DateTimeRegistryEditorInfo)ItemSet.ImporterSecurityFilingMessageUsageReportDate.EditorInfo).DateTimeFormat);
		}

		public void TestLicencedRegistryItem()
		{
			Assert(ItemSet.ImporterSecurityFilingDataImportDirectory is ILicencedRegistryItem);
		}

		public void TestHVLVImporterSecurityFilingMessagesGroup()
		{
			TestGenericRegistryItem(ItemSet.HVLVImporterSecurityFilingMessagesGroup,
				"HVLVImporterSecurityFilingMessagesGroup",
				ISFRegistry.Categories.Customs_UnitedStatesofAmerica_Import_ABI_ImporterSecurityFiling,
				"HVLV Messaging Group",
				"Group to receive Importer Security Filing messaging notifications for filings that originate from HVL shipments. If 'Send Error Only' ticked, only when an error occurs then send the message notification.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter);

			CombineAssertions(() =>
			{
				AssertEquals("SendMode", Core.Constants.EmailTo.StaffMember, ItemSet.HVLVImporterSecurityFilingMessagesGroup.DefaultValue.SendMode);
				AssertEquals("SendGroup", Guid.Empty, ItemSet.HVLVImporterSecurityFilingMessagesGroup.DefaultValue.SendGroupPK);
				AssertEquals("ErrorOnly", true, ItemSet.HVLVImporterSecurityFilingMessagesGroup.DefaultValue.SendErrorOnly);
			});
		}
	}
}
