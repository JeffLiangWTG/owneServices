using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ALPOConsolWithShipmentValueObjectDataAdapter : ValueObjectDataAdapter<ForwardingShipment, Xsd.AdvantageEnterpriseVersion01>
	{
		public ALPOConsolWithShipmentValueObjectDataAdapter()
		{
			Factory = new BusinessObjectFactory();
			Germany = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Germany));
		}

		readonly RefCountry Germany;
		readonly BusinessObjectFactory Factory;

		#region Export

		#region SENDER

		public Xsd.SENDER ExportSender(ForwardingShipment shipment)
		{
			Xsd.SENDER sender = new Xsd.SENDER();
			ExportSender(sender, shipment);
			return sender;
		}

		protected void ExportSender(Xsd.SENDER sENDER, ForwardingShipment shipment)
		{
			sENDER.ID_SENDERSYSTEM = FreightDataRegistry.Instance.ALPOExportClientNo.Value;
			sENDER.SENDER_REFERENZ = shipment.JS_UniqueConsignRef;
			GlbStaff currentUser = Factory.Load<GlbStaff>(Environment.Env.CurrentUser.PK);
			GenRegCertAccredMaintList certificate = currentUser.Certificates.GetFirstCertificate(CertificateTypePairList.Codes.DB1);

			if (certificate == null)
			{
				sENDER.Benutzer_ID = RemoveEnterpriseCodeFromUserLogin(currentUser.GS_LoginName);
			}
			else
			{
				sENDER.Benutzer_ID = certificate.XZ_RefNumber;
			}
		}

		#endregion

		#region EMPFANGSSYSTEM

		public Xsd.EMPFANGSSYSTEM ExportEMPFANGSSYSTEM()
		{
			Xsd.EMPFANGSSYSTEM eMPFANGSSYSTEM = new Xsd.EMPFANGSSYSTEM();
			ExportEMPFANGSSYSTEM(eMPFANGSSYSTEM);
			return eMPFANGSSYSTEM;
		}

		protected void ExportEMPFANGSSYSTEM(Xsd.EMPFANGSSYSTEM eMPFANGSSYSTEM)
		{
			eMPFANGSSYSTEM.ID_EMPFANGSSYSTEM = Xsd.ID_EMPFANGSSYSTEM.ALPO;
		}

		#endregion

		#region NachrichtenZeitstempel

		public Xsd.NachrichtenZeitstempel ExportNachrichtenZeitstempel()
		{
			Xsd.NachrichtenZeitstempel nachrichtenZeitstempel = new Xsd.NachrichtenZeitstempel();
			ExportNachrichtenZeitstempel(nachrichtenZeitstempel);
			return nachrichtenZeitstempel;
		}

		protected void ExportNachrichtenZeitstempel(Xsd.NachrichtenZeitstempel nachrichtenZeitstempel)
		{
			nachrichtenZeitstempel.Tag = ZDateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			nachrichtenZeitstempel.Zeit = ZDateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
		}

		#endregion

		#region Hafenauftrag

		#region Body
#if DEBUG
		public
#endif
 Xsd.Hafenauftrag ExportHafenauftrag(ForwardingShipment shipment)
		{
			Xsd.Hafenauftrag hafenauftrag = new Xsd.Hafenauftrag();
			ExportHafenauftrag(hafenauftrag, shipment);
			return hafenauftrag;
		}

#if DEBUG
		public
