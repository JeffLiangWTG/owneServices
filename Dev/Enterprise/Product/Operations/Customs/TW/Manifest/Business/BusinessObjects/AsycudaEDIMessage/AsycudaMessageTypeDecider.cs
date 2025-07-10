using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.TW;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaMessageTypeDecider : TypeDecider, IEDIMessageTypeDecider
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
				case MessageTypeList.Codes.FCF:
				case MessageTypeList.Codes.FHR:
				case MessageTypeList.Codes.FHM:
					return typeof(AsycudaMessage);
				default:
					return typeof(TWMessage);
			}
		}
	}
}
