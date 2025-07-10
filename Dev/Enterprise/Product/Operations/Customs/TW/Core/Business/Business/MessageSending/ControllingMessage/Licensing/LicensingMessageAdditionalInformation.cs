using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageAdditionalInformation : IAdditionalInformation
	{
		public LicensingMessageAdditionalInformation(ZString content)
			: this(default, default, content) { }

		public LicensingMessageAdditionalInformation(ZString processNumber, ZString delProcessNumber)
			: this(processNumber, delProcessNumber, default) { }

		public LicensingMessageAdditionalInformation(ZString processNumber, ZString delProcessNumber, ZString content)
		{
			this.processNumber = processNumber;
			this.delProcessNumber = delProcessNumber;
			this.content = content;
		}

		readonly ZString processNumber;
		readonly ZString delProcessNumber;
		readonly ZString content;

		ZInt IAdditionalInformation.CopyQuantity => ZInt.Zero;

		ZString IAdditionalInformation.StatementCode => null;

		ZString IAdditionalInformation.StatementDescription => null;

		ZString IAdditionalInformation.PackingHouse => null;

		ZString IAdditionalInformation.ProcessNumber => processNumber;

		ZString IAdditionalInformation.Content => content;

		ZString IAdditionalInformation.ApprovalID => null;

		ZString IAdditionalInformation.DelProcessNumber => delProcessNumber;
	}
}
