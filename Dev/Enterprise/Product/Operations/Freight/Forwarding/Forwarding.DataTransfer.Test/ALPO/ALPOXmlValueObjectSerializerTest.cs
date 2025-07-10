using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ALPOXmlValueObjectSerializerTest : TestCaseWithFactory
	{
		public void TestALPOXmlValueObjectSerializer()
		{
			using (var memoryStream = new MemoryStream())
			{
				var serializer = new ALPOXmlValueObjectSerializer(Adapter.ValueObjectType);
				serializer.ExportXmlData(memoryStream, Adapter, new CommonShipment[] { Shipment }, new ValueObjectExportContext(new NotificationBuffer()), null, null, null);

				using (var streamReader = new StreamReader(memoryStream))
				{
					memoryStream.Position = 0;

					var xmlStr = streamReader.ReadToEnd();
					var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xmlStr);
					var xmlNodeList = xmlDoc.GetElementsByTagName("AdvantageEnterpriseVersion0.1");
					Assert(xmlStr, xmlNodeList != null);
					AssertEquals(xmlStr, 1, xmlNodeList.Count);

					var attributes = xmlNodeList[0].Attributes;
					AssertEquals(xmlStr, 2, attributes.Count);
					AssertEquals(xmlStr, "http://www.w3.org/2001/XMLSchema-instance", attributes["xmlns:xsi"].Value);
					AssertEquals(xmlStr, "ALPO-AUFTRAG_V1_03.xsd", attributes["xmlns:noNamespaceSchemaLocation"].Value);
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Adapter = new ALPOConsolWithShipmentValueObjectDataAdapter();
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_BookingReference = "BookingReference";
			Consol.JK_RL_NKDischargePort = "USORD";

			ShippingLine = Factory.New<OrgHeader>();
			ShippingLine.OH_FullName = "ShippingLine";
			ShippingLine.OH_Code = "SHIPLINE";
			Consol.JK_OA_ShippingLineAddress = ShippingLine.MainAddress.PK;

			SendingForwarder = Factory.New<OrgHeader>();
			SendingForwarder.OH_FullName = "Sending Forwarder";
			SendingForwarder.OH_Code = "SFRWDR";

			Consol.JK_OA_SendingForwarderAddress = SendingForwarder.MainAddress.PK;

			CommonShipment someShipment = Consol.Shipments.AddNew();
			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_HouseBill = "HAWB1234321";
			Shipment.JS_UniqueConsignRef = "UniqueConsRef";
			Shipment.JS_PackingMode = "FCL";
			Shipment.JS_MarksAndNumbers = "MarksAndNums";
			Shipment.JS_UniqueConsignRef = "S0000001";
			Shipment.JS_RL_NKOrigin = "DEBRE";
			Shipment.JS_RL_NKDestination = "USLAX";

			ALPOTransport = Shipment.Transports.AddNew();
			ALPOTransport.JW_Vessel = "Transport Name";
			ALPOTransport.JW_TransportType = "PRE";
			ALPOTransport.JW_TransportMode = "SEA";

			CFSOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			RefCountry deCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "DE"));
			CFSOrg.CustomsCodes.AddNew("EID", "ADZB", deCountry);
			Shipment.JS_OA_ImportReleaseDepot_ZAddress.SetOrgWithoutSettingDefaultAddress(CFSOrg.PK);
			Factory.Save();

			PackLine innerLine1 = Shipment.InnerPackLines.AddNew();
			innerLine1.JL_RefNumber = "InnerRefN1";
			PackLine innerLine2 = Shipment.InnerPackLines.AddNew();
			innerLine2.JL_RefNumber = "InnerRefN2";
			PackLine innerLine3 = Shipment.InnerPackLines.AddNew();
			innerLine3.JL_RefNumber = "InnerRefN1";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			UNDG = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			UNDG.DG_PG = "III";

			#region RefContainers

			RefContainer1 = Factory.New<RefContainer>();
			RefContainer1.RC_Code = "40GP";
			RefContainer1.RC_ISOType = "1234";

			RefContainer2 = Factory.New<RefContainer>();
			RefContainer2.RC_Code = "20GP";
			RefContainer2.RC_ISOType = "1235";

			#endregion

			#region Containers

			Container1 = Consol.Containers.AddNew();
			Container1.JC_ContainerNum = "CNTR000001";
			Container1.JC_ContainerMode = "LCL";
			Container1.JC_RC = RefContainer1.PK;
			Container1.JC_IsEmptyContainer = false;

			Container1.JC_TareWeight = 1.1;
			Container1.JC_GrossWeight = 1.2;

			PackLine11 = Shipment.OuterPackLines.AddNew();
			UNDGDataItem dGItem = PackLine11.UNDGs.AddNew();
			dGItem.LinkDefault(subs);
			dGItem.DI_DGFlashPoint = 1.11;

			PackLine11.JL_JS = Shipment.PK;
			PackLine11.JL_JC = Container1.PK;
			PackLine11.JL_RH_NKCommodityCode = "CCd";
			PackLine11.JL_ActualWeight = 11.1;
			PackLine11.JL_OutturnComment = "Outturn Comment";
			PackLine11.JL_MarksAndNumbers = "MarksAndNumbers";
			PackLine11.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Carton;
			PackLine11.JL_Description = "Description";
			PackLine11.JL_RefNumber = "RefNum";

			PackLine12 = Shipment.OuterPackLines.AddNew();
			PackLine12.JL_JS = Shipment.PK;
			PackLine12.JL_JC = Container1.PK;
			PackLine12.JL_ActualWeight = 12.1;
			PackLine12.JL_OutturnComment = "Outturn Comment2";
			PackLine12.JL_MarksAndNumbers = "MarksAndNumbers2";
			PackLine12.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Drum;
			PackLine12.JL_RefNumber = "RefNum";

			Container2 = Consol.Containers.AddNew();
			Container2.JC_ContainerNum = "CNTR000002";
			Container2.JC_ContainerMode = "BLK";

			PackLine21 = Shipment.OuterPackLines.AddNew();
			PackLine21.JL_JS = Shipment.PK;
			PackLine21.JL_JC = Container2.PK;
			PackLine21.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Piece;
			PackLine21.JL_RefNumber = "RefNum21";

			PackLine22 = Shipment.OuterPackLines.AddNew();
			PackLine22.JL_JS = Shipment.PK;
			PackLine22.JL_JC = Container2.PK;
			PackLine22.JL_RefNumber = "RefNum22";

			Container3 = Consol.Containers.AddNew();
			Container3.JC_ContainerNum = "CNTR000003";
			Container3.JC_RC = RefContainer2.PK;
			Container3.JC_ContainerMode = "FCL";
			Container3.JC_IsEmptyContainer = true;
			PackLine31 = Shipment.OuterPackLines.AddNew();
			PackLine31.JL_JS = Shipment.PK;
			PackLine31.JL_JC = Container3.PK;
			PackLine31.JL_RefNumber = "RefNum31";

			#endregion
		}

		#region Implementation

		OrgHeader CFSOrg;
		OrgHeader SendingForwarder;
		OrgHeader ShippingLine;
		Transport ALPOTransport;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		RefContainer RefContainer1;
		RefContainer RefContainer2;

		CommonContainer Container1;
		PackLine PackLine11;
		PackLine PackLine12;
		CommonContainer Container2;
		PackLine PackLine21;
		PackLine PackLine22;
		CommonContainer Container3;
		PackLine PackLine31;
		UNDGSubstance UNDG;

		ALPOConsolWithShipmentValueObjectDataAdapter Adapter;

		#endregion

		#endregion
	}
}
