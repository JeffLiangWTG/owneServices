using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.ZA;

namespace Enterprise.Customs.ZA.Business
{
	public class EDIMessageTypeDecider : TypeDecider, IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var em_MessageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			switch (em_MessageType)
			{
				case ZAEDIMessageTypeList.Codes.ExternalWarehouse:
					return typeof(EWHMessage);
				default:
					return typeof(EDIMessage);
			}
		}
	}
}
