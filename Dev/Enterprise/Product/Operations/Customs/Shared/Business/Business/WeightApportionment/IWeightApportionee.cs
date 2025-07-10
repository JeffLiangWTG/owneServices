using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public interface IWeightApportionee
	{
		ZDecimal Amount { get; }
		ZDecimal Weight { get; set; }
		ZString WeightUQ { get; set; }
		ZDecimal NetWeight { get; set; }
		ZString NetWeightUQ { get; set; }
		bool NeedToApportionNetWeight { get; }
		ZDecimal MinimumReapportionedLineWeight { get; }
	}

	public interface IWeightHolder
	{
		ZWeight TotalWeight { get; }
		ZWeight TotalNetWeight { get; }
		IWeightApportionee[] AllApportionees { get; }
		IWeightHolder[] WeightHolders { get; }
	}
}
