using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business
{
	public class CusPartShipTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var mAWB = factory.Load<CusMAWB>(new ZGuid(row[CusPartShip.Schema.CG_CM_LinkToPartMaster]));
			var hAWB = factory.Load<CusHAWB>(new ZGuid(row[CusPartShip.Schema.CG_CS]));
			string applicationCode = "";
			if (hAWB != null)
			{
				if (hAWB.MAWB != null)
				{
					applicationCode = hAWB.MAWB.CM_ApplicationCode;
				}
			}
			else if (mAWB != null)
			{
				applicationCode = mAWB.CM_ApplicationCode;
			}
			switch (applicationCode)
			{
				case ApplicationCodeList.Codes.GbCcsuk:
					return hAWB != null ? ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ISplitHouse>() : mAWB != null ? ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ISplitBasic>() : null;
				case ApplicationCodeList.Codes.AUAirCargo:
				case ApplicationCodeList.Codes.AUCMR:
				case ApplicationCodeList.Codes.AUExDoc:
				case ApplicationCodeList.Codes.AUSeaCargo:
				default:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusPartShip>();
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
