using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACERegionDistrictPortProcessor : RegionDistrictPortProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[TopLevel(typeof(ERFF101), typeof(ERFF201), typeof(ERFF301), typeof(ERFF401))]
	abstract class RegionDistrictPortProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void Process()
		{
		}

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.RegionDistrictPort; }
		}
	}
}
