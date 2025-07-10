using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class ContactsModuleFilter : ModuleGuidForeignCollectionFilter
	{
		protected ContactsModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ContactsModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ModuleIDs.OrgContacts, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public ContactsModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, ModuleIDs.OrgContacts, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}

		public new ModuleGuidFilterValidation Validation => (ContactsModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new ContactsModuleFilterValidation(this);

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};

		internal SchemaGuidColumn ForeignKeyColumn => ForeignKey;
	}

	#region class Validation

	public class ContactsModuleFilterValidation : ModuleGuidFilterValidation
	{
		public ContactsModuleFilterValidation(ContactsModuleFilter parent)
			: base(parent)
		{
		}

		protected override void CheckSelectedFiltersDescription()
		{
			base.CheckSelectedFiltersDescription();
			if (!Env.Security.OrgContactView.IsAllowed)
			{
				GetParent().SelectedFiltersDescriptionInfo.AddError(Env.Security.OrgContactView.ErrorMessageForNotAllowed);
			}
		}
	}

	#endregion class Validation
}
