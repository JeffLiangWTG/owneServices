using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ManufacturerAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<ManufacturerAddressCodeTypes>
	{
		public override void TestIDCodeTypes()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.FDAEstablishmentIdentifier }, AddressCodeTypes.IDCodeTypes);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, AddressCodeTypes.IDCodeTypes);
		}

		public override void TestFRICodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, Constants.OrgCusCodeType.CustomCode }, AddressCodeTypes.FRICodeTypes);
		}

		protected override ManufacturerAddressCodeTypes GetAddressCodeTypes()
		{
			var invoiceLine = (JobComInvoiceLine)Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return new ManufacturerAddressCodeTypes(invoiceLine.ManufacturerDocAddress);
		}
	}
}
