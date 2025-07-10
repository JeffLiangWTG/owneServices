using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class PersonAccreditationAttemptsFilter : ModuleGuidForeignCollectionFilter
	{
		protected PersonAccreditationAttemptsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public PersonAccreditationAttemptsFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ModuleIDs.GlbAccreditationAttempt, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public PersonAccreditationAttemptsFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, ModuleIDs.GlbAccreditationAttempt, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}
	}
}
