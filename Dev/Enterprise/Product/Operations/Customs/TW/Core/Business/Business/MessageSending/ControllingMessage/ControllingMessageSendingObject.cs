using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageSendingObject : AutoControllingMessageSendingObject
	{
		public ControllingMessageSendingObject(CusTWControllingMessageHeader header) : base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		public CusTWControllingMessageHeader Header { get; }

		public override ZString MessageType => Header.TW1_ControllingMessageType;

		public override ZString Description => Header.ControllingMessageTypeDescription;

		public override ZString MessageNumber => Header.TW1_FunctionalReferenceId;

		public override ZString CertificateType => Header.TW1_CertificateType;

		public override ZString CertificateTypeDescription => Header.CertificateTypeDescription;

		public override void ValidateShouldSend()
		{
			Validation.ValidateShouldSend();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
		}
	}
}
