using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IJobDeclarationMessageSendingObject
	{
		ZBool ShouldSend { get; }
		CusEntryHeader Header { get; }
	}

	public class JobDeclarationMessageSendingObject : AutoJobDeclarationMessageSendingObject, IJobDeclarationMessageSendingObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobDeclarationMessageSendingObject(CusEntryHeader header) : base(header.Factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Header = Argument.NotNull(header, nameof(header));
				SetMessageSendingObjectDefaultValues();
			}
		}

		public CusEntryHeader Header { get; }

		#region Overrides of AutoJobDeclarationMessageSendingObject

		protected override bool MessageType_ReadOnly => true;

		public override ZString DeclarationType => Header.EntryInstruction?.CEI_Style ?? ZString.Empty;

		public override ZString EntryStatus => Header.CH_EntryStatus;

		protected override bool LocalReferenceNumber_ReadOnly => true;

		protected override bool MovementReferenceNumber_ReadOnly => true;

		protected virtual void SetMessageSendingObjectDefaultValues()
		{
			MovementReferenceNumber = Header.MovementReferenceNumber;
			LocalReferenceNumber = Header.CH_BGMReference;
			MessageType = GetDefaultMessageType();
		}

		protected virtual ZString GetDefaultMessageType()
		{
			return Header.CH_MessageType;
		}

		public override void ValidateShouldSend()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateShouldSend();
			}
		}

		#endregion
	}
}
