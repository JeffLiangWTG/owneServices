using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.ZA.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business
{
	public class REQDOCEDIMessage : SARSEDIMessage, IInterchangeSenderIdProvider
	{
		public REQDOCEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Override Properties

		public override ZString LocalReferenceNumber
		{
			get
			{
				if (!lrn.HasValue)
				{
					lrn = REQDOCHelper?.LocalReferenceNumber ?? ZString.Empty;
				}
				return lrn.Value;
			}
		}
		ZString? lrn;

		REQDOCMessageHelper REQDOCHelper => reqdocHelper ?? (reqdocHelper = REQDOCMessageHelper.New(this));
		REQDOCMessageHelper reqdocHelper;

		public override ZString ParentMessageNumber
		{
			get
			{
				if (!parentMessageNumber.HasValue)
				{
					parentMessageNumber = EM_MessageNum;
				}

				return parentMessageNumber.Value;
			}
		}
		ZString? parentMessageNumber;

		#endregion

		#region IInterchangeSenderIdProvider

		ZString IInterchangeSenderIdProvider.SenderID => REQDOCHelper?.MessageSender ?? ZString.Empty;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.REQDOC;
		}
	}
}
