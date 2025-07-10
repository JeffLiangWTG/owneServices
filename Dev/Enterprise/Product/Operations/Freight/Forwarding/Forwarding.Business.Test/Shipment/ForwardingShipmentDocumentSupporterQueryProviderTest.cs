using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentDocumentSupporterQueryProviderTest : TestCaseWithFactory
	{
		public void TestGetDebtorsToPrint()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_OA_LocalChargesAddr = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;

			AccChargeCode chargeCode = CreateChargeCode("tstcode");
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			JobCharge charge1 = CreateLineCharge(job, org1.PK, 20.000M, chargeCode.PK);
			charge1.JR_OH_SellAccount = org1.PK;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			JobCharge charge2 = CreateLineCharge(job, org2.PK, 20.000M, chargeCode.PK);
			charge2.JR_OH_SellAccount = org2.PK;

			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterQueryProvider();

			AssertNull(queryProvider.GetDebtorsToPrint(null));

			DocumentShipment docShipment = new DocumentShipment(shipment, Core.Constants.DataContext.ChargeSheet);
			docShipment.SetDefaultsFromDataContext();

			AssertContainsExactElementsInAnyOrder("Prerequisite", new[] { org1, org2 }, docShipment.DebtorsToPrint.Cast<DebtorToSelectFromForPrinting>().Select((d) => d.Debtor));

			DebtorToSelectFromForPrinting[] debtorsToPrint = queryProvider.GetDebtorsToPrint(docShipment);

			AssertContainsExactElementsInAnyOrder(docShipment.DebtorsToPrint, debtorsToPrint);
		}

		public void TestGetImportCargoLabelToPrint()
		{
			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterQueryProvider();
			AssertNull(queryProvider.GetImportCargoLabelToPrint(null));

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = 10;

			var label = new DocumentImportCargoLabel(shipment);
			AssertEquals(label, queryProvider.GetImportCargoLabelToPrint(label));
		}

		public void TestGetLetterOfIndemnityOptions()
		{
			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterQueryProvider();

			LetterOfIndemnityOptions options = queryProvider.GetLetterOfIndemnityOptions(null);
			AssertNotNull(options);

			options = queryProvider.GetLetterOfIndemnityOptions(new DocumentShipment(Factory.New<ForwardingShipment>(), Core.Constants.DataContext.LetterOfIndemnity));
			AssertNotNull(options);
		}

		public void TestPrintAWBBarcodeLabel()
		{
			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterQueryProvider();
			AssertEquals(false, queryProvider.PrintAWBBarcodeLabel());
		}

		public void TestGetTransportToPrint()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Transport transport = shipment.Transports.AddNew();

			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterQueryProvider();

			AssertNull(queryProvider.GetTransportToPrint(null));

			DocumentShipment docShipment = new DocumentShipment(shipment, Core.Constants.DataContext.GenericFreightJob);

			AssertEquals("Prerequisite", 1, docShipment.Shipment.TransportsIncludingRelated.Count);
			AssertEquals("Prerequisite", transport, docShipment.Shipment.TransportsIncludingRelated[0]);

			Transport transprtToPrint = queryProvider.GetTransportToPrint(docShipment);

			AssertEquals(transprtToPrint, transport);
		}

		#region Implementaion

		AccChargeCode CreateChargeCode(string chargeCode)
		{
			AccChargeCode code = Factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "test charege code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			return code;
		}

		JobCharge CreateLineCharge(JobHeader haderBizObj, ZGuid chargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_JH = haderBizObj.PK;
			charge.JR_GE = haderBizObj.JH_GE;
			charge.JR_GB = haderBizObj.JH_GB;
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalSellAmt = amount;
			charge.JR_OH_SellAccount = chargesPK;
			return charge;
		}

		#endregion
	}
}
