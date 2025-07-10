using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestPopulateConsigneeDocumentaryAddress()
		{
			Enterprise.Registry.Business.eServices.eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<JobDeclaration>();

			AssertNull("Pre-Condition: Consignee is Empty", declaration.Consignee);
			Factory.SaveForTesting();
			AssertConsigneeDocumentaryAddress("ConsigneeDocumentaryAddress is missing when Consignee does not exist on Declaration", declaration, null);

			var decConsignee = CreateOrganisation("DEC CONSIGNEE", "CONSDEC");
			declaration.JE_OH_Consignee = decConsignee.PK;
			AssertEquals("Pre-Condition: Consignee is as was set on Declaration", decConsignee.PK, declaration.Consignee.PK);
			Factory.SaveForTesting();
			AssertConsigneeDocumentaryAddress("ConsigneeDocumentaryAddress is from Declaration", declaration, decConsignee);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Factory.SaveForTesting();
			AssertConsigneeDocumentaryAddress("ConsigneeDocumentaryAddress on Declaration is ignored when Shipment exists", shipment, null);

			var shipConsignee = CreateOrganisation("SHIP CONSIGNEE", "CONSSHIP");
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = shipConsignee.PK;
			Factory.SaveForTesting();
			AssertConsigneeDocumentaryAddress("ConsigneeDocumentaryAddress is from Shipment", shipment, shipConsignee);
		}

		void AssertConsigneeDocumentaryAddress(ZString message, BusinessObject sourceBO, OrgHeader consignee)
		{
			var dataWritingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, sourceBO));

			IOrganizationAddressCollectionParent dataObject = null;
			if (sourceBO is ForwardingShipment shipment)
			{
				dataObject = new ShipmentDataObjectWriter(dataWritingManager, true, true).GetDataObject(shipment);
			}
			else if (sourceBO is JobDeclaration declaration)
			{
				dataObject = new DeclarationDataObjectWriter(dataWritingManager).GetDataObject(declaration);
			}

			var consigneeDocumentaryAddresses = dataObject.OrganizationAddressCollection?.Where(addr => addr.AddressType.HasValue && addr.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress)).ToList();
			var consigneeData = consigneeDocumentaryAddresses?.FirstOrDefault();
			CombineAssertions(message, () =>
			{
				AssertEquals("CompanyName", consignee?.OH_FullName, consigneeData?.CompanyName);
				AssertLessThanOrEqualTo("Count", consigneeDocumentaryAddresses?.Count ?? 0, 1);
			});
		}

		public void TestPopulateExtraOrganisation()
		{
			var consignee = CreateOrganisation("CONSIGNEE", "CONSTEST");
			var buyer = CreateOrganisation("BUYER", "BUYTEST");
			var claimant = CreateOrganisation("CLAIMANT", "CLAIMTEST");
			var handlingAgent = CreateOrganisation("HANDLING AGENT", "HNDAGTTEST");
			var inwardCarrierAgent = CreateOrganisation("INWARD CARRIER AGENT", "INWDTEST");
			var outwardCarrierAgent = CreateOrganisation("OUTWARD CARRIER AGENT", "OUTWDTEST");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Consignee = consignee.PK;
			declaration.JE_OH_Buyer = buyer.PK;
			declaration.JE_OH_Claimant = claimant.PK;
			declaration.JE_OH_HandlingAgent = handlingAgent.PK;
			declaration.JE_OH_InwardCarrierAgent = inwardCarrierAgent.PK;
			declaration.OutwardShippingLineForwarderPK = outwardCarrierAgent.PK;
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			var consigneeData = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			AssertEquals(consignee.OH_FullName, consigneeData.CompanyName);
			var buyerData = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.BuyerDocumentaryAddress));
			AssertEquals(buyer.OH_FullName, buyerData.CompanyName);
			var claimantData = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ClaimantAddress));
			AssertEquals(claimant.OH_FullName, claimantData.CompanyName);
			var handlingAgentData = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.CarrierHandlingAgent));
			AssertEquals(handlingAgent.OH_FullName, handlingAgentData.CompanyName);
			var inwardCarrierAgentData = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.InwardCarrierAgent));
			AssertEquals(inwardCarrierAgent.OH_FullName, inwardCarrierAgentData.CompanyName);
			var outwardCarrierAgentData = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.OutwardCarrierAgent));
			AssertEquals(outwardCarrierAgent.OH_FullName, outwardCarrierAgentData.CompanyName);
		}

		public void TestPopulateTradersRemarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			var remark1 = declaration.TradersRemarks.AddNew();
			remark1.CSI_Description = "TEST TRADER REMARK 1";
			var remark2 = declaration.TradersRemarks.AddNew();
			remark2.CSI_Description = "TEST TRADER REMARK 2";
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			var tradersRemarks = declarationData.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.TradersRemarks);
			AssertEquals(2, tradersRemarks.Count());
			var tradersRemarksInOrder = tradersRemarks.OrderBy(x => x.Order).ToArray();
			AssertEquals(1, tradersRemarksInOrder[0].Order);
			AssertEquals("TEST TRADER REMARK 1", tradersRemarksInOrder[0].Reference);
			AssertEquals(2, tradersRemarksInOrder[1].Order);
			AssertEquals("TEST TRADER REMARK 2", tradersRemarksInOrder[1].Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}

		OrgHeader CreateOrganisation(ZString name, ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = name + " ADDRESS 1";
			return org;
		}
	}
}
