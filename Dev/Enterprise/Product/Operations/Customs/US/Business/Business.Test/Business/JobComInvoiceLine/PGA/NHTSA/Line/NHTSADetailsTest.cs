using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSADetails))]
	public class NHTSADetailsTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NHTSADetails>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<NHTSADetails>();
			originalBO.AdditionalNumbers.AddNew();
			originalBO.PermitAndLicenses.AddNew();

			var newBO = (NHTSADetails)originalBO.Clone();

			AssertEquals(1, newBO.AdditionalNumbers.Count);
			AssertEquals(1, newBO.PermitAndLicenses.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (NHTSADetails)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(NHTSADetails), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.AdditionalNumbers[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.PermitAndLicenses[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.AdditionalNumbers[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.PermitAndLicenses[0].Factory.GetHashCode());
		}

		public void TestUS_NHTIdentityNumber()
		{
			AssertEquals(ZString.Empty, Details.US_NHTIdentityNumber);

			var additionalNumber = Details.FirstAdditionalNumber ?? Details.AdditionalNumbers.AddNew();
			additionalNumber.US_NHTAdditionalIdentityNumber = "123456789";
			AssertEquals("123456789", Details.US_NHTIdentityNumber);

			additionalNumber.US_NHTAdditionalIdentityNumber = "2345678";
			AssertEquals("2345678", Details.US_NHTIdentityNumber);

			Details.AdditionalNumbers.AddNew().US_NHTAdditionalIdentityNumber = "3456789";
			AssertEquals("2345678", Details.US_NHTIdentityNumber);

			Details.US_NHTIdentityNumber = "87654321";
			AssertEquals("87654321", additionalNumber.US_NHTAdditionalIdentityNumber);
		}

		public void TestUS_NHTIdentityNumQualifier()
		{
			AssertEquals(ZString.Empty, Details.US_NHTIdentityNumQualifier);

			var additionalNumber = Details.FirstAdditionalNumber ?? Details.AdditionalNumbers.AddNew();
			additionalNumber.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.ModelNumber;
			AssertEquals(ItemIdentityNumberQualifierList.Codes.ModelNumber, Details.US_NHTIdentityNumQualifier);

			additionalNumber.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			AssertEquals(ItemIdentityNumberQualifierList.Codes.EngineNumber, Details.US_NHTIdentityNumQualifier);

			Details.AdditionalNumbers.AddNew().US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertEquals(ItemIdentityNumberQualifierList.Codes.EngineNumber, Details.US_NHTIdentityNumQualifier);

			Details.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, additionalNumber.US_NHTAdditionalIdentityNumQualifier);
		}

		[TestDate(2015, 06, 11)]
		public void TestUS_NHTLPCODate()
		{
			AssertEquals(ZDateTime.Empty, Details.US_NHTLPCODate);

			var permitAndLicense = Details.FirstNHTSAPermitAndLicenses ?? Details.PermitAndLicenses.AddNew();
			permitAndLicense.US_NHTLPCODate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, Details.US_NHTLPCODate);

			permitAndLicense.US_NHTLPCODate = ZDateTime.Today.AddDays(5);
			AssertEquals(ZDateTime.Today.AddDays(5), Details.US_NHTLPCODate);

			Details.PermitAndLicenses.AddNew().US_NHTLPCODate = ZDateTime.Today.AddDays(-5);
			AssertEquals(ZDateTime.Today.AddDays(5), Details.US_NHTLPCODate);

			Details.US_NHTLPCODate = ZDateTime.Today.AddDays(-1);
			AssertEquals(ZDateTime.Today.AddDays(-1), permitAndLicense.US_NHTLPCODate);
		}

		public void TestUS_NHTLPCODateType()
		{
			AssertEquals(ZString.Empty, Details.US_NHTLPCODateType);

			var permitAndLicense = Details.FirstNHTSAPermitAndLicenses ?? Details.PermitAndLicenses.AddNew();
			permitAndLicense.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateApplicationReceived;
			AssertEquals(LPCODateQualifierList.Codes.DateApplicationReceived, Details.US_NHTLPCODateType);

			permitAndLicense.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			AssertEquals(LPCODateQualifierList.Codes.DateIssuedOrSigned, Details.US_NHTLPCODateType);

			Details.PermitAndLicenses.AddNew().US_NHTLPCODateType = LPCODateQualifierList.Codes.EffectiveDate;
			AssertEquals(LPCODateQualifierList.Codes.DateIssuedOrSigned, Details.US_NHTLPCODateType);

			Details.US_NHTLPCODateType = LPCODateQualifierList.Codes.ExpirationDate;
			AssertEquals(LPCODateQualifierList.Codes.ExpirationDate, permitAndLicense.US_NHTLPCODateType);
		}

		public void TestUS_NHTLPCONumber()
		{
			AssertEquals(ZString.Empty, Details.US_NHTLPCONumber);

			var permitAndLicense = Details.FirstNHTSAPermitAndLicenses ?? Details.PermitAndLicenses.AddNew();
			permitAndLicense.US_NHTLPCONumber = "123456";
			AssertEquals("123456", Details.US_NHTLPCONumber);

			permitAndLicense.US_NHTLPCONumber = "234567";
			AssertEquals("234567", Details.US_NHTLPCONumber);

			Details.PermitAndLicenses.AddNew().US_NHTLPCONumber = "3456789";
			AssertEquals("234567", Details.US_NHTLPCONumber);

			Details.US_NHTLPCONumber = "123456789";
			AssertEquals("123456789", permitAndLicense.US_NHTLPCONumber);
		}

		public void TestUS_NHTLPCOQuantity()
		{
			AssertEquals(ZDecimal.Zero, Details.US_NHTLPCOQuantity);

			var permitAndLicense = Details.FirstNHTSAPermitAndLicenses ?? Details.PermitAndLicenses.AddNew();
			permitAndLicense.US_NHTLPCOQuantity = 123456m;
			AssertEquals(123456m, Details.US_NHTLPCOQuantity);

			permitAndLicense.US_NHTLPCOQuantity = 234567m;
			AssertEquals(234567m, Details.US_NHTLPCOQuantity);

			Details.PermitAndLicenses.AddNew().US_NHTLPCOQuantity = 3456789m;
			AssertEquals(234567m, Details.US_NHTLPCOQuantity);

			Details.US_NHTLPCOQuantity = 12345678m;
			AssertEquals(12345678m, permitAndLicense.US_NHTLPCOQuantity);
		}

		public void TestUS_NHTLPCOType()
		{
			AssertEquals(ZString.Empty, Details.US_NHTLPCOType);

			var permitAndLicense = Details.FirstNHTSAPermitAndLicenses ?? Details.PermitAndLicenses.AddNew();
			permitAndLicense.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			AssertEquals(NHTSALPCOTypeList.Codes.NH0, Details.US_NHTLPCOType);

			permitAndLicense.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH2;
			AssertEquals(NHTSALPCOTypeList.Codes.NH2, Details.US_NHTLPCOType);

			Details.PermitAndLicenses.AddNew().US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH3;
			AssertEquals(NHTSALPCOTypeList.Codes.NH2, Details.US_NHTLPCOType);

			Details.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			AssertEquals(NHTSALPCOTypeList.Codes.NH0, permitAndLicense.US_NHTLPCOType);
		}

		public void TestHumanReadableNames()
		{
			AssertEquals("Number Type", Details.US_NHTIdentityNumQualifierInfo.HumanReadableName);
			AssertEquals("Number", Details.US_NHTIdentityNumberInfo.HumanReadableName);
			AssertEquals("LPCO Type", Details.US_NHTLPCOTypeInfo.HumanReadableName);
			AssertEquals("LPCO Number", Details.US_NHTLPCONumberInfo.HumanReadableName);
			AssertEquals("LPCO Date Type", Details.US_NHTLPCODateTypeInfo.HumanReadableName);
			AssertEquals("LPCO Date", Details.US_NHTLPCODateInfo.HumanReadableName);
			AssertEquals("LPCO Quantity", Details.US_NHTLPCOQuantityInfo.HumanReadableName);
		}

		public void TestCloneNHTSADetails()
		{
			Details.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			Details.US_NHTIdentityNumber = "10325476981";

			var newNumber = Details.AdditionalNumbers.AddNew();
			newNumber.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.ModelNumber;
			newNumber.US_NHTAdditionalIdentityNumber = "123456";

			var clonedDetails = (NHTSADetails)Details.Clone();
			AssertEquals(2, clonedDetails.AdditionalNumbers.Count);
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, clonedDetails.AdditionalNumbers[0].US_NHTAdditionalIdentityNumQualifier);
			AssertEquals("10325476981", clonedDetails.AdditionalNumbers[0].US_NHTAdditionalIdentityNumber);
			AssertEquals(ItemIdentityNumberQualifierList.Codes.ModelNumber, clonedDetails.AdditionalNumbers[1].US_NHTAdditionalIdentityNumQualifier);
			AssertEquals("123456", clonedDetails.AdditionalNumbers[1].US_NHTAdditionalIdentityNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Details;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Details;
		}

		protected override IEnumerable<NHTSADetails> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var header = invoiceLine.NHTSALines.AddNew();
			yield return header.NHTSADetails.AddNew();
		}

		NHTSADetails Details
		{
			get
			{
				if (details == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var header = invoiceLine.NHTSALines.AddNew();
					details = header.NHTSADetails.AddNew();
				}
				return details;
			}
		}

		NHTSADetails details;
	}
}
