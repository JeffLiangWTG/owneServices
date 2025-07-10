using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonShipmentDocumentSupporterForTest))]
	sealed class CommonShipmentDocumentTest : DocumentSupporterTest
	{
		public void TestQueryProvider()
		{
			CommonShipmentDocumentSupporterForTest documentSupporter = new CommonShipmentDocumentSupporterForTest(Factory.New<CommonShipment>());
			ICommonShipmentDocumentSupporterQueryProvider queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is CommonShipmentDocumentSupporterQueryProvider);

			Factory.SetValue<ICommonShipmentDocumentSupporterQueryProvider, CommonShipmentDocumentSupporterQueryProviderForTest>();

			documentSupporter = new CommonShipmentDocumentSupporterForTest(Factory.New<CommonShipment>());
			queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is CommonShipmentDocumentSupporterQueryProviderForTest);

			ICommonShipmentDocumentSupporterQueryProvider queryProviderInSaveTransaction = null;
			Factory.Saving += _ => queryProviderInSaveTransaction = documentSupporter.QueryProvider;
			Factory.Save();
			AssertNotNull(queryProviderInSaveTransaction);
			Assert(queryProviderInSaveTransaction is CommonShipmentDocumentSupporterQueryProvider);
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable businessObject)
		{
			CommonShipment shipment = (CommonShipment)businessObject;

			if (command.SU_MenuName == "Bill Of Lading (Rail)")
			{
				shipment.JS_HouseBillOfLadingType = "EAG";
			}
			else if (command.SU_MenuName == "Bill Of Lading To Preprinted")
			{
				shipment.JS_HouseBillOfLadingType = "FIP";
			}
			else
			{
				shipment.JS_HouseBillOfLadingType = "FIA";
			}
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			bool result = false;

			if (documentCommand.SU_MenuName.StartsWith("Bill of Entry")
				|| documentCommand.SU_MenuName.StartsWith("Exchange Control Declaration")
				|| documentCommand.SU_MenuName.StartsWith("Customs and Excise")
				|| documentCommand.SU_MenuName.StartsWith("Shipper Document Pack")
				|| documentCommand.SU_MenuPath.StartsWith("CMD")
				|| documentCommand.SU_MenuName.Contains("DA306")
				|| documentCommand.SU_MenuName.Contains("Container Liability Statement")
				|| documentCommand.SU_MenuName.Contains("Authority To Deal")
				|| documentCommand.SU_MenuName.Contains("EFT Payment Advice"))
			{
				result = true;
			}
			else
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.SouthAfrica)
				{
					if ((documentCommand.SU_MenuName.StartsWith("Post Consolidation")
						|| documentCommand.SU_MenuName.StartsWith("Pre Consolidation")))
					{
						result = true;
					}
				}
			}

			return result;
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.CoLoadShipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00148999";
			shipment.JS_HouseBillOfLadingType = "FIA";
			shipment.Consols.AddNew();
			PackLine line = shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_DeclarationReference] = "S00148999";
			MethodInfo method = declaration.GetType().GetMethod("SetupMergedForTest");
			if (method != null)
			{
				method.Invoke(declaration, Array.Empty<object>());
			}

			return shipment;
		}

		public void TestLocalTransportRecipientShouldDefaultFromBizONotRegistry()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "INBOM";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Precondition - CommonShipment is an import", true, shipment.IsImport());
			AssertNull("Precondition - delivery cartage company is not set", shipment.DocsAndCartage.DeliveryCartageCo);

			var registryAirCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, registryAirCartageCo.PK.ToGuid());

			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);
			AssertNull("Recipient should come from the BizO, not the registry", contact);

			OrgHeader bizOCartageCompany = Factory.NewWithValidTestData<OrgHeader>();
			shipment.DocsAndCartage.DeliveryCartageCoPK = bizOCartageCompany.PK;
			contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);
			AssertEquals("Recipient should come from the BizO, not the registry", bizOCartageCompany.PK, contact.OrgHeader.PK);
		}

		public void TestGetOverriddenDeliveryDetails()
		{
			CommonShipment shipment = Factory.New<CommonShipmentForTest>();
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "Consignee 1";
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "Consignor 1";

			IDocAddress result = ((IDocumentSupportable)shipment).DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.Consignor, DocumentDirection.ANY);
			AssertNull(result);

			result = ((IDocumentSupportable)shipment).DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.Consignee, DocumentDirection.ANY);
			AssertNull(result);

			result = ((IDocumentSupportable)shipment).DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.Receivables, DocumentDirection.ANY);
			AssertNull(result);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			result = ((IDocumentSupportable)shipment).DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.Consignor, DocumentDirection.ANY);
			AssertEquals(shipment.ConsignorDocumentaryAddress, result);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			result = ((IDocumentSupportable)shipment).DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals(shipment.ConsigneeDocumentaryAddress, result);

			result = ((IDocumentSupportable)shipment).DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.Receivables, DocumentDirection.ANY);
			AssertNull(result);
		}

		public void TestDepotContactOrganisation()
		{
			ZQuery query = new ZQuery();

			OrgHeader shipmentArrivalCFS = Factory.LoadTop1<OrgHeader>(query);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, shipmentArrivalCFS.PK);
			OrgHeader shipmentDepartureCFS = Factory.LoadTop1<OrgHeader>(query);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, shipmentDepartureCFS.PK);
			OrgHeader consolArrivalCFS = Factory.LoadTop1<OrgHeader>(query);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consolArrivalCFS.PK);
			OrgHeader consolDepartureCFS = Factory.LoadTop1<OrgHeader>(query);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "INBOM";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = "SEA";

			shipment.JS_OA_ExportReceivingDepot = shipmentDepartureCFS.Addresses[0].PK;
			shipment.JS_OA_ImportReleaseDepot = shipmentArrivalCFS.Addresses[0].PK;

			CommonConsol departureConsol = Factory.New<CommonConsol>();
			departureConsol.JK_ConsolMode = "FCL";
			departureConsol.JK_RL_NKLoadPort = "INBOM";
			departureConsol.JK_RL_NKDischargePort = "SGSIN";

			departureConsol.JK_OA_PackDepotAddress = consolDepartureCFS.Addresses[0].PK;

			CommonConsol arrivalConsol = Factory.New<CommonConsol>();
			arrivalConsol.JK_ConsolMode = "FCL";
			arrivalConsol.JK_RL_NKLoadPort = "SGSIN";
			arrivalConsol.JK_RL_NKDischargePort = "AUBNE";

			arrivalConsol.JK_OA_UnpackDepotAddress = consolArrivalCFS.Addresses[0].PK;

			shipment.Consols.Add(departureConsol);
			shipment.Consols.Add(arrivalConsol);

			AssertEquals("Arrival CFS from shipment", shipmentArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.Depot, DocumentDirection.ARV).OrgHeader);
			AssertEquals("Arrival CFS from shipment", shipmentArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ImportAirDepot, DocumentDirection.ARV).OrgHeader);
			AssertEquals("Arrival CFS from shipment", shipmentArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ImportSeaDepot, DocumentDirection.ARV).OrgHeader);
			AssertEquals("Arrival CFS from shipment", shipmentArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ImportDepot, DocumentDirection.ARV).OrgHeader);

			AssertEquals("Departure CFS from shipment", shipmentDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.Depot, DocumentDirection.DEP).OrgHeader);
			AssertEquals("Departure CFS from shipment", shipmentDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportAirDepot, DocumentDirection.DEP).OrgHeader);
			AssertEquals("Departure CFS from shipment", shipmentDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaDepot, DocumentDirection.DEP).OrgHeader);
			AssertEquals("Departure CFS from shipment", shipmentDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportDepot, DocumentDirection.DEP).OrgHeader);

			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			AssertEquals("Arrival CFS from consol", consolArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.Depot, DocumentDirection.ARV).OrgHeader);
			AssertEquals("Arrival CFS from consol", consolArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ImportAirDepot, DocumentDirection.ARV).OrgHeader);
			AssertEquals("Arrival CFS from consol", consolArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ImportSeaDepot, DocumentDirection.ARV).OrgHeader);
			AssertEquals("Arrival CFS from consol", consolArrivalCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ImportDepot, DocumentDirection.ARV).OrgHeader);

			AssertEquals("Departure CFS from consol", consolDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.Depot, DocumentDirection.DEP).OrgHeader);
			AssertEquals("Departure CFS from consol", consolDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportAirDepot, DocumentDirection.DEP).OrgHeader);
			AssertEquals("Departure CFS from consol", consolDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaDepot, DocumentDirection.DEP).OrgHeader);
			AssertEquals("Departure CFS from consol", consolDepartureCFS, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportDepot, DocumentDirection.DEP).OrgHeader);
		}

		public void TestControllingCustomerContactOrganisation()
		{
			var shipment = Factory.New<CommonShipment>();
			var controllingCustomer = Factory.New<OrgHeader>();

			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			AssertEquals(controllingCustomer, shipment.ControllingCustomer);
			AssertEquals("Controlling Customer from shipment", controllingCustomer, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingCustomer, DocumentDirection.ARV).OrgHeader);

			shipment.ControllingCustomerAddress.E2_AddressOverride = true;
			AssertNull((OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingCustomer, DocumentDirection.ARV).OrgHeader);
		}

		public void TestControllingAgentContactOrganisation()
		{
			var shipment = Factory.New<CommonShipment>();
			var controllingAgent = Factory.New<OrgHeader>();

			var controllingAgentAddress = shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);
			controllingAgentAddress.OrganisationPK = controllingAgent.PK;
			AssertEquals("Controlling Agent from shipment", controllingAgent, (OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingAgent, DocumentDirection.ARV).OrgHeader);

			controllingAgentAddress.E2_AddressOverride = true;
			AssertNull((OrgHeader)shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingAgent, DocumentDirection.ARV).OrgHeader);
		}
	}
}
