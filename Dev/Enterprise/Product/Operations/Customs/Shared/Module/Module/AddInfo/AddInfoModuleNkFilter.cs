using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class AddInfoModuleNkFilter : ModuleNkFilter
	{
		public AddInfoModuleNkFilter(ZString description, ModuleIdentifier id, IBusinessObjectCollection list, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty)
			: base(description, (v) => AddInfoExactNkQuery(v, addInfoSchemaColumn, addInfoProperty), id, list)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new[]
								 {
									 String.Empty,
									 ComparisonConstants.Exact,
								 };
			}
		}

		static ZQuery AddInfoExactNkQuery(ZString value, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty)
		{
			return AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, value, addInfoSchemaColumn, addInfoProperty);
		}
	}
}
