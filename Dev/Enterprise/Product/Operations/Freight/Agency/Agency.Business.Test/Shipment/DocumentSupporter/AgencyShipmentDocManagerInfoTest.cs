using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentDocManagerInfo))]
	internal class AgencyShipmentDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestRelatedObjects()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer realContainer = shipment.RealContainers.AddNew();
			realContainer.JC_ContainerNum = "TEST4100013";
			realContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentContainer bookedContainer = shipment.BookedContainers.AddNew();
			bookedContainer.JC_ContainerNum = "TEST4100013";
			bookedContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			AgencyShipmentDocManagerInfo info = new AgencyShipmentDocManagerInfo(shipment);
			AssertContainsExactElementsInAnyOrder("Related Objects", (b) => b.HumanReadableName, new BusinessObject[] { realContainer, bookedContainer }, info.RelatedObjects);
		}

		public void TestInvoiceHeaderRetrieved()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "SS1212333";
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_IsDebtor = ZBool.True;
			debtor.OH_Code = "DEBTOR";
			AccTransactionHeader invoiceHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoiceHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoiceHeader.AH_OH = debtor.PK;
			invoiceHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			invoiceHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			Factory.Save();
			AgencyShipmentDocManagerInfo info = new AgencyShipmentDocManagerInfo(shipment);
			AssertContainsExactElementsInAnyOrder("Related Objects", new BusinessObject[] { invoiceHeader }, info.RelatedObjects);
		}

		#region Implementation
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<AgencyShipment>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "Consignor";
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "Consignee";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = voyage.Sailings[0].PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_GoodsDescription = "Goods Description";
			return shipment;
		}
		#endregion
	}
}
