using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestTaxOrFeeMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				var ji = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				var tax1 = (JobComInvoiceLineTax)Factory.BOFactory.New<Integration.Customs.EU.IJobComInvoiceLineTax>();
				tax1.JLT_JI = ji.PK;
				tax1.JLT_Type = "A00";
				tax1.JLT_Amount = 123m;
				tax1.JLT_BaseQuantity = 456m;
				tax1.JLT_BaseValue = 789m;
				tax1.JLT_MethodOfCalculation = "ABCD";
				tax1.JLT_MethodOfPayment = "F";
				tax1.JLT_RateOverrideReasonCode = "X";
				tax1.JLT_Rate = 100m;
				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var shipmentData = (UniversalShipment)writer.GetDataObject(declaration);
				var invoiceData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0];
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				AssertEquals("invoiceLineData>Taxes>Count", 1, invoiceLineData.TaxOrFeeCollection.Count);
				var t = invoiceLineData.TaxOrFeeCollection[0];
				AssertEquals("A00", t.Type.Code);
				AssertEquals(123m, t.Amount);
				AssertEquals(456m, t.BaseQuantity);
				AssertEquals(789m, t.BaseValue);
				AssertEquals("ABCD", t.MethodOfCalculation.Code);
				AssertEquals("F", t.MethodOfPayment.Code);
				AssertEquals("X", t.RateReasonOverride.Code);
				AssertEquals(100m, t.Rate);
			}
		}
	}
}
