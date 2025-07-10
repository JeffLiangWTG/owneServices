using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ForwardingShipmentExtensionsTest : TestCaseWithFactory
	{
		public void TestGetEffectiveITNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolITNo = consol.Numbers.AddNew();
			consolITNo.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			consolITNo.CE_EntryNum = "V1";
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentITNo = shipment.Numbers.AddNew();
			shipmentITNo.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			shipmentITNo.CE_EntryNum = "V2";
			AssertEquals("IT taken from shipment", "V2", shipment.GetEffectiveITNumber(consol));
			shipment.Numbers.RemoveAndDeleteAll();
			AssertEquals("IT taken from consol", "V1", shipment.GetEffectiveITNumber(consol));
		}

		public void TestGetContainersFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			packLine1.JL_PackageCount = 1;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_PackageCount = 1;
			AssertEquals("2 containers", 2, new List<ForwardingContainer>(shipment.GetUniqueContainersFromConsol(consol)).Count);
			var buyerConsol = Factory.New<ForwardingConsol>();
			buyerConsol.JK_TransportMode = Constants.TransportModes.Rail;
			buyerConsol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			var buyerContainer1 = buyerConsol.Containers.AddNew();
			buyerContainer1.JC_ContainerNum = "CONT1";
			buyerContainer1.AMSNumber = "AMS001";
			var buyerContainer2 = buyerConsol.Containers.AddNew();
			buyerContainer2.JC_ContainerNum = "CONT2";
			buyerContainer2.AMSNumber = "AMS002";
			var buyerLeaderShipment = Factory.New<ForwardingShipment>();
			buyerLeaderShipment.Consols.Add(buyerConsol);
			buyerLeaderShipment.JS_TransportMode = Constants.TransportModes.Rail;
			buyerLeaderShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			buyerLeaderShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			var buyerCoLoadShipment = buyerLeaderShipment.CoLoadShipments.AddNew();
			buyerCoLoadShipment.JS_TransportMode = Constants.TransportModes.Rail;
			buyerCoLoadShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			buyerCoLoadShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var buyerPackLine1 = buyerLeaderShipment.OuterPackLines.AddNew();
			buyerPackLine1.JL_JC = buyerContainer1.PK;
			buyerPackLine1.JL_PackageCount = 1;
			var buyerPackLine2 = buyerCoLoadShipment.OuterPackLines.AddNew();
			buyerPackLine2.JL_JC = buyerContainer2.PK;
			buyerPackLine2.JL_PackageCount = 1;
			AssertEquals("2 containers", 2, new List<ForwardingContainer>(buyerLeaderShipment.GetUniqueContainersFromConsol(buyerConsol)).Count);
		}

		public void TestGetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.AMSNumber = "MB1";
			container1.ITReferenceNumber = "V1";
			var container2 = consol.Containers.AddNew();
			container2.AMSNumber = ZString.Empty;
			container2.ITReferenceNumber = "V2";
			var container3 = consol.Containers.AddNew();
			container3.AMSNumber = ZString.Empty;
			container3.ITReferenceNumber = ZString.Empty;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			var shipmentITNo = shipment.Numbers.AddNew();
			shipmentITNo.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			shipmentITNo.CE_EntryNum = "V3";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			packLine1.JL_PackageCount = 1;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_PackageCount = 2;
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = container3.PK;
			packLine3.JL_PackageCount = 6;
			AssertEquals("2 IT no from container without new BOL", 2, shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(consol).Count);
			Assert("Contain V2", shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(consol).Contains(new KeyValuePair<ZString, ZInt>("V2", 2)));
			Assert("Contain V3", shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(consol).Contains(new KeyValuePair<ZString, ZInt>("V3", 6)));
			container3.ITReferenceNumber = "V4";
			AssertEquals("2 IT no from container without new BOL", 2, shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(consol).Count);
			Assert("Contain V2", shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(consol).Contains(new KeyValuePair<ZString, ZInt>("V2", 2)));
			Assert("Contain V4", shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(consol).Contains(new KeyValuePair<ZString, ZInt>("V4", 6)));
		}

		public void TestGetValidHouseBillIssuerCodes()
		{
			Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var org = Factory.New<OrgHeader>();
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;

			AssertEquals("Number of ValidHouseBillIssuerCodes from Org. Proxy", 1, shipment.GetValidHouseBillIssuerCodes().Count());
			AssertEquals("ValidHouseBillIssuerCodes from Org. Proxy", "XXXX", shipment.GetValidHouseBillIssuerCodes().FirstOrDefault());

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Number of ValidHouseBillIssuerCodes from Org. Proxy", 2, shipment.GetValidHouseBillIssuerCodes().Count());
			AssertEquals("ValidHouseBillIssuerCodes from Org. Proxy", "ABCD", shipment.GetValidHouseBillIssuerCodes().FirstOrDefault());
			AssertEquals("ValidHouseBillIssuerCodes from Org. Proxy", "XXXX", shipment.GetValidHouseBillIssuerCodes().LastOrDefault());

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Number of ValidHouseBillIssuerCodes from Org. Proxy", 2, shipment.GetValidHouseBillIssuerCodes().Count());
			AssertEquals("ValidHouseBillIssuerCodes from Org. Proxy", "ABCD", shipment.GetValidHouseBillIssuerCodes().FirstOrDefault());
			AssertEquals("ValidHouseBillIssuerCodes from Org. Proxy", "XXXX", shipment.GetValidHouseBillIssuerCodes().LastOrDefault());

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TruckCarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Number of ValidHouseBillIssuerCodes from Org. Proxy", 2, shipment.GetValidHouseBillIssuerCodes().Count());
			AssertEquals("ValidHouseBillIssuerCodes from Org. Proxy", "OTT1", shipment.GetValidHouseBillIssuerCodes().FirstOrDefault());
			AssertEquals("ValidHouseBillIssuerCodes from Org. Proxy", "XXXX", shipment.GetValidHouseBillIssuerCodes().LastOrDefault());
		}
	}
}
