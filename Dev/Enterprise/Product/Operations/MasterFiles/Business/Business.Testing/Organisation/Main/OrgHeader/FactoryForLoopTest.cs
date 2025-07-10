using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FactoryForLoopTest : BusinessObjectFactory
	{
		protected override BusinessObject[] LoadCore(string tableOrViewName, Type bizOType, ZQuery effectiveFilter)
		{
			if (bizOType == typeof(OrgHeader))
			{
				bizOType = typeof(OrgHeaderForLoopTest);
			}
			return base.LoadCore(tableOrViewName, bizOType, effectiveFilter);
		}

		public override BusinessObject Load(Type bizOType, ZGuid pK)
		{
			if (bizOType == typeof(OrgHeader))
			{
				bizOType = typeof(OrgHeaderForLoopTest);
			}
			return base.Load(bizOType, pK);
		}

		protected override BusinessObject CreateBusinessObject(System.Data.DataRow row, Type bizOType, ITypeDeciderContext typeDeciderContext = null)
		{
			if (bizOType == typeof(OrgHeader))
			{
				bizOType = typeof(OrgHeaderForLoopTest);
			}
			return base.CreateBusinessObject(row, bizOType, typeDeciderContext);
		}

		public override BusinessObject LoadFromUniqueKey(Type bizOType, CargoWise.Schema.SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue)
		{
			if (bizOType == typeof(OrgHeader))
			{
				bizOType = typeof(OrgHeaderForLoopTest);
			}
			return base.LoadFromUniqueKey(bizOType, uniqueKeyColumn, uniqueKeyValue);
		}

		public override BusinessObject New(Type bizOType, Guid initialisingPk)
		{
			if (bizOType == typeof(OrgHeader))
			{
				bizOType = typeof(OrgHeaderForLoopTest);
			}
			return base.New(bizOType, initialisingPk);
		}

		public override BusinessObject[] Load(Type bizOType, ZQuery sQLFilter)
		{
			if (bizOType == typeof(OrgHeader))
			{
				bizOType = typeof(OrgHeaderForLoopTest);
			}
			return base.Load(bizOType, sQLFilter);
		}
	}
}
