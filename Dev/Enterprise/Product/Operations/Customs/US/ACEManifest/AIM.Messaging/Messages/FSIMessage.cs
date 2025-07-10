using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FSIMessage : FSNMessage
	{
		public FSIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString MessageTypeCode => Constants.AIMMessageSubTypes.FSI;
		public override ZString MessageTypeDescription => "Freight Status Information";

		protected override ZString GetMessageInterpretationHeader()
		{
			return ZString.Empty;
		}
	}
}
