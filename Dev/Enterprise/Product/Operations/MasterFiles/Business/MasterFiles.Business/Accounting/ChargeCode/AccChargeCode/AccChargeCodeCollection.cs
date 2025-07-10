using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccChargeCode)]
	public class AccChargeCodeCollection : BusinessObjectCollection<AccChargeCode>, IAccChargeCodeCollection
	{
		public AccChargeCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccChargeCodeCollection(BusinessObjectFactory factory, GlbCompany company) : this(factory)
		{
			if (company != null)
			{
				companyPK = company.PK.ToGuid();
			}
		}

		public AccChargeCodeCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : this(factory, sQLFilter, Env.CurrentCompany.PK)
		{
		}

		public AccChargeCodeCollection(BusinessObjectFactory factory, ZQuery sQLFilter, Guid companyPK) : base(factory, sQLFilter)
		{
			this.companyPK = companyPK;
		}

		public AccChargeCodeCollection(BusinessObjectFactory factory, ZQuery sQLFilter, Guid companyPK, bool includeGlobal) : base(factory, sQLFilter)
		{
			this.companyPK = companyPK;
			this.includeGlobal = includeGlobal;
		}

		readonly Guid companyPK = Env.CurrentCompany.PK;

		readonly ZBool includeGlobal;

		internal bool ContainsCode(string aC_Code)
		{
			var filter1 = new ZQuery(AccChargeCodeSchema.AC_Code, aC_Code);
			var filter2 = CreateRelationshipFilter();
			var chargeCode = Factory.LoadTop1(typeof(AccChargeCode), new ZQuery(filter1, JoinCondition.And, filter2));
			return chargeCode != null;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (companyPK == Guid.Empty)
			{
				return new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, null);
			}
			else
			{
				var filter = new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, companyPK);
				if (includeGlobal)
				{
					filter.AddToFilter(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, null), JoinCondition.Or);
				}

				return filter;
			}
		}
	}
}
