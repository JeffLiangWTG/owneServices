using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.TW;

namespace Enterprise.Customs.TW.Business
{
	public class TWMessageTypeDecider : TypeDecider, IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var eM_MessageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			switch (eM_MessageType)
			{
				case MessageTypeList.Codes.TPC:
					return typeof(N5110EDIMessage);
				case MessageTypeList.Codes.TAD:
					return typeof(N5111EDIMessage);
				case MessageTypeList.Codes.IRM:
					return typeof(N5116EDIMessage);
				case MessageTypeList.Codes.ERM:
					return typeof(N5204EDIMessage);
				default:
					return typeof(TWMessage);
			}
		}
	}
}
