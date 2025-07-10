using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSADetailsCollection))]
	public class NHTSADetailsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultNumberType()
		{
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			var line = Header.NHTSADetails.AddNew();
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, line.US_NHTIdentityNumQualifier);

			Header.NHTSADetails.RemoveAndDeleteAll();
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			line = Header.NHTSADetails.AddNew();
			AssertEquals(string.Empty, line.US_NHTIdentityNumQualifier);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Header.NHTSADetails;
		}

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
					Factory.Save();
				}
				return header;
			}
		}
		NHTSAHeader header;

		#endregion
	}
}