#endif
 void ExportHafenauftrag(Xsd.Hafenauftrag hafenauftrag, ForwardingShipment shipment)
		{
			hafenauftrag.KopfDaten = ExportKopfDaten(shipment);
			hafenauftrag.PositionsDaten = ExportPositionsDaten(shipment);
		}

		#endregion

		#region KopfDaten

		#region Body

		public Xsd.KopfDaten ExportKopfDaten(ForwardingShipment shipment)
		{
			Xsd.KopfDaten kopfDaten = new Xsd.KopfDaten();
			ExportKopfDaten(kopfDaten, shipment);
			return kopfDaten;
		}

		void ExportKopfDaten(Xsd.KopfDaten kopfDaten, ForwardingShipment shipment)
		{
			kopfDaten.Bearbeitungszustand = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.Bearbeitungszustand.New;
			kopfDaten.Kommunikationsart = GetKommunikationsart(shipment);
			kopfDaten.Auftragsart = GetOrderType(shipment);
			kopfDaten.Containerauftrag = GetContainerauftrag(shipment);
			kopfDaten.Warenrichtung = GetWarenrichtung(shipment);
			kopfDaten.Gefahrgut = GetDGoods(shipment);
			kopfDaten.GefahrgutSpecified = true;
			kopfDaten.AdresseAG = GetAdresseAG(shipment);
			kopfDaten.Locationen = GetLocationen(shipment);
			kopfDaten.Schiffidentifikation = GetVesselIdentification(shipment);
			kopfDaten.DivAngaben = GetMiscellaneousRemarks(shipment);
			kopfDaten.VorNachTransport = GetVorNachTransport(shipment);
		}

		#endregion

		#region Kommunikationsart

		public Xsd.Kommunikationsart GetKommunikationsart(ForwardingShipment shipment)
		{
			Xsd.Kommunikationsart result = Xsd.Kommunikationsart.BHT;
			RefUNLOCO aLPOPort = ALPOHelper.GetALPOPort(shipment);

			if (aLPOPort != null && aLPOPort.RL_IATA == "HAM")
			{
				result = Xsd.Kommunikationsart.ZAPP;
			}

			return result;
		}

		#endregion

		#region Auftragsart - OrderType

		public string GetOrderType(ForwardingShipment shipment)
		{
			string result = string.Empty;
			RefUNLOCO aLPOPort = ALPOHelper.GetALPOPort(shipment);

			if (aLPOPort != null)
			{
				if (aLPOPort.RL_IATA == "HAM")
				{
					result = "HDS";
				}
				else
				{
					switch (shipment.JS_PackingMode)
					{
						case ("FCL"):
							result = (ALPOHelper.IsALPOImport(shipment)) ? "138" : "125";
							break;
						case ("ROR"):
							result = "023";
							break;
						default:
							result = "022";
							break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Containerauftrag

		public Xsd.Containerauftrag GetContainerauftrag(ForwardingShipment shipment)
		{
			Xsd.Containerauftrag result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.Containerauftrag.False;
			if (shipment.JS_PackingMode == "FCL")
			{
				result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.Containerauftrag.True;
			}
			return result;
		}

		#endregion

		#region Warenrichtung

		internal Xsd.Warenrichtung GetWarenrichtung(ForwardingShipment shipment)
		{
			Xsd.Warenrichtung result = Xsd.Warenrichtung.Export;

			if (ALPOHelper.IsALPOImport(shipment))
			{
				result = Xsd.Warenrichtung.Import;
			}

			return result;
		}

		#endregion

		#region Gefahrgut

		Xsd.Gefahrgut GetDGoods(ForwardingShipment shipment1)
		{
			foreach (ForwardingConsol consol in shipment1.Consols)
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					foreach (ForwardingPackLine line in shipment.OuterPackLines)
					{
						foreach (UNDGDataItem dgItem in line.UNDGs)
						{
							if (dgItem.Substance != null)
							{
								return Xsd.Gefahrgut.True;
							}
						}
					}
				}
			}
			return Xsd.Gefahrgut.False;
		}

		#endregion

		#region AdresseAG

		public Xsd.AdresseAG GetAdresseAG(ForwardingShipment shipment)
		{
			Xsd.AdresseAG result = new Xsd.AdresseAG();
			ZString name = GlbCompany.CurrentCompany.OrgProxy != null ? GlbCompany.CurrentCompany.OrgProxy.OH_FullNameTruncated : ZString.Empty;
			result.Name = name.SubstringSafe(0, 35);
			result.Referenzen = GetAdresseAGReferenzen(shipment);
			return result;
		}

		public Xsd.Referenzen GetAdresseAGReferenzen(ForwardingShipment shipment)
		{
			Xsd.Referenzen result = new Xsd.Referenzen();
			bool referenzenSet = false;
			if (shipment.Consols.Count > 0 && (shipment.PackingMode == "FCL" || shipment.PackingMode == "LCL"))
			{
				if (shipment.Consols.Count > 0)
				{
					result.Sender = shipment.Consols[0].JK_UniqueConsignRef;
					result.SpeditionsbuchNr = shipment.Consols[0].JK_UniqueConsignRef;
					referenzenSet = true;
				}
			}
			if (!referenzenSet)
			{
				result.Sender = shipment.JS_UniqueConsignRef;
				result.SpeditionsbuchNr = shipment.JS_UniqueConsignRef;
			}
			result.Abteilung = GlbDepartment.CurrentDepartment.GE_Code;
			return result;
		}

		#endregion

		#region Locationen - Locations

		Xsd.Locationen GetLocationen(ForwardingShipment shipment)
		{
			Xsd.Locationen result = new Xsd.Locationen();
			string schuppenCode = GetSchuppenCode(shipment);
			if (schuppenCode != null)
			{
				result.SchuppenCode = schuppenCode;
			}
			result.Hafen = GetPort(shipment);
			return result;
		}

		#region SchuppenCode

		public string GetSchuppenCode(ForwardingShipment shipment)
		{
			OrgAddress cFSAddress = null;

			if (ALPOHelper.IsALPOExport(shipment))
			{
				cFSAddress = shipment.ExportReceivingDepot;

				if (cFSAddress == null)
				{
					ForwardingConsol consol = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => ALPOHelper.IsALPOPort(c.LoadPort));
					cFSAddress = (consol != null) ? consol.PackDepotAddress : null;
				}
			}
			else if (ALPOHelper.IsALPOImport(shipment))
			{
				cFSAddress = shipment.ImportReleaseDepot;

				if (cFSAddress == null)
				{
					ForwardingConsol consol = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => ALPOHelper.IsALPOPort(c.DischargePort));
					cFSAddress = (consol != null) ? consol.UnpackDepotAddress : null;
				}
			}

			OrgHeader cFS = (cFSAddress != null) ? cFSAddress.Header : null;
			OrgCusCode code = null;

			if (cFS != null)
			{
				code = cFS.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(GermanyOrgCusCodeInfo.OrgCusCodes.CWC, Germany);

				if (code == null)
				{
					code = cFS.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.EDISiteID, Germany);
				}
			}

			return (code != null) ? code.OK_CustomsRegNo : null;
		}

		#endregion

		#region Hafen - Port

		public Xsd.Hafen GetPort(ForwardingShipment shipment)
		{
			Xsd.Hafen result = new Xsd.Hafen();

			RefUNLOCO origin = ALPOHelper.GetOrigin(shipment);
			result.LadeHafen = origin != null ? origin.RL_Code : ZString.Empty;

			RefUNLOCO destination = ALPOHelper.GetDestination(shipment);
			result.LoeschHafen = destination != null ? destination.RL_Code : ZString.Empty;
			result.EndbestimmungsHafen = shipment.JS_RL_NKDestination;

			return result;
		}

		#endregion

		#endregion

		#region Schiffidentifikation - VesselIdentification:

		public Xsd.Schiffidentifikation GetVesselIdentification(ForwardingShipment shipment)
		{
			Xsd.Schiffidentifikation result = new Xsd.Schiffidentifikation();
			result.IsSpecified = false;

			Transport shipmentTransport = ALPOHelper.GetShipmentTransport(shipment);
			Transport consolTransport = null;
			if (shipment.Consols.Count > 0 && shipment.Consols.GetEarliestConsol().Transports.Count > 0)
			{
				consolTransport = shipment.Consols.GetEarliestConsol().Transports[0];
			}

			if (shipmentTransport != null || consolTransport != null)
			{
				ZString vesselName = CoalesceVesselName(shipmentTransport, consolTransport);
				if (!vesselName.IsEmpty)
				{
					result.IsSpecified = true;
					result.SchiffsName = vesselName;
					result.AdresseCA = GetAddresseCA(shipment);
					result.SchiffsDaten = GetSchiffsDaten(shipment, shipmentTransport, consolTransport);
				}
			}

			return result;
		}

		ZString CoalesceVesselName(Transport transport1, Transport transport2)
		{
			if (transport1 != null && !transport1.JW_Vessel.IsEmpty)
			{
				return transport1.JW_Vessel;
			}
			else if (transport2 != null && !transport2.JW_Vessel.IsEmpty)
			{
				return transport2.JW_Vessel;
			}

			return ZString.Empty;
		}

		#region AddresseCA

		public Xsd.AdresseCA GetAddresseCA(ForwardingShipment shipment)
		{
			Xsd.AdresseCA result = new Xsd.AdresseCA();
			RefUNLOCO aLPOPort = ALPOHelper.GetALPOPort(shipment);

			if (aLPOPort != null)
			{
				string code = (aLPOPort.RL_Code == "DEHAM") ? GermanyOrgCusCodeInfo.OrgCusCodes.ZAP : GermanyOrgCusCodeInfo.OrgCusCodes.BHT;
				ForwardingConsol consol = (ALPOHelper.IsALPOImport(shipment)) ? shipment.Consols.GetLatestConsol() : shipment.Consols.GetEarliestConsol();
				OrgCusCode cusCode = null;

				if (consol != null && consol.ShippingLine != null)
				{
					cusCode = consol.ShippingLine.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(code, Germany);
					result.Name = consol.ShippingLine.OH_FullNameTruncated;
				}

				result.Kundennummer.Nummer = (cusCode != null) ? cusCode.OK_CustomsRegNo : ZString.Empty;
				result.Kundennummer.ShouldCreateElementForEmptyValue = false;
			}

			return result;
		}

		#endregion

		#region SchiffsDaten

		Xsd.SchiffsDaten GetSchiffsDaten(ForwardingShipment shipment, Transport shipmentTransport, Transport consolTransport)
		{
			Xsd.SchiffsDaten result = new Xsd.SchiffsDaten();
			RefVessel vessel = CoalesceVessel(shipmentTransport, consolTransport);
			if (vessel != null)
			{
				result.LloydsNr = vessel.RV_LloydsNumber;
			}

			ZDateTime etd = CoalesceETD(shipment, shipmentTransport, consolTransport);
			result.DatumETS.Tag = etd.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			result.DatumETS.Zeit = etd.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

			ZDateTime eta = CoalesceETA(shipment, shipmentTransport, consolTransport);
			result.DatumETA.Tag = eta.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			result.DatumETA.Zeit = eta.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

			return result;
		}

		RefVessel CoalesceVessel(Transport transport1, Transport transport2)
		{
			RefVessel result = null;
			if (transport1 != null && !transport1.JW_Vessel.IsEmpty)
			{
				result = transport1.Vessel;
			}
			else if (transport2 != null && !transport2.JW_Vessel.IsEmpty)
			{
				result = transport2.Vessel;
			}

			return result;
		}

		ZDateTime CoalesceETD(ForwardingShipment shipment, Transport shipmentTransport, Transport consolTransport)
		{
			ZDateTime result = ZDateTime.Empty;
			if (shipmentTransport != null && !shipmentTransport.JW_ETD.IsEmpty)
			{
				result = shipmentTransport.JW_ETD;
			}
			else if (!shipment.JS_E_DEP.IsEmpty)
			{
				result = shipment.JS_E_DEP;
			}
			else if (consolTransport != null && !consolTransport.JW_ETD.IsEmpty)
			{
				result = consolTransport.JW_ETD;
			}

			return result;
		}

		ZDateTime CoalesceETA(ForwardingShipment shipment, Transport shipmentTransport, Transport consolTransport)
		{
			ZDateTime result = ZDateTime.Empty;
			if (shipmentTransport != null && !shipmentTransport.JW_ETA.IsEmpty)
			{
				result = shipmentTransport.JW_ETA;
			}
			else if (!shipment.JS_E_ARV.IsEmpty)
			{
				result = shipment.JS_E_ARV;
			}
			else if (consolTransport != null && !consolTransport.JW_ETA.IsEmpty)
			{
				result = consolTransport.JW_ETA;
			}

			return result;
		}

		#endregion

		#endregion

		#region DivAngaben - MiscellaneousRemarks:

		public Xsd.DivAngaben GetMiscellaneousRemarks(ForwardingShipment shipment)
		{
			Xsd.DivAngaben result = new Xsd.DivAngaben();
			result.Buchungsnummer = shipment.Consols.Count > 0 ? shipment.Consols.GetEarliestConsol().JK_BookingReference : ZString.Empty;
			result.Hauptmarkierung = shipment.JS_MarksAndNumbers;
			return result;
		}

		#endregion

		#region VorNachTransport - Pre and Post carrage:

		Xsd.VorNachTransport GetVorNachTransport(ForwardingShipment shipment)
		{
			Xsd.VorNachTransport result = new Xsd.VorNachTransport();
			result.Verkehrstraeger = GetVerkehrstraeger(shipment);
			return result;
		}

		#region Verkehrstraeger

		public Xsd.Verkehrstraeger GetVerkehrstraeger(ForwardingShipment shipment)
		{
			string transportMode = null;
			foreach (Transport transport in shipment.Transports)
			{
				if (transport.JW_TransportType == "PRE")
				{
					transportMode = transport.JW_TransportMode;
					break;
				}
			}

			Xsd.Verkehrstraeger result = Xsd.Verkehrstraeger.Truck;
			if (transportMode != null)
			{
				switch (transportMode)
				{
					case "SEA":
					case "IWT":
						result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.Verkehrstraeger.Ship;
						break;

					case "RAI":
						result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.Verkehrstraeger.Rail;
						break;

					case "ROA":
						result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.Verkehrstraeger.Truck;
						break;
				}
			}
			return result;
		}

		#endregion

		#endregion

		#endregion

		#region PositionsDaten - GoodsItemsDetails:

		#region Body

		public Xsd.PositionsDaten ExportPositionsDaten(ForwardingShipment shipment)
		{
			Xsd.PositionsDaten positionsDaten = new Xsd.PositionsDaten();
			ExportPositionsDaten(positionsDaten, shipment);
			return positionsDaten;
		}

		void ExportPositionsDaten(Xsd.PositionsDaten positionsDaten, ForwardingShipment shipment)
		{
			positionsDaten.WarenVerpackungsDaten = GetEquipmentsAndGoodsData(shipment);
		}

		#endregion

		#region EquipmentsAndGoodsData

		public Xsd.WarenVerpackungsDatenCollection GetEquipmentsAndGoodsData(ForwardingShipment shipment)
		{
			Xsd.WarenVerpackungsDatenCollection result = new Xsd.WarenVerpackungsDatenCollection();
			int containerCounter = 0;
			int lossContainerCounter = 0;

			foreach (CommonContainer container in shipment.Containers)
			{
				if (container.JC_ContainerMode == Constants.ContainerModes.FCL
					|| container.JC_ContainerMode == Constants.ContainerModes.LCL
					|| container.JC_ContainerMode == Constants.ContainerModes.BuyersConsol
					|| container.JC_ContainerMode == Constants.ContainerModes.Groupage)
				{
					containerCounter++;
					var newItem = result.AddNew();
					newItem.Zaehler = containerCounter;
					newItem.Gefahrgut = GetDG(container.PackLines.ToArray<PackLine>());
					newItem.Spezifikation = GetSpecification(container);
					newItem.WarenPositionen = GetGoodsItems(container);
				}
			}

			if (containerCounter == 0)
			{
				lossContainerCounter++;
				var newItem = result.AddNew();
				newItem.Zaehler = lossContainerCounter;
				newItem.Gefahrgut = GetDG(shipment.OuterPackLines.ToArray<PackLine>());
				newItem.WarenPositionen = GetGoodsItems(shipment.OuterPackLines.ToArray<PackLine>());
				newItem.Spezifikation.ShouldCreateElementForEmptyValue = false;
				newItem.Spezifikation.IsSpecified = false;
			}

			return result;
		}

		#endregion

		#region DG For WarenVerpackungsDaten

		public Xsd.Gefahrgut GetDG(PackLine[] packLines)
		{
			foreach (PackLine packLine in packLines)
			{
				foreach (UNDGDataItem dgItem in packLine.UNDGs)
				{
					if (dgItem.Substance != null)
					{
						return Xsd.Gefahrgut.True;
					}
				}
			}
			return Xsd.Gefahrgut.False;
		}

		#endregion

		#region Specification

		public Xsd.Spezifikation GetSpecification(CommonContainer container)
		{
			Xsd.Spezifikation result = new Xsd.Spezifikation();

			if (container != null)
			{
				result.Identifikation = container.JC_ContainerNum;
				result.ContainerType = container.Container != null ? container.Container.RC_ISOType : ZString.Empty;
				result.LeerContainer = container.JC_IsEmptyContainer ? Xsd.LeerContainer.True : Xsd.LeerContainer.False;
				result.MasseGewichte = GetMassAndWeight(container);
				result.ContainerZusatzangaben = GetContainerZusatzangaben(container);
				result.ShipperOwned = Xsd.ShipperOwned.False;
				result.ShipperOwnedSpecified = true;
				result.LeerContainerSpecified = true;
				result.MasseGewichte.ShouldCreateElementForEmptyValue = false;
				result.ContainerZusatzangaben.ShouldCreateElementForEmptyValue = false;
			}

			return result;
		}

		#region GetContainerZusatzangaben

		Xsd.ContainerZusatzangaben GetContainerZusatzangaben(CommonContainer container)
		{
			Xsd.ContainerZusatzangaben result = new Xsd.ContainerZusatzangaben();

			if (!container.JC_SealNum.IsEmpty)
			{
				result.Siegelnummer1 = container.JC_SealNum;
			}

			if (!container.JC_AdditionalSealNum.IsEmpty)
			{
				result.Siegelnummer2 = container.JC_AdditionalSealNum;
			}

			if (!container.JC_Additional2SealNum.IsEmpty)
			{
				result.Siegelnummer3 = container.JC_Additional2SealNum;
			}

			return result;
		}

		#endregion

		#region GetMassAndWeight

		public Xsd.MasseGewichte GetMassAndWeight(CommonContainer container)
		{
			Xsd.MasseGewichte result = new Xsd.MasseGewichte();

			if (container != null)
			{
				result.Ueberstaende.IsSpecified = false;
				result.Masse.IsSpecified = false;

				if (!container.JC_TareWeight.IsEmpty)
				{
					result.Gewichte.TaraGewicht = Core.Constants.Weight.Convert(container.JC_TareWeight, container.JC_GrossWeightUQ, Core.Constants.Weight.Kilograms);
					result.Gewichte.TaraEinheit = Xsd.TaraEinheit.kg;
				}

				if (!container.JC_GrossWeight.IsEmpty)
				{
					result.Gewichte.BruttoGewicht = Core.Constants.Weight.Convert(container.JC_GrossWeight, container.JC_GrossWeightUQ, Core.Constants.Weight.Kilograms);
					result.Gewichte.BruttoEinheit = Xsd.BruttoEinheit.kg;
				}

				if (!container.GoodsWeight.IsEmpty)
				{
					result.Gewichte.NettoGewicht = Core.Constants.Weight.Convert(container.GoodsWeight, container.GoodsWeightUQ, Core.Constants.Weight.Kilograms);
					result.Gewichte.NettoEinheit = Xsd.NettoEinheit.kg;
				}

				result.Gewichte.ShouldCreateElementForEmptyValue = false;

				if (!container.JC_Calc_TotalVolumeInM3.IsEmpty)
				{
					result.Volumen.Wert = container.JC_Calc_TotalVolumeInM3;
					result.Volumen.Einheit = Xsd.Einheit.cbm;
					result.Volumen.EinheitSpecified = result.Volumen.WertSpecified = true;
				}
				else
				{
					result.Volumen.IsSpecified = false;
				}
			}

			result.Volumen.ShouldCreateElementForEmptyValue = false;

			return result;
		}

		#endregion

		#endregion

		#region WarenPositionen - GoodsItems:

		Xsd.WarenPositionen GetGoodsItems(CommonContainer container)
		{
			Xsd.WarenPositionen result = new Xsd.WarenPositionen();
			result.WarenDaten = GetOuterPacks(container.PackLines.ToArray<PackLine>(), container);
			return result;
		}

		Xsd.WarenPositionen GetGoodsItems(PackLine[] packLines)
		{
			Xsd.WarenPositionen result = new Xsd.WarenPositionen();
			result.WarenDaten = GetOuterPacks(packLines, null);
			return result;
		}

		#region WarenDaten - OuterPacks

		public Xsd.WarenDatenCollection GetOuterPacks(PackLine[] packLines, CommonContainer container)
		{
			Xsd.WarenDatenCollection result = new Xsd.WarenDatenCollection();
			int warenDatenCounter = 0;
			int refNCounter = 0;
			foreach (PackLine line in packLines)
			{
				if (line.IsOuterPackType)
				{
					warenDatenCounter++;
					Xsd.WarenDaten newItem = result.AddNew();
					newItem.Zaehler = warenDatenCounter;
					newItem.Bemerkungen = line.JL_OutturnComment;

					newItem.Gefahrgut = Xsd.Gefahrgut.False;
					foreach (UNDGDataItem dgItem in line.UNDGs)
					{
						if (dgItem.Substance != null)
						{
							newItem.Gefahrgut = Xsd.Gefahrgut.True;
							break;
						}
					}

					newItem.Markierung = line.JL_MarksAndNumbers;
					newItem.WarenAngaben = GetGoodsInformation(refNCounter, warenDatenCounter, line, container);
				}
			}
			return result;
		}

		#region GoodsInformation

		public Xsd.WarenAngaben GetGoodsInformation(int refNCounter, int outerPackNumber, PackLine line, CommonContainer container)
		{
			Xsd.WarenAngaben result = new Xsd.WarenAngaben();
			result.Umschlagshinweise.IsSpecified = false;
			result.ZusatzAngaben.IsSpecified = false;

			result.Anzahl = line.JL_PackageCount.ToString();
			result.VerpackungsArt = GetPackagingType(line);
			result.VerpackungsArtSpecified = true;
			result.Warenbeschreibung = line.JL_Description;
			result.BLGWaCo = line.JL_RH_NKCommodityCode.IsNumbersOnlyOrEmpty ? line.JL_RH_NKCommodityCode : ZString.Empty;
			result.MarksNos = line.JL_MarksAndNumbers;

			if (line.UNDGs.Count > 0)
			{
				foreach (UNDGDataItem dGDataItem in line.UNDGs)
				{
					if (dGDataItem.Substance != null)
					{
						result.GefahrgutPositionen.GefahrgutDaten.Add(GetDangerousGoodsData(line, dGDataItem));
					}
				}
			}
			else
			{
				result.GefahrgutPositionen.ShouldCreateElementForEmptyValue = false;
				result.GefahrgutPositionen.IsSpecified = false;
			}
			if (line.Shipment.JS_PackingMode == "FCL" || line.Shipment.JS_PackingMode == "LCL")
			{
				result.MasseGewichte.IsSpecified = false;
			}
			else
			{
				result.MasseGewichte = GetWarenAngabenMasseGewichte(line);
			}

			result.ZollPositionen.ShouldCreateElementForEmptyValue = false;
			result.ZollPositionen = GetCustomsData(refNCounter, outerPackNumber, line, container, line.Shipment);
			result.ZollPositionen.IsSpecified = !line.JL_RefNumber.IsEmpty;

			return result;
		}

		#region DangerousGoodsData

		public Xsd.GefahrgutDaten GetDangerousGoodsData(PackLine line, UNDGDataItem uNDGItem)
		{
			Xsd.GefahrgutDaten result = new Xsd.GefahrgutDaten();
			result.Land.IsSpecified = false;
			result.Radioaktiv.IsSpecified = false;
			result.Zaehler = 1;
			OrgHeader sendingForwarder = line.Shipment.Consols.Count > 0 && line.Shipment.Consols[0] != null ? line.Shipment.Consols[0].SendingForwarder : null;
			if (sendingForwarder != null)
			{
				result.Aussteller = sendingForwarder.OH_FullNameTruncated;
			}

			result.Anzahl = line.JL_PackageCount.ToString();

			result.Klasse = uNDGItem.Substance.DG_Class;
			result.UNNR = uNDGItem.Substance.DG_UNNO;
			result.PrimaerLabel = uNDGItem.Substance.DG_SubLabel1;
			result.VPGGruppe = GetVPGGruppe(uNDGItem.Substance);
			result.Staumethode = uNDGItem.Substance.DG_StowCat;
			result.EinheitFP = Xsd.EinheitFP.C;
			result.Flammpunkt = decimal.Truncate(uNDGItem.DI_DGFlashPoint).ToString("+#;-#;0");
			result.EinheitFPSpecified = !result.Flammpunkt.IsEmpty;
			result.FlammpunktSpecified = !result.Flammpunkt.IsEmpty;
			result.GGBrutto = line.JL_ActualWeight;
			result.TechnischeBezeichnung = uNDGItem.Substance.DG_PSN;
			result.LimitedQuantities = uNDGItem.DI_IsLimitedQuantity ? Xsd.LimitedQuantities.True : Xsd.LimitedQuantities.False;
			result.LimitedQuantitiesSpecified = true;

			return result;
		}

		#endregion

		#region WarenAngabenMasseGewichte

		Xsd.MasseGewichte GetWarenAngabenMasseGewichte(PackLine line)
		{
			Xsd.MasseGewichte result = new Xsd.MasseGewichte();
			result.Ueberstaende.IsSpecified = false;
			result.Masse.IsSpecified = false;

			result.Volumen.ShouldCreateElementForEmptyValue = false;
			result.Gewichte.ShouldCreateElementForEmptyValue = false;

			if (line.JL_ActualWeight != ZDecimal.Zero)
			{
				result.Gewichte.BruttoGewicht = line.JL_ActualWeight;
				result.Gewichte.BruttoEinheit = Xsd.BruttoEinheit.kg;
				result.Gewichte.BruttoEinheitSpecified = result.Gewichte.BruttoGewichtSpecified = true;
			}
			else
			{
				result.Gewichte.IsSpecified = false;
			}

			if (line.JL_ActualVolume != ZDecimal.Zero)
			{
				result.Volumen.Wert = line.JL_ActualVolume;
				result.Volumen.Einheit = Xsd.Einheit.cbm;
				result.Volumen.EinheitSpecified = result.Volumen.WertSpecified = true;
			}
			else
			{
				result.Volumen.IsSpecified = false;
			}

			return result;
		}

		#endregion

		#region VerpackungsArt - PackagingType:

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public Xsd.VerpackungsArt GetPackagingType(PackLine line)
		{
			Xsd.VerpackungsArt result = Xsd.VerpackungsArt.CH;
			switch (line.JL_F3_NKPackType)
			{
				case Enterprise.Core.Constants.PkgUnit.Bag:
					result = Xsd.VerpackungsArt.BG;
					break;
				case Enterprise.Core.Constants.PkgUnit.BaleCompressed:
					result = Xsd.VerpackungsArt.BL;
					break;
				case Enterprise.Core.Constants.PkgUnit.BaleUncompressed:
					result = Xsd.VerpackungsArt.BN;
					break;
				case Enterprise.Core.Constants.PkgUnit.Basket:
					result = Xsd.VerpackungsArt.BK;
					break;
				case Enterprise.Core.Constants.PkgUnit.Bottle:
					result = Xsd.VerpackungsArt.BO;
					break;
				case Enterprise.Core.Constants.PkgUnit.Box:
					result = Xsd.VerpackungsArt.BX;
					break;
				case Enterprise.Core.Constants.PkgUnit.BreakBulk:
					result = Xsd.VerpackungsArt.CH;
					break;
				case Enterprise.Core.Constants.PkgUnit.BulkBag:
					result = Xsd.VerpackungsArt.BP;
					break;
				case Enterprise.Core.Constants.PkgUnit.Bundle:
					result = Xsd.VerpackungsArt.BE;
					break;
				case Enterprise.Core.Constants.PkgUnit.Carton:
					result = Xsd.VerpackungsArt.CT;
					break;
				case Enterprise.Core.Constants.PkgUnit.Case:
					result = Xsd.VerpackungsArt.CS;
					break;
				case Enterprise.Core.Constants.PkgUnit.Coil:
					result = Xsd.VerpackungsArt.CL;
					break;
				case Enterprise.Core.Constants.PkgUnit.Container:
					result = Xsd.VerpackungsArt.BR;
					CommonContainer container = (Factory.Load<CommonContainer>(line.JL_JC));
					if (container != null && container.JC_Is40GP)
					{
						result = Xsd.VerpackungsArt.BK;
					}
					break;

				case Enterprise.Core.Constants.PkgUnit.Cradle:
					result = Xsd.VerpackungsArt.CR;
					break;
				case Enterprise.Core.Constants.PkgUnit.Crate:
					result = Xsd.VerpackungsArt.CR;
					break;
				case Enterprise.Core.Constants.PkgUnit.Cylinder:
					result = Xsd.VerpackungsArt.CY;
					break;
				case Enterprise.Core.Constants.PkgUnit.Dozen:
					result = Xsd.VerpackungsArt.CH;
					break;
				case Enterprise.Core.Constants.PkgUnit.Drum:
					result = Xsd.VerpackungsArt.BA;
					break;
				case Enterprise.Core.Constants.PkgUnit.Envelope:
					result = Xsd.VerpackungsArt.CV;
					break;
				case Enterprise.Core.Constants.PkgUnit.Gross:
					result = Xsd.VerpackungsArt.VO;
					break;
				case Enterprise.Core.Constants.PkgUnit.Keg:
					result = Xsd.VerpackungsArt.BA;
					break;
				case Enterprise.Core.Constants.PkgUnit.Mix:
					result = Xsd.VerpackungsArt.CH;
					break;
				case Enterprise.Core.Constants.PkgUnit.Package:
					result = Xsd.VerpackungsArt.PK;
					break;
				case Enterprise.Core.Constants.PkgUnit.Pail:
					result = Xsd.VerpackungsArt.BJ;
					break;
				case Enterprise.Core.Constants.PkgUnit.Pallet:
					result = Xsd.VerpackungsArt.PX;
					break;
				case Enterprise.Core.Constants.PkgUnit.Piece:
					result = Xsd.VerpackungsArt.CV;
					break;
				case Enterprise.Core.Constants.PkgUnit.Reel:
					result = Xsd.VerpackungsArt.BD;
					break;
				case Enterprise.Core.Constants.PkgUnit.Roll:
					result = Xsd.VerpackungsArt.BH;
					break;
				case Enterprise.Core.Constants.PkgUnit.Sheet:
					result = Xsd.VerpackungsArt.BD;
					break;
				case Enterprise.Core.Constants.PkgUnit.Skid:
					result = Xsd.VerpackungsArt.SI;
					break;
				case Enterprise.Core.Constants.PkgUnit.Spool:
					result = Xsd.VerpackungsArt.BB;
					break;
				case Enterprise.Core.Constants.PkgUnit.Tube:
					result = Xsd.VerpackungsArt.EN;
					break;
				case Enterprise.Core.Constants.PkgUnit.Unit:
					result = Xsd.VerpackungsArt.CH;
					break;
			}

			return result;
		}

		#endregion

		#region VPGGruppe - packing group

		public Xsd.VPGGruppe GetVPGGruppe(UNDGSubstance uNDGSubstance)
		{
			Xsd.VPGGruppe result = Xsd.VPGGruppe.I;
			switch (uNDGSubstance.DG_PG)
			{
				case "1":
				case "I":
					result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.VPGGruppe.I;
					break;

				case "2":
				case "II":
					result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.VPGGruppe.II;
					break;

				case "3":
				case "III":
					result = Enterprise.DataTransfer.Xml.XsdVersion1.ALPO.VPGGruppe.III;
					break;
			}

			return result;
		}

		#endregion

		#region ZollPositionen - CustomsData

		public Xsd.ZollPositionen GetCustomsData(int refNCounter, int outerPackNumber, PackLine line, CommonContainer container, CommonShipment shipment)
		{
			Xsd.ZollPositionen result = new Xsd.ZollPositionen();
			List<string> refNumList = new List<string>();
			bool usingInner = false;
			if (container != null && (container.JC_ContainerMode == "FCL" || container.JC_ContainerMode == "LCL"))
			{
				if (!line.JL_RefNumber.IsEmpty)
				{
					refNumList.Add(line.JL_RefNumber);
				}
			}
			else
			{
				if (shipment != null)
				{
					if (outerPackNumber == 1)
					{
						foreach (PackLine packLine in shipment.InnerPackLines)
						{
							if (!packLine.JL_RefNumber.IsEmpty && !refNumList.Contains(packLine.JL_RefNumber))
							{
								refNumList.Add(packLine.JL_RefNumber);
								usingInner = true;
							}
						}
					}

					if (refNumList.Count == 0)
					{
						foreach (PackLine packLine in shipment.OuterPackLines)
						{
							if (!packLine.JL_RefNumber.IsEmpty && packLine.PK == line.PK)
							{
								refNumList.Add(packLine.JL_RefNumber);
							}
						}
					}
				}
			}

			foreach (string refN in refNumList)
			{
				refNCounter++;
				var zollDaten = result.ZollDaten.AddNew();
				zollDaten.ZollPosZaehler = outerPackNumber;
				zollDaten.ZollPosZaehlerSpecified = true;
				zollDaten.AESZollDaten.AESZaehler = outerPackNumber.ToString(CultureInfo.InvariantCulture);
				zollDaten.AESZollDaten.MRN = refN;
				zollDaten.AESZollDaten.MRNKomplett = Xsd.MRNKomplett.True;
				if (usingInner)
				{
					outerPackNumber++;
				}
			}
			return result;
		}

		public List<string> GetRefNumListFromOuterPacks(CommonContainer container)
		{
			List<string> refNumList = new List<string>();
			foreach (PackLine lineWithRefNum in container.PackLines)
			{
				if (!lineWithRefNum.JL_RefNumber.IsEmpty && lineWithRefNum.IsOuterPackType)
				{
					if (!refNumList.Contains(lineWithRefNum.JL_RefNumber))
					{
						refNumList.Add(lineWithRefNum.JL_RefNumber);
					}
				}
			}
			return refNumList;
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#endregion

		#endregion

		public void ExportAdvantageEnterpriseVersion01(ForwardingShipment shipment, Xsd.AdvantageEnterpriseVersion01 result)
		{
			result.SENDER = ExportSender(shipment);
			result.EMPFANGSSYSTEM = ExportEMPFANGSSYSTEM();
			result.NachrichtenZeitstempel = ExportNachrichtenZeitstempel();
			result.Hafenauftrag = ExportHafenauftrag(shipment);
		}

		public Xsd.AdvantageEnterpriseVersion01 ExportAdvantageEnterpriseVersion01(ForwardingShipment shipment)
		{
			Xsd.AdvantageEnterpriseVersion01 result = new Xsd.AdvantageEnterpriseVersion01();
			ExportAdvantageEnterpriseVersion01(shipment, result);
			return result;
		}

		#endregion

		#region Implementation

		public override XmlSchema CollectionSchema
		{
			get { throw new NotSupportedException("CollectionSchema has not been implemented yet"); }
		}

		protected override void ExportToValueObjectCore(ForwardingShipment bizObj, Xsd.AdvantageEnterpriseVersion01 constructedValueObject, IValueObjectExportContext context)
		{
			ExportAdvantageEnterpriseVersion01(bizObj, constructedValueObject);
		}

		protected override void ImportFromValueObjectCore(ForwardingShipment bizObj, Xsd.AdvantageEnterpriseVersion01 value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("ImportFromValueObjectCore has not been implemented yet");
		}

		public override string RootCollectionElementName
		{
			get { throw new NotSupportedException("RootCollectionElementName has not been implemented yet"); }
		}

		public override string RootElementName
		{
			get { return "AdvantageEnterpriseVersion0.1"; }
		}

		public override XmlSchema Schema
		{
			get { throw new NotSupportedException("Schema has not been implemented yet"); }
		}

		ZString RemoveEnterpriseCodeFromUserLogin(ZString loginName)
		{
			var enterpriseCodePrefix = FormattableString.Invariant($"{ObjectFactory.Get<IProductRegistration>()?.Key?.EnterpriseCode}."); // programmatic constant

			if (loginName.StartsWith(enterpriseCodePrefix, StringComparison.OrdinalIgnoreCase))
			{
				return loginName.Substring(enterpriseCodePrefix.Length);
			}

			return loginName;
		}

		#endregion
	}
}
