using System;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class TopLevelPackPackingLineDataObjectReader<T> : DataObjectReader<PackingLine, T> where T : AgencyShipmentContainer
	{
		public TopLevelPackPackingLineDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<PackingLine, T> bizObjProvider = null) : base(dataObject, logger, factory)
		{
			this.bizObjProvider = bizObjProvider;
		}

		readonly Func<PackingLine, T> bizObjProvider;

		protected override T GetExistingBusinessObject()
		{
			return bizObjProvider != null ? bizObjProvider(dataObject) : null;
		}

		protected override void PopulateBusinessObject(T container)
		{
			if (dataObject.Vehicle != null)
			{
				container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			}

			SetValue(container, JobContainerSchema.JC_ContainerNum, dataObject.ReferenceNumber);

			SetValue(container, JobContainerSchema.JC_TotalHeight, dataObject.Height);
			SetValue(container, JobContainerSchema.JC_TotalLength, dataObject.Length);
			SetValue(container, JobContainerSchema.JC_TotalWidth, dataObject.Width);
			SetValue(container, JobContainerSchema.JC_TotalUnitOfMeasure, dataObject.LengthUnit);

			SetValue(container, JobContainerSchema.JC_GrossVolume, dataObject.Volume);
			SetValue(container, JobContainerSchema.JC_GrossVolumeUQ, dataObject.VolumeUnit);
			SetValue(container, JobContainerSchema.JC_GrossWeight, dataObject.Weight);
			SetValue(container, JobContainerSchema.JC_GrossWeightUQ, dataObject.WeightUnit);

			SetValue(container, JobContainerSchema.JC_Description, dataObject.GetCleanSingleLineGoodsDescription());
			SetValue(container, JobContainerSchema.JC_MarksAndNumbers, dataObject.MarksAndNos);
			SetValue(container, JobContainerSchema.JC_RH_NKContainerCommodityCode, dataObject.Commodity);
			SetValue(container, JobContainerSchema.JC_HarmonisedCode, dataObject.HarmonisedCode);
			SetValue(container, JobContainerSchema.JC_GoodsValue, dataObject.LinePrice);
			SetValue(container, JobContainerSchema.JC_RX_NKGoodsCurrency, dataObject.LinePriceCurrency);

			SetValue(container, JobContainerSchema.JC_ContainerCount, dataObject.PackQty);

			PopulateBusinessObjectFromVehicle(container);

			if (dataObject.UNDGCollection != null)
			{
				container.UNDGs.DeleteAll();
				foreach (var source in dataObject.UNDGCollection)
				{
					var reader = new UNDGDataObjectReader(source, logger, factory);
					container.UNDGs.Add(reader.ReadIntoBusinessObject());
				}
			}

			SetValue(container, JobContainerSchema.JC_GrossVolume, container.JC_GrossVolume);
		}

		void PopulateBusinessObjectFromVehicle(T container)
		{
			if (dataObject.Vehicle != null)
			{
				var vehicle = dataObject.Vehicle;
				SetValue(container, JobContainerSchema.JC_VehicleColor, vehicle.Color);
				SetValue(container, JobContainerSchema.JC_VehicleMake, vehicle.Make);
				SetValue(container, JobContainerSchema.JC_VehicleModel, vehicle.Model);
				SetValue(container, JobContainerSchema.JC_VehicleNumberOfDoors, vehicle.NumberOfDoors);
				SetValue(container, JobContainerSchema.JC_VehicleTransmission, vehicle.Transmission);
				SetValue(container, JobContainerSchema.JC_VehicleYear, vehicle.Year);
			}
		}
	}
}



