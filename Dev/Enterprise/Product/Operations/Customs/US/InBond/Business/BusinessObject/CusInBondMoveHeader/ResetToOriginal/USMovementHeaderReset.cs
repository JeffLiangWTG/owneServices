using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class USMovementHeaderReset : AutoUSMovementHeaderReset
	{
		public USMovementHeaderReset(IResetToOriginal businessEntity, USMovementHeaderResetCollection coll)
			: base(businessEntity.Factory)
		{
			this.businessEntity = businessEntity;
			this.Coll = coll;
		}
		readonly IResetToOriginal businessEntity;
		internal readonly USMovementHeaderResetCollection Coll;

		#region New Properties

		public ZString RO_MovementUniqueCode
		{
			get { return businessEntity.InBondNumber; }
		}

		public ZString RO_CustomsStatusDescription
		{
			get { return businessEntity.MovementDescription; }
		}

		public ZString RO_CustomsStatus
		{
			get { return businessEntity.CustomsStatus; }
		}

		public ZString RO_BillNumber => businessEntity.BillNumber;

		public ZString RO_ContainerNumber => businessEntity.ContainerNumber;

		public ZString RO_Level => businessEntity.Level;

		#endregion

		public override ZBool RO_ResetToOriginal
		{
			get { return base.RO_ResetToOriginal; }
			set
			{
				base.RO_ResetToOriginal = value;

				foreach (USMovementHeaderReset uSMovementHeaderReset in Coll)
				{
					if (uSMovementHeaderReset != this)
					{
						uSMovementHeaderReset.Validation.ValidateRO_ResetToOriginal();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(RO_ResetReason_ReadOnly))]
		public override ZString RO_ResetReason
		{
			get { return base.RO_ResetReason; }
			set { base.RO_ResetReason = value; }
		}

		public bool RO_ResetReason_ReadOnly
		{
			get { return !RO_ResetToOriginal; }
		}

		public void ResetToOriginal()
		{
			businessEntity.ResetStatus(RO_ResetReason);
			businessEntity.LogManager.CancelAll();
			businessEntity.Messages.UpdateStatusOfAllMessagesTo(EDIMessage.Status.Discarded);
		}
	}
}
