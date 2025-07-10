using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	/// <summary>
	/// This processor expects one message to contain all foreign ports. All the other ports that are not contained in the message will be deleted
	/// See 'RemoveUnused'
	/// </summary>
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACEForeignPortProcessor : ForeignPortProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[TopLevel(typeof(ERFF104))]
	abstract class ForeignPortProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : DeletingProcessor<AutoUSCForeignPort, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void Process()
		{
		}

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.ForeignPort; }
		}
	}
}
