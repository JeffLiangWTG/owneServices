using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.GUI
{
	public class WiseRatesModuleTextFilter : ModuleTextFilter
	{
		public WiseRatesModuleTextFilter(FilterCategory category, ModuleFilterCollection parentCollection) : base(category, parentCollection)
		{
		}

		public WiseRatesModuleTextFilter(ZString description, SchemaStringColumn filterColumn) : base(description, filterColumn)
		{
		}

		public WiseRatesModuleTextFilter(ZString description, SchemaStringColumn filterColumn, IList list) : base(description, filterColumn, list)
		{
		}

		public WiseRatesModuleTextFilter(ZString description, GetTextQuery query) : base(description, query)
		{
		}

		public WiseRatesModuleTextFilter(ZString description) : base(description, (GetTextQuery)BlankQuery)
		{
		}

		public WiseRatesModuleTextFilter(ZString description, GetTextQuery queryDelegate, GetList listDelegate) : base(description, queryDelegate, listDelegate)
		{
		}

		public WiseRatesModuleTextFilter(ZString description, GetList listDelegate) : base(description, BlankQuery, listDelegate)
		{
		}

		public override bool HasComparisonOperator => false;

		static ZQuery BlankQuery(ZString value) => ZQuery.NoResultQuery;
	}
}
