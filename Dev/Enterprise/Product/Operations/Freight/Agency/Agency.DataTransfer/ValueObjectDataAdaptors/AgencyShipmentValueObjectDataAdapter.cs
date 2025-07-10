using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class AgencyShipmentValueObjectDataAdapter<TShipment>
		: ValueObjectDataAdapter<TShipment, Xsd.AgencyBillOfLading>
		where TShipment : AgencyShipment
	{
		#region Export

		protected override void ExportToValueObjectCore(TShipment bizObj, Xsd.AgencyBillOfLading constructedValueObject, IValueObjectExportContext context)
		{
			ExportShipmentDetail(bizObj, constructedValueObject, context);
			ExportContainers(bizObj, constructedValueObject, context);
			ExportPackages(constructedValueObject, context, bizObj.OuterPackLines.Cast<AgencyShipmentPackLine>());

			ReferenceNumberDataAdapter.ExportReferenceNumbers(bizObj.Numbers, constructedValueObject.ReferenceNumbers, context);

			ExportAddresses(bizObj, constructedValueObject, context);
			ExportBilling(bizObj, constructedValueObject, context);
			ExportNotes(bizObj, constructedValueObject, context);
			DocDataValueObjectDataAdapter.ExportData(bizObj, constructedValueObject.DocData, context);
			AddExportEvent(constructedValueObject, bizObj, context);
		}

		void ExportShipmentDetail(TShipment bizObj, Xsd.AgencyBillOfLading constructedValueObject, IValueObjectExportContext context)
		{
			constructedValueObject.BillNumber = bizObj.JS_HouseBill;
			constructedValueObject.BookingReference = bizObj.JS_CFSReference;
			constructedValueObject.CargoType = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(bizObj.JS_PackingMode, string.Empty, context);
			constructedValueObject.CargoTypeSpecified = true;
			constructedValueObject.ChargesApply = bizObj.JS_HBLAWBChargesDisplay;

			constructedValueObject.CopyBills = bizObj.JS_NoCopyBills;
			constructedValueObject.CopyBillsSpecified = true;

			if (GlbBranch.CurrentBranch.Country != null)
			{
				constructedValueObject.CustomsEntryNumber.Country = GlbBranch.CurrentBranch.Country.Code;
			}

			constructedValueObject.CustomsEntryNumber.Type = bizObj.CustomsEntryNumberType;
			constructedValueObject.CustomsEntryNumber.Number = bizObj.CustomsEntryNumber;
			constructedValueObject.Description = bizObj.JS_GoodsDescription;

			constructedValueObject.Destination = Xsd.UNLOCO.FromPortCode(bizObj.Factory, bizObj.JS_RL_NKDestination);
			constructedValueObject.PortOfDestination.Port = Xsd.UNLOCO.FromPortCode(bizObj.Factory, bizObj.JS_RL_NKDestination);
			constructedValueObject.PortOfDestination.EstimatedDateTime = bizObj.JS_E_ARV;

			constructedValueObject.Discharge = Xsd.UNLOCO.FromPortCode(bizObj.Factory, bizObj.JS_NKDischargePort);
			constructedValueObject.INCOTerm = bizObj.JS_INCO;
			constructedValueObject.InterimReceipt = bizObj.JS_InterimReceipt;
			constructedValueObject.IssueDate = bizObj.JS_HouseBillIssueDate;
			constructedValueObject.Load = Xsd.UNLOCO.FromPortCode(bizObj.Factory, bizObj.JS_NKLoadPort);
			constructedValueObject.MarksAndNumbers = bizObj.JS_MarksAndNumbers;
			constructedValueObject.ServiceLevel = bizObj.JS_RS_NKServiceLevel;

			constructedValueObject.Origin = Xsd.UNLOCO.FromPortCode(bizObj.Factory, bizObj.JS_RL_NKOrigin);
			constructedValueObject.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(bizObj.Factory, bizObj.JS_RL_NKOrigin);
			constructedValueObject.PortOfOrigin.EstimatedDateTime = bizObj.JS_E_DEP;

			constructedValueObject.OriginalBills = bizObj.JS_NoOriginalBills;
			constructedValueObject.OriginalBillsSpecified = true;

			constructedValueObject.PackageSummary.NumberOfPacks = Convert.ToUInt32(bizObj.JS_OuterPacks);
			constructedValueObject.PackageSummary.PackType = bizObj.JS_F3_NKPackType;
			constructedValueObject.PackageSummary.Volume.Value = bizObj.JS_ActualVolume;
			constructedValueObject.PackageSummary.Volume.DimensionType = bizObj.JS_UnitOfVolume;
			constructedValueObject.PackageSummary.Weight.Value = bizObj.JS_ActualWeight;
			constructedValueObject.PackageSummary.Weight.DimensionType = bizObj.JS_UnitOfWeight;
			constructedValueObject.PackageSummary.Width.IsSpecified = true;
			constructedValueObject.PackageSummary.Length.IsSpecified = true;
			constructedValueObject.PackageSummary.Height.IsSpecified = true;
			constructedValueObject.PackageSummary.HazardousGoods.IsSpecified = false;

			constructedValueObject.ReleaseType = AgencyReleaseTypeXmlCodeMappings.Instance.GetExternalCode(bizObj.JS_ReleaseType, string.Empty, context);
			constructedValueObject.ReleaseTypeSpecified = true;

			JobSailing sailing = bizObj.Sailing;

			if (sailing != null)
			{
				constructedValueObject.Sailing.VesselName = sailing.JX_JV_NKVessel;
				constructedValueObject.Sailing.VoyageNo = sailing.JX_JV_VoyageFlight;
				constructedValueObject.Sailing.ETD = sailing.JX_JA_E_DEP;
				constructedValueObject.Sailing.ETA = sailing.JX_JB_E_ARV;
				constructedValueObject.Sailing.ATD = sailing.JX_JA_A_DEP;
				constructedValueObject.Sailing.ATA = sailing.JX_JB_A_ARV;

				RefVessel vessel = sailing.Vessel;

				if (vessel != null)
				{
					constructedValueObject.Sailing.LloydsNo = vessel.RV_LloydsNumber;
					constructedValueObject.Sailing.CargoCarrierCode = vessel.RV_CarrierCode;
				}

				if (sailing.Voyage != null && sailing.Voyage.Line != null)
				{
					constructedValueObject.Sailing.Carrier = new OrganisationValueObjectDataAdapter().ExportToValueObject(sailing.Voyage.Line, context);
				}
			}

			constructedValueObject.ShippedOnBoard.Code = bizObj.JS_ShippedOnBoard;
			constructedValueObject.ShippedOnBoard.Date = bizObj.JS_ShippedOnBoardDate;
			constructedValueObject.ShippersReference = bizObj.JS_BookingReference;

			constructedValueObject.Carrier = new OrganisationValueObjectDataAdapter().ExportToValueObject(bizObj.BookedShippingLine, context);
			constructedValueObject.Principal = new OrganisationValueObjectDataAdapter().ExportToValueObject(bizObj.Principal, context);
			constructedValueObject.SendingAgent = new AddressValueObjectHelper(Res.GetString("74a19982-4677-4629-8608-000026a2ad33", "Principal's Sending Agent for Origin '{0}'", bizObj.JS_RL_NKOrigin)).ToAddressReference(bizObj.SendingAgentAddress, context);
			constructedValueObject.ReceivingAgent = new AddressValueObjectHelper(Res.GetString("5146038a-4ef3-4cee-9547-b647482edc63", "Principal's Receiving Agent for Destination '{0}'", bizObj.JS_RL_NKDestination)).ToAddressReference(bizObj.ReceivingAgentAddress, context);
		}

		void ExportContainers(TShipment bizObj, Xsd.AgencyBillOfLading constructedValueObject, IValueObjectExportContext context)
		{
			if (bizObj.IsTopLevelPacksMode)
			{
				var packages = bizObj.ShippingContainers.Cast<AgencyShipmentContainer>()
					.Select(AgencyShipmentPackLineAdapter.New);

				ExportPackages(constructedValueObject, context, packages);
			}
			else
			{
				AgencyShipmentContainerDependentCollection containers = bizObj.ShippingContainers;

				foreach (AgencyShipmentContainer container in containers)
				{
					if (ContainerFilter == null || ContainerFilter(container))
					{
						Xsd.Container valueContainer = constructedValueObject.Containers.AddNew();
						ExportContainer(context, container, valueContainer);
					}
				}
			}
		}

		protected virtual void ExportContainer(IValueObjectExportContext context, AgencyShipmentContainer container, Xsd.Container valueContainer)
		{
			valueContainer.ReleaseNumber = container.JC_ReleaseNum;
			valueContainer.ContainerNumber = container.JC_ContainerNum;

			RefContainer containerType = container.RefContainer;

			if (containerType != null)
			{
				valueContainer.ContainerType.ISOCode = containerType.RC_ISOType;
				valueContainer.ContainerType.ContainerCode = containerType.RC_Code;
				valueContainer.ContainerType.IsSpecified = true;
			}

			valueContainer.Seal = container.JC_SealNum;
			valueContainer.PackingMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(container.JC_ContainerMode, string.Empty, context);
			ExportContainerWeights(valueContainer, container);
			valueContainer.IsEmptyContainer = container.JC_IsEmptyContainer;
			valueContainer.IsEmptyContainerSpecified = true;
			valueContainer.IsDamaged = container.JC_IsDamaged;
			valueContainer.IsDamagedSpecified = true;
			valueContainer.IsShipperOwnedContainer = container.JC_IsShipperOwned;
			valueContainer.IsShipperOwnedContainerSpecified = true;
			valueContainer.SetPointTemperatureUnit = container.JC_SetPointTempUnit;
			valueContainer.SetPointTemperatureUnitSpecified = true;
			valueContainer.SetPointTemperature = container.JC_SetPointTemp;
			valueContainer.SetPointTemperatureSpecified = true;
			valueContainer.HumidityPercent = container.JC_HumidityPercent;
			valueContainer.HumidityPercentSpecified = true;
			valueContainer.AirVentFlowRateUnit = container.JC_AirVentFlowRateUnit;
			valueContainer.AirVentFlowRateUnitSpecified = true;
			valueContainer.AirVentFlow = container.JC_AirVentFlow;
			valueContainer.AirVentFlowSpecified = true;
		}

		void ExportPackages(Xsd.AgencyBillOfLading constructedValueObject, IValueObjectExportContext context, IEnumerable<AgencyShipmentPackLine> packLines)
		{
			foreach (AgencyShipmentPackLine packLine in packLines)
			{
				if (ContainerFilter != null && (packLine.Container == null || !ContainerFilter(packLine.Container)))
				{
					continue;
				}

				Xsd.Package valuePackage = constructedValueObject.Packages.AddNew();

				valuePackage.NumberOfPacks = Convert.ToUInt32(packLine.JL_PackageCount);
				valuePackage.PackType = packLine.JL_F3_NKPackType;
				valuePackage.RefNumber = packLine.JL_RefNumber;
				valuePackage.MarksAndNumbers = packLine.JL_MarksAndNumbers;
				valuePackage.GoodsDescription = packLine.JL_DetailedDescription;
				valuePackage.Volume.DimensionType = packLine.JL_ActualVolumeUQ;
				valuePackage.Volume.Value = packLine.JL_ActualVolume;
				valuePackage.Weight.DimensionType = packLine.JL_ActualWeightUQ;
				valuePackage.Weight.Value = packLine.JL_ActualWeight;
				valuePackage.Length.DimensionType = packLine.JL_UnitOfDimension;
				valuePackage.Length.Value = packLine.JL_Length;
				valuePackage.Height.DimensionType = packLine.JL_UnitOfDimension;
				valuePackage.Height.Value = packLine.JL_Height;
				valuePackage.Width.DimensionType = packLine.JL_UnitOfDimension;
				valuePackage.Width.Value = packLine.JL_Width;
				valuePackage.CommodityCode = packLine.JL_RH_NKCommodityCode;

				if (packLine.Container != null)
				{
					valuePackage.ContainerNumber = packLine.Container.JC_ContainerNum;
				}

				ExportCustomAttribute(packLine, valuePackage);

				ExportHazardousGoodsDetail(packLine, valuePackage, context);
			}
		}

		void ExportCustomAttribute(AgencyShipmentPackLine packLine, Xsd.Package valuePackage)
		{
			if (packLine.JL_CustomDate1.IsValid)
			{
				valuePackage.Custom.Date1 = packLine.JL_CustomDate1;
			}

			if (packLine.JL_CustomDate2.IsValid)
			{
				valuePackage.Custom.Date2 = packLine.JL_CustomDate2;
			}

			valuePackage.Custom.Decimal1 = packLine.JL_CustomDecimal1;
			valuePackage.Custom.Decimal2 = packLine.JL_CustomDecimal2;
			valuePackage.Custom.Flag1 = packLine.JL_CustomFlag1 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			valuePackage.Custom.Flag2 = packLine.JL_CustomFlag2 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			valuePackage.Custom.Text1 = packLine.JL_CustomAttrib1;
			valuePackage.Custom.Text2 = packLine.JL_CustomAttrib2;
			valuePackage.Custom.Text3 = packLine.JL_CustomAttrib3;
			valuePackage.Custom.Text4 = packLine.JL_CustomAttrib4;
		}

		void ExportAddresses(TShipment bizObj, Xsd.AgencyBillOfLading constructedValueObject, IValueObjectExportContext context)
		{
			DocAddressValueObjectHelper addressHelper = new DocAddressValueObjectHelper("");
			addressHelper.ExportToValueObjectCollection(bizObj.DocAddresses, constructedValueObject.Addresses.DocAddress, context);
		}

		void ExportHazardousGoodsDetail(PackLine packline, Xsd.Package valuePackage, IValueObjectExportContext context)
		{
			string errorContext = (packline.Shipment == null) ? "PackLine" : Res.GetString("5c1b4cf6-e260-4489-b85e-d0bc49ac7e00", "Pack Line on {0}", packline.Shipment.HumanReadableName);
			valuePackage.DangerousGoods.ExportFromUNDGDataItems(packline.UNDGs.ToArray(), errorContext, context);

			// Legacy
			valuePackage.HazardousGoods.ExportSingleItemFromUNDGDataItems(packline.UNDGs.ToArray());
		}

		void ExportBilling(TShipment shipment, Xsd.AgencyBillOfLading shipmentValue, IValueObjectExportContext context)
		{
			JobHeader header = new JobHeader.Loader(shipment).Load();

			if (header == null || SystemDataRegistry.Instance.IncludeBillingInfoInAgencyXMLFile.Value == Constants.IncludeBillingInfoInXMLMethod.Codes.NotInclude)
			{
				shipmentValue.Billing.IsSpecified = false;
			}
			else
			{
				IValueObjectDataAdapter dataAdapter = CreateBillingDataAdapter();
				dataAdapter.ExportToValueObject(shipment, shipmentValue.Billing, context);
			}
		}

		void ExportNotes(TShipment agencyShipment, Xsd.AgencyBillOfLading value, IValueObjectExportContext context)
		{
			value.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(agencyShipment.Notes, context);
		}

		static void ExportContainerWeights(Xsd.Container containerValue, CommonContainer freightContainer)
		{
			string unit = freightContainer.JC_GrossWeightUQ;
			string description = freightContainer.BindToLists.WeightUnits.GetDescriptionFromCode(freightContainer.JC_GrossWeightUQ);

			containerValue.GrossWeight.IsSpecified = true;
			containerValue.GrossWeight.DimensionType = unit;
			containerValue.GrossWeight.Value = freightContainer.JC_GrossWeight;
			containerValue.GrossWeight.Description = description;

			containerValue.NetWeight.IsSpecified = true;
			containerValue.NetWeight.DimensionType = unit;
			containerValue.NetWeight.Value = freightContainer.JC_Calc_NetWeight;
			containerValue.NetWeight.Description = description;

			containerValue.WeightSpecified = true;
			containerValue.Weight = freightContainer.JC_TareWeight;
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(TShipment bizObj, Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			context.Notify(new InfoNotification(Res.GetString("{0A1E90C2-0F35-4d0a-A284-7A2DEDAABB89}", "Importing shipment with bill '{0}'.", value.BillNumber)));
			ImportShipmentDetail(bizObj, value, context);
			ImportContainers(bizObj, value, context);
			ImportPackages(bizObj, value, context);

			ReferenceNumberDataAdapter.ImportReferenceNumbers(bizObj.Numbers, value.ReferenceNumbers, context);

			bizObj.DefaultWeightAndVolumeUnits();

			ImportAddresses(bizObj, value, context);
			ImportBilling(bizObj, value, context);
			ImportNotes(bizObj, value, context);
			DocDataValueObjectDataAdapter.ImportData(bizObj, value.DocData, context);
			AddImportEvent(bizObj);
		}

		protected override void AfterImportFromValueObject(TShipment bizObj, Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			base.AfterImportFromValueObject(bizObj, value, context);
			bizObj.DeactivateActiveBusinessObjectCollections();
		}

		void ImportShipmentDetail(TShipment bizObj, Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			ZString load = GetPortCode(context, value.Load);
			ZString discharge = GetPortCode(context, value.Discharge);

			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_HouseBillInfo, value.BillNumber);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_CFSReferenceInfo, value.BookingReference);

			if (value.CargoTypeSpecified)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_PackingModeInfo, ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(value.CargoType, string.Empty, context));
			}

			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_HBLAWBChargesDisplayInfo, value.ChargesApply);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.CustomsEntryNumberTypeInfo, value.CustomsEntryNumber.Type);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.CustomsEntryNumberInfo, value.CustomsEntryNumber.Number);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_GoodsDescriptionInfo, value.Description);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.DetailedGoodsDescriptionNoteTextInfo, value.Description);

			if (value.PortOfDestination.IsSpecified)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_RL_NKDestinationInfo, value.PortOfDestination.Port.Value, ForeignKeyType.PortNK);
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_E_ARVInfo, value.PortOfDestination.EstimatedDateTime);
			}
			else if (value.Destination.IsSpecified)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_RL_NKDestinationInfo, value.Destination.Value, ForeignKeyType.PortNK);
			}
			else if (bizObj.JS_RL_NKDestination.IsEmpty)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_RL_NKDestinationInfo, value.Discharge.Value, ForeignKeyType.PortNK);
			}

			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_NKDischargePortInfo, discharge);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_INCOInfo, value.INCOTerm);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_InterimReceiptInfo, value.InterimReceipt);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_HouseBillIssueDateInfo, value.IssueDate);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_NKLoadPortInfo, load);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_MarksAndNumbersInfo, value.MarksAndNumbers);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_RS_NKServiceLevelInfo, value.ServiceLevel);

			if (value.PortOfOrigin.IsSpecified)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_RL_NKOriginInfo, value.PortOfOrigin.Port.Value, ForeignKeyType.PortNK);
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_E_DEPInfo, value.PortOfOrigin.EstimatedDateTime);
			}
			else if (value.Origin.IsSpecified)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_RL_NKOriginInfo, value.Origin.Value, ForeignKeyType.PortNK);
			}
			else if (bizObj.JS_RL_NKOrigin.IsEmpty)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_RL_NKOriginInfo, value.Load.Value, ForeignKeyType.PortNK);
			}

			bizObj.JS_OuterPacks = (int)value.PackageSummary.NumberOfPacks;
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_F3_NKPackTypeInfo, value.PackageSummary.PackType, ForeignKeyType.PackTypeCodeNK);
			XsdDimensionValue.ImportVolume(bizObj.JS_ActualVolumeInfo, bizObj.JS_UnitOfVolumeInfo, value.PackageSummary.Volume, JobShipmentSchema.JS_ActualVolume, context, Res.GetString("46409a26-ef9f-4fde-8875-7391d7bf4f51", "Package summary volume"));
			XsdDimensionValue.ImportWeight(bizObj.JS_ActualWeightInfo, bizObj.JS_UnitOfWeightInfo, value.PackageSummary.Weight, JobShipmentSchema.JS_ActualWeight, context, Res.GetString("c18872b5-3a27-4c53-8f24-543bd83f2c2d", "Package summary weight"));

			if (!bizObj.Lookups.JS_PackType_List.ContainsCode(bizObj.JS_F3_NKPackType))
			{
				bizObj.JS_F3_NKPackType = Constants.PkgUnit.Package;
			}

			//Context.SetPropertyInfoValueIfValueNotEmpty(BizObj.JS_ReleaseTypeInfo, AgencyReleaseTypeXmlCodeMappings.Instance.GetEnterpriseCode(Value.ReleaseType, string.Empty, Context));
			context.SetPropertyInfoValue(bizObj.JS_ReleaseTypeInfo, AgencyReleaseTypeXmlCodeMappings.Instance.GetEnterpriseCode(value.ReleaseType, string.Empty, context), value.ReleaseTypeSpecified);

			if (value.OriginalBillsSpecified)
			{
				bizObj.JS_NoOriginalBills = value.OriginalBills;
			}

			if (value.CopyBillsSpecified)
			{
				bizObj.JS_NoCopyBills = value.CopyBills;
			}

			ZString vesselName = VesselNameImportHelper.MatchVesselAndGetVesselName(value.Sailing, context.Factory);

			ZGuid carrierPK = value.Sailing.Carrier.IsSpecified
				? context.FindOrCreateTempOrganisationPK(value.Sailing.Carrier, bizObj, OrganisationTypes.Carrier)
				: ZGuid.Empty;

			JobVoyage voyage = new JobVoyage.Loader(bizObj.Factory).Load(Constants.TransportModes.Sea, vesselName, value.Sailing.VoyageNo, carrierPK);

			if (voyage == null)
			{
				voyage = bizObj.Factory.New<JobVoyage>();
				context.SetPropertyInfoValue(voyage.JV_RV_NKVesselInfo, vesselName, ForeignKeyType.VesselNameNK);
				context.SetPropertyInfoValue(voyage.JV_VoyageFlightInfo, value.Sailing.VoyageNo, value.Sailing.VoyageNoSpecified);

				if (!carrierPK.IsEmpty)
				{
					voyage.JV_OH_Line = carrierPK;
				}
			}

			ZDateTime etd = value.Sailing.ETD;
			ZDateTime eta = value.Sailing.ETA;

			if (eta < etd)
			{
				eta = etd;
			}

			CreateLoadingIfNoneExists(voyage, load, etd);
			CreateDischargeIfNoneExists(voyage, discharge, eta);
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(load, discharge);

			if (sailing != null)
			{
				bizObj.JS_JX = sailing.PK;
			}

			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_ShippedOnBoardInfo, value.ShippedOnBoard.Code);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_ShippedOnBoardDateInfo, value.ShippedOnBoard.Date);
			context.SetPropertyInfoValueIfValueNotEmpty(bizObj.JS_BookingReferenceInfo, value.ShippersReference);

			if (value.Carrier.IsSpecified)
			{
				bizObj.JS_OA_BookedShippingLineAddress = context.FindOrCreateTempOrganisation(value.Carrier, bizObj, OrganisationTypes.Carrier).MainAddress.PK;
			}

			if (value.Principal.IsSpecified)
			{
				bizObj.JS_OH_DeliveryAgent = context.FindOrCreateTempOrganisationPK(value.Principal, bizObj, OrganisationTypes.Forwarder);
			}
		}

		void ImportContainers(TShipment bizObj, Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			AgencyShipmentContainerDependentCollection containers = bizObj.IsBillOfLadingStage ? bizObj.RealContainers : bizObj.BookedContainers;

			foreach (Xsd.Container valueContainer in value.Containers)
			{
				AgencyShipmentContainer container = FindOrCreateContainer(containers, valueContainer.ContainerNumber);
				ImportContainer(context, valueContainer, container);
			}
		}

		protected virtual void ImportContainer(IValueObjectImportContext context, Xsd.Container valueContainer, AgencyShipmentContainer container)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(container.JC_ContainerNumInfo, valueContainer.ContainerNumber);
			context.SetPropertyInfoValueIfValueNotEmpty(container.JC_ReleaseNumInfo, valueContainer.ReleaseNumber);

			ZGuid containerTypePK = GetContainerTypePK(context, valueContainer.ContainerType);
			if (!containerTypePK.IsEmpty)
			{
				container.JC_RC = containerTypePK;
			}

			context.SetPropertyInfoValueIfValueNotEmpty(container.JC_SealNumInfo, valueContainer.Seal);
			ImportContainerWeights(valueContainer, container, context);
			container.JC_IsEmptyContainer = valueContainer.IsEmptyContainer;
			container.JC_IsDamaged = valueContainer.IsDamaged;
			container.JC_IsShipperOwned = valueContainer.IsShipperOwnedContainer;
			container.JC_ContainerMode = ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(valueContainer.PackingMode, string.Empty, context);

			if (valueContainer.SetPointTemperatureSpecified)
			{
				context.SetPropertyInfoValue(container.JC_SetPointTempInfo, valueContainer.SetPointTemperature, JobContainerSchema.JC_SetPointTemp);
			}

			if (valueContainer.SetPointTemperatureUnitSpecified)
			{
				context.SetPropertyInfoValue(container.JC_SetPointTempUnitInfo, valueContainer.SetPointTemperatureUnit);
			}

			if (valueContainer.HumidityPercentSpecified)
			{
				container.JC_HumidityPercent = valueContainer.HumidityPercent;
			}

			if (valueContainer.AirVentFlowSpecified)
			{
				context.SetPropertyInfoValue(container.JC_AirVentFlowInfo, valueContainer.AirVentFlow, JobContainerSchema.JC_AirVentFlow);
			}

			if (valueContainer.AirVentFlowRateUnitSpecified)
			{
				context.SetPropertyInfoValue(container.JC_AirVentFlowRateUnitInfo, valueContainer.AirVentFlowRateUnit);
			}
		}

		void ImportPackages(TShipment bizObj, Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			bizObj.OuterPackLines.RemoveAndDeleteAll();
			bizObj.TopLevelPacks.RemoveAndDeleteAll();

			Func<AgencyShipmentPackLine> packLineCreator = null;

			if (bizObj.IsTopLevelPacksMode)
			{
				packLineCreator = () =>
				{
					var topLevelPack = bizObj.ShippingContainers.AddNew();
					return AgencyShipmentPackLineAdapter.New(topLevelPack);
				};
			}
			else
			{
				packLineCreator = () => bizObj.OuterPackLines.AddNew();
			}

			AgencyShipmentContainerDependentCollection containers = bizObj.ShippingContainers;

			foreach (Xsd.Package valuePackage in value.Packages)
			{
				AgencyShipmentPackLine packLine = packLineCreator();

				packLine.JL_PackageCount = (int)valuePackage.NumberOfPacks;
				context.SetPropertyInfoValue(packLine.JL_F3_NKPackTypeInfo, valuePackage.PackType, ForeignKeyType.PackTypeCodeNK);
				context.SetPropertyInfoValue(packLine.JL_MarksAndNumbersInfo, valuePackage.MarksAndNumbers, valuePackage.MarksAndNumbersSpecified);
				context.SetPropertyInfoValue(packLine.JL_DescriptionInfo, valuePackage.GoodsDescription, valuePackage.GoodsDescriptionSpecified);
				context.SetPropertyInfoValue(packLine.JL_DetailedDescriptionInfo, valuePackage.GoodsDescription, valuePackage.GoodsDescriptionSpecified);

				if (valuePackage.Length.DimensionType == valuePackage.Height.DimensionType &&
					valuePackage.Height.DimensionType == valuePackage.Width.DimensionType)
				{
					context.SetPropertyInfoValue(packLine.JL_UnitOfDimensionInfo, DimensionUQXmlCodeMappings.Instance.GetEnterpriseCode(valuePackage.Length.DimensionType, Res.GetString("2fd54ecf-9b25-4c1a-9ee6-2d46322ceb72", "Pack line dimension type"), context), valuePackage.Length.DimensionTypeSpecified);
					packLine.JL_Length = valuePackage.Length.Value;
					packLine.JL_Height = valuePackage.Height.Value;
					packLine.JL_Width = valuePackage.Width.Value;
				}
				else
				{
					context.Notify(new ErrorNotification(FreightErrorType.LengthWidthHeightDimensionTypeMustBeSame));
				}

				XsdDimensionValue.ImportVolume(packLine.JL_ActualVolumeInfo, packLine.JL_ActualVolumeUQInfo, valuePackage.Volume, JobPackLinesSchema.JL_ActualVolume, context, Res.GetString("ab0d194d-17ef-435d-8762-8d7d652a1922", "Pack line actual volume"));
				XsdDimensionValue.ImportWeight(packLine.JL_ActualWeightInfo, packLine.JL_ActualWeightUQInfo, valuePackage.Weight, JobPackLinesSchema.JL_ActualWeight, context, Res.GetString("e89f4a29-ff3e-4409-9306-6212ed1353ef", "Pack line actual weight"));

				if (valuePackage.RefNumberSpecified)
				{
					context.SetPropertyInfoValue(packLine.JL_RefNumberInfo, valuePackage.RefNumber);
				}

				if (valuePackage.CommodityCodeSpecified)
				{
					context.SetPropertyInfoValue(packLine.JL_RH_NKCommodityCodeInfo, valuePackage.CommodityCode);
				}

				AgencyShipmentContainer container = FindContainer(containers, valuePackage.ContainerNumber);
				if (container != null)
				{
					packLine.JL_JC = container.PK;
				}

				ImportCustomAttribute(context, valuePackage, packLine);

				ImportHazardousGoodsDetail(packLine, valuePackage, context);
			}
		}

		void ImportCustomAttribute(IValueObjectImportContext context, Xsd.Package valuePackage, AgencyShipmentPackLine packLine)
		{
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib1Info, valuePackage.Custom.Text1, valuePackage.Custom.IsSpecified, "CustomText1");
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib2Info, valuePackage.Custom.Text2, valuePackage.Custom.IsSpecified, "CustomText2");
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib3Info, valuePackage.Custom.Text3, valuePackage.Custom.IsSpecified, "CustomText3");
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib4Info, valuePackage.Custom.Text4, valuePackage.Custom.IsSpecified, "CustomText4");

			if (!valuePackage.Custom.Date1.IsEmpty)
			{
				context.SetPropertyInfoValue(packLine.JL_CustomDate1Info, valuePackage.Custom.Date1.ToSmallDateTime().ToDateTime());
			}

			if (!valuePackage.Custom.Date2.IsEmpty)
			{
				context.SetPropertyInfoValue(packLine.JL_CustomDate2Info, valuePackage.Custom.Date2.ToSmallDateTime().ToDateTime());
			}

			if (valuePackage.Custom.Decimal1Specified)
			{
				packLine.JL_CustomDecimal1 = valuePackage.Custom.Decimal1;
			}

			if (valuePackage.Custom.Decimal2Specified)
			{
				packLine.JL_CustomDecimal2 = valuePackage.Custom.Decimal2;
			}

			if (valuePackage.Custom.Flag1Specified)
			{
				packLine.JL_CustomFlag1 = (valuePackage.Custom.Flag1 == Xsd.TrueFalse.@true);
			}

			if (valuePackage.Custom.Flag2Specified)
			{
				packLine.JL_CustomFlag2 = (valuePackage.Custom.Flag2 == Xsd.TrueFalse.@true);
			}
		}

		void ImportAddresses(TShipment bizObj, Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			DocAddressValueObjectHelper addressHelper = new DocAddressValueObjectHelper("");
			addressHelper.ImportFromValueObjectCollection(value.Addresses.DocAddress, bizObj.DocAddresses, context);
		}

		void ImportHazardousGoodsDetail(PackLine packline, Xsd.Package valuePackage, IValueObjectImportContext context)
		{
			string errorContext = (packline.Shipment == null) ? "PackLine" : Res.GetString("5c1b4cf6-e260-4489-b85e-d0bc49ac7e00", "Pack Line on {0}", packline.Shipment.HumanReadableName);
			var hasData1 = valuePackage.DangerousGoods.ImportToUNDGDataItems(() => packline.UNDGs, errorContext, context);

			// Legacy
			var hasData2 = valuePackage.HazardousGoods.ImportSingleItemToUNDGDataItems(() => packline.UNDGs);
			if (hasData1 || hasData2)
			{
				if (packline.UNDGs.Count > 0)
				{
					ZGuid contactPK = new ContactValueObjectHelper(errorContext).FromContactReferenceGetContactPK(valuePackage.DGContact, context);
					if (contactPK.IsEmpty)
					{
						packline.UNDGs[packline.UNDGs.Count - 1].DI_OC_DGContact = contactPK;
					}
				}
			}
		}

		protected virtual void ImportBilling(TShipment shipment, Xsd.AgencyBillOfLading shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.Billing.IsSpecified && ShouldImportBilling(shipment))
			{
				IValueObjectDataAdapter dataAdapter = CreateBillingDataAdapter();
				dataAdapter.ImportFromValueObject(shipment, shipmentValue.Billing, context);
			}
		}

		static bool ShouldImportBilling(TShipment shipment)
		{
			string value = SystemDataRegistry.Instance.ImportBillingInfoFromAgencyXmlFile.Value;

			switch (value)
			{
				case Constants.ImportBillingInfoFromXMLMethod.Codes.Never:
					return false;
				case Constants.ImportBillingInfoFromXMLMethod.Codes.NewOnly:
					return !shipment.IsInDatabase;
				case Constants.ImportBillingInfoFromXMLMethod.Codes.Always:
					return true;
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognised option '{0}'", value));
			}
		}

		void ImportNotes(TShipment agencyShipment, Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			new NoteValueObjectDataAdapter().ImportNotesAndAttachToBusinessObjectNotes(agencyShipment.Notes, value.Notes, context);

			if (!value.Description.IsEmpty)
			{
				agencyShipment.JS_GoodsDescription = value.Description.SubstringSafe(0, JobShipmentSchema.JS_GoodsDescription.MaxLength);

				if (agencyShipment.DetailedGoodsDescriptionNoteText.IsEmpty)
				{
					agencyShipment.DetailedGoodsDescriptionNoteText = value.Description;
				}
			}
			else
			{
				if (agencyShipment.JS_GoodsDescription.IsEmpty)
				{
					agencyShipment.JS_GoodsDescription = agencyShipment.DetailedGoodsDescriptionNoteText.SubstringSafe(0, JobShipmentSchema.JS_GoodsDescription.MaxLength);
				}
			}
		}

		static void ImportContainerWeights(Xsd.Container containerValue, CommonContainer freightContainer, IValueObjectImportContext context)
		{
			if (containerValue.GrossWeight.IsSpecified && containerValue.NetWeight.IsSpecified)
			{
				string grossUnit;
				string netUnit;
				string targetUnit;

				decimal grossValue;
				decimal netValue;

				if (Constants.Weight.ContainsCode(containerValue.GrossWeight.DimensionType))
				{
					targetUnit = grossUnit = containerValue.GrossWeight.DimensionType;

					if (Constants.Weight.ContainsCode(containerValue.NetWeight.DimensionType))
					{
						netUnit = containerValue.NetWeight.DimensionType;
					}
					else
					{
						netUnit = targetUnit;
					}
				}
				else if (Constants.Weight.ContainsCode(containerValue.NetWeight.DimensionType))
				{
					targetUnit = grossUnit = netUnit = containerValue.NetWeight.DimensionType;
				}
				else if (Constants.Weight.ContainsCode(freightContainer.JC_GrossWeightUQ))
				{
					targetUnit = grossUnit = netUnit = freightContainer.JC_GrossWeightUQ;
				}
				else
				{
					targetUnit = grossUnit = netUnit = Constants.Weight.Kilograms;
				}

				grossValue = Constants.Weight.Convert(containerValue.GrossWeight.Value, grossUnit, targetUnit);
				netValue = Constants.Weight.Convert(containerValue.NetWeight.Value, netUnit, targetUnit);

				freightContainer.JC_GrossWeightUQ = targetUnit;
				context.SetPropertyInfoValue(freightContainer.JC_TareWeightInfo, grossValue - netValue, JobContainerSchema.JC_TareWeight);
				context.SetPropertyInfoValue(freightContainer.JC_GrossWeightInfo, grossValue, JobContainerSchema.JC_GrossWeight);
			}
			else if (containerValue.GrossWeight.IsSpecified)
			{
				if (Constants.Weight.ContainsCode(containerValue.GrossWeight.DimensionType))
				{
					if (Constants.Weight.ContainsCode(freightContainer.JC_GrossWeightUQ))
					{
						decimal tareWeight = Constants.Weight.Convert(freightContainer.JC_TareWeight, freightContainer.JC_GrossWeightUQ, containerValue.GrossWeight.DimensionType);
						context.SetPropertyInfoValue(freightContainer.JC_TareWeightInfo, tareWeight, JobContainerSchema.JC_TareWeight);
					}

					freightContainer.JC_GrossWeightUQ = containerValue.GrossWeight.DimensionType;
				}

				context.SetPropertyInfoValue(freightContainer.JC_GrossWeightInfo, containerValue.GrossWeight.Value, JobContainerSchema.JC_GrossWeight);
			}
			else if (containerValue.NetWeight.IsSpecified)
			{
				if (Constants.Weight.ContainsCode(containerValue.NetWeight.DimensionType))
				{
					if (Constants.Weight.ContainsCode(freightContainer.JC_GrossWeightUQ))
					{
						decimal tareWeight = Constants.Weight.Convert(freightContainer.JC_TareWeight, freightContainer.JC_GrossWeightUQ, containerValue.NetWeight.DimensionType);
						context.SetPropertyInfoValue(freightContainer.JC_TareWeightInfo, tareWeight, JobContainerSchema.JC_TareWeight);
					}

					freightContainer.JC_GrossWeightUQ = containerValue.NetWeight.DimensionType;
				}

				context.SetPropertyInfoValue(freightContainer.JC_GrossWeightInfo, containerValue.NetWeight.Value + freightContainer.JC_TareWeight, JobContainerSchema.JC_GrossWeight);
			}
			else
			{
				context.SetPropertyInfoValue(freightContainer.JC_TareWeightInfo, containerValue.Weight, JobContainerSchema.JC_TareWeight, Res.GetString("ffad8bf1-f28b-41a2-a3ac-2d889bf0c764", "Container tare weight"));
			}
		}

		#endregion

		public Predicate<AgencyShipmentContainer> ContainerFilter { get; set; }

		#region Implementation

		static ZGuid GetContainerTypePK(IValueObjectImportContext context, Xsd.ContainerType type)
		{
			ZGuid result = ZGuid.Empty;

			if (!type.ContainerCode.IsEmpty)
			{
				result = context.Converter.GetPKFromNKGivenFKType(context.Factory, type.ContainerCode, ForeignKeyType.ContainerCodeNK, context);
			}

			if (result.IsEmpty)
			{
				ZQuery filter = new ZQuery(RefContainerSchema.RC_ISOType, type.ISOCode);
				RefContainer refContainer = context.Factory.LoadTop1<RefContainer>(filter);

				result = refContainer == null ? ZGuid.Empty : refContainer.PK;
			}

			return result;
		}

		static IValueObjectDataAdapter CreateBillingDataAdapter()
		{
			return (IValueObjectDataAdapter)ObjectFactory.Get<Accounting.Integration.IAgencyBillingDataAdapter>();
		}

		void CreateDischargeIfNoneExists(JobVoyage voyage, ZString portCode, ZDateTime eta)
		{
			portCode = portCode.SubstringSafe(0, 5);
			VoyageDestination destination = voyage.Destinations.GetDestinationFromDischarge(portCode);

			if (destination == null)
			{
				destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = portCode;
			}

			if (!eta.IsEmpty)
			{
				destination.JB_E_ARV = eta;
			}
		}

		void CreateLoadingIfNoneExists(JobVoyage voyage, ZString portCode, ZDateTime etd)
		{
			portCode = portCode.SubstringSafe(0, 5);
			VoyageOrigin origin = voyage.Origins.GetOriginFromLoading(portCode);

			if (origin == null)
			{
				origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = portCode;
			}

			if (!etd.IsEmpty)
			{
				origin.JA_E_DEP = etd;
			}
		}

		AgencyShipmentContainer FindOrCreateContainer(AgencyShipmentContainerDependentCollection containers, ZString containerNumber)
		{
			return FindContainer(containers, containerNumber) ?? containers.AddNew();
		}
		AgencyShipmentContainer FindContainer(AgencyShipmentContainerDependentCollection containers, ZString containerNumber)
		{
			foreach (AgencyShipmentContainer container in containers)
			{
				if (container.JC_ContainerNum == containerNumber)
				{
					return container;
				}
			}
			return null;
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.AgencyBillsOfLadingSchema; }
		}
		public override string RootCollectionElementName
		{
			get { return "AgencyBillsOfLading"; }
		}
		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleAgencyBillOfLadingSchema; }
		}
		public override string RootElementName
		{
			get { return "AgencyBillOfLading"; }
		}

		protected override TShipment FindBusinessObject(Xsd.AgencyBillOfLading value, IValueObjectImportContext context)
		{
			ZGuid carrierPK = value.Sailing.Carrier.IsSpecified
				? context.FindOrganisationPK(value.Sailing.Carrier, null, OrganisationTypes.Carrier)
				: ZGuid.Empty;

			var locator = new AgencyShipmentLocator<TShipment>(
				context.Factory,
				value.BillNumber,
				GetPortCode(context, value.Load),
				GetPortCode(context, value.Discharge),
				VesselNameImportHelper.MatchVesselAndGetVesselName(value.Sailing, context.Factory),
				value.Sailing.VoyageNo,
				carrierPK);

			return locator.Find();
		}

		ZString GetPortCode(IValueObjectImportContext context, Xsd.UNLOCO unlocoValue)
		{
			var result = ZString.Empty;

			if (unlocoValue.IsSpecified)
			{
				result = context.ConvertRawStringToZTypeValue<ZString>(unlocoValue.Value, ForeignKeyType.PortNK);
			}
			return result.SubstringSafe(0, 5);
		}

		protected override bool ShouldUpdateExistingObject(TShipment bizObj, INotifications notifications)
		{
			QueryUserYesNoYesAllNoAllEventArgs queryUserargs = new QueryUserYesNoYesAllNoAllEventArgs();
			queryUserargs.Message = Res.GetString("0dd5144f-847c-4604-8809-5437891099d5", "An existing Bill of Lading ({0}, {1}) has been found, do you wish to update it?", bizObj.JS_UniqueConsignRef, bizObj.JS_HouseBill);
			queryUserargs.Response = true;
			notifications.QueryUser(queryUserargs);
			return queryUserargs.Response;
		}

		DocDataValueObjectDataAdapter DocDataValueObjectDataAdapter
		{
			get { return docDataValueObjectDataAdapter ?? (docDataValueObjectDataAdapter = new DocDataValueObjectDataAdapter()); }
		}
		DocDataValueObjectDataAdapter docDataValueObjectDataAdapter;

		#endregion
	}
}




