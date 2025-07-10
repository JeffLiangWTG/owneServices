using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSAPermitAndLicenses))]
	public class NHTSAPermitAndLicensesTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NHTSAPermitAndLicenses>
	{
		public void TestUS_NHTLPCOType()
		{
			PermitAndLicenses.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			AssertEquals(NHTSALPCOTypeList.Codes.NH0, Details.US_NHTLPCOType);

			PermitAndLicenses.US_NHTLPCOType = ZString.Empty;
			AssertEquals(ZString.Empty, Details.US_NHTLPCOType);
		}

		public void TestUS_NHTLPCONumber()
		{
			PermitAndLicenses.US_NHTLPCONumber = "A-23-456";
			AssertEquals("A-23-456", Details.US_NHTLPCONumber);

			PermitAndLicenses.US_NHTLPCONumber = ZString.Empty;
			AssertEquals(ZString.Empty, Details.US_NHTLPCONumber);
		}

		public void TestUS_NHTLPCODateType()
		{
			PermitAndLicenses.US_NHTLPCODateType = LPCODateQualifierList.Codes.EffectiveDate;
			AssertEquals(LPCODateQualifierList.Codes.EffectiveDate, Details.US_NHTLPCODateType);

			PermitAndLicenses.US_NHTLPCODateType = ZString.Empty;
			AssertEquals(ZString.Empty, Details.US_NHTLPCODateType);
		}

		[TestDate(2015, 06, 09)]
		public void TestUS_NHTLPCODate()
		{
			PermitAndLicenses.US_NHTLPCODate = ZDateTime.Today;
			AssertEquals(new DateTime(2015, 06, 09), Details.US_NHTLPCODate);

			PermitAndLicenses.US_NHTLPCODate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, Details.US_NHTLPCODate);
		}

		public void TestUS_NHTLPCOQuantity()
		{
			PermitAndLicenses.US_NHTLPCOQuantity = 100m;
			AssertEquals(100m, Details.US_NHTLPCOQuantity);

			PermitAndLicenses.US_NHTLPCOQuantity = 0m;
			AssertEquals(0m, Details.US_NHTLPCOQuantity);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return PermitAndLicenses;
		}

		protected override IEnumerable<NHTSAPermitAndLicenses> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var header = invoiceLine.NHTSALines.AddNew();
			var details = header.NHTSADetails.AddNew();
			yield return details.PermitAndLicenses.AddNew();
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
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					var header = invoiceLine.NHTSALines.AddNew();
					details = header.NHTSADetails.AddNew();
				}
				return details;
			}
		}
		NHTSADetails details;

		NHTSAPermitAndLicenses PermitAndLicenses
		{
			get { return permitAndLicenses ?? (permitAndLicenses = Details.PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().FirstOrDefault() ?? Details.PermitAndLicenses.AddNew()); }
		}
		NHTSAPermitAndLicenses permitAndLicenses;
	}
}
