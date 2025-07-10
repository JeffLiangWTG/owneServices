using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMMessageTypeDecider : TypeDecider, Integration.Customs.US.IUEMEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => typeof(UEMEDIMessage);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return typeof(UEMEDIMessage);
		}

		public override Type GetTypeForNew() => typeof(UEMEDIMessage);
	}
}
