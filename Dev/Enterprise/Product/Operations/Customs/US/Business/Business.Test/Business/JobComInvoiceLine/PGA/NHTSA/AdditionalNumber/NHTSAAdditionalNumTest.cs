using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSAAdditionalNum))]
	public class NHTSAAdditionalNumTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NHTSAAdditionalNum>
	{
		public void TestUS_NHTAdditionalIdentityNumQualifier()
		{
			AdditionalNum.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, Details.US_NHTIdentityNumQualifier);

			AdditionalNum.US_NHTAdditionalIdentityNumQualifier = ZString.Empty;
			AssertEquals(ZString.Empty, Details.US_NHTIdentityNumQualifier);
		}

		public void TestUS_NHTAdditionalIdentityNumber()
		{
			AdditionalNum.US_NHTAdditionalIdentityNumber = "SALLDHMV2AA100000";
			AssertEquals("SALLDHMV2AA100000", Details.US_NHTIdentityNumber);

			AdditionalNum.US_NHTAdditionalIdentityNumber = ZString.Empty;
			AssertEquals(ZString.Empty, Details.US_NHTIdentityNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AdditionalNum;
		}

		protected override IEnumerable<NHTSAAdditionalNum> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var header = invoiceLine.NHTSALines.AddNew();
			var details = header.NHTSADetails.AddNew();
			yield return details.AdditionalNumbers.AddNew();
		}

		NHTSADetails Details
		{
			get
			{
				if (details == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var header = invoiceLine.NHTSALines.AddNew();
					details = header.NHTSADetails.AddNew();
				}
				return details;
			}
		}
		NHTSADetails details;

		NHTSAAdditionalNum AdditionalNum
		{
			get { return additionalNum ?? (additionalNum = Details.AdditionalNumbers.OfType<NHTSAAdditionalNum>().FirstOrDefault() ?? Details.AdditionalNumbers.AddNew()); }
		}
		NHTSAAdditionalNum additionalNum;
	}
}
