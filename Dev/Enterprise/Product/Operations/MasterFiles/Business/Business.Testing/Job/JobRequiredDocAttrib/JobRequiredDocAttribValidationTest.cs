using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocAttribValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckD0_AttribDisplayValue()
		{
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var reqDoc = Factory.New<JobRequiredDocument>();
				var attrib1 = reqDoc.Attributes.AddNew();
				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
				attrib1.D0_AttribDisplayValue = "1234567";
				attrib1.Validation.ValidateD0_AttribDisplayValue();
				AssertHasErrorContaining(attrib1.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidProtocolloFormat);
				attrib1.D0_AttribDisplayValue = "12345678901234567-123456";
				AssertNoErrors(attrib1.D0_AttribDisplayValueInfo);

				var attrib2 = reqDoc.Attributes.AddNew();
				attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate;
				attrib2.D0_AttribDisplayValue = "1223232";
				AssertHasErrorContaining(attrib2.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidDocumentReceivedDate);
				attrib2.D0_AttribDisplayValue = "15-11-2016";
				AssertNoErrors(attrib2.D0_AttribDisplayValueInfo);

				var attrib3 = reqDoc.Attributes.AddNew();
				attrib3.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
				var attribInfo = attrib3.D0_AttribDisplayValueInfo;
				attrib3.D0_AttribDisplayValue = "abcde";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.InvalidCeilingLimit);
				attrib3.D0_AttribDisplayValue = "5,000.00";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.InvalidCeilingLimit);
				attrib3.D0_AttribDisplayValue = "-500,00";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.CeilingLimitMustBePositive);
				attrib3.D0_AttribDisplayValue = "0";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.CeilingLimitMustBePositive);
				attrib3.D0_AttribDisplayValue = "10000";
				AssertNoErrors(attribInfo);
				attrib3.D0_AttribDisplayValue = "5.000,00";
				AssertNoErrors(attribInfo);
				attrib3.D0_AttribDisplayValue = "1000,25";
				AssertNoErrors(attribInfo);

				var attrib4 = reqDoc.Attributes.AddNew();
				attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BuyerIssueDate;
				attrib4.D0_AttribDisplayValue = "1223232";
				AssertHasErrorContaining(attrib4.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidBuyerIssueDate);
				attrib4.D0_AttribDisplayValue = "15-11-2016";
				AssertNoErrors(attrib4.D0_AttribDisplayValueInfo);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var importer = Factory.New<OrgHeader>();
				var reqDoc1 = importer.RequiredDocuments.AddNew();
				reqDoc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				reqDoc1.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib1 = reqDoc1.CustomsDistrictDocAttrib;
				attrib1.D0_AttribDisplayValue = ZString.Empty;
				AssertHasErrorContaining(attrib1.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.GetErrorForTaiwanAttorneyAttributeValueMissing(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));

				attrib1.D0_AttribDisplayValue = "F";
				AssertHasErrorContaining(attrib1.D0_AttribDisplayValueInfo, ListValidation.InvalidCodeError);

				attrib1.D0_AttribDisplayValue = TaiwanCustomsDistrictList.Codes.A;
				AssertNoErrorContaining(attrib1.D0_AttribDisplayValueInfo, ListValidation.InvalidCodeError);
				var boxNumberAttrib1 = reqDoc1.BoxNumberDocAttrib;
				boxNumberAttrib1.D0_AttribDisplayValue = "123";
				var bondedIDAttrib1 = reqDoc1.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib1.D0_AttribDisplayValue = "abc";

				var reqDoc2 = importer.RequiredDocuments.AddNew();
				reqDoc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc2.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				reqDoc2.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib2 = reqDoc2.CustomsDistrictDocAttrib;
				attrib2.D0_AttribDisplayValue = TaiwanCustomsDistrictList.Codes.A;

				var bondedIDAttrib2 = reqDoc2.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib2.D0_AttribDisplayValue = "abc";
				var boxNumberAttrib2 = reqDoc2.BoxNumberDocAttrib;
				boxNumberAttrib2.D0_AttribDisplayValue = "123";
				AssertHasErrorContaining(boxNumberAttrib2.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);

				boxNumberAttrib2.D0_AttribDisplayValue = "456";
				AssertNoErrorContaining(boxNumberAttrib2.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);

				var reqDoc3 = importer.RequiredDocuments.AddNew();
				reqDoc3.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc3.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				reqDoc3.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib3 = reqDoc3.CustomsDistrictDocAttrib;
				attrib3.D0_AttribDisplayValue = TaiwanCustomsDistrictList.Codes.A;
				var boxNumberAttrib3 = reqDoc3.BoxNumberDocAttrib;
				boxNumberAttrib3.D0_AttribDisplayValue = "123";
				var bondedIDAttrib3 = reqDoc3.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib3.D0_AttribDisplayValue = "abc";

				var reqDoc4 = importer.RequiredDocuments.AddNew();
				reqDoc4.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc4.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				reqDoc4.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib4 = reqDoc4.CustomsDistrictDocAttrib;
				attrib4.D0_AttribDisplayValue = TaiwanCustomsDistrictList.Codes.A;
				var boxNumberAttrib4 = reqDoc4.BoxNumberDocAttrib;
				boxNumberAttrib4.D0_AttribDisplayValue = "123";
				var bondedIDAttrib4 = reqDoc4.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib4.D0_AttribDisplayValue = "abc";
				attrib4.Validation.ValidateD0_AttribDisplayValue();
				AssertHasErrorContaining(attrib4.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);

				attrib4.D0_AttribDisplayValue = TaiwanCustomsDistrictList.Codes.B;
				boxNumberAttrib4.D0_AttribDisplayValue = "123";
				bondedIDAttrib4.D0_AttribDisplayValue = "abc";
				attrib4.Validation.ValidateD0_AttribDisplayValue();
				AssertNoErrorContaining(attrib4.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var reqDoc = Factory.New<JobRequiredDocument>();
				var attrib1 = reqDoc.Attributes.AddNew();
				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;

				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
				attrib1.D0_AttribDisplayValue = "1234567";
				AssertHasErrorContaining(attrib1.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidTradePreferenceCodeInUnitedStates);
				attrib1.D0_AttribDisplayValue = "US";
				AssertHasErrorContaining(attrib1.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.InvalidTradePreferenceCodeInUnitedStates);
				attrib1.D0_AttribDisplayValue = "S";
				AssertNoErrors(attrib1.D0_AttribDisplayValueInfo);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
				helper.CreatePreferenceForCountry("10", "10", CountryCodes.Canada);

				var reqDoc = Factory.New<JobRequiredDocument>();
				var attrib1 = reqDoc.Attributes.AddNew();
				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;

				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
				attrib1.D0_AttribDisplayValue = "1234567";
				AssertHasErrorContaining(attrib1.D0_AttribDisplayValueInfo, ListValidation.InvalidCodeMessageError.ToString());
				attrib1.D0_AttribDisplayValue = "CA";
				AssertHasErrorContaining(attrib1.D0_AttribDisplayValueInfo, ListValidation.InvalidCodeMessageError.ToString());
				attrib1.D0_AttribDisplayValue = "10";
				AssertNoErrors(attrib1.D0_AttribDisplayValueInfo);
			}
		}

		public void TestCheckD0_AttribValue()
		{
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var reqDoc = Factory.New<JobRequiredDocument>();
				var attrib1 = reqDoc.Attributes.AddNew();
				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
				attrib1.D0_AttribValue = "1234567";
				AssertHasErrorContaining(attrib1.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidProtocolloFormat);
				attrib1.D0_AttribValue = "12345678901234567-123456";
				AssertNoErrors(attrib1.D0_AttribValueInfo);

				var attrib2 = reqDoc.Attributes.AddNew();
				attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate;
				attrib2.D0_AttribValue = "1223232";
				AssertHasErrorContaining(attrib2.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidDocumentReceivedDate);
				attrib2.D0_AttribValue = "15-11-2016";
				AssertNoErrors(attrib2.D0_AttribValueInfo);

				var attrib3 = reqDoc.Attributes.AddNew();
				attrib3.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
				var attribInfo = attrib3.D0_AttribValueInfo;
				attrib3.D0_AttribValue = "abcde";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.InvalidCeilingLimit);
				attrib3.D0_AttribValue = "5.000,00";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.InvalidCeilingLimit);
				attrib3.D0_AttribValue = "-500.00";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.CeilingLimitMustBePositive);
				attrib3.D0_AttribValue = "0";
				AssertHasErrorContaining(attribInfo, JobRequiredDocAttribValidation.CeilingLimitMustBePositive);
				attrib3.D0_AttribValue = "10000";
				AssertNoErrors(attribInfo);
				attrib3.D0_AttribValue = "1000.25";
				AssertNoErrors(attribInfo);

				var attrib4 = reqDoc.Attributes.AddNew();
				attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BuyerIssueDate;
				attrib4.D0_AttribValue = "1223232";
				AssertHasErrorContaining(attrib4.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidBuyerIssueDate);
				attrib4.D0_AttribValue = "15-11-2016";
				AssertNoErrors(attrib4.D0_AttribValueInfo);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var importer = Factory.New<OrgHeader>();
				var reqDoc1 = importer.RequiredDocuments.AddNew();
				reqDoc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				reqDoc1.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib1 = reqDoc1.CustomsDistrictDocAttrib;
				attrib1.D0_AttribValue = ZString.Empty;
				AssertHasErrorContaining(attrib1.D0_AttribValueInfo, JobRequiredDocAttribValidation.GetErrorForTaiwanAttorneyAttributeValueMissing(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));

				attrib1.D0_AttribValue = "F";
				AssertHasErrorContaining(attrib1.D0_AttribValueInfo, ListValidation.InvalidCodeError);

				attrib1.D0_AttribValue = TaiwanCustomsDistrictList.Codes.A;
				AssertNoErrorContaining(attrib1.D0_AttribValueInfo, ListValidation.InvalidCodeError);
				var boxNumberAttrib1 = reqDoc1.BoxNumberDocAttrib;
				boxNumberAttrib1.D0_AttribValue = "123";
				var bondedIDAttrib1 = reqDoc1.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib1.D0_AttribValue = "abc";

				var reqDoc2 = importer.RequiredDocuments.AddNew();
				reqDoc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc2.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				reqDoc2.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib2 = reqDoc2.CustomsDistrictDocAttrib;
				attrib2.D0_AttribValue = TaiwanCustomsDistrictList.Codes.A;

				var boxNumberAttrib2 = reqDoc2.BoxNumberDocAttrib;
				boxNumberAttrib2.D0_AttribValue = "123";
				var bondedIDAttrib2 = reqDoc2.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib2.D0_AttribValue = "abc";
				AssertHasErrorContaining(bondedIDAttrib2.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);

				boxNumberAttrib2.D0_AttribValue = "456";
				AssertNoErrorContaining(boxNumberAttrib2.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);

				var reqDoc3 = importer.RequiredDocuments.AddNew();
				reqDoc3.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc3.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				reqDoc3.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib3 = reqDoc3.CustomsDistrictDocAttrib;
				attrib3.D0_AttribValue = TaiwanCustomsDistrictList.Codes.A;
				var boxNumberAttrib3 = reqDoc3.BoxNumberDocAttrib;
				boxNumberAttrib3.D0_AttribValue = "123";
				var bondedIDAttrib3 = reqDoc3.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib3.D0_AttribValue = "abc";

				var reqDoc4 = importer.RequiredDocuments.AddNew();
				reqDoc4.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				reqDoc4.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				reqDoc4.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				var attrib4 = reqDoc4.CustomsDistrictDocAttrib;
				attrib4.D0_AttribValue = TaiwanCustomsDistrictList.Codes.A;
				var boxNumberAttrib4 = reqDoc4.BoxNumberDocAttrib;
				boxNumberAttrib4.D0_AttribValue = "123";
				var bondedIDAttrib4 = reqDoc4.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID);
				bondedIDAttrib4.D0_AttribValue = "abc";
				attrib4.Validation.ValidateD0_AttribValue();
				AssertHasErrorContaining(attrib4.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);

				attrib4.D0_AttribValue = TaiwanCustomsDistrictList.Codes.B;
				bondedIDAttrib4.D0_AttribValue = "def";
				attrib4.Validation.ValidateD0_AttribValue();
				AssertNoErrorContaining(attrib4.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidPowerOfAttorneyNumber);

				var customsDistrictDocAttrib = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
				customsDistrictDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
				var boxNumberDocAttrib = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
				boxNumberDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;

				var jobRequiredDocumentForTestBoxNumber = Factory.New<JobRequiredDocumentForTestBoxNumber>();
				jobRequiredDocumentForTestBoxNumber.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				jobRequiredDocumentForTestBoxNumber.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				jobRequiredDocumentForTestBoxNumber.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				jobRequiredDocumentForTestBoxNumber.Attributes.DeleteAll();
				customsDistrictDocAttrib.D0_EQ = jobRequiredDocumentForTestBoxNumber.PK;
				boxNumberDocAttrib.D0_EQ = jobRequiredDocumentForTestBoxNumber.PK;

				customsDistrictDocAttrib.D0_AttribValue = "A";

				boxNumberDocAttrib.D0_AttribValue = ZString.Empty;
				AssertHasErrorContaining(boxNumberDocAttrib.D0_AttribValueInfo, JobRequiredDocAttribValidation.GetErrorForTaiwanAttorneyAttributeValueMissing(JobRequiredDocAttribTypeList.Codes.BoxNumber));

				boxNumberDocAttrib.D0_AttribValue = "123";
				AssertNoErrorContaining(boxNumberDocAttrib.D0_AttribValueInfo, JobRequiredDocAttribValidation.GetErrorForTaiwanAttorneyAttributeValueMissing(JobRequiredDocAttribTypeList.Codes.BoxNumber));
				AssertHasMessageErrorContaining(boxNumberDocAttrib.D0_AttribValueInfo, ListValidation.InvalidCodeMessageError);
				AssertNoErrorContaining(boxNumberDocAttrib.D0_AttribValueInfo, "Box Number must consist of exactly three alphanumeric characters.");

				boxNumberDocAttrib.D0_AttribValue = "111";
				AssertNoMessageErrorContaining(boxNumberDocAttrib.D0_AttribValueInfo, ListValidation.InvalidCodeMessageError);

				customsDistrictDocAttrib.D0_AttribValue = "B";
				boxNumberDocAttrib.D0_AttribValue = "111";
				AssertHasMessageErrorContaining(boxNumberDocAttrib.D0_AttribValueInfo, ListValidation.InvalidCodeMessageError);

				boxNumberDocAttrib.D0_AttribValue = "6666";
				AssertHasErrorContaining(boxNumberDocAttrib.D0_AttribValueInfo, "Box Number must consist of exactly three alphanumeric characters.");
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var reqDoc = Factory.New<JobRequiredDocument>();
				var attrib1 = reqDoc.Attributes.AddNew();
				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;

				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
				attrib1.D0_AttribValue = "1234567";
				AssertHasErrorContaining(attrib1.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidTradePreferenceCodeInUnitedStates);
				attrib1.D0_AttribValue = "US";
				AssertHasErrorContaining(attrib1.D0_AttribValueInfo, JobRequiredDocAttribValidation.InvalidTradePreferenceCodeInUnitedStates);
				attrib1.D0_AttribValue = "S";
				AssertNoErrors(attrib1.D0_AttribValueInfo);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
				helper.CreatePreferenceForCountry("10", "10", CountryCodes.Canada);

				var reqDoc = Factory.New<JobRequiredDocument>();
				var attrib1 = reqDoc.Attributes.AddNew();
				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;

				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
				attrib1.D0_AttribValue = "1234567";

				AssertHasErrorContaining(attrib1.D0_AttribValueInfo, ListValidation.InvalidCodeMessageError.ToString());
				attrib1.D0_AttribValue = "CA";
				AssertHasErrorContaining(attrib1.D0_AttribValueInfo, ListValidation.InvalidCodeMessageError.ToString());
				attrib1.D0_AttribValue = "10";
				AssertNoErrors(attrib1.D0_AttribValueInfo);
			}
		}

		public void TestCheckD0_AttribName()
		{
			JobRequiredDocument reqDoc = Factory.New<JobRequiredDocument>();
			var attrib1 = reqDoc.Attributes.AddNew();
			attrib1.D0_AttribName = "Z!Z";
			AssertHasErrorContaining(attrib1.D0_AttribNameInfo, ListValidation.InvalidCodeError);
			attrib1.D0_AttribName = ZString.Empty;
			AssertNoErrorContaining(attrib1.D0_AttribNameInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(attrib1.D0_AttribNameInfo, MandatoryValidation.MustBeEntered);
			foreach (CodeDescriptionPair pair in new JobRequiredDocAttribTypeList())
			{
				attrib1.D0_AttribName = pair.Code;
				AssertNoErrorContaining(attrib1.D0_AttribNameInfo, ListValidation.InvalidCodeError);
				AssertNoErrorContaining(attrib1.D0_AttribNameInfo, MandatoryValidation.MustBeEntered);
			}

			using (WorkflowDataRegistry.Instance.EnableWorkflowValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WorkflowValidationProcessTypeCollection { new WorkflowValidationProcessType { ProcessType = "BRK" } }))
			{
				var attrib6 = reqDoc.Attributes.AddNew();
				attrib6.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1;

				attrib6.D0_AttribName = "Custom1:1234567890";
				AssertNoError(attrib6.D0_AttribNameInfo, ListValidation.InvalidCodeError);

				attrib6.D0_AttribName = "Custom11:1234567890";
				AssertHasErrorContaining(attrib6.D0_AttribNameInfo, ListValidation.InvalidCodeError);
			}

			string portOfEntryWarning = JobRequiredDocAttribValidation.AttributeIsOnlyApplicableToPowerOfAttorney(JobRequiredDocAttribTypeList.Codes.PortOfEntry);
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertHasWarning(attrib1.D0_AttribNameInfo, portOfEntryWarning);
			string directionWarning = JobRequiredDocAttribValidation.AttributeIsOnlyApplicableToPowerOfAttorney(JobRequiredDocAttribTypeList.Codes.Direction);
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			AssertHasWarning(attrib1.D0_AttribNameInfo, directionWarning);

			foreach (string docType in new string[] { Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding })
			{
				reqDoc.EQ_DocType = docType;
				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
				AssertNoWarning(attrib1.D0_AttribNameInfo, portOfEntryWarning);
				attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
				AssertNoWarning(attrib1.D0_AttribNameInfo, directionWarning);
			}

			var attrib2 = reqDoc.Attributes.AddNew();
			attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			attrib2.D0_AttribValue = ImportExportCodeList.Codes.Export;
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertHasWarning(attrib1.D0_AttribNameInfo, JobRequiredDocAttribValidation.PortOfEntryNotApplicableForExport);
			attrib1.D0_AttribName = "ZZZ";
			AssertNoWarning(attrib1.D0_AttribNameInfo, JobRequiredDocAttribValidation.PortOfEntryNotApplicableForExport);
			attrib2.D0_AttribValue = ImportExportCodeList.Codes.Import;
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertNoWarning(attrib1.D0_AttribNameInfo, JobRequiredDocAttribValidation.PortOfEntryNotApplicableForExport);
			attrib2.D0_AttribName = "ZZZ";
			attrib2.D0_AttribValue = ImportExportCodeList.Codes.Export;
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertNoWarning(attrib1.D0_AttribNameInfo, JobRequiredDocAttribValidation.PortOfEntryNotApplicableForExport);
			AssertNoError(attrib2.D0_AttribNameInfo, JobRequiredDocAttribValidation.OnlyOneAttributeTypeIsAllowedPerRecord(JobRequiredDocAttribTypeList.Codes.PortOfEntry));
			attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertHasError(attrib2.D0_AttribNameInfo, JobRequiredDocAttribValidation.OnlyOneAttributeTypeIsAllowedPerRecord(JobRequiredDocAttribTypeList.Codes.PortOfEntry));
			attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			AssertNoError(attrib2.D0_AttribNameInfo, JobRequiredDocAttribValidation.OnlyOneAttributeTypeIsAllowedPerRecord(JobRequiredDocAttribTypeList.Codes.PortOfEntry));

			var attribNames = new string[] { JobRequiredDocAttribTypeList.Codes.SellerControlNumber, JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate, JobRequiredDocAttribTypeList.Codes.BuyerIssueDate, JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference };
			var attrib3 = reqDoc.Attributes.AddNew();
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.Invoice;
			reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			foreach (string attribName in attribNames)
			{
				attrib3.D0_AttribName = attribName;
				AssertHasErrorContaining(attrib3.D0_AttribNameInfo, JobRequiredDocAttribValidation.CurrentLoginCompanyDoesNotSupportDeclarationOfIntent(attribName));
				AssertHasErrorContaining(attrib3.D0_AttribNameInfo, JobRequiredDocAttribValidation.AttributeIsOnlyApplicableToVATExporterExemption(attribName));
				AssertHasErrorContaining(attrib3.D0_AttribNameInfo, JobRequiredDocAttribValidation.DocumentRelatedCountryDoesNotSupportDeclarationOfIntent(attribName));
				if (attribName == JobRequiredDocAttribTypeList.Codes.SellerControlNumber)
				{
					AssertHasErrorContaining(attrib3.D0_AttribNameInfo, JobRequiredDocAttribValidation.SellerControlNumberIsOnlyApplicableToEXVDebtor);
				}
			}

			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			attrib3.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			AssertHasErrorContaining(attrib3.D0_AttribNameInfo, JobRequiredDocAttribValidation.CurrentLoginCompanyDoesNotSupportDeclarationOfIntent(JobRequiredDocAttribTypeList.Codes.CompanyCode));
			AssertHasErrorContaining(attrib3.D0_AttribNameInfo, JobRequiredDocAttribValidation.DocumentRelatedCountryDoesNotSupportDeclarationOfIntent(JobRequiredDocAttribTypeList.Codes.CompanyCode));
			AssertNoError(attrib3.D0_AttribNameInfo, JobRequiredDocAttribValidation.CompanyCodeAttributeDoesNotSupportDocumentType(new ZString[] { "EXV", "POA", "POC", "POF" }));

			reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Italy;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				foreach (string attribName in attribNames)
				{
					attrib3.D0_AttribName = attribName;
					AssertNoErrors(attrib3.D0_AttribNameInfo);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AssertWarningForTaiwanAttorneyAttributeUsedByNonApplicableSettings(reqDoc, JobRequiredDocAttribTypeList.Codes.CustomsDistrict);
				AssertWarningForTaiwanAttorneyAttributeUsedByNonApplicableSettings(reqDoc, JobRequiredDocAttribTypeList.Codes.BondedID);
			}

			var attrib4 = reqDoc.Attributes.AddNew();
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			AssertNoError(attrib4.D0_AttribNameInfo, JobRequiredDocAttribValidation.CompanyCodeAttributeDoesNotSupportDocumentType(new ZString[] { "EXV", "POA", "POC", "POF" }));
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			AssertNoError(attrib4.D0_AttribNameInfo, JobRequiredDocAttribValidation.CompanyCodeAttributeDoesNotSupportDocumentType(new ZString[] { "EXV", "POA", "POC", "POF" }));
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.Invoice;
			attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			AssertHasErrorContaining(attrib4.D0_AttribNameInfo, JobRequiredDocAttribValidation.CompanyCodeAttributeDoesNotSupportDocumentType(new ZString[] { "EXV", "POA", "POC", "POF" }));

			var attrib5 = reqDoc.Attributes.AddNew();
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			attrib5.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			string errorDocType = JobRequiredDocAttribValidation.TradePreferenceCodeAttributeDoesNotSupportDocumentType(new ZString[] { "COO" }, new ZString[] { "US", "CA" });
			AssertHasErrorContaining(attrib5.D0_AttribNameInfo, errorDocType);
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Singapore;
			attrib5.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			string errorRelatedCountry = JobRequiredDocAttribValidation.TradePreferenceCodeAttributeDoesNotSupportDocumentType(new ZString[] { "COO" }, new ZString[] { "US", "CA" });
			AssertHasErrorContaining(attrib5.D0_AttribNameInfo, errorRelatedCountry);
			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
			attrib5.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			AssertNoError(attrib5.D0_AttribNameInfo, JobRequiredDocAttribValidation.TradePreferenceCodeAttributeDoesNotSupportDocumentType(new ZString[] { "COO" }, new ZString[] { "US", "CA" }));
			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;
			attrib5.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			AssertNoError(attrib5.D0_AttribNameInfo, JobRequiredDocAttribValidation.TradePreferenceCodeAttributeDoesNotSupportDocumentType(new ZString[] { "COO" }, new ZString[] { "US", "CA" }));
		}

		void AssertWarningForTaiwanAttorneyAttributeUsedByNonApplicableSettings(JobRequiredDocument reqDoc, string attributeName)
		{
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Italy;

			var warningMessage = JobRequiredDocAttribValidation.GetWarningForTaiwanAttorneyAttributeUsedByNonApplicableSettings(attributeName);
			var attrib = reqDoc.Attributes.AddNew();
			attrib.D0_AttribName = attributeName;
			AssertHasWarning(attrib.D0_AttribNameInfo, warningMessage);

			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			attrib.D0_AttribName = attributeName;
			AssertHasWarning(attrib.D0_AttribNameInfo, warningMessage);

			reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			attrib.D0_AttribName = attributeName;
			AssertHasWarning(attrib.D0_AttribNameInfo, warningMessage);

			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			attrib.D0_AttribName = attributeName;
			AssertNoWarning(attrib.D0_AttribNameInfo, warningMessage);

			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PreAlert;
			attrib.D0_AttribName = attributeName;
			AssertHasWarning(attrib.D0_AttribNameInfo, warningMessage);

			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			attrib.D0_AttribName = attributeName;
			AssertNoWarning(attrib.D0_AttribNameInfo, warningMessage);

			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.QuarantineCertificate;
			attrib.D0_AttribName = attributeName;
			AssertHasWarning(attrib.D0_AttribNameInfo, warningMessage);

			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			attrib.D0_AttribName = attributeName;
			AssertHasWarning(attrib.D0_AttribNameInfo, warningMessage);
		}

		public void TestRowErrorForCostaRicaSpecificAttributes()
		{
			var reqDoc = Factory.New<JobRequiredDocument>();

			var attrib1 = reqDoc.Attributes.AddNew();
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;

			var errorMessageExvDocumentType = @"Attribute: 'COSTA RICA EXV DOCUMENT TYPE' is required for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'";
			var errorMessageIssuingAuthorityName = @"Attribute: 'ISSUING AUTHORITY NAME' is required for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'";

			var errorMessages = new string[] { errorMessageExvDocumentType, errorMessageIssuingAuthorityName };
			TestRowErrorForCostaRicaEXVAttributes();

			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType;
			attrib1.D0_AttribValue = CostaRicaEXVDocumentTypeList.Codes.ComprasAutorizadas;

			errorMessages = new string[] { errorMessageIssuingAuthorityName };
			TestRowErrorForCostaRicaEXVAttributes();

			var attrib2 = reqDoc.Attributes.AddNew();
			attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName;
			attrib2.D0_AttribValue = "bla bla";

			errorMessages = Array.Empty<string>();
			TestRowErrorForCostaRicaEXVAttributes();

			reqDoc.Attributes.Delete(attrib2);
			attrib1.RunPreSaveValidation();
			Assert("Row validation should be added", reqDoc.RowErrors.Contains(errorMessageIssuingAuthorityName));

			void TestRowErrorForCostaRicaEXVAttributes()
			{
				reqDoc.ClearRowNotifications();

				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;

				attrib1.RunPreSaveValidation();
				foreach (var errorMessage in errorMessages)
				{
					AssertNoRowError(reqDoc, errorMessage);
				}

				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.CostaRica;
				attrib1.RunPreSaveValidation();
				AssertEquals(errorMessages.Length, reqDoc.RowErrors.Count());
				foreach (var errorMessage in errorMessages)
				{
					Assert("Row validation should be added for CR country/region", reqDoc.RowErrors.Contains(errorMessage));
				}

				reqDoc.ClearRowNotifications();

				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
				attrib1.RunPreSaveValidation();
				foreach (var errorMessage in errorMessages)
				{
					AssertNoRowError(reqDoc, errorMessage);
				}

				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				attrib1.RunPreSaveValidation();
				AssertEquals(errorMessages.Length, reqDoc.RowErrors.Count());
				foreach (var errorMessage in errorMessages)
				{
					Assert("Row validation should be added for DBT Usage", reqDoc.RowErrors.Contains(errorMessage));
				}

				reqDoc.ClearRowNotifications();

				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.DemandLetter;
				attrib1.RunPreSaveValidation();
				foreach (var errorMessage in errorMessages)
				{
					AssertNoRowError(reqDoc, errorMessage);
				}

				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				Assert("Precondition", reqDoc.EQ_DocUsage.IsEmpty);
				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				attrib1.RunPreSaveValidation();
				foreach (var errorMessage in errorMessages)
				{
					Assert("Row validation should be added for EXV Document type", reqDoc.RowErrors.Contains(errorMessage));
				}
			}
		}

		public void TestValidationCheckForCostaRicaSpecificAttributes()
		{
			var reqDoc = Factory.New<JobRequiredDocument>();

			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Brazil;
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;

			var attrib1 = reqDoc.Attributes.AddNew();
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType;

			TestValidationCheckForCostaRicaAttributes();

			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName;

			TestValidationCheckForCostaRicaAttributes();

			void TestValidationCheckForCostaRicaAttributes()
			{
				var errorMessage = string.Format(CultureInfo.InvariantCulture, "Value for Attribute: '{0}' is required for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'", attrib1.D0_AttribName);
				var warningMessage = string.Format(CultureInfo.InvariantCulture, "Attribute: '{0}' is only used for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'", attrib1.D0_AttribName);

				AssertNoError("Value not mandatory for non-CR country", attrib1.D0_AttribValueInfo, errorMessage);
				if (attrib1.IsCostaRicaEXVDocumentType)
				{
					AssertHasWarning(attrib1.D0_AttribNameInfo, warningMessage);
				}

				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.CostaRica;
				attrib1.RunPreSaveValidation();

				AssertHasError("Value for attribute is mandatory", attrib1.D0_AttribValueInfo, errorMessage);

				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
				attrib1.RunPreSaveValidation();

				AssertNoError("Value not mandatory for non-DBT usage", attrib1.D0_AttribValueInfo, errorMessage);
				if (attrib1.IsCostaRicaEXVDocumentType)
				{
					AssertHasWarning(attrib1.D0_AttribNameInfo, warningMessage);
				}

				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				attrib1.RunPreSaveValidation();

				AssertHasError("Value for attribute is mandatory", attrib1.D0_AttribValueInfo, errorMessage);

				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.DemandLetter;
				attrib1.RunPreSaveValidation();

				AssertNoError("Value not mandatory for non-EXV Doc type", attrib1.D0_AttribValueInfo, errorMessage);
				if (attrib1.IsCostaRicaEXVDocumentType)
				{
					AssertHasWarning(attrib1.D0_AttribNameInfo, warningMessage);
				}

				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				Assert("Precondition", reqDoc.EQ_DocUsage.IsEmpty);
				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				attrib1.RunPreSaveValidation();

				AssertHasError("Value for attribute is mandatory", attrib1.D0_AttribValueInfo, errorMessage);
			}
		}

		public void TestCountryRegionInCompanyCodeMatchesCountryRegionField()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var reqDoc = Factory.New<JobRequiredDocument>();
				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
				var attrib = reqDoc.Attributes.AddNew();
				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
				attrib.D0_AttribDisplayValue = GlbCompany.CurrentCompany.GC_Code;
				AssertHasError(attrib.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.CountryCodeOfCompanyDifferFromCountryRegionField);

				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;
				attrib.Validation.ValidateD0_AttribDisplayValue();
				AssertNoError(attrib.D0_AttribDisplayValueInfo, JobRequiredDocAttribValidation.CountryCodeOfCompanyDifferFromCountryRegionField);
			}
		}

		public void TestGetWarningForTaiwanPOAAttributeUsedByNonApplicableSettings()
		{
			AssertEquals("Attribute: 'XX' is only used for Country/Region: 'Taiwan', Document Type in (POA,POC) and Usage: 'BRK'", JobRequiredDocAttribValidation.GetWarningForTaiwanAttorneyAttributeUsedByNonApplicableSettings("XX"));
		}

		public void TestErrorForTaiwanPOAAttributeValueMissing()
		{
			AssertEquals("Value for Attribute: 'CUSTOMS DISTRICT' is required.", JobRequiredDocAttribValidation.GetErrorForTaiwanAttorneyAttributeValueMissing(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
		}
	}
}
