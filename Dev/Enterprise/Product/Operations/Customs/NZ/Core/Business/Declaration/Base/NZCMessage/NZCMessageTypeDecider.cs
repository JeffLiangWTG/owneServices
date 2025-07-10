using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCMessageTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			string messageType = row[EDIMessageSchema.EM_MessageType.Name].ToString();
			if (string.IsNullOrEmpty(messageType) && row[EDIMessageSchema.EM_MessageSubType.Name].ToString() == MessageTypeList.Codes.TWR)
			{
				messageType = MessageTypeList.Codes.TWR;
			}

			bool isTSW = Enterprise.Customs.NZ.TradeSingleWindow.MessageTypeList.IsTSWCode(factory, messageType);

			if (isTSW)
			{
				result = typeof(TSWMessage);
			}
			else if (messageType == NZCMessage.MessageTypes.OutwardReport.MessageType)
			{
				result = typeof(OutwardReportMessage);
			}
			else
			{
				result = typeof(NZCMessage);
			}

			return result;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
