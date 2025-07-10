using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListTypeDecider : TypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var clhInFactoryCache = factory.GetBizOsForPK((Guid)row[ContainerLoadListHeaderSchema.PK.Name]).OfType<CommonContainerLoadList>();
			if (clhInFactoryCache.IsCountEqualTo(1))
			{
				return clhInFactoryCache.Single().GetType();
			}

			var loadModeName = row[ContainerLoadListHeaderSchema.CLH_LoadMode.Name];

			switch (loadModeName)
			{  
				case CommonContainerLoadListLoadModeList.Codes.CY:
					return ObjectFactory.GetType<ICYContainerLoadList>();
				case CommonContainerLoadListLoadModeList.Codes.CFS:
					return ObjectFactory.GetType<ICFSContainerLoadList>();
				default:
					return GetTypeForNew();
			}
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainerLoadList>();
		}

		public override Type GetTypeForBinding()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainerLoadList>();
		}
	}
}
