using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class JobContainerValueObjectDataAdapter<TBusinessObject, TValueObject> : FreightValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : CommonContainer
		where TValueObject : Xsd.Container
	{
		protected override bool RegistryDefaultForImporting
		{
			get { return true; }
		}

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Containers"; }
		}

		public override string RootElementName
		{
			get { return (NoResString)"Container"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleJobContainer; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		protected override void ImportFromValueObjectCore(TBusinessObject bizObj, TValueObject containerValue, IValueObjectImportContext context)
		{
			CommonContainer freightContainer = bizObj;

			if (!containerValue.ContainerNumber.IsEmpty && containerValue.ContainerCount != 1)
			{
				context.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, Res.GetString("cdab5d89-6823-4621-bc22-437478bf7b6b", "You cannot specify a Container Number and a Container Count at the same time")));
			}

			context.SetPropertyInfoValue(freightContainer.JC_ContainerNumInfo, containerValue.ContainerNumber, containerValue.ContainerNumberSpecified);
			freightContainer.JC_ContainerCount = (ZShort)containerValue.ContainerCount;

			new ContainerValueObjectHelper(context).ImportContainerType(freightContainer.JC_RCInfo, containerValue.ContainerType);

			ImportContainerWeights(containerValue, freightContainer, context);

			context.SetPropertyInfoValue(freightContainer.JC_SealNumInfo, containerValue.Seal, containerValue.SealSpecified);
			context.SetPropertyInfoValue(freightContainer.JC_AdditionalSealNumInfo, containerValue.Seal2, containerValue.Seal2Specified);
			context.SetPropertyInfoValue(freightContainer.JC_Additional2SealNumInfo, containerValue.Seal3, containerValue.Seal3Specified);
			context.SetPropertyInfoValueIfValueNotEmpty(freightContainer.JC_ContainerModeInfo, ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(containerValue.PackingMode.ToString(), Res.GetString("4df3fb01-660b-4d2a-a58d-85d1989ab313", "Container {0}", freightContainer.JC_ContainerNum), context));

			if (containerValue.DeliveryModeSpecified)
			{
				context.SetPropertyInfoValue(freightContainer.JC_DeliveryModeInfo, containerValue.DeliveryMode, containerValue.DeliveryModeSpecified);
			}

			if (containerValue.EstimatedDelivery.IsValid)
			{
				context.SetPropertyInfoValue(freightContainer.JC_ArrivalEstimatedDeliveryInfo, containerValue.EstimatedDelivery.ToDateTime());
			}

			if (containerValue.LCLAvailable.IsValid)
			{
				context.SetPropertyInfoValue(freightContainer.JC_LCLAvailableInfo, containerValue.LCLAvailable.ToDateTime());
			}

			if (containerValue.FCLAvailable.IsValid)
			{
				context.SetPropertyInfoValue(freightContainer.JC_FCLAvailableInfo, containerValue.FCLAvailable.ToDateTime());
			}

			context.SetPropertyInfoValue(freightContainer.JC_RH_NKContainerCommodityCodeInfo, containerValue.CommodityCode, containerValue.CommodityCodeSpecified);
			context.SetPropertyInfoValue(freightContainer.JC_ReleaseNumInfo, containerValue.ReleaseNumber, containerValue.ReleaseNumberSpecified);

			if (containerValue.SetPointTemperatureSpecified)
			{
				context.SetPropertyInfoValue(freightContainer.JC_SetPointTempInfo, containerValue.SetPointTemperature, JobContainerSchema.JC_SetPointTemp);
			}

			if (!containerValue.SetPointTemperatureUnit.IsEmpty)
			{
				context.SetPropertyInfoValue(freightContainer.JC_SetPointTempUnitInfo, containerValue.SetPointTemperatureUnit);
			}

			if (containerValue.HumidityPercentSpecified)
			{
				freightContainer.JC_HumidityPercent = containerValue.HumidityPercent;
			}

			if (containerValue.IsShipperOwnedContainerSpecified)
			{
				freightContainer.JC_IsShipperOwned = containerValue.IsShipperOwnedContainer;
			}

			#region Export Process
			if (containerValue.ExportProcess.IsSpecified)
			{
				if (!containerValue.ExportProcess.BookingReference.IsEmpty)
				{
					context.SetPropertyInfoValue(freightContainer.JC_ReleaseNumInfo, containerValue.ExportProcess.ReleaseNumber, containerValue.ExportProcess.ReleaseNumberSpecified);
				}
				if (containerValue.ExportProcess.EmptyRequiredBy.IsValid)
				{
					freightContainer.JC_EmptyRequired = containerValue.ExportProcess.EmptyRequiredBy;
				}
				if (containerValue.ExportProcess.EstimatedFullPickup.IsValid)
				{
					freightContainer.JC_DepartureEstimatedPickup = containerValue.ExportProcess.EstimatedFullPickup;
				}
				if (containerValue.ExportProcess.CartageAdvised.IsValid)
				{
					freightContainer.JC_DepartureCartageAdvised = containerValue.ExportProcess.CartageAdvised;
				}
				context.SetPropertyInfoValue(freightContainer.JC_DepartureSlotReferenceInfo, containerValue.ExportProcess.SlotBookingRef, containerValue.ExportProcess.SlotBookingRefSpecified);

				if (containerValue.ExportProcess.SlotDate.IsValid)
				{
					freightContainer.JC_DepartureSlotDateTime = containerValue.ExportProcess.SlotDate;
				}
				context.SetPropertyInfoValue(freightContainer.JC_DepartureCartageRefInfo, containerValue.ExportProcess.CartageRef, containerValue.ExportProcess.CartageRefSpecified);

				if (containerValue.ExportProcess.PickupEmptyFrom != null)
				{
					freightContainer.JC_OA_DepartureContainerYardAddress = new AddressValueObjectHelper(Res.GetString("b2d4f0b9-7350-4754-9ee2-12bbce3ff771", "Pickup Empty From on {0}", freightContainer.JC_ContainerNum)).FromAddressReferenceGetAddressPK(containerValue.ExportProcess.PickupEmptyFrom, context);
				}
				if (containerValue.ExportProcess.ContainerYardGateOut.IsValid)
				{
					freightContainer.JC_ContainerYardEmptyPickupGateOut = containerValue.ExportProcess.ContainerYardGateOut;
				}
				if (containerValue.ExportProcess.WharfGateIn.IsValid)
				{
					freightContainer.JC_FCLWharfGateIn = containerValue.ExportProcess.WharfGateIn;
				}
				if (containerValue.ExportProcess.CartageComplete.IsValid)
				{
					freightContainer.JC_DepartureCartageComplete = containerValue.ExportProcess.CartageComplete;
				}
				if (containerValue.ExportProcess.ShippedOnboard.IsValid)
				{
					freightContainer.JC_FCLOnBoardVessel = containerValue.ExportProcess.ShippedOnboard;
				}
				if (containerValue.ExportProcess.DemurrageTime.IsValid)
				{
					freightContainer.DepartureTruckWaitTime = containerValue.ExportProcess.DemurrageTime;
				}
			}
			#endregion

			#region Import Process
			if (containerValue.ImportProcess.IsSpecified)
			{
				context.SetPropertyInfoValue(freightContainer.JC_ContainerImportDOReleaseInfo, containerValue.ImportProcess.ReleaseNumber, containerValue.ImportProcess.ReleaseNumberSpecified);

				if (containerValue.ImportProcess.FCLAvailable.IsValid)
				{
					freightContainer.JC_FCLAvailable = containerValue.ImportProcess.FCLAvailable;
				}

				if (containerValue.ImportProcess.FCLStorage.IsValid)
				{
					freightContainer.JC_ArrivalCTOStorageStartDate = containerValue.ImportProcess.FCLStorage;
				}

				if (containerValue.ImportProcess.LCLAvailable.IsValid)
				{
					freightContainer.JC_LCLAvailable = containerValue.ImportProcess.LCLAvailable;
				}

				if (containerValue.ImportProcess.LCLStorage.IsValid)
				{
					freightContainer.JC_LCLStorageCommences = containerValue.ImportProcess.LCLStorage;
				}

				if (containerValue.ImportProcess.WharfUnload.IsValid)
				{
					freightContainer.JC_FCLUnloadFromVessel = containerValue.ImportProcess.WharfUnload;
				}

				if (containerValue.ImportProcess.SlotDate.IsValid)
				{
					freightContainer.JC_ArrivalSlotDateTime = containerValue.ImportProcess.SlotDate;
				}

				context.SetPropertyInfoValue(freightContainer.JC_ArrivalSlotReferenceInfo, containerValue.ImportProcess.SlotBookingRef, containerValue.ImportProcess.SlotBookingRefSpecified);
				context.SetPropertyInfoValue(freightContainer.JC_ArrivalCartageRefInfo, containerValue.ImportProcess.CartageRef, containerValue.ImportProcess.CartageRefSpecified);

				if (containerValue.ImportProcess.WharfGateOut.IsValid)
				{
					freightContainer.JC_FCLWharfGateOut = containerValue.ImportProcess.WharfGateOut;
				}

				if (containerValue.ImportProcess.EstimatedDelivery.IsValid)
				{
					freightContainer.JC_ArrivalEstimatedDelivery = containerValue.ImportProcess.EstimatedDelivery;
				}

				if (containerValue.ImportProcess.CartageAdvised.IsValid)
				{
					freightContainer.JC_ArrivalCartageAdvised = containerValue.ImportProcess.CartageAdvised;
				}

				if (containerValue.ImportProcess.CartageComplete.IsValid)
				{
					freightContainer.JC_ArrivalCartageComplete = containerValue.ImportProcess.CartageComplete;
				}

				if (containerValue.ImportProcess.DeliverEmptyTo != null)
				{
					freightContainer.JC_OA_ArrivalContainerYardAddress = new AddressValueObjectHelper(Res.GetString("cac222c6-fac9-4291-8b99-f012d1fc2c88", "Deliver Empty To on {0}", freightContainer.JC_ContainerNum)).FromAddressReferenceGetAddressPK(containerValue.ImportProcess.DeliverEmptyTo, context);
				}

				if (containerValue.ImportProcess.EmptyReady.IsValid)
				{
					freightContainer.JC_EmptyReadyForReturn = containerValue.ImportProcess.EmptyReady;
				}

				if (containerValue.ImportProcess.EmptyReturnRequiredBy.IsValid)
				{
					freightContainer.JC_EmptyReturnedBy = containerValue.ImportProcess.EmptyReturnRequiredBy;
				}

				if (containerValue.ImportProcess.EmptyReturnedOn.IsValid)
				{
					freightContainer.JC_ContainerYardEmptyReturnGateIn = containerValue.ImportProcess.EmptyReturnedOn;
				}

				if (containerValue.ImportProcess.DemurrageTime.IsValid)
				{
					freightContainer.ArrivalTruckWaitTime = containerValue.ImportProcess.DemurrageTime;
				}
			}
			#endregion
		}

		static void ImportContainerWeights(TValueObject containerValue, CommonContainer freightContainer, IValueObjectImportContext context)
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
				else
				{
					targetUnit = grossUnit = netUnit = freightContainer.JC_GrossWeightUQ;
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
						context.SetPropertyInfoValue(freightContainer.JC_TareWeightInfo, Constants.Weight.Convert(freightContainer.JC_TareWeight, freightContainer.JC_GrossWeightUQ, containerValue.GrossWeight.DimensionType), JobContainerSchema.JC_TareWeight);
					}

					freightContainer.JC_GrossWeightUQ = containerValue.GrossWeight.DimensionType;
				}

				context.SetPropertyInfoValue(freightContainer.JC_GrossWeightInfo, containerValue.GrossWeight.Value, JobContainerSchema.JC_GrossWeight);
			}
			else if (containerValue.NetWeight.IsSpecified)
			{
				if (Constants.Weight.ContainsCode(freightContainer.JC_GrossWeightUQ))
				{
					context.SetPropertyInfoValue(freightContainer.JC_TareWeightInfo, Constants.Weight.Convert(freightContainer.JC_TareWeight, freightContainer.JC_GrossWeightUQ, containerValue.NetWeight.DimensionType), JobContainerSchema.JC_TareWeight);
				}

				if (Constants.Weight.ContainsCode(containerValue.NetWeight.DimensionType))
				{
					freightContainer.JC_GrossWeightUQ = containerValue.NetWeight.DimensionType;
				}

				context.SetPropertyInfoValue(freightContainer.JC_GrossWeightInfo, containerValue.NetWeight.Value + freightContainer.JC_TareWeight, JobContainerSchema.JC_GrossWeight);
			}
			else if (containerValue.WeightSpecified)
			{
				context.SetPropertyInfoValue(freightContainer.JC_GrossWeightInfo, containerValue.Weight, JobContainerSchema.JC_GrossWeight);
			}
		}

		protected override void ExportToValueObjectCore(TBusinessObject bizObj, TValueObject constructedValueObject, IValueObjectExportContext context)
		{
			CommonContainer freightContainer = bizObj;
			Xsd.Container xsdContainer = constructedValueObject;

			xsdContainer.ContainerNumber = freightContainer.JC_ContainerNum;
			xsdContainer.ContainerCount = freightContainer.JC_ContainerCount;
			xsdContainer.ContainerType = new ContainerTypeValueObjectDataAdapter().ExportToValueObject(freightContainer.Container, context);
			xsdContainer.Seal = freightContainer.JC_SealNum;
			xsdContainer.Seal2 = freightContainer.JC_AdditionalSealNum;
			xsdContainer.Seal3 = freightContainer.JC_Additional2SealNum;

			ExportContainerWeights(constructedValueObject, freightContainer);

			xsdContainer.PackingMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(freightContainer.JC_ContainerMode, Res.GetString("78698785-031b-484c-89c5-2d20e6687ace", "Container {0}", freightContainer.JC_ContainerNum), context);

			if (freightContainer.JC_ArrivalEstimatedDelivery.IsValid)
			{
				xsdContainer.EstimatedDelivery = freightContainer.JC_ArrivalEstimatedDelivery.ToDateTime();
			}

			if (!freightContainer.JC_DeliveryMode.IsEmpty)
			{
				xsdContainer.DeliveryMode = freightContainer.JC_DeliveryMode;
				xsdContainer.DeliveryModeSpecified = true;
			}

			if (freightContainer.JC_LCLAvailable.IsValid)
			{
				xsdContainer.LCLAvailable = freightContainer.JC_LCLAvailable;
			}

			if (freightContainer.JC_FCLAvailable.IsValid)
			{
				xsdContainer.FCLAvailable = freightContainer.JC_FCLAvailable;
			}
			xsdContainer.ReleaseNumber = freightContainer.JC_ReleaseNum;
			xsdContainer.CommodityCode = freightContainer.JC_RH_NKContainerCommodityCode;

			if (freightContainer.JC_IsControlledAtmosphere)
			{
				xsdContainer.SetPointTemperature = freightContainer.JC_SetPointTemp;
				xsdContainer.SetPointTemperatureSpecified = true;
				xsdContainer.SetPointTemperatureUnit = freightContainer.JC_SetPointTempUnit;
				xsdContainer.HumidityPercent = freightContainer.JC_HumidityPercent;
				xsdContainer.HumidityPercentSpecified = true;
			}

			if (freightContainer.JC_IsShipperOwned)
			{
				xsdContainer.IsShipperOwnedContainer = freightContainer.JC_IsShipperOwned;
				xsdContainer.IsShipperOwnedContainerSpecified = true;
			}

			#region Export Process

			xsdContainer.ExportProcess.ReleaseNumber = freightContainer.JC_ReleaseNum;

			xsdContainer.ExportProcess.EmptyRequiredBy = freightContainer.JC_EmptyRequired;
			xsdContainer.ExportProcess.EstimatedFullPickup = freightContainer.JC_DepartureEstimatedPickup;
			xsdContainer.ExportProcess.CartageAdvised = freightContainer.JC_DepartureCartageAdvised;
			xsdContainer.ExportProcess.SlotBookingRef = freightContainer.JC_DepartureSlotReference;
			xsdContainer.ExportProcess.SlotDate = freightContainer.JC_DepartureSlotDateTime;
			xsdContainer.ExportProcess.CartageRef = freightContainer.JC_DepartureCartageRef;

			xsdContainer.ExportProcess.PickupEmptyFrom = new AddressValueObjectHelper(Res.GetString("a55b9f25-8c00-4b75-abf1-28480461383e", "Pickup Empty From for {0}", freightContainer.JC_ContainerNum)).ToAddressReference(freightContainer.DepartureContainerYardAddress, context);

			xsdContainer.ExportProcess.ContainerYardGateOut = freightContainer.JC_ContainerYardEmptyPickupGateOut;
			xsdContainer.ExportProcess.WharfGateIn = freightContainer.JC_FCLWharfGateIn;
			xsdContainer.ExportProcess.CartageComplete = freightContainer.JC_DepartureCartageComplete;
			xsdContainer.ExportProcess.ShippedOnboard = freightContainer.JC_FCLOnBoardVessel;
			xsdContainer.ExportProcess.DemurrageTime = freightContainer.DepartureTruckWaitTime;

			#endregion

			#region Import Process

			xsdContainer.ImportProcess.ReleaseNumber = freightContainer.JC_ContainerImportDORelease;
			xsdContainer.ImportProcess.FCLAvailable = freightContainer.JC_FCLAvailable;
			xsdContainer.ImportProcess.FCLStorage = freightContainer.JC_ArrivalCTOStorageStartDate;
			xsdContainer.ImportProcess.LCLAvailable = freightContainer.JC_LCLAvailable;
			xsdContainer.ImportProcess.LCLStorage = freightContainer.JC_LCLStorageCommences;
			xsdContainer.ImportProcess.WharfUnload = freightContainer.JC_FCLUnloadFromVessel;
			xsdContainer.ImportProcess.SlotBookingRef = freightContainer.JC_ArrivalSlotReference;
			xsdContainer.ImportProcess.SlotDate = freightContainer.JC_ArrivalSlotDateTime;
			xsdContainer.ImportProcess.CartageRef = freightContainer.JC_ArrivalCartageRef;
			xsdContainer.ImportProcess.WharfGateOut = freightContainer.JC_FCLWharfGateOut;
			xsdContainer.ImportProcess.EstimatedDelivery = freightContainer.JC_ArrivalEstimatedDelivery;
			xsdContainer.ImportProcess.CartageAdvised = freightContainer.JC_ArrivalCartageAdvised;
			xsdContainer.ImportProcess.CartageComplete = freightContainer.JC_ArrivalCartageComplete;

			xsdContainer.ImportProcess.DeliverEmptyTo = new AddressValueObjectHelper(Res.GetString("7f452d4d-7fc0-40de-96db-e5407747c175", "Deliver Empty To for {0}", freightContainer.JC_ContainerNum)).ToAddressReference(freightContainer.ArrivalContainerYardAddress, context);

			xsdContainer.ImportProcess.EmptyReady = freightContainer.JC_EmptyReadyForReturn;
			xsdContainer.ImportProcess.EmptyReturnRequiredBy = freightContainer.JC_EmptyReturnedBy;
			xsdContainer.ImportProcess.EmptyReturnedOn = freightContainer.JC_ContainerYardEmptyReturnGateIn;

			xsdContainer.ImportProcess.DemurrageTime = freightContainer.ArrivalTruckWaitTime;

			#endregion
		}

		static void ExportContainerWeights(TValueObject containerValue, CommonContainer freightContainer)
		{
			if (!freightContainer.JC_GrossWeight.IsEmpty)
			{
				string unit = freightContainer.JC_GrossWeightUQ;
				string description = freightContainer.BindToLists.WeightUnits.GetDescriptionFromCode(unit);

				containerValue.GrossWeight.IsSpecified = true;
				containerValue.GrossWeight.DimensionType = unit;
				containerValue.GrossWeight.Description = description;
				containerValue.GrossWeight.Value = freightContainer.JC_GrossWeight;

				containerValue.NetWeight.IsSpecified = true;
				containerValue.NetWeight.DimensionType = unit;
				containerValue.NetWeight.Description = description;
				containerValue.NetWeight.Value = freightContainer.JC_Calc_NetWeight;

				containerValue.WeightSpecified = true;
				containerValue.Weight = freightContainer.JC_GrossWeight;
			}
		}
	}
}
