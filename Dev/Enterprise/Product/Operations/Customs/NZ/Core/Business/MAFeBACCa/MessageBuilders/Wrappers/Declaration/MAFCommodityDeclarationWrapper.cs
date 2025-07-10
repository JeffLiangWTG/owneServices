namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;

	public partial class MAFPlugInSupportDeclarationWrapper
	{
		class MAFCommodityDeclarationWrapper : IMAFCommodity
		{
			public MAFCommodityDeclarationWrapper(JobDeclaration declaration)
			{
				this.declaration = declaration;
			}

			#region Implementation of IMAFCommodity

			public ZString GoodsType
			{
				get { return GoodsTypeList.Codes.Miscellaneous; }
			}

			public ZString GoodsDescription
			{
				get { return declaration.JE_GoodsDescription; }
			}

			public IEnumerable<ZString> TariffCodes
			{
				get { return System.Array.Empty<ZString>(); }
			}

			public IEnumerable<IMAFMeasurement> GoodsMeasurements
			{
				get { yield return new MAFMeasurement { MeasurementValue = 0, MeasurementUQ = MeasurementUQList.Codes.kilograms }; }
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
				get { return new MAFMeasurement(); }
			}

			public IMAFMeasurement Measure
			{
				get { return new MAFMeasurement(); }
			}

			#endregion

			readonly JobDeclaration declaration;
		}

		#region MAFMeasurement

		class MAFMeasurement : IMAFMeasurement
		{
			public ZString MeasurementUQ { get; set; }
			public ZDecimal MeasurementValue { get; set; }
		}

		#endregion
	}
}
