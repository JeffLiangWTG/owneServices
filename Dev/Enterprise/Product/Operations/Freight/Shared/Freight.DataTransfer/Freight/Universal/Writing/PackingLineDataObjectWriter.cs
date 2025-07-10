using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class PackingLineDataObjectWriter<T> : DataObjectWriter<T, PackingLine> where T : PackLine
	{
		public PackingLineDataObjectWriter(BindToLists listCache, IDataWritingManager manager)
			: base(manager)
		{
			this.listCache = listCache;
		}

		readonly BindToLists listCache;

		protected override PackingLine PopulateDataObject(T packLineBO)
		{
			var packingLineData = new PackingLine(writeManager.WriterStrategy);

			var container = packLineBO.JL_JC.IsValid ? packLineBO.Factory.Load<CommonContainer>(packLineBO.JL_JC) : null;

			if (container != null)
			{
				packingLineData.ContainerNumber = container.JC_ContainerNum;
			}

			packingLineData.PackingLineID = packLineBO.JL_PackLineId;
			packingLineData.Commodity = ListHelper.GetWithDescription<Commodity>(packLineBO.JL_RH_NKCommodityCode, packLineBO.RefCommodity_List);
			packingLineData.ContainerPackingOrder = packLineBO.JL_ContainerPackingOrder;
			packingLineData.HarmonisedCode = packLineBO.JL_HarmonisedCode;
			packingLineData.Height = packLineBO.JL_Height;
			packingLineData.ItemNo = packLineBO.JL_ItemNo;
			packingLineData.Length = packLineBO.JL_Length;
			packingLineData.MarksAndNos = packLineBO.JL_MarksAndNumbers;
			packingLineData.CountryOfOrigin = ListHelper.GetWithName<Country>(packLineBO.JL_RN_NKOrigin, packLineBO.CountryOfOrigin_List);
			packingLineData.OutturnQty = packLineBO.JL_Outturn;
			packingLineData.OutturnDamagedQty = packLineBO.JL_Damaged;
			packingLineData.OutturnPillagedQty = packLineBO.JL_Pillaged;
			packingLineData.OutturnComment = packLineBO.JL_OutturnComment;
			packingLineData.OutturnedHeight = packLineBO.JL_OutturnedHeight;
			packingLineData.OutturnedLength = packLineBO.JL_OutturnedLength;
			packingLineData.OutturnedVolume = packLineBO.JL_OutturnedVolume;
			packingLineData.OutturnedWeight = packLineBO.JL_OutturnedWeight;
			packingLineData.OutturnedWidth = packLineBO.JL_OutturnedWidth;
			packingLineData.PackQty = new ZLong(packLineBO.JL_PackageCount);
			packingLineData.PackType = ListHelper.GetWithDescription<PackageType>(packLineBO.JL_F3_NKPackType, packLineBO.JL_F3_NKPackType_List);
			packingLineData.ReferenceNumber = packLineBO.JL_RefNumber;
			packingLineData.ExportReferenceNumber = packLineBO.JL_ExportRefNumber;
			packingLineData.ImportReferenceNumber = packLineBO.JL_ImportRefNumber;
			packingLineData.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(packLineBO.JL_UnitOfDimension, listCache.DimensionUnits);
			packingLineData.Volume = packLineBO.JL_ActualVolume;
			packingLineData.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(packLineBO.JL_ActualVolumeUQ, listCache.VolumeUnits);
			packingLineData.Weight = packLineBO.JL_ActualWeight;
			packingLineData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(packLineBO.JL_ActualWeightUQ, listCache.WeightUnits);
			packingLineData.Width = packLineBO.JL_Width;
			packingLineData.GoodsDescription = packLineBO.JL_Description;
			packingLineData.LoadingMeters = packLineBO.JL_LoadingMeters;
			packingLineData.EndItemNo = packLineBO.JL_EndItemNo;
			packingLineData.LinePrice = packLineBO.JL_LinePrice;
			packingLineData.DetailedDescription = packLineBO.JL_DetailedDescription;

			packingLineData.LastKnownCFSStatusDate = packLineBO.JL_LastKnownTransitWarehouseStatusDateTime;
			packingLineData.LastKnownCFSStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(packLineBO.JL_LastKnownTransitWarehouseStatus, packLineBO.JL_LastKnownTransitWarehouseStatus_List);
			packingLineData.AddOrgAddress(writeManager, packLineBO.LastKnownTransitWarehouseAddress, AddressTypes.LastKnownCFSFacility);

			packingLineData.RequiresTemperatureControl = packLineBO.JL_RequiresTemperatureControl;
			if (packLineBO.JL_RequiresTemperatureControl)
			{
				packingLineData.RequiredTemperatureMinimum = packLineBO.JL_RequiredTemperatureMinimum;
				packingLineData.RequiredTemperatureMaximum = packLineBO.JL_RequiredTemperatureMaximum;
				packingLineData.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = packLineBO.JL_RequiredTemperatureUnit, Description = Constants.Temperature.GetDescription(packLineBO.JL_RequiredTemperatureUnit) };
			}

			packingLineData.SetUNDGCollection(() =>
			{
				if (packLineBO.UNDGs.Count > 0)
				{
					var list = new List<UNDG>();
					foreach (var item in packLineBO.UNDGs)
					{
						var writer = new UNDGDataObjectWriter(writeManager);
						list.Add(writer.GetDataObject(item));
					}
					return list;
				}
				return null;
			});

			packingLineData.SetClassificationCollection(() =>
			{
				var collection = packingLineData.ClassificationCollection;
				if (packLineBO.HarmonisedCodes.Any())
				{
					collection = new DataObjectList<Classification>();
				}
				foreach (var item in packLineBO.HarmonisedCodes)
				{
					var harmonisedCodeWriter = new ClassificationDataObjectWriter<JobPackLineHarmonisedCode>(writeManager);
					collection.Add(harmonisedCodeWriter.GetDataObject(item));
				}
				return collection;
			});

			if (packLineBO.Shipment.JS_PackingMode == Constants.ContainerModes.RollOnRollOff)
			{
				packingLineData.Vehicle = PopulateVehicleDataObject(packLineBO);
			}

			if (packLineBO.Shipment?.IsAir ?? false)
			{
				packingLineData.IsHighRisk = packLineBO.JL_IsHighRisk;
				packingLineData.AviationSecurityAdditionalInspectionType = ListHelper.GetWithDescription<CodeDescriptionPair>(packLineBO.JL_AdditionalInspectionTypeCode, packLineBO.AdditionalInspectionTypes);
				packingLineData.AviationSecurityInspectionType = ListHelper.GetWithDescription<CodeDescriptionPair>(packLineBO.JL_InspectionTypeCode, packLineBO.InspectionTypes);
			}

			return packingLineData;
		}

		protected Vehicle PopulateVehicleDataObject(T packLineBO)
		{
			return new Vehicle()
			{
				Color = packLineBO.JL_VehicleColor,
				Make = packLineBO.JL_VehicleMake,
				Model = packLineBO.JL_VehicleModel,
				NumberOfDoors = packLineBO.JL_VehicleNumberOfDoors,
				Transmission = ListHelper.GetWithDescription<CodeDescriptionPair>(packLineBO.JL_VehicleTransmission, packLineBO.JL_VehicleTransmission_List),
				Year = packLineBO.JL_VehicleYear,
			};
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(T packLineBO)
		{
			var writingHelper = new CustomFieldsDataObjectWritingHelper<PackLine>(packLineBO, new PackLineCustomFieldsDescriptor());
			return writingHelper.GetUserDefinedValues();
		}
	}
}
