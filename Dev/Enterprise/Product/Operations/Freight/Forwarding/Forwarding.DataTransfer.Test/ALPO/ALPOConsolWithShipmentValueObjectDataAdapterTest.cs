using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ALPOConsolWithShipmentValueObjectDataAdapterTest : TestCaseWithFactory
	{
		#region Sender

		public void TestExportSenderNoCertificateAndNoEnterpriseCodeInLogin()
		{
			var enterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;

			AssertEquals("prerequisite: Enterprise Code exists",
				false, string.IsNullOrWhiteSpace(enterpriseCode));

			AssertEquals("prerequisite: Current User login name does not start with Enterprise Code",
				false, GlbStaff.CurrentUser.GS_LoginName.StartsWith(enterpriseCode));

			FreightDataRegistry.Instance.ALPOExportClientNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestZKV");

			var result = Adapter.ExportSender(Shipment);
			AssertEquals(GlbStaff.CurrentUser.GS_LoginName, result.Benutzer_ID);
		}

		public void TestExportSenderNoCertificateAndEnterpriseCodeInLogin()
		{
			var enterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;

			AssertEquals("prerequisite: Enterprise Code exists",
				false, string.IsNullOrWhiteSpace(enterpriseCode));

			AssertEquals("prerequisite: Current User login name does not start with Enterprise Code",
				false, GlbStaff.CurrentUser.GS_LoginName.StartsWith(enterpriseCode));

			var loginNameWithoutEnterpriseCode = GlbStaff.CurrentUser.GS_LoginName;

			currentStaff.GS_LoginName = enterpriseCode + "." + GlbStaff.CurrentUser.GS_LoginName;
			Factory.Save();

			AssertEquals("prerequisite: Current User login name starts with Enterprise Code",
				true, GlbStaff.CurrentUser.GS_LoginName.StartsWith(enterpriseCode));

			FreightDataRegistry.Instance.ALPOExportClientNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestZKV");

			var result = Adapter.ExportSender(Shipment);
			AssertEquals(loginNameWithoutEnterpriseCode, result.Benutzer_ID);
		}

		public void TestExportSenderWithCertificate()
		{
			GenRegCertAccredMaintList certificate = currentStaff.Certificates.AddNew();
			certificate.XZ_Type = CertificateTypePairList.Codes.DB1;
			certificate.XZ_RefNumber = "DBHLicNumber";

			Factory.Save();

			string exportClientNo = "TestZKV";
			FreightDataRegistry.Instance.ALPOExportClientNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exportClientNo);
			AssertEquals(exportClientNo, FreightDataRegistry.Instance.ALPOExportClientNo.Value);

			var result = Adapter.ExportSender(Shipment);
			AssertEquals(exportClientNo, result.ID_SENDERSYSTEM);
			AssertEquals(Shipment.JS_UniqueConsignRef, result.SENDER_REFERENZ);
			AssertEquals("DBHLicNumber", result.Benutzer_ID);
		}

		#endregion

		#region GetKommunikationsart

		public void TestGetKommunikationsart_Origin()
		{
			Shipment.JS_RL_NKOrigin = "DEBRE";
			Xsd.Kommunikationsart result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.BHT, result);

			Shipment.JS_RL_NKOrigin = "DEBRV";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.BHT, result);

			Shipment.JS_RL_NKOrigin = "DEHAM";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.ZAPP, result);

			Transport transport = Shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			transport.JW_RL_NKLoadPort = "DEBRE";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.BHT, result);

			transport.JW_RL_NKLoadPort = "DEBRV";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.BHT, result);

			transport.JW_RL_NKLoadPort = "DEHAM";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.ZAPP, result);
		}

		public void TestGetKommunikationsart_Destination()
		{
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "DEBRE";
			Xsd.Kommunikationsart result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.BHT, result);

			Shipment.JS_RL_NKDestination = "DEHAM";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.ZAPP, result);

			Transport transport = Shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			Shipment.JS_RL_NKDestination = "AUSYD";
			transport.JW_RL_NKDiscPort = "DEBRE";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.BHT, result);

			transport.JW_RL_NKDiscPort = "DEHAM";
			result = Adapter.GetKommunikationsart(Shipment);
			AssertEquals(Xsd.Kommunikationsart.ZAPP, result);
		}

		#endregion

		#region Warenrichtung

		public void TestWarenrichtung()
		{
			Shipment.JS_RL_NKOrigin = "DEBRE";
			Xsd.Warenrichtung result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			Shipment.JS_RL_NKOrigin = "DECUX";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			Shipment.JS_RL_NKOrigin = "DEBRV";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			Shipment.JS_RL_NKOrigin = "DEHAM";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "DEBRE";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);

			Shipment.JS_RL_NKDestination = "DECUX";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);

			Shipment.JS_RL_NKDestination = "DEBRV";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);

			Shipment.JS_RL_NKOrigin = "DEHAM";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);

			Transport transport = Shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "NZAKL";

			transport.JW_RL_NKLoadPort = "DEBRE";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			transport.JW_RL_NKLoadPort = "DECUX";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			transport.JW_RL_NKLoadPort = "DEBRV";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			transport.JW_RL_NKLoadPort = "DEHAM";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Export, result);

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "DEBRE";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);

			transport.JW_RL_NKDiscPort = "DECUX";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);

			transport.JW_RL_NKDiscPort = "DEBRV";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);

			transport.JW_RL_NKDiscPort = "DEHAM";
			result = Adapter.GetWarenrichtung(Shipment);
			AssertEquals(Xsd.Warenrichtung.Import, result);
		}

		#endregion

		#region Auftragsart

		public void TestAuftragsart_Export()
		{
			Shipment.JS_RL_NKOrigin = "DEBRE";
			string result = Adapter.GetOrderType(Shipment);
			AssertEquals("125", result);

			Shipment.JS_RL_NKOrigin = "DEBRV";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("125", result);

			Shipment.JS_PackingMode = "ROR";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("023", result);

			Shipment.JS_PackingMode = "LCL";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			Shipment.JS_PackingMode = "LQD";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			Shipment.JS_RL_NKOrigin = "DEHAM";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("HDS", result);

			Transport transport = Shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_PackingMode = "FCL";

			transport.JW_RL_NKLoadPort = "DEBRE";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("125", result);

			transport.JW_RL_NKLoadPort = "DEBRV";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("125", result);

			Shipment.JS_PackingMode = "ROR";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("023", result);

			Shipment.JS_PackingMode = "LCL";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			Shipment.JS_PackingMode = "LQD";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			transport.JW_RL_NKLoadPort = "DEHAM";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("HDS", result);
		}

		public void TestAuftragsart_Import()
		{
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "DEBRE";
			string result = Adapter.GetOrderType(Shipment);
			AssertEquals("138", result);

			Shipment.JS_RL_NKDestination = "DEBRV";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("138", result);

			Shipment.JS_PackingMode = "ROR";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("023", result);

			Shipment.JS_PackingMode = "LCL";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			Shipment.JS_PackingMode = "LQD";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			Shipment.JS_RL_NKDestination = "DEHAM";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("HDS", result);

			Transport transport = Shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_PackingMode = "FCL";

			transport.JW_RL_NKDiscPort = "DEBRE";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("138", result);

			transport.JW_RL_NKDiscPort = "DEBRV";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("138", result);

			Shipment.JS_PackingMode = "ROR";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("023", result);

			Shipment.JS_PackingMode = "LCL";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			Shipment.JS_PackingMode = "LQD";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("022", result);

			transport.JW_RL_NKDiscPort = "DEHAM";
			result = Adapter.GetOrderType(Shipment);
			AssertEquals("HDS", result);
		}

		#endregion

		#region Containerauftrag

		public void TestGetContainerauftrag()
		{
			Shipment.JS_PackingMode = "FCL";
			Xsd.Containerauftrag result = Adapter.GetContainerauftrag(Shipment);
			AssertEquals(Xsd.Containerauftrag.True, result);
			Shipment.JS_PackingMode = "LCL";
			result = Adapter.GetContainerauftrag(Shipment);
			AssertEquals(Xsd.Containerauftrag.False, result);
			Shipment.JS_PackingMode = "ROR";
			result = Adapter.GetContainerauftrag(Shipment);
			AssertEquals(Xsd.Containerauftrag.False, result);
		}

		#endregion

		#region AdresseAG

		public void TestGetAdresseAG()
		{
			Xsd.AdresseAG result = Adapter.GetAdresseAG(Shipment);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.OH_FullName, result.Name);
			Assert(result.Referenzen.IsSpecified);

			string orgProxyName = GlbCompany.CurrentCompany.OrgProxy.OH_FullNameTruncated;
			try
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_FullName = "Organization name longer than 35 characters GmBH";
				result = Adapter.GetAdresseAG(Shipment);
				AssertEquals("Max accepted length of name: 35 chars", "Organization name longer than 35 ch", result.Name);
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_FullName = orgProxyName;
			}

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			result = Adapter.GetAdresseAG(Shipment);
			AssertEquals(ZString.Empty, result.Name);
			Assert(result.Referenzen.IsSpecified);
		}

		public void TestGetAdresseAGReferenzen()
		{
			Shipment.JS_PackingMode = "BBK";
			Xsd.Referenzen result = Adapter.GetAdresseAGReferenzen(Shipment);
			AssertEquals(Shipment.JS_UniqueConsignRef, result.Sender);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, result.Abteilung);
			AssertEquals(Shipment.JS_UniqueConsignRef, result.SpeditionsbuchNr);
		}

		public void TestGetAdresseAGReferenzenForFCL()
		{
			ForwardingShipment shipmentForTest = Factory.New<ForwardingShipment>();
			shipmentForTest.JS_UniqueConsignRef = "ShipConsRef";
			shipmentForTest.JS_PackingMode = "FCL";

			Xsd.Referenzen result = null;
			result = Adapter.GetAdresseAGReferenzen(shipmentForTest);
			AssertEquals("JS_UniqueConsignRef as no consols", "ShipConsRef", result.Sender);
			AssertEquals("JS_UniqueConsignRef as no consols", "ShipConsRef", result.SpeditionsbuchNr);

			ForwardingConsol consolForTest = shipmentForTest.Consols.AddNew();
			consolForTest.JK_UniqueConsignRef = "ConsolForTest";

			result = Adapter.GetAdresseAGReferenzen(shipmentForTest);
			AssertEquals("JK_UniqueConsignRef as there is consol", "ConsolForTest", result.Sender);
			AssertEquals("JK_UniqueConsignRef as there is consol", "ConsolForTest", result.SpeditionsbuchNr);
		}

		#endregion

		#region WarenVerpackungsDaten

		public void TestGetWarenVerpackungsDaten()
		{
			string[] containermode = new string[]
			{
					Constants.ContainerModes.BuyersConsol,
					Constants.ContainerModes.Groupage,
					Constants.ContainerModes.FCL,
					Constants.ContainerModes.LCL
			};
			foreach (var mode in containermode)
			{
				Shipment.Containers.First().JC_ContainerMode = mode;
				Xsd.WarenVerpackungsDatenCollection result = Adapter.GetEquipmentsAndGoodsData(Shipment);
				AssertEquals(1, result[0].Zaehler);
				AssertEquals(2, result[1].Zaehler);
				AssertEquals(Xsd.Gefahrgut.True, result[0].Gefahrgut);
				AssertEquals(Xsd.Gefahrgut.False, result[1].Gefahrgut);
				Assert("Should be Spezifikation item for containers in FCL,LCL", result[0].Spezifikation.IsSpecified);
				Assert(result[0].WarenPositionen.IsSpecified);
				Assert(result[1].WarenPositionen.IsSpecified);
			}
		}

		#endregion

		#region MiscellaneousRemarks

		public void TestGetMiscellaneousRemarks()
		{
			Xsd.DivAngaben result = Adapter.GetMiscellaneousRemarks(Shipment);
			AssertEquals(Consol.JK_BookingReference, result.Buchungsnummer);
			AssertEquals(Shipment.JS_MarksAndNumbers, result.Hauptmarkierung);
		}

		#endregion

		#region Verkehrstraeger

		public void TestGetVerkehrstraeger()
		{
			ALPOTransport.JW_TransportMode = "SEA";
			Xsd.Verkehrstraeger result = Adapter.GetVerkehrstraeger(Shipment);
			AssertEquals(Xsd.Verkehrstraeger.Ship, result);
			ALPOTransport.JW_TransportMode = "RAI";
			result = Adapter.GetVerkehrstraeger(Shipment);
			AssertEquals(Xsd.Verkehrstraeger.Rail, result);
			ALPOTransport.JW_TransportMode = "ROA";
			result = Adapter.GetVerkehrstraeger(Shipment);
			AssertEquals(Xsd.Verkehrstraeger.Truck, result);
			ALPOTransport.JW_TransportMode = "IWT";
			result = Adapter.GetVerkehrstraeger(Shipment);
			AssertEquals(Xsd.Verkehrstraeger.Ship, result);
		}

		#endregion

		#region SchuppenCode

		public void TestSchuppenCode()
		{
			Consol.JK_OA_PackDepotAddress = CFSOrgAddress1.PK;
			string result = Adapter.GetSchuppenCode(Shipment);
			AssertEquals("SchuppenCode should be found from Consol Departure CFS, as Shipment originated in ALPO port and Shipment Pickup CFS is empty.", "ADZA", result);

			Shipment.JS_OA_ExportReceivingDepot = CFSOrgAddress2.PK;
			result = Adapter.GetSchuppenCode(Shipment);
			AssertEquals("SchuppenCode should be found from Shipment Pickup CFS, as Shipment originated in ALPO port.", "ADZB", result);
		}

		#endregion

		#region Port

		public void TestPort()
		{
			Xsd.Hafen result = Adapter.GetPort(Shipment);
			AssertEquals(Shipment.JS_RL_NKOrigin, result.LadeHafen);
			AssertEquals(Shipment.JS_RL_NKDestination, result.LoeschHafen);
			AssertEquals(Shipment.JS_RL_NKDestination, result.EndbestimmungsHafen);

			Transport transport = Shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "DEBRV";
			transport.JW_RL_NKDiscPort = "AUMEL";
			result = Adapter.GetPort(Shipment);
			AssertEquals(transport.JW_RL_NKLoadPort, result.LadeHafen);
			AssertEquals(transport.JW_RL_NKDiscPort, result.LoeschHafen);
		}

		#endregion

		#region AddresseCA

		public void TestGetAddresseCA()
		{
			ForwardingConsol secondConsol = Shipment.Consols.AddNew();
			secondConsol.JK_RL_NKLoadPort = "USORD";
			secondConsol.JK_RL_NKDischargePort = "USLAX";

			Transport secondTransport = secondConsol.Transports[0];
			secondTransport.JW_ETD = ZDateTime.Today;
			secondTransport.JW_ETA = ZDateTime.Today.AddDays(-1);

			Transport transport = Consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today.AddDays(-10);
			transport.JW_ETA = ZDateTime.Today.AddDays(-11);

			OrgHeader secondShippingLine = Factory.New<OrgHeader>();
			secondShippingLine.OH_FullName = "ShippingLine 2";
			secondShippingLine.OH_Code = "SHIPLINE 2";
			secondConsol.JK_OA_ShippingLineAddress = secondShippingLine.MainAddress.PK;

			RefCountry deCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "DE"));
			ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "101", deCountry);
			ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.BHT, "102", deCountry);
			secondShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "201", deCountry);
			secondShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.BHT, "202", deCountry);

			Xsd.AdresseCA result = Adapter.GetAddresseCA(Shipment);
			AssertEquals(ShippingLine.OH_FullName, result.Name);

			Shipment.JS_RL_NKOrigin = "DEHAM";
			result = Adapter.GetAddresseCA(Shipment);
			AssertEquals("ALPO DEHAM export, earliest consol is used.", "101", result.Kundennummer.Nummer);

			Shipment.JS_RL_NKOrigin = "DEBRE";
			result = Adapter.GetAddresseCA(Shipment);
			AssertEquals("ALPO DEBRE export, earliest consol is used.", "102", result.Kundennummer.Nummer);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "DEHAM";
			result = Adapter.GetAddresseCA(Shipment);
			AssertEquals("ALPO DEHAM import, latest consol is used.", "201", result.Kundennummer.Nummer);

			Shipment.JS_RL_NKDestination = "DEBRE";
			result = Adapter.GetAddresseCA(Shipment);
			AssertEquals("ALPO DEBRE import, latest consol is used.", "202", result.Kundennummer.Nummer);
		}

		#endregion

		#region DG

		public void TestDG()
		{
			AssertEquals(Xsd.Gefahrgut.True, Adapter.GetDG(Container1.PackLines.ToArray<PackLine>()));
			AssertEquals(Xsd.Gefahrgut.False, Adapter.GetDG(Container2.PackLines.ToArray<PackLine>()));
		}

		#endregion

		#region Specification

		public void TestGetSpecification()
		{
			Xsd.Spezifikation result = Adapter.GetSpecification(Container1);

			AssertEquals(result.Identifikation, Container1.JC_ContainerNum);
			AssertEquals(result.ContainerType, Container1.Container.RC_ISOType);
			AssertEquals(result.ShipperOwned, Xsd.ShipperOwned.False);
			AssertEquals(result.ShipperOwnedSpecified, true);
			AssertEquals(result.LeerContainer, Xsd.LeerContainer.False);
			AssertEquals(result.LeerContainerSpecified, true);
			AssertEquals(result.MasseGewichte.IsSpecified, true);
			AssertEquals(result.ContainerZusatzangaben.IsSpecified, true);
			AssertEquals(result.ContainerZusatzangaben.Siegelnummer1, Container1.JC_SealNum);
			AssertEquals(result.ContainerZusatzangaben.Siegelnummer2, Container1.JC_AdditionalSealNum);
			AssertEquals(result.ContainerZusatzangaben.Siegelnummer3, Container1.JC_Additional2SealNum);

			Container1.JC_RC = ZGuid.Empty;
			result = Adapter.GetSpecification(Container1);

			AssertEquals(result.Identifikation, Container1.JC_ContainerNum);
			AssertEquals(result.ContainerType, ZString.Empty);
			AssertEquals(result.ShipperOwned, Xsd.ShipperOwned.False);
			AssertEquals(result.ShipperOwnedSpecified, true);
			AssertEquals(result.LeerContainer, Xsd.LeerContainer.False);
			AssertEquals(result.LeerContainerSpecified, true);
			AssertEquals(result.MasseGewichte.IsSpecified, true);
			AssertEquals(result.ContainerZusatzangaben.IsSpecified, true);
			AssertEquals(result.ContainerZusatzangaben.Siegelnummer1, Container1.JC_SealNum);
			AssertEquals(result.ContainerZusatzangaben.Siegelnummer2, Container1.JC_AdditionalSealNum);
			AssertEquals(result.ContainerZusatzangaben.Siegelnummer3, Container1.JC_Additional2SealNum);
		}

		#region GetMassAndWeight

		public void TestGetMassAndWeight()
		{
			Xsd.MasseGewichte result = Adapter.GetMassAndWeight(Container1);

			Assert(result.Gewichte.TaraGewichtSpecified);
			Assert(result.Gewichte.TaraEinheitSpecified);
			AssertEquals(Core.Constants.Weight.Convert(Container1.JC_TareWeight, Container1.JC_GrossWeightUQ, Core.Constants.Weight.Kilograms), result.Gewichte.TaraGewicht);
			AssertEquals(Xsd.TaraEinheit.kg, result.Gewichte.TaraEinheit);

			AssertEquals(Core.Constants.Weight.Convert(Container1.JC_GrossWeight, Container1.JC_GrossWeightUQ, Core.Constants.Weight.Kilograms), result.Gewichte.BruttoGewicht);
			AssertEquals(Xsd.BruttoEinheit.kg, result.Gewichte.BruttoEinheit);

			Assert(result.Gewichte.NettoGewichtSpecified);
			Assert(result.Gewichte.NettoEinheitSpecified);
			AssertEquals(Core.Constants.Weight.Convert(Container1.GoodsWeight, Container1.GoodsWeightUQ, Core.Constants.Weight.Kilograms), result.Gewichte.NettoGewicht);
			AssertEquals(Xsd.NettoEinheit.kg, result.Gewichte.NettoEinheit);

			Assert(result.Volumen.WertSpecified);
			Assert(result.Volumen.EinheitSpecified);
			AssertEquals(Container1.JC_Calc_TotalVolumeInM3, result.Volumen.Wert);
			AssertEquals(Xsd.Einheit.cbm, result.Volumen.Einheit);

			result = Adapter.GetMassAndWeight(Container2);
			Assert(!result.Gewichte.TaraGewichtSpecified);
			Assert(!result.Gewichte.TaraEinheitSpecified);
			Assert(!result.Gewichte.NettoGewichtSpecified);
			Assert(!result.Gewichte.NettoEinheitSpecified);
			Assert(!result.Volumen.WertSpecified);
			Assert(!result.Volumen.EinheitSpecified);
		}

		#endregion

		#endregion

		#region OuterPacks

		public void TestOuterPacks()
		{
			Xsd.WarenDatenCollection result = Adapter.GetOuterPacks(Container1.PackLines.ToArray<PackLine>(), Container1);
			Assert(result.Count == 2);

			Xsd.WarenDaten newItem = result[0];

			AssertEquals(1, newItem.Zaehler);
			AssertEquals(PackLine11.JL_OutturnComment, newItem.Bemerkungen);
			AssertEquals(Xsd.Gefahrgut.True, newItem.Gefahrgut);
			AssertEquals(PackLine11.JL_MarksAndNumbers, newItem.Markierung);
			Assert(newItem.WarenAngaben.IsSpecified);

			newItem = result[1];
			AssertEquals(2, newItem.Zaehler);
			AssertEquals(PackLine12.JL_OutturnComment, newItem.Bemerkungen);
			AssertEquals(Xsd.Gefahrgut.False, newItem.Gefahrgut);
			AssertEquals(PackLine12.JL_MarksAndNumbers, newItem.Markierung);
			Assert(newItem.WarenAngaben.IsSpecified);
		}

		#endregion

		#region GoodsInformation

		public void TestGetGoodsInformation()
		{
			Xsd.WarenAngaben result = Adapter.GetGoodsInformation(1, 1, PackLine11, Container1);
			AssertEquals(PackLine11.JL_PackageCount.ToString(), result.Anzahl);
			AssertEquals(Xsd.VerpackungsArt.CT, result.VerpackungsArt);
			AssertEquals(PackLine11.JL_Description, result.Warenbeschreibung);
			AssertEquals(ZString.Empty, result.BLGWaCo);
			AssertEquals(PackLine11.JL_MarksAndNumbers, result.MarksNos);
		}

		#endregion

		#region PackagingType

		public void TestGetPackagingType()
		{
			Xsd.VerpackungsArt result = Adapter.GetPackagingType(PackLine11);
			AssertEquals(Xsd.VerpackungsArt.CT, result);
			result = Adapter.GetPackagingType(PackLine12);
			AssertEquals(Xsd.VerpackungsArt.BA, result);
			result = Adapter.GetPackagingType(PackLine21);
			AssertEquals(Xsd.VerpackungsArt.CV, result);

			PackLine packline = Shipment.OuterPackLines.AddNew();

			packline.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.BaleCompressed;
			AssertEquals(Xsd.VerpackungsArt.BL, Adapter.GetPackagingType(packline));

			packline.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.BaleUncompressed;
			AssertEquals(Xsd.VerpackungsArt.BN, Adapter.GetPackagingType(packline));

			packline.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Box;
			AssertEquals(Xsd.VerpackungsArt.BX, Adapter.GetPackagingType(packline));

			packline.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Case;
			AssertEquals(Xsd.VerpackungsArt.CS, Adapter.GetPackagingType(packline));

			packline.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Package;
			AssertEquals(Xsd.VerpackungsArt.PK, Adapter.GetPackagingType(packline));

			packline.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Pallet;
			AssertEquals(Xsd.VerpackungsArt.PX, Adapter.GetPackagingType(packline));

			packline.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Skid;
			AssertEquals(Xsd.VerpackungsArt.SI, Adapter.GetPackagingType(packline));
		}

		#endregion

		#region DangerousGoodsData

		public void TestGetDangerousGoodsData()
		{
			PackLine11.UNDGs[0].DI_DGFlashPoint = -12.85;
			var result = Adapter.GetDangerousGoodsData(PackLine11, PackLine11.UNDGs[0]);
			AssertEquals("-12", result.Flammpunkt);

			PackLine11.UNDGs[0].DI_DGFlashPoint = 0;
			result = Adapter.GetDangerousGoodsData(PackLine11, PackLine11.UNDGs[0]);
			AssertEquals("0", result.Flammpunkt);

			PackLine11.UNDGs[0].DI_DGFlashPoint = 1.11;
			result = Adapter.GetDangerousGoodsData(PackLine11, PackLine11.UNDGs[0]);

			Assert(result.Zaehler == 1);
			AssertEquals(SendingForwarder.OH_FullName, result.Aussteller);
			AssertEquals(PackLine11.JL_PackageCount.ToString(), result.Anzahl);
			AssertEquals(PackLine11.UNDGs[0].Substance.DG_Class, result.Klasse);
			AssertEquals(PackLine11.UNDGs[0].Substance.DG_UNNO, result.UNNR);
			AssertEquals(PackLine11.UNDGs[0].Substance.DG_SubLabel1, result.PrimaerLabel);
			AssertEquals(Xsd.VPGGruppe.III, result.VPGGruppe);
			AssertEquals(PackLine11.UNDGs[0].Substance.DG_StowCat, result.Staumethode);
			AssertEquals(Xsd.EinheitFP.C, result.EinheitFP);
			AssertEquals("+1", result.Flammpunkt);
			AssertEquals(true, result.EinheitFPSpecified);
			AssertEquals(true, result.FlammpunktSpecified);
			AssertEquals(PackLine11.JL_ActualWeight, result.GGBrutto);
			AssertEquals(PackLine11.UNDGs[0].Substance.DG_PSN, result.TechnischeBezeichnung);
			AssertEquals(Xsd.LimitedQuantities.False, result.LimitedQuantities);
			Assert(result.LimitedQuantitiesSpecified);
		}

		#endregion

		#region VPGGruppe

		public void TestGetVPGGruppe()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var uNDG = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a").FirstOrDefault();
			uNDG.DG_PG = "1";
			AssertEquals(Xsd.VPGGruppe.I, Adapter.GetVPGGruppe(uNDG));
			uNDG.DG_PG = "I";
			AssertEquals(Xsd.VPGGruppe.I, Adapter.GetVPGGruppe(uNDG));
			uNDG.DG_PG = "2";
			AssertEquals(Xsd.VPGGruppe.II, Adapter.GetVPGGruppe(uNDG));
			uNDG.DG_PG = "II";
			AssertEquals(Xsd.VPGGruppe.II, Adapter.GetVPGGruppe(uNDG));
			uNDG.DG_PG = "3";
			AssertEquals(Xsd.VPGGruppe.III, Adapter.GetVPGGruppe(uNDG));
			uNDG.DG_PG = "III";
			AssertEquals(Xsd.VPGGruppe.III, Adapter.GetVPGGruppe(uNDG));
		}

		#endregion

		#region NumListFromOuterPacks

		public void TestGetRefNumListFromOuterPacks()
		{
			List<string> result = Adapter.GetRefNumListFromOuterPacks(Container1);
			AssertEquals("should be only 1 item in result because Lines have same refN", 1, result.Count);
			AssertEquals(PackLine11.JL_RefNumber, result[0]);
			result = Adapter.GetRefNumListFromOuterPacks(Container2);
			AssertEquals("should be 2 items", 2, result.Count);
			AssertEquals(PackLine21.JL_RefNumber, result[0]);
			AssertEquals(PackLine22.JL_RefNumber, result[1]);
		}

		#endregion

		#region VesselIdentification

		public void TestGetVesselIdentification()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			Xsd.Schiffidentifikation result = Adapter.GetVesselIdentification(shipment);
			AssertEquals(false, result.IsSpecified);
			AssertEquals("", result.SchiffsName);

			ForwardingConsol consol = shipment.Consols.AddNew();
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals(false, result.IsSpecified);
			AssertEquals("", result.SchiffsName);

			Transport transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "DEHAM";
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals(false, result.IsSpecified);
			AssertEquals("", result.SchiffsName);

			consol.Transports[0].JW_Vessel = "Vessel1";
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals(true, result.IsSpecified);
			AssertEquals("Vessel1", result.SchiffsName);
			AssertEquals("", result.SchiffsDaten.LloydsNr);

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel1";
			vessel.RV_LloydsNumber = "111";
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("111", result.SchiffsDaten.LloydsNr);

			transport.JW_Vessel = "Vessel2";
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals(true, result.IsSpecified);
			AssertEquals("Vessel2", result.SchiffsName);
			AssertEquals("", result.SchiffsDaten.LloydsNr);

			vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel2";
			vessel.RV_LloydsNumber = "222";
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("222", result.SchiffsDaten.LloydsNr);

			consol.Transports[0].JW_ETD = new ZDateTime(2009, 2, 3);
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("2009-02-03", result.SchiffsDaten.DatumETS.Tag);

			shipment.JS_E_DEP = new ZDateTime(2009, 2, 4);
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("2009-02-04", result.SchiffsDaten.DatumETS.Tag);

			transport.JW_ETD = new ZDateTime(2009, 2, 5);
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("2009-02-05", result.SchiffsDaten.DatumETS.Tag);

			consol.Transports[0].JW_ETA = new ZDateTime(2009, 6, 3);
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("2009-06-03", result.SchiffsDaten.DatumETA.Tag);

			shipment.JS_E_ARV = new ZDateTime(2009, 6, 4);
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("2009-06-04", result.SchiffsDaten.DatumETA.Tag);

			transport.JW_ETA = new ZDateTime(2009, 6, 5);
			result = Adapter.GetVesselIdentification(shipment);
			AssertEquals("2009-06-05", result.SchiffsDaten.DatumETA.Tag);
		}

		#endregion

		#region CustomsData

		public void TestGetCustomsData()
		{
			Xsd.ZollPositionen result = Adapter.GetCustomsData(1, 1, PackLine11, Container1, Shipment);
			AssertEquals("should be only 1 item in result because Lines have same refN", result.ZollDaten.Count, 1);
			AssertEquals("1", result.ZollDaten[0].AESZollDaten.AESZaehler);
			AssertEquals(PackLine11.JL_RefNumber, result.ZollDaten[0].AESZollDaten.MRN);
			AssertEquals(Xsd.MRNKomplett.True, result.ZollDaten[0].AESZollDaten.MRNKomplett);

			result = Adapter.GetCustomsData(1, 1, PackLine11, Container2, Shipment);
			AssertEquals("Container with mode not in FCL-LCL should get inner pack lines RefNs first", 2, result.ZollDaten.Count);
			AssertEquals("1", result.ZollDaten[0].AESZollDaten.AESZaehler);
			AssertEquals("InnerRefN1", result.ZollDaten[0].AESZollDaten.MRN);
			AssertEquals(Xsd.MRNKomplett.True, result.ZollDaten[0].AESZollDaten.MRNKomplett);

			AssertEquals("2", result.ZollDaten[1].AESZollDaten.AESZaehler);
			AssertEquals("InnerRefN2", result.ZollDaten[1].AESZollDaten.MRN);
			AssertEquals(Xsd.MRNKomplett.True, result.ZollDaten[1].AESZollDaten.MRNKomplett);
		}

		public void TestIsSpecified()
		{
			PackLine line = PackLine11;
			line.JL_RefNumber = ZString.Empty;
			line.JL_RH_NKCommodityCode = "C123";
			Xsd.WarenAngaben resultToTest = Adapter.GetGoodsInformation(1, 1, line, null);
			AssertEquals(ZString.Empty, resultToTest.BLGWaCo);
			Assert(!resultToTest.ZollPositionen.IsSpecified);
			line.JL_RefNumber = "REF123";
			line.JL_RH_NKCommodityCode = "123";
			resultToTest = Adapter.GetGoodsInformation(1, 1, line, null);
			AssertEquals("123", resultToTest.BLGWaCo);
			Assert(resultToTest.ZollPositionen.IsSpecified);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Adapter = new ALPOConsolWithShipmentValueObjectDataAdapter();
			currentStaff = Factory.Load<GlbStaff>(Environment.Env.CurrentUser.PK);

			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_BookingReference = "BookingReference";
			Consol.JK_RL_NKLoadPort = "DEBRE";
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
			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			Shipment.JS_MarksAndNumbers = "MarksAndNums";
			Shipment.JS_UniqueConsignRef = "S0000001";
			Shipment.JS_RL_NKOrigin = "DEBRE";
			Shipment.JS_RL_NKDestination = "USLAX";

			ALPOTransport = Shipment.Transports.AddNew();
			ALPOTransport.JW_Vessel = "Transport Name";
			ALPOTransport.JW_TransportType = "PRE";
			ALPOTransport.JW_TransportMode = "SEA";

			RefCountry deCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "DE"));
			CFSOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			CFSOrg1.CustomsCodes.AddNew("EID", "ADZA", deCountry);
			CFSOrgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			CFSOrgAddress1.OA_Code = "CFS1";
			CFSOrgAddress1.OA_OH = CFSOrg1.PK;

			CFSOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			CFSOrg2.CustomsCodes.AddNew("EID", "ADZB", deCountry);
			CFSOrgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			CFSOrgAddress2.OA_Code = "CFS2";
			CFSOrgAddress2.OA_OH = CFSOrg2.PK;
			Factory.Save();

			PackLine innerLine1 = Shipment.InnerPackLines.AddNew();
			innerLine1.JL_RefNumber = "InnerRefN1";
			PackLine innerLine2 = Shipment.InnerPackLines.AddNew();
			innerLine2.JL_RefNumber = "InnerRefN2";
			PackLine innerLine3 = Shipment.InnerPackLines.AddNew();
			innerLine3.JL_RefNumber = "InnerRefN1";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1234";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			UNDG = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "A", "IMO").FirstOrDefault();
			UNDG.DG_PG = "III";
			UNDG.DG_SubLabel1 = "Lbl1";
			UNDG.DG_Class = "X";
			UNDG.DG_StowCat = "A";
			UNDG.DG_PSN = "P";

			#region RefContainers

			RefContainer1 = Factory.New<RefContainer>();
			RefContainer1.RC_Code = "30GP";
			RefContainer1.RC_ISOType = "1234";

			RefContainer2 = Factory.New<RefContainer>();
			RefContainer2.RC_Code = "10GP";
			RefContainer2.RC_ISOType = "1235";

			#endregion

			#region Containers

			Container1 = Consol.Containers.AddNew();
			Container1.JC_ContainerNum = "CNTR000001";
			Container1.JC_ContainerMode = Constants.ContainerModes.LCL;
			Container1.JC_RC = RefContainer1.PK;
			Container1.JC_IsEmptyContainer = false;

			Container1.JC_TareWeight = 1.1;
			Container1.JC_GrossWeight = 1.2;

			Container1.JC_SealNum = "Seal 1";
			Container1.JC_AdditionalSealNum = "Seal 2";
			Container1.JC_Additional2SealNum = "Seal 3";

			PackLine11 = Shipment.OuterPackLines.AddNew();
			PackLine11.JL_JS = Shipment.PK;

			UNDGDataItem dGItem = PackLine11.UNDGs.AddNew();
			dGItem.DI_DG = UNDG.PK;
			dGItem.DI_DGFlashPoint = 1.11;

			PackLine11.JL_JC = Container1.PK;
			PackLine11.JL_RH_NKCommodityCode = "CCd";
			PackLine11.JL_ActualWeight = 11.1;
			PackLine11.JL_OutturnComment = "Outturn Comment";
			PackLine11.JL_MarksAndNumbers = "MarksAndNumbers";
			PackLine11.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Carton;
			PackLine11.JL_Description = "Description";
			PackLine11.JL_RefNumber = "RefNum";
			PackLine11.JL_ActualVolume = 10;

			PackLine12 = Shipment.OuterPackLines.AddNew();
			PackLine12.JL_JS = Shipment.PK;
			PackLine12.JL_JC = Container1.PK;
			PackLine12.JL_ActualWeight = 12.1;
			PackLine12.JL_OutturnComment = "Outturn Comment2";
			PackLine12.JL_MarksAndNumbers = "MarksAndNumbers2";
			PackLine12.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Drum;
			PackLine12.JL_RefNumber = "RefNum";
			PackLine12.JL_ActualVolume = 20;

			Container2 = Consol.Containers.AddNew();
			Container2.JC_ContainerNum = "CNTR000002";
			Container2.JC_ContainerMode = Constants.ContainerModes.Bulk;

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
			Container3.JC_ContainerMode = Constants.ContainerModes.FCL;
			Container3.JC_IsEmptyContainer = true;
			PackLine31 = Shipment.OuterPackLines.AddNew();
			PackLine31.JL_JS = Shipment.PK;
			PackLine31.JL_JC = Container3.PK;
			PackLine31.JL_RefNumber = "RefNum31";

			#endregion
		}

		#region Implementation

		OrgHeader CFSOrg1;
		OrgHeader CFSOrg2;
		OrgAddress CFSOrgAddress1;
		OrgAddress CFSOrgAddress2;
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
		GlbStaff currentStaff;

		#endregion

		#endregion
	}
}
