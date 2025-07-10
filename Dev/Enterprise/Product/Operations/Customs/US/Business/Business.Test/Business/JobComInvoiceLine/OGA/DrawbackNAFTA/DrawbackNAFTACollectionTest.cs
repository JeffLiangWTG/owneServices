using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackNAFTACollection))]
	public class DrawbackNAFTACollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCountryOfExport()
		{
			InvoiceLine.Declaration.US_NAFTADrawbackCountry = Core.Constants.CountryCodes.Mexico;
			var drawbackNAFTA = InvoiceLine.DrawbackNAFTAs.AddNew();
			AssertEquals(ZString.Empty, drawbackNAFTA.US_DRWNAFTACountryOfExport);

			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawbackNAFTA = InvoiceLine.DrawbackNAFTAs.AddNew();
			AssertEquals(Core.Constants.CountryCodes.Mexico, drawbackNAFTA.US_DRWNAFTACountryOfExport);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return DrawbackNAFTAs;
		}

		DrawbackNAFTACollection DrawbackNAFTAs
		{
			get { return drawbackNAFTAs ?? (drawbackNAFTAs = InvoiceLine.DrawbackNAFTAs); }
		}
		DrawbackNAFTACollection drawbackNAFTAs;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
