using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ImmutableObject(true)]
	public class MQEDIMessageTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			string messageType = (string)row[EDIMessageSchema.EM_MessageType.Name];

			switch (messageType)
			{
				case ApplicationIdentifierCodeList.Codes.BrokerManifestDownload:
					return typeof(AMSBrokerDownloadMQEDIMessage);
				case ApplicationIdentifierCodeList.Codes.LineRelease:
					return typeof(LineReleaseMQEDIMessage);
				default:
					return typeof(MQEDIMessage);
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
