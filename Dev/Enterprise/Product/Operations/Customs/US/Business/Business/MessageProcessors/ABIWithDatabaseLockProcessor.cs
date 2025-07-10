using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using static CargoWise.Definitions.Customs.US;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public abstract class ACEABIWithDatabaseLockProcessor : ABIWithDatabaseLockProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>, IDatabaseLockProcessor
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected ABIWithDatabaseLockProcessor()
		{
		}

		protected abstract ReferenceLockType ReferenceLockType { get; }

		#region IDatabaseLockProcessor Members

		string IDatabaseLockProcessor.GetLockType()
		{
			return ReferenceLockType.ToString();
		}

		#endregion
	}
}
