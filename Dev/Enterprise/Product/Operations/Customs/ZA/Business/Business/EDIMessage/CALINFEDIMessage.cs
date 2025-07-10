using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business
{
	public class CALINFEDIMessage : SARSEDIMessage
	{
		public CALINFEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static class CALINFMessageTypes
		{
			public const string SCH = "SCH";
			public const string ASC = "ASC";
		}

		public static class CALINFMessageTypeNames
		{
			public const string SCH = "Maritime Schedule";
			public const string ASC = "Air Schedule";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.CALINF;
		}

		public override ZString LocalReferenceNumber => ParentMessageNumber;

		public override ZString ParentMessageNumber => CALINFMessageHelper?.DocumentNumber ?? ZString.Empty;

		CALINFMessageHelper CALINFMessageHelper => calinfMessageHelper ?? (calinfMessageHelper = CALINFMessageHelper.New(this));
		CALINFMessageHelper calinfMessageHelper;
	}
}
