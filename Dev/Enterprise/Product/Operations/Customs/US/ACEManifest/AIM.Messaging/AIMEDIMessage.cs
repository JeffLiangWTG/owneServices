using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMEDIMessage : EDIMessage
	{
		public AIMEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.USAMA;
		}

		protected string Sender => ApplicationCodeList.Codes.USAMA + "SND";

		protected string Receiver => ApplicationCodeList.Codes.USAMA + "RCV";

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		protected override string GetMessageReferenceNumber()
		{
			return Environment.Env.NumberFountains.EDIFACTNumberFountain("M", Sender, Receiver).GetNextFormatted(Factory);
		}

		public virtual ZString ComponentIdentifier => EM_MessageText.Left(3);
	}
}
