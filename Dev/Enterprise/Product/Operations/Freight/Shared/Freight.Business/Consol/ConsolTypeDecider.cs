using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ConsolTypeDecider : TypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var consolsInFactoryCache = factory.GetBizOsForPK((Guid)row[JobConsolSchema.PK.Name]).OfType<CommonConsol>();
			if (consolsInFactoryCache.IsCountEqualTo(1))
			{
				return consolsInFactoryCache.Single().GetType();
			}

			if ((bool)row[JobConsolSchema.JK_IsForwarding.Name] && (bool)row[JobConsolSchema.JK_IsCFS.Name])
			{
				return factory.GetFreightDomainContext() == FreightDomainContext.CFS
					? ObjectFactory.GetType<Integration.CFS.ICFSLoadListConsol>()
					: ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>();
			}

			if ((bool)row[JobConsolSchema.JK_IsForwarding.Name])
			{
				return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>();
			}

			if ((bool)row[JobConsolSchema.JK_IsCFS.Name])
			{
				return ObjectFactory.GetType<Integration.CFS.ICFSLoadListConsol>();
			}

			return GetTypeForNew();
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonConsol>();
		}

		public override Type GetTypeForBinding()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonConsol>();
		}
	}
}
