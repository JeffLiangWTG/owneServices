namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using System.Collections.Generic;
	using CargoWise.Types;

	public interface IMAFCommodity
	{
		ZString GoodsType { get; } // M
		ZString GoodsDescription { get; } // M 255
		IEnumerable<ZString> TariffCodes { get; } // O 11
		IEnumerable<IMAFMeasurement> GoodsMeasurements { get; } // M 1+
		bool? IsNew { get; } // M
		ZInt MergedLineNumber { get; }

		//Cover Sheet
		bool RequirePermits { get; }
		IMAFMeasurement Quantity { get; }
		IMAFMeasurement Measure { get; }
	}
}
