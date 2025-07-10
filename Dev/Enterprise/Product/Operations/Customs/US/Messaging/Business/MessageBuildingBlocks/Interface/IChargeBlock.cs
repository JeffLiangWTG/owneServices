using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IChargeBlock
	{
		ZString AccountingClassCode { get; set; }
		ZDecimal UserFeeAmount { get; set; }
	}
}
