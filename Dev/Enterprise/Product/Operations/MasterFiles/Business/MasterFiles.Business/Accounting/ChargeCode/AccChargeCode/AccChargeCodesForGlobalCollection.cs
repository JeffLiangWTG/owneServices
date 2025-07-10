using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccChargeCode)]
	public class AccChargeCodesForGlobalCollection : ActiveBusinessObjectCollection<AccChargeCode>
	{
		readonly ZString code;

		public AccChargeCodesForGlobalCollection(BusinessObjectFactory factory) : base(factory)
		{
			code = ZString.Empty;
		}

		public AccChargeCodesForGlobalCollection(BusinessObjectFactory factory, ZString code) : base(factory)
		{
			if (code.IsEmpty)
			{
				throw new ArgumentException("Charge code is required");
			}

			this.code = code;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, GlbCompany.GetDemoCompany(Factory).PK);

			if (!code.IsEmpty)
			{
				query.AddToFilter(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, code);
			}
			else
			{
				// Force query to return 0 results
				query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, null);
			}

			return query;
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		public ZString Code
		{
			get
			{
				return code;
			}
		}
	}
}
