using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IHazardousMaterial
	{
		ZBool IsHazRelevant { get; }
		ZBool IsFlashPointTempRelevant { get; }
		ZString HazMatCode { get; }
		ZString HazMatClass { get; }
		ZString HazMatQualifier { get; }
		ZString HazMatDesc { get; }
		ZString ContactName { get; }
		ZDecimal FlashPointTemp { get; }
		ZString HazMatClassificationDesc { get; }
	}
}
