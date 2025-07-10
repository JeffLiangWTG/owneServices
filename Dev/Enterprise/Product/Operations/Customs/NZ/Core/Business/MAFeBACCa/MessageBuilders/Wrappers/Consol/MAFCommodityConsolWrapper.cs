namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Freight.Forwarding.Business;

	public partial class MAFPlugInSupportConsolWrapper
	{
		class MAFCommodityConsolWrapper : IMAFCommodity
		{
			public MAFCommodityConsolWrapper(ForwardingConsol consol)
			{
				this.consol = consol;
			}

			#region Implementation of IMAFCommodity

			public ZString GoodsType
			{
				get { return GoodsTypeList.Codes.Miscellaneous; }
			}

			public ZString GoodsDescription
			{
				get { return CargoTypeList.Codes.Fak; }
			}

			public IEnumerable<ZString> TariffCodes
			{
				get { return System.Array.Empty<ZString>(); }
			}

			public IEnumerable<IMAFMeasurement> GoodsMeasurements
			{
				get { yield return new MAFMeasurement { MeasurementValue = consol.JK_TotalShipmentQuantity, MeasurementUQ = MeasurementUQList.Codes.unit }; }
			}

			public bool? IsNew
			{
				get { return false; }
			}

			public ZInt MergedLineNumber
			{
				get { return 0; }
			}

			public bool RequirePermits
			{
				get { return true; }
			}

			public IMAFMeasurement Quantity
			{
				get { return GoodsMeasurements.First(); }
			}

			public IMAFMeasurement Measure
			{
				get
				{
					var measure = new MAFMeasurement();
					if (consol.JK_TotalShipmentWeight != 0)
					{
						measure.MeasurementValue = consol.JK_TotalShipmentWeight;
						measure.MeasurementUQ = consol.JK_TotalShipmentWeightUnit;
					}
					else if (consol.JK_TotalShipmentVolume != 0)
					{
						measure.MeasurementValue = consol.JK_TotalShipmentVolume;
						measure.MeasurementUQ = consol.JK_TotalShipmentVolumeUnit;
					}
					return measure;
				}
			}

			#endregion

			#region MAFMeasurement

			class MAFMeasurement : IMAFMeasurement
			{
				public ZString MeasurementUQ { get; set; }
				public ZDecimal MeasurementValue { get; set; }
			}

			#endregion

			readonly ForwardingConsol consol;
		}
	}
}
