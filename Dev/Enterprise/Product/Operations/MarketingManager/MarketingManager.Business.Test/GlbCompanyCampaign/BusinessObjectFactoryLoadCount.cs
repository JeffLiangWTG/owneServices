using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	public sealed class BusinessObjectFactoryLoadCount : BusinessObjectFactory
	{
		public override BusinessObject Load(Type bizOType, ZGuid pk)
		{
			loadCount++;
			return base.Load(bizOType, pk);
		}

		public int LoadCount => loadCount;
		int loadCount;
	}
}
