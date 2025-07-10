using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class CusImportManifestValueObjectDataAdapter : ValueObjectDataAdapter<CusSeaManTranHead, Xsd.CusImportManifest>
	{
		public CusImportManifestValueObjectDataAdapter(bool withOrgMatching)
		{
			this.WithOrgMatching = withOrgMatching;
		}

		public CusImportManifestValueObjectDataAdapter()
			: this(true)
		{
		}

		protected readonly bool WithOrgMatching;

		#region Overrides

		public override string RootCollectionElementName
		{
			get { return "CusImportManifests"; }
		}

		public override string RootElementName
		{
			get { return "CusImportManifest"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleCusImportManifestSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return XmlSchemaDefinitions.Instance.CusImportManifestsSchema; }
		}

		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.CusImportManifestsCollection); }
		}

		#endregion

		#region Import

		#region ImportFromValueObject

		protected override void ImportFromValueObjectCore(CusSeaManTranHead bizObj, Xsd.CusImportManifest value, IValueObjectImportContext context)
		{
			SetTranHeadDetails(bizObj, value, context);
		}

		#endregion

		#region SetTranHeadDetails

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		protected virtual void SetTranHeadDetails(CusSeaManTranHead tranHead, Xsd.CusImportManifest xmlImportManifest, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;

			ZString vesselCode = GetVesselCode(tranHead.Factory, xmlImportManifest.Vessel.Name, xmlImportManifest.Vessel.Lloyds, context);
			context.SetPropertyInfoValue(tranHead.BT_VesselNameInfo, vesselCode, ForeignKeyType.VesselNameNK, (NoResString)"Vessel Name");
			context.SetPropertyInfoValue(tranHead.BT_VoyageNumInfo, xmlImportManifest.VoyageNumber, xmlImportManifest.VoyageNumberSpecified, (NoResString)"Voyage Number");
			if (xmlImportManifest.LastForeignPortOfDeparture.IsSpecified)
			{
				if (xmlImportManifest.LastForeignPortOfDeparture.Port.IsSpecified)
				{
					context.SetPropertyInfoValue(tranHead.BT_RL_NKPortOfLastForeignPortInfo, xmlImportManifest.LastForeignPortOfDeparture.Port.Value, ForeignKeyType.PortNK, "Last Foreign Port");
				}

				tranHead.BT_PortOfLastForeignPortATD = xmlImportManifest.LastForeignPortOfDeparture.ActualDateTime;
			}

			SetArrivalPortDetails(tranHead.Arrivals, xmlImportManifest.Arrivals, context);
			SetOceanBillDetails(tranHead.OceanBills, xmlImportManifest.OceanBills, context);
			SetSlotChartererDetails(tranHead.SlotCharterers, xmlImportManifest.SlotCharterers, context);
		}

		internal ZString GetVesselCode(BusinessObjectFactory factory, ZString name, ZString lloyds, IValueObjectImportContext context)
		{
			ZString result = ZString.Empty;

			ZString vesselLloyds = lloyds.Trim().ToUpper();
			RefVessel vessel = vesselLloyds.IsEmpty ? null : RefVessel.LookupVesselByLloyds(lloyds, factory);

			if (vessel == null)
			{
				ZString vesselCode = name.Trim().ToUpper();
				vessel = vesselCode.IsEmpty ? null : RefVessel.LookupVesselByCode(vesselCode, factory);

				if (vessel == null && !vesselCode.IsEmpty)
				{
					vessel = RefVessel.New(factory);
					context.SetPropertyInfoValueIfValueNotEmpty(vessel.RV_CodeInfo, vesselCode);
					context.SetPropertyInfoValueIfValueNotEmpty(vessel.RV_LloydsNumberInfo, vesselLloyds);
				}
			}

			if (vessel != null)
			{
				result = vessel.RV_Code;
			}

			return result;
		}

		#endregion

		#region SetArrivalPortDetails

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		protected virtual void SetArrivalPortDetails(CusSeaManArrivalPortCollection arrivals, Xsd.CusImportManifestArrivalCollection xmlArrivals, IValueObjectImportContext context)
		{
			INotifications notifications = context;

			CusSeaManArrivalPort firstPort = null;
			foreach (Xsd.CusImportManifestArrival xmlArrival in xmlArrivals)
			{
				CusSeaManArrivalPort arrival = arrivals.AddNew();

				context.SetPropertyInfoValue(arrival.BA_RL_NKArrivalPortInfo, xmlArrival.ArrivalPort.Port.Value, ForeignKeyType.PortNK, "Arrival Port");
				arrival.BA_ArrivalPortATA = xmlArrival.ArrivalPort.ActualDateTime;
				arrival.BA_ArrivalPortETA = xmlArrival.ArrivalPort.EstimatedDateTime;
				context.SetPropertyInfoValue(arrival.BA_BerthCodeInfo, xmlArrival.BerthCode, xmlArrival.BerthCodeSpecified, "Berth Code");

				if (xmlArrival.CTOAddress.IsSpecified)
				{
					AddressValueObjectHelper helper = new AddressValueObjectHelper("CTO Address");

					Xsd.AddressReference reference = xmlArrival.CTOAddress;
					ZGuid cTOGuid = helper.FromAddressReferenceGetAddressPK(reference, context);

					if (!cTOGuid.IsEmpty)
					{
						arrival.BA_OA_CTOAddress = cTOGuid;
					}
				}

				if (firstPort != null && arrival.BA_ArrivalPortETA < firstPort.BA_ArrivalPortETA)
				{
					firstPort = arrival;
				}
			}

			if (firstPort != null)
			{
				firstPort.BA_IsFirstArrival = true;
			}
		}

		#endregion

		#region SetOceanBillDetails

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		protected internal virtual void SetOceanBillDetails(CusSeaManOBLHeaderCollection oceanBills, Xsd.CusImportManifestOceanBillCollection xmlOceanBills, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notifications = context;

			foreach (Xsd.CusImportManifestOceanBill xmlOceanBill in xmlOceanBills)
			{
				CusSeaManOBLHeader oceanBill = oceanBills.AddNew();

				context.SetPropertyInfoValue(oceanBill.BO_OceanBillInfo, xmlOceanBill.OceanBillNumber, xmlOceanBill.OceanBillNumberSpecified, "Ocean Bill Number");
				context.SetPropertyInfoValue(oceanBill.BO_HeaderCargoTypeInfo, xmlOceanBill.CargoType, xmlOceanBill.CargoTypeSpecified, "Cargo Type");
				context.SetPropertyInfoValue(oceanBill.BO_RN_NKGoodsCountryOfOriginInfo, xmlOceanBill.CountryOfOrigin, xmlOceanBill.CountryOfOriginSpecified, "Country of Origin");
				context.SetPropertyInfoValue(oceanBill.BO_PaymentMethodInfo, xmlOceanBill.MethodOfPayment, xmlOceanBill.MethodOfPaymentSpecified, "Method of Payment");

				if (xmlOceanBill.PortOfDischarge.IsSpecified)
				{
					context.SetPropertyInfoValue(oceanBill.BO_RL_NKDischargePortInfo, xmlOceanBill.PortOfDischarge.Port.Value, ForeignKeyType.PortNK, "Port of Discharge");
				}

				if (xmlOceanBill.PortOfLoading.IsSpecified)
				{
					context.SetPropertyInfoValue(oceanBill.BO_RL_NKLoadPortInfo, xmlOceanBill.PortOfLoading.Port.Value, ForeignKeyType.PortNK, "Port of Loading");
				}

				if (xmlOceanBill.PortOfOrigin.IsSpecified)
				{
					context.SetPropertyInfoValue(oceanBill.BO_RL_NKOriginPortInfo, xmlOceanBill.PortOfOrigin.Port.Value, ForeignKeyType.PortNK, "Port of Origin");
				}

				if (xmlOceanBill.PortOfDestination.IsSpecified)
				{
					context.SetPropertyInfoValue(oceanBill.BO_RL_NKDestinationPortInfo, xmlOceanBill.PortOfDestination.Port.Value, ForeignKeyType.PortNK, "Port of Destination");
				}

				if (oceanBill.BO_RL_NKOriginPort.IsEmpty)
				{
					oceanBill.BO_RL_NKOriginPort = oceanBill.BO_RL_NKLoadPort;
				}

				if (oceanBill.BO_RL_NKDestinationPort.IsEmpty)
				{
					oceanBill.BO_RL_NKDestinationPort = oceanBill.BO_RL_NKDischargePort;
				}

				oceanBill.BO_FreightForwarderIndicator = xmlOceanBill.IsForwarder;

				if (WithOrgMatching)
				{
					ImportOrgsWithMatching(context, xmlOceanBill, oceanBill);
				}
				else
				{
					ImportOrgsWithoutMatching(context, xmlOceanBill, oceanBill);
				}

				context.SetPropertyInfoValue(oceanBill.BO_HeaderCargoTypeInfo, xmlOceanBill.CargoType, xmlOceanBill.CargoTypeSpecified);

				if (oceanBill.BO_HeaderCargoType != "I")
				{
					CusSeaManOBLHeader currentOceanBill = oceanBill;
					bool isFirstDetail = true;

					CusSeaManArrivalPort port = GetArrivalPort(oceanBill.TransportHeader, oceanBill.BO_RL_NKDischargePort);

					foreach (Xsd.CusImportManifestOceanBillOceanBillDetail xmlOceanBillDetail in xmlOceanBill.OceanBillDetails)
					{
						if (!isFirstDetail)
						{
							currentOceanBill = (CusSeaManOBLHeader)oceanBill.Clone();
							oceanBills.Add(currentOceanBill);
						}
						else
						{
							isFirstDetail = false;
						}
						Xsd.CusImportManifestOceanBillOceanBillDetailCollection tmpCollection = new Xsd.CusImportManifestOceanBillOceanBillDetailCollection();
						tmpCollection.Add(xmlOceanBillDetail);
						SetOceanBillDetailDetails(currentOceanBill.Details, tmpCollection, context);
						if (port != null)
						{
							currentOceanBill.BO_BA = port.PK;
						}
					}
				}
				else
				{
					SetOceanBillDetailDetails(oceanBill.Details, xmlOceanBill.OceanBillDetails, context);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		void ImportOrgsWithMatching(IValueObjectImportContext context, Xsd.CusImportManifestOceanBill xmlOceanBill, CusSeaManOBLHeader oceanBill)
		{
			OrganisationValueObjectDataAdapter orgDataAdapter = new OrganisationValueObjectDataAdapter();
			if (xmlOceanBill.Consignee.IsSpecified)
			{
				if (!XmlOrgHasAddresses(xmlOceanBill.Consignee) && !xmlOceanBill.Consignee.OrganisationDetails.Name.IsEmpty)
				{
					context.SetPropertyInfoValue(oceanBill.BO_ConsigneeNameInfo, xmlOceanBill.Consignee.OrganisationDetails.Name, xmlOceanBill.Consignee.OrganisationDetails.NameSpecified, "Consignee Name");
				}
				else
				{
					oceanBill.BO_OH_Consignee = context.FindOrCreateTempOrganisationPK(xmlOceanBill.Consignee, oceanBill, OrganisationTypes.Consignee);
				}
			}

			if (xmlOceanBill.Consignor.IsSpecified)
			{
				if (!XmlOrgHasAddresses(xmlOceanBill.Consignor) && !xmlOceanBill.Consignor.OrganisationDetails.Name.IsEmpty)
				{
					context.SetPropertyInfoValue(oceanBill.BO_ConsignorNameInfo, xmlOceanBill.Consignor.OrganisationDetails.Name, xmlOceanBill.Consignor.OrganisationDetails.NameSpecified, "Consignor Name");
				}
				else
				{
					oceanBill.BO_OH_Consignor = context.FindOrCreateTempOrganisationPK(xmlOceanBill.Consignor, oceanBill, OrganisationTypes.Consignor);
				}
			}
		}

		void ImportOrgsWithoutMatching(IValueObjectImportContext context, Xsd.CusImportManifestOceanBill xmlOceanBill, CusSeaManOBLHeader oceanBill)
		{
			if (xmlOceanBill.Consignee.IsSpecified)
			{
				context.SetPropertyInfoValue(oceanBill.BO_ConsigneeNameInfo, xmlOceanBill.Consignee.OrganisationDetails.Name, xmlOceanBill.Consignee.OrganisationDetails.NameSpecified);
				Xsd.OrgAddress address = xmlOceanBill.Consignee.OrganisationDetails.Addresses.GetMainOrFirstAddress();

				if (address != null)
				{
					context.SetPropertyInfoValue(oceanBill.BO_ConsigneeAddress1Info, address.AddressLine1, address.AddressLine1Specified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsigneeAddress2Info, address.AddressLine2, address.AddressLine2Specified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsigneeCityInfo, address.CityOrSuburb, address.CityOrSuburbSpecified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsigneePostCodeInfo, address.PostCode, address.PostCodeSpecified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsigneeStateInfo, address.StateOrProvince, address.StateOrProvinceSpecified);
					context.SetPropertyInfoValue(oceanBill.BO_RN_NKConsigneeCountryCodeInfo, address.Location.Country, ForeignKeyType.CountryNK);
				}
			}

			if (xmlOceanBill.Consignor.IsSpecified)
			{
				context.SetPropertyInfoValue(oceanBill.BO_ConsignorNameInfo, xmlOceanBill.Consignor.OrganisationDetails.Name, xmlOceanBill.Consignor.OrganisationDetails.NameSpecified);
				Xsd.OrgAddress address = xmlOceanBill.Consignor.OrganisationDetails.Addresses.GetMainOrFirstAddress();

				if (address != null)
				{
					context.SetPropertyInfoValue(oceanBill.BO_ConsignorAddress1Info, address.AddressLine1, address.AddressLine1Specified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsignorAddress2Info, address.AddressLine2, address.AddressLine2Specified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsignorCityInfo, address.CityOrSuburb, address.CityOrSuburbSpecified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsignorPostCodeInfo, address.PostCode, address.PostCodeSpecified);
					context.SetPropertyInfoValue(oceanBill.BO_ConsignorStateInfo, address.StateOrProvince, address.StateOrProvinceSpecified);
					context.SetPropertyInfoValue(oceanBill.BO_RN_NKConsignorCountryCodeInfo, address.Location.Country, ForeignKeyType.CountryNK);
				}
			}
		}

		CusSeaManArrivalPort GetArrivalPort(CusSeaManTranHead tranHead, ZString portCode)
		{
			foreach (CusSeaManArrivalPort port in tranHead.Arrivals)
			{
				if (port.BA_RL_NKArrivalPort == portCode)
				{
					return port;
				}
			}

			return null;
		}

		internal bool XmlOrgHasAddresses(Xsd.Organisation xmlOrg)
		{
			bool result = true;

			if (xmlOrg.OrganisationDetails.Addresses.Count == 0)
			{
				result = false;
			}
			else
			{
				if (xmlOrg.OrganisationDetails.Addresses[0].AddressLine1.Trim().IsEmpty && xmlOrg.OrganisationDetails.Addresses[0].AddressLine2.Trim().IsEmpty)
				{
					result = false;
				}
			}

			return result;
		}

		#endregion

		#region SetOceanBillDetailDetails

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		protected virtual void SetOceanBillDetailDetails(CusSeaManOBLDetailCollection oceanBillDetails, Xsd.CusImportManifestOceanBillOceanBillDetailCollection xmlOceanBillDetails, IValueObjectImportContext context)
		{
			foreach (Xsd.CusImportManifestOceanBillOceanBillDetail xmlOceanBillDetail in xmlOceanBillDetails)
			{
				CusSeaManOBLDetail oceanBillDetail = oceanBillDetails.AddNew();

				context.SetPropertyInfoValue(oceanBillDetail.BD_ContainerNumberInfo, xmlOceanBillDetail.ContainerNumber, xmlOceanBillDetail.ContainerNumberSpecified, "Container Number");
				context.SetPropertyInfoValue(oceanBillDetail.BD_LineCargoTypeInfo, xmlOceanBillDetail.ContainerMode, xmlOceanBillDetail.ContainerModeSpecified, "Container Mode");

				context.SetPropertyInfoValue(oceanBillDetail.BD_ContainerSizeOrISOCodeInfo, xmlOceanBillDetail.ContainerSizeOrISOCode, xmlOceanBillDetail.ContainerSizeOrISOCodeSpecified, "Container ISO Code");
				context.SetPropertyInfoValue(oceanBillDetail.BD_TypeOfContainerInfo, xmlOceanBillDetail.ContainerType, xmlOceanBillDetail.ContainerTypeSpecified, "Container Type");

				SetRefContainer(oceanBillDetail, xmlOceanBillDetail);

				context.SetPropertyInfoValue(oceanBillDetail.BD_SealNoInfo, xmlOceanBillDetail.SealNumber, xmlOceanBillDetail.SealNumberSpecified, "Container Seal Number");
				context.SetPropertyInfoValue(oceanBillDetail.BD_GoodsDescriptionInfo, xmlOceanBillDetail.GoodsDescription, xmlOceanBillDetail.GoodsDescriptionSpecified, "Goods Description");
				context.SetPropertyInfoValue(oceanBillDetail.BD_MarksAndNumbersInfo, xmlOceanBillDetail.MarksAndNumbers, xmlOceanBillDetail.MarksAndNumbersSpecified, "Marks and Numbers");
				context.SetPropertyInfoValue(oceanBillDetail.BD_PackTypeInfo, xmlOceanBillDetail.PackageType, xmlOceanBillDetail.PackageTypeSpecified, "Package Type");

				oceanBillDetail.BD_NoOfPacks = xmlOceanBillDetail.NumberOfPackages;

				if (xmlOceanBillDetail.Volume.IsSpecified)
				{
					context.SetPropertyInfoValue(oceanBillDetail.BD_CargoVolumeUMInfo, xmlOceanBillDetail.Volume.DimensionType, xmlOceanBillDetail.Volume.DimensionTypeSpecified, "Cargo Volume Units");
					oceanBillDetail.BD_CargoVolume = xmlOceanBillDetail.Volume.Value;
				}

				if (xmlOceanBillDetail.Weight.IsSpecified)
				{
					context.SetPropertyInfoValue(oceanBillDetail.BD_GrossWeightUMInfo, xmlOceanBillDetail.Weight.DimensionType, xmlOceanBillDetail.Weight.DimensionTypeSpecified, "Gross Weight Units");
					oceanBillDetail.BD_GrossWeight = xmlOceanBillDetail.Weight.Value;
				}

				if (xmlOceanBillDetail.Indicators.IsSpecified)
				{
					Xsd.CusImportManifestOceanBillOceanBillDetailIndicators indicators = xmlOceanBillDetail.Indicators;
					oceanBillDetail.BD_ReportableDocsIndicator = indicators.ReportableDocuments;
					oceanBillDetail.BD_FumigationIndicator = indicators.Fumigation;
					oceanBillDetail.BD_HazardousIndicator = indicators.HazardousGoods;
					oceanBillDetail.BD_PerishableIndicator = indicators.PerishableGoods;
					oceanBillDetail.BD_PersonalEffectsIndicator = indicators.PersonalEffects;
					oceanBillDetail.BD_SACIndicator = indicators.SAC;
					oceanBillDetail.BD_ShipperOwnedContainerIndicator = indicators.ShipperOwnedContainer;
					oceanBillDetail.BD_TimberIndicator = indicators.Timber;
				}
			}
		}

		void SetRefContainer(CusSeaManOBLDetail oceanBillDetail, Xsd.CusImportManifestOceanBillOceanBillDetail xmlOceanBillDetail)
		{
			ZString trimmedISOCode = xmlOceanBillDetail.ContainerSizeOrISOCode.Trim();
			ZString trimmedTypeCode = xmlOceanBillDetail.ContainerType.Trim();

			RefContainer.Loader containerLoader = new RefContainer.Loader(oceanBillDetail.Factory);
			RefContainer container = containerLoader.LoadFromCode(trimmedISOCode);
			if (container == null)
			{
				container = containerLoader.LoadFromCode(trimmedTypeCode);
				if (container == null)
				{
					container = containerLoader.LoadFromISOType(trimmedISOCode);
				}
			}

			if (container != null)
			{
				oceanBillDetail.BD_RC_ContainerType = container.PK;
			}
			else if (oceanBillDetail.BD_LineCargoType != Constants.BreakBulk && oceanBillDetail.BD_LineCargoType != Constants.Bulk)
			{
				CMRContainerUtilities containerUtility = new CMRContainerUtilities();
				ZString iSOCode = oceanBillDetail.BD_ContainerSizeOrISOCode;
				oceanBillDetail.BD_TypeOfContainer = containerUtility.GetContainerTypeMappingCodeOrEmpty(containerUtility.GetContainerTypeCodeFromISOCode(iSOCode));
				oceanBillDetail.BD_ContainerSizeOrISOCode = containerUtility.GetContainerSizeCodeFromISOCode(iSOCode);
			}
			else
			{
				// Do nothing
			}
		}

		#region Constants

		abstract class Constants
		{
			public const string Bulk = "BLK";
			public const string BreakBulk = "B/B";
		}

		#endregion

		#endregion

		#region SetSlotChartererDetails

		protected virtual void SetSlotChartererDetails(CusSeaManSlotOrgCollection slotOrgs, Xsd.OrganisationCollection xmlSlotOrgs, IValueObjectImportContext context)
		{
			foreach (Xsd.Organisation xmlSlotOrg in xmlSlotOrgs)
			{
				CusSeaManSlotOrg slotOrg = slotOrgs.AddNew();

				OrganisationValueObjectDataAdapter orgDataAdapter = new OrganisationValueObjectDataAdapter();
				slotOrg.BS_OH_SlotCharterer = context.FindOrCreateTempOrganisationPK(xmlSlotOrg, slotOrg, OrganisationTypes.Carrier);
			}
		}

		#endregion

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(CusSeaManTranHead bizObj, Xsd.CusImportManifest constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting is not supported.");
		}

		#endregion
	}
}
