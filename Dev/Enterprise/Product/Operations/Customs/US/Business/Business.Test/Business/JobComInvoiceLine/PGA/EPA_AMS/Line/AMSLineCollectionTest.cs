using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSLineCollection))]
	public class AMSLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AMSLineCollection(AMSHeader);
		}

		public void TestSetDefaultsForNewChildForMO1()
		{
			AssertSetDefaultsForNewChildFromInvoiceLineToAmsLine(AMSProgramList.Codes.MO1, (consigneeAddress) => consigneeAddress, 150m, "KG");
		}

		public void TestSetDefaultsForNewChildForMO5()
		{
			AssertSetDefaultsForNewChildFromInvoiceLineToAmsLine(AMSProgramList.Codes.MO5, (consigneeAddress) => consigneeAddress, 150m, "KG");
		}

		public void TestSetDefaultsForNewChildForPN1()
		{
			AssertSetDefaultsForNewChildFromInvoiceLineToAmsLine(AMSProgramList.Codes.PN1, (consigneeAddress) => consigneeAddress, 150m, "KG");
		}

		public void TestSetDefaultsForNewChildForMO2()
		{
			AssertSetDefaultsForNewChildFromInvoiceLineToAmsLine(AMSProgramList.Codes.MO2, _ => ZGuid.Empty, ZDecimal.Zero, ZString.Empty);

			AMSHeader.US_Program = AMSProgramList.Codes.MO2;
			var invoiceline = AMSHeader.InvoiceLine;

			invoiceline.JI_CustomsUnitQty = "L";
			invoiceline.JI_CustomsQuantity = 100m;
			invoiceline.JI_NetWeight = 120m;
			invoiceline.JI_NetWeightUQ = "KG";

			var amsLine2 = AMSHeader.AMSLines.AddNew();
			AssertEquals(120m, amsLine2.US_NetWeight);
			AssertEquals("KG", amsLine2.US_NetWeightUQ);
			AssertEquals(LPCOTypeList.Codes.AM6, amsLine2.US_CertType);
		}

		void AssertSetDefaultsForNewChildFromInvoiceLineToAmsLine(ZString programCode, Func<ZGuid, ZGuid> getExpectedAddressFromConsignee, ZDecimal expectedPackages, ZString expectedPackagesUQ)
		{
			var datetimeToSet = new ZDateTime(2016, 11, 07);
			AMSHeader.Declaration.US_InspecDate = datetimeToSet;
			AMSHeader.US_Program = programCode;
			var invoiceline = AMSHeader.InvoiceLine;
			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			invoiceline.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceline.JI_InvoiceQuantity = 150m;
			invoiceline.JI_InvoiceUQ = "KG";

			var amsLine = AMSHeader.AMSLines.AddNew();
			AssertEquals(datetimeToSet, amsLine.US_InspecDateTime);
			AssertEquals(getExpectedAddressFromConsignee(ultimateConsignee.MainAddress.PK), amsLine.US_OA_Applicant);
			AssertEquals(expectedPackages, amsLine.US_Packages);
			AssertEquals(expectedPackagesUQ, amsLine.US_PackagesUQ);

			invoiceline.JI_CustomsUnitQty = "L";
			invoiceline.JI_CustomsQuantity = 100m;
			invoiceline.JI_NetWeight = 120m;
			invoiceline.JI_NetWeightUQ = "KG";

			var amsLine2 = AMSHeader.AMSLines.AddNew();
			AssertEquals(120m, amsLine2.US_NetWeight);
			AssertEquals("KG", amsLine2.US_NetWeightUQ);

			invoiceline.JI_CustomsUnitQty = "KG";
			var amsLine3 = AMSHeader.AMSLines.AddNew();
			AssertEquals(120m, amsLine3.US_NetWeight);
			AssertEquals("KG", amsLine3.US_NetWeightUQ);
		}

		public void TestSetDefaultsForNewChildForMO4()
		{
			AMSHeader.US_Program = AMSProgramList.Codes.MO4;
			var invoiceline = AMSHeader.InvoiceLine;
			invoiceline.JI_CustomsQuantity = 150m;
			invoiceline.JI_CustomsUnitQty = "KG";

			invoiceline.JI_CustomsUnitQty = "L";
			invoiceline.JI_CustomsQuantity = 100m;
			invoiceline.JI_NetWeight = 120m;
			invoiceline.JI_NetWeightUQ = "KG";

			var amsLine2 = AMSHeader.AMSLines.AddNew();
			AssertEquals(120m, amsLine2.US_NetWeight);
			AssertEquals("KG", amsLine2.US_NetWeightUQ);
		}

		AMS AMSHeader
		{
			get
			{
				if (amsHeader == null)
				{
					var fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					var invoiceLine = fDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
					amsHeader = invoiceLine.AMSLines.AddNew();
				}
				return amsHeader;
			}
		}
		AMS amsHeader;
	}
}
