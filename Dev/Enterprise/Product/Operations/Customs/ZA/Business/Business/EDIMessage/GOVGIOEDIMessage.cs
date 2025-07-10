using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business
{
	public class GOVGIOEDIMessage : SARSEDIMessage
	{
		public GOVGIOEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.GOVGIO;
		}

		public override ZString LocalReferenceNumber => ParentMessageNumber;

		public override ZString ParentMessageNumber => GOVGIOMessageHelper?.DocumentNumber ?? ZString.Empty;

		GOVGIOMessageHelper GOVGIOMessageHelper => govgioMessageHelper ?? (govgioMessageHelper = GOVGIOMessageHelper.New(this));
		GOVGIOMessageHelper govgioMessageHelper;
	}
}
