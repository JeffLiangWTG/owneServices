using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	[ImmutableObject(true)]
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.US.eManifest.IEDIMessageTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			switch (messageType)
			{
				case MessageTypes.Codes.SyntaxError:
					return typeof(SyntaxErrorMessage);
				default:
					return typeof(EDIMessage);
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
