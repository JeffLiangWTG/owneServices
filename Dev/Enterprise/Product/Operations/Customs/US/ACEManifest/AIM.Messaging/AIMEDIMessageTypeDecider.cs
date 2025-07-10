using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AIM.Messaging
{
	[ImmutableObject(true)]
	public class AIMEDIMessageTypeDecider : TypeDecider, Integration.Customs.US.IAIMEDIMessageTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch (row[EDIMessage.Schema.EM_MessageSubType].ToString().Trim())
			{
				case Constants.AIMMessageSubTypes.FER:
					return typeof(FERMessage);
				case Constants.AIMMessageSubTypes.FSI:
					return typeof(FSIMessage);
				case Constants.AIMMessageSubTypes.FSN:
					return typeof(FSNMessage);
				case Constants.AIMMessageSubTypes.FSC:
					return typeof(FSCMessage);
				default:
					return typeof(AIMEDIMessage);
			}
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}
