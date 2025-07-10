using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ImmutableObject(true)]
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.US.IEDIMessageTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = row[EDIMessageSchema.Constants.EM_ApplicationCode].ToString().Trim();

			switch (applicationCode)
			{
				case EDIMessage.ApplicationCodes.USCustomsExport:
					return typeof(AESTIREDIMessage);
				case EDIMessage.ApplicationCodes.USeBond:
					return typeof(EBondEDIMessage);
				case CBPEDIInterchange.ApplicationCodes.AMS:
					{
						var amsTypeDecider = (TypeDecider)ObjectFactory.Get<Integration.Customs.US.USAMS.IEDIMessageTypeDecider>();
						return amsTypeDecider.GetTypeForLoad(row, factory);
					}
				default:
					return new MQEDIMessageTypeDecider().GetTypeForLoad(row, factory);
			}
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}
