using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.eTail.Business
{
	public class HVLVEDIMessageTypeDecider : TypeDecider, Integration.IHVLVEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return typeof(XmlEDIMessage);
		}
	}
}
