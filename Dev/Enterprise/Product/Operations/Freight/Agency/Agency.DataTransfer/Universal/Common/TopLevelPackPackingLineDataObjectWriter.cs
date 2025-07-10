using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class TopLevelPackPackingLineDataObjectWriter : DataObjectWriter<AgencyShipmentContainer, PackingLine>
	{
		public TopLevelPackPackingLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override PackingLine PopulateDataObject(AgencyShipmentContainer containerBO)
		{
			var packingLineData = new PackingLine(writeManager.WriterStrategy);

			packingLineData.ReferenceNumber = containerBO.JC_ContainerNum;

			packingLineData.GoodsDescription = containerBO.JC_Description;
			packingLineData.MarksAndNos = containerBO.JC_MarksAndNumbers;
			packingLineData.Commodity = ListHelper.GetWithDescription<Commodity>(containerBO.JC_RH_NKContainerCommodityCode, containerBO.ContainerCommodityCode_List);
			packingLineData.HarmonisedCode = containerBO.JC_HarmonisedCode;

			packingLineData.Weight = containerBO.JC_GrossWeight;
			packingLineData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(containerBO.JC_GrossWeightUQ, containerBO.JC_GrossWeightUQ_List);
			packingLineData.Volume = containerBO.JC_GrossVolume;
			packingLineData.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(containerBO.JC_GrossVolumeUQ, containerBO.JC_GrossVolumeUQ_List);

			packingLineData.LinePrice = containerBO.JC_GoodsValue;
			packingLineData.LinePriceCurrency = ListHelper.GetWithDescription<Currency>(containerBO.JC_RX_NKGoodsCurrency, containerBO.RefCurrency_List);

			packingLineData.PackQty = new ZLong(containerBO.JC_ContainerCount);

			packingLineData.Height = containerBO.JC_TotalHeight;
			packingLineData.Length = containerBO.JC_TotalLength;
			packingLineData.Width = containerBO.JC_TotalWidth;
			packingLineData.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(containerBO.JC_TotalUnitOfMeasure, containerBO.JC_TotalUnitOfMeasure_List);

			if (containerBO.IsRollOnRollOff)
			{
				packingLineData.Vehicle = PopulateVehicleDataObject(containerBO);
			}

			packingLineData.SetUNDGCollection(() =>
			{
				var result = new List<UNDG>();
				foreach (var item in containerBO.UNDGs)
				{
					var writer = new UNDGDataObjectWriter(writeManager);
					result.Add(writer.GetDataObject(item));
				}
				return result;
			});

			return packingLineData;
		}

		Vehicle PopulateVehicleDataObject(AgencyShipmentContainer containerBO)
		{
			var vehicle = new Vehicle();
			vehicle.Color = containerBO.JC_VehicleColor;
			vehicle.Make = containerBO.JC_VehicleMake;
			vehicle.Model = containerBO.JC_VehicleModel;
			vehicle.NumberOfDoors = containerBO.JC_VehicleNumberOfDoors;
			vehicle.Transmission = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.JC_VehicleTransmission, containerBO.JC_VehicleTransmission_List);
			vehicle.Year = containerBO.JC_VehicleYear;
			return vehicle;
		}
	}
}
