using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESEconomicOperatorProvider : IEconomicOperator
{
	readonly JobDocAddress jobDocAddress;

	public AESEconomicOperatorProvider(JobDocAddress jobDocAddress)
	{
		this.jobDocAddress = Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
	}

	public string IdentificationNumber => jobDocAddress.E2_GovRegNum.ToString();
}
