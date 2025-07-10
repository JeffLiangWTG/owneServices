using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(VehicleDetailsCollection))]
	public class VehicleDetailsCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new VehicleDetailsCollection(Vehicle);
		}

		Vehicle Vehicle
		{
			get
			{
				if (vehicle == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					vehicle = invoiceLine.VehicleLines.AddNew();
				}
				return vehicle;
			}
		}
		Vehicle vehicle;

		#endregion
	}
}
