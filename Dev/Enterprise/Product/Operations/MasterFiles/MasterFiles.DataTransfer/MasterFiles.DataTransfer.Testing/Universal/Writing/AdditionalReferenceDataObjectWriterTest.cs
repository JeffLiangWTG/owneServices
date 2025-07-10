namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.UniversalDataBuss.DataObjects.Core;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.UniversalDataBuss.Integration;
	using ICusEntryNumber = Enterprise.Integration.Customs.ICusEntryNumber;

	public class AdditionalReferenceDataObjectWriterTest : TestCaseWithFactory
	{
		#region TestBasicAdditionalReferenceLevelMappings

		public void TestBasicAdditionalReferenceLevelMappings()
		{
			var additionalReferenceBO = SetupAdditionalReference(Factory);
			var writer = new AdditionalReferenceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, additionalReferenceBO)));
			var additionalReferenceDataObject = writer.GetDataObject(additionalReferenceBO);
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);

			CombineAssertions(delegate
			{
				AssertContents(additionalReferenceDataObject);
			});
		}

		#endregion

		#region CountryOfIssue

		public void TestCountryOfIssue_WhenCountryCodeIsEmpty_ReturnsNull()
		{
			var additionalReferenceBO = SetupAdditionalReference(Factory);
			var writer = new AdditionalReferenceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, additionalReferenceBO)));

			var additionalReferenceDataObject = writer.GetDataObject(additionalReferenceBO);

			AssertNull(additionalReferenceDataObject.CountryOfIssue);
		}

		public void TestCountryOfIssue_WhenCountryCodeIsNotEmpty_ReturnsCountryWithCorrectCodeAndName()
		{
			var countryCode = "MX";
			var expectedCcountry = new Country() { Code = countryCode, Name = "Mexico" };

			var additionalReferenceBO = SetupAdditionalReference(Factory, countryCode: countryCode);
			var writer = new AdditionalReferenceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, additionalReferenceBO)));

			var additionalReferenceDataObject = writer.GetDataObject(additionalReferenceBO);

			AssertEquals(expectedCcountry.Code, additionalReferenceDataObject.CountryOfIssue.Code);
			AssertEquals(expectedCcountry.Name, additionalReferenceDataObject.CountryOfIssue.Name);
		}

		public void TestCountryOfIssue_WhenCountryCodeIsNotEmpty_ReturnsCountryWhenRecipientRoleDetailsIsNull()
		{
			var countryCode = "MX";
			var expectedCcountry = new Country() { Code = countryCode, Name = "Mexico" };

			var additionalReferenceBO = SetupAdditionalReference(Factory, countryCode: countryCode);
			var writer = new AdditionalReferenceDataObjectWriter(new DataWritingManager(new ActionInfo(null, additionalReferenceBO)));

			var additionalReferenceDataObject = writer.GetDataObject(additionalReferenceBO);

			AssertEquals(expectedCcountry.Code, additionalReferenceDataObject.CountryOfIssue.Code);
			AssertEquals(expectedCcountry.Name, additionalReferenceDataObject.CountryOfIssue.Name);
		}

		#endregion

		#region SetupAdditionalReference

		public static BusinessObject SetupAdditionalReference(BusinessObjectFactory factory, string entryType = "AMS", string entryNum = "CE00001", string countryCode = "")
		{
			var cusEntryNumber = factory.New<ICusEntryNumber>();
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_EntryLineReference = "INFORMER";
			cusEntryNumber.CE_Category = "OTH";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2011, 3, 3);
			cusEntryNumber.CE_RN_NKCountryCode = countryCode;

			return (BusinessObject)cusEntryNumber;
		}

		#endregion

		#region AssertContents

		public static void AssertContents(AdditionalReference additionalReferenceDataObject, string entryType = "AMS", string entryTypeDescription = "AMS Number", string entryNum = "CE00001")
		{
			AssertEquals("additionalReferenceDataObject.IssueDate", new ZDateTime(2011, 3, 3), additionalReferenceDataObject.IssueDate);
			AssertEquals("additionalReferenceDataObject.ReferenceNumber", entryNum, additionalReferenceDataObject.ReferenceNumber);
			AssertEquals("additionalReferenceDataObject.ContextInformation", "INFORMER", additionalReferenceDataObject.ContextInformation);
			AssertEquals("additionalReferenceDataObject.Type.Code", entryType, additionalReferenceDataObject.Type.Code);
			AssertEquals("additionalReferenceDataObject.Type.Description", entryTypeDescription, additionalReferenceDataObject.Type.Description);
		}

		#endregion
	}
}
