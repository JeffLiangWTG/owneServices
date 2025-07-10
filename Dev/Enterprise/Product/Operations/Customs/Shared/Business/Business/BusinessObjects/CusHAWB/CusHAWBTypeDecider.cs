using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business
{
	public class CusHAWBTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var mAWB = factory.Load<CusMAWB>(new ZGuid(row[CusHAWB.Schema.CS_CM]));
			var applicationCode = mAWB != null ? mAWB.CM_ApplicationCode.ToString() : row[CusHAWB.Schema.CS_ApplicationCode].ToString();

			switch (applicationCode)
			{
				case Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff:
				case Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWB>();
				case ApplicationCodeList.Codes.GbCcsuk: //"CUK"
					return ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusHAWB>();
				case Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages:
				case "":
					{
						var decider = TypeDecider.GetTypeDeciderFromType(ObjectFactory.GetType<Integration.Customs.AU.ICusHAWBBase>());
						return decider.GetTypeForLoad(row, factory);
					}
				default:
					return typeof(CusHAWB);
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
