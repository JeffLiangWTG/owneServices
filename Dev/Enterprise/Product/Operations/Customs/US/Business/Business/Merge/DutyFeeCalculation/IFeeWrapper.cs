
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	interface IFeeWrapper : IRateWrapper
	{
		ZString ComputationCode { get; }
	}
}
