using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	class CusInBondEventTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var type = (string)row[CusInBondEventSchema.BN_Type.Name];
			switch (type)
			{
				case CusInBondEventTypes.Codes.Transshipment:
					return ObjectFactory.GetType<Integration.Customs.EU.NCTS.IEnRouteTransshipment>();
				case CusInBondEventTypes.Codes.Seal:
					return ObjectFactory.GetType<Integration.Customs.EU.NCTS.IEnRouteSeal>();
				case CusInBondEventTypes.Codes.Incident:
					return ObjectFactory.GetType<Integration.Customs.EU.NCTS.IEnRouteIncident>();
				default:
					return typeof(CusInBondEvent);
			}
		}

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;
	}
}
