using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	[ImmutableObject(true)]
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.US.USAMS.IEDIMessageTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return typeof(AMSEDIMessage);
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
