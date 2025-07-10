using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business
{
	public class COSTCOEDIMessage : SARSEDIMessage
	{
		public COSTCOEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.COSTCO;
		}

		public override ZString LocalReferenceNumber => ParentMessageNumber;

		public override ZString ParentMessageNumber => COSTCOMessageHelper?.DocumentNumber ?? ZString.Empty;

		COSTCOMessageHelper COSTCOMessageHelper => costcoMessageHelper ?? (costcoMessageHelper = COSTCOMessageHelper.New(this));
		COSTCOMessageHelper costcoMessageHelper;
	}
}
