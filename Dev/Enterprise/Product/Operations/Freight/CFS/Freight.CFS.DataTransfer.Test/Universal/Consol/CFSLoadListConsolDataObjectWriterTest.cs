using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CFS.DataTransfer.Universal.Testing
{
	class CFSLoadListConsolDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriting()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "NLAMS";
			consol.JK_AgentsReference = "TECHNO SAUSAGE";
			consol.JK_BookingReference = "JIBBLY JOBBLY";
			consol.JK_MasterBillNum = "CHUNKYLOVER53";
			consol.JK_CustomsReference = "PURPLE DINOSAURS";

			consol.JK_OH_Forwarder = CreateOrg("FWDORG").PK;
			consol.JK_OA_ShippingLineAddress = CreateOrg("SHPORG").MainAddress.PK;
			consol.JK_OA_EmptyContainerYard = CreateOrg("CYORG").MainAddress.PK;
			consol.JK_OA_CTOAddress = CreateOrg("CTOORG").MainAddress.PK;
			consol.JK_OA_CartageCoAddress = CreateOrg("CTGORG").MainAddress.PK;
			consol.JK_OA_DepotAddress = CreateOrg("DEPORG").MainAddress.PK;

			var additionalTransport = consol.Transports.AddNew();
			additionalTransport.JW_RL_NKLoadPort = "GBLON";
			additionalTransport.JW_RL_NKDiscPort = "USCHI";

			consol.Containers.AddNew().JC_ContainerNum = "MMMM9999999";
			consol.Containers.AddNew().JC_ContainerNum = "ROFL7777777";

			consol.Shipments.AddNew().JS_GoodsDescription = "BACON PANCAKES";
			consol.Shipments.AddNew().JS_GoodsDescription = "MY LEFT ELBOW";

			var dataObject = new CFSLoadListConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), true).GetDataObject(consol);

			CombineAssertions(() =>
			{
				AssertEquals(Constants.TransportModes.Sea, dataObject.TransportMode.Code);
				AssertEquals(Constants.ContainerModes.FCL, dataObject.ContainerMode.Code);
				AssertEquals("AUMEL", dataObject.PortOfLoading.Code);
				AssertEquals("NLAMS", dataObject.PortOfDischarge.Code);
				AssertEquals("TECHNO SAUSAGE", dataObject.AgentsReference);
				AssertEquals("JIBBLY JOBBLY", dataObject.BookingConfirmationReference);
				AssertEquals("CHUNKYLOVER53", dataObject.WayBillNumber);
				AssertEquals("PURPLE DINOSAURS", dataObject.CFSReference);

				AssertOrg(dataObject, AddressTypes.Forwarder, "FWDORG");
				AssertOrg(dataObject, nameof(DocAddressType.ShippingLineAddress), "SHPORG");
				AssertOrg(dataObject, AddressTypes.ContainerYardAddress, "CYORG");
				AssertOrg(dataObject, AddressTypes.CTOAddress, "CTOORG");
				AssertOrg(dataObject, nameof(DocAddressType.LocalCartageAddress1), "CTGORG");
				AssertOrg(dataObject, AddressTypes.DepotAddress, "DEPORG");

				AssertEquals(2, dataObject.TransportLegCollection.Count);
				AssertEquals("AUMEL", dataObject.TransportLegCollection[0].PortOfLoading.Code);
				AssertEquals("NLAMS", dataObject.TransportLegCollection[0].PortOfDischarge.Code);
				AssertEquals("GBLON", dataObject.TransportLegCollection[1].PortOfLoading.Code);
				AssertEquals("USCHI", dataObject.TransportLegCollection[1].PortOfDischarge.Code);

				AssertEquals(2, dataObject.ContainerCollection.Count);
				AssertEquals("MMMM9999999", dataObject.ContainerCollection[0].ContainerNumber);
				AssertEquals("ROFL7777777", dataObject.ContainerCollection[1].ContainerNumber);

				AssertEquals(2, dataObject.SubShipmentCollection.Count);
				AssertEquals("BACON PANCAKES", dataObject.SubShipmentCollection[0].GoodsDescription);
				AssertEquals("MY LEFT ELBOW", dataObject.SubShipmentCollection[1].GoodsDescription);
			});
		}

		public void TestWriting_ContainerPackLineLinking()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_MarksAndNumbers = "PACK1";
			packLine1.SetContainer(container1.PK);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_MarksAndNumbers = "PACK2";
			packLine2.SetContainer(container1.PK);
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_MarksAndNumbers = "PACK3";
			packLine3.SetContainer(container2.PK);

			var dataObject = new CFSLoadListConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), true).GetDataObject(consol);

			Action<string, string> assertPacking = (packLine, container) =>
			{
				var containerDO = dataObject.ContainerCollection.First(x => x.ContainerNumber.ToString() == container);
				var packLineDO = dataObject.SubShipmentCollection[0].PackingLineCollection.First(x => x.MarksAndNos.ToString() == packLine);
				AssertEquals(packLine + " packed into " + container, packLineDO.ContainerLink, containerDO.Link);
			};

			assertPacking("PACK1", "CONTAINER1");
			assertPacking("PACK2", "CONTAINER1");
			assertPacking("PACK3", "CONTAINER2");
		}

		public void TestCFSReferenceMaxLength()
		{
			var cfsRefWithMaxLength = "12345678901234567890123456789012345";

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "NLAMS";
			consol.JK_CustomsReference = cfsRefWithMaxLength;

			var dataObject = new CFSLoadListConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), true).GetDataObject(consol);

			CombineAssertions(() =>
			{
				AssertEquals(cfsRefWithMaxLength, dataObject.CFSReference);
				AssertHasCustomAttribute<MaxLengthAttribute>(dataObject.GetType(), nameof(dataObject.CFSReference), false, attr => attr.MaxLength == cfsRefWithMaxLength.Length);
			});
		}

		#region Implementation

		OrgHeader CreateOrg(ZString code)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;

			return org;
		}

		void AssertOrg(Shipment dataObject, string addressType, ZString code)
		{
			var addressDO = dataObject.OrganizationAddressCollection.FirstOrDefault(addressType);
			AssertNotNull(addressDO);
			AssertEquals(code, addressDO.OrganizationCode);
		}

		#endregion
	}
}
