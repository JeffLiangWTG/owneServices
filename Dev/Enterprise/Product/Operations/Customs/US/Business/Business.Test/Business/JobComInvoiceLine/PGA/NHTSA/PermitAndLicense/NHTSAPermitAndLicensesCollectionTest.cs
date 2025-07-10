using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSAPermitAndLicensesCollection))]
	public class NHTSAPermitAndLicensesCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddAndDeleteLPCOCollection()
		{
			PermitAndLicense.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			permitAndLicense.US_NHTLPCONumber = "A-12-345";
			AssertEquals(NHTSALPCOTypeList.Codes.NH0, DetailsLine.US_NHTLPCOType);
			AssertEquals("A-12-345", DetailsLine.US_NHTLPCONumber);

			var newPermitAndLicense = DetailsLine.PermitAndLicenses.AddNew();
			newPermitAndLicense.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH3;
			newPermitAndLicense.US_NHTLPCONumber = "VSA-039";
			AssertEquals(NHTSALPCOTypeList.Codes.NH0, DetailsLine.US_NHTLPCOType);
			AssertEquals("A-12-345", DetailsLine.US_NHTLPCONumber);

			DetailsLine.PermitAndLicenses.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, DetailsLine.US_NHTLPCOType);
			AssertEquals(ZString.Empty, DetailsLine.US_NHTLPCONumber);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return DetailsLine.PermitAndLicenses;
		}

		NHTSADetails DetailsLine
		{
			get
			{
				if (detailsLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var header = invoiceLine.NHTSALines.AddNew();
					detailsLine = header.NHTSADetails.AddNew();
					Factory.Save();
				}
				return detailsLine;
			}
		}
		NHTSADetails detailsLine;

		NHTSAPermitAndLicenses PermitAndLicense
		{
			get { return permitAndLicense ?? (permitAndLicense = DetailsLine.PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().FirstOrDefault() ?? DetailsLine.PermitAndLicenses.AddNew()); }
		}
		NHTSAPermitAndLicenses permitAndLicense;

		#endregion
	}
}
