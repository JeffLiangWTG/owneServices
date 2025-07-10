using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class PersonAssociationsOrgGroupWrapper : PersonAssociationsTreeBizObjWrapper
	{
		public PersonAssociationsOrgGroupWrapper(PersonAssociationsTreeModel treeModel, OrgHeader organization,
			IEnumerable<PersonAssociationsTreeBizObjWrapper> children)
			: base(treeModel, children)
		{
			this.organization = organization;
		}

		public PersonAssociationsOrgGroupWrapper(PersonAssociationsTreeModel treeModel, OrgHeader organization)
			: base(treeModel)
		{
			this.organization = organization;
		}

		public readonly OrgHeader organization;

		#region Properties

		public OrgHeader Organization => organization;

		#endregion

		#region Overrides

		#region Active

		public override ZBool Active => organization.OH_IsActive;

		#endregion

		#region City

		public override ZString City => ZString.Empty;

		#endregion

		#region Description

		public override ZString Description => organization.OH_FullName;

		#endregion

		#region Grouping

		public override ZString Grouping => Enterprise.MasterFiles.Business.Res.GetString("PersonAssociationsOrgGroupWrapper|OrgGroup", "Management Group");

		#endregion

		#region State

		public override ZString State => ZString.Empty;

		#endregion

		#region CreatedTime

		public override ZString CreatedTime => ZString.Empty;

		#endregion

		#region Country

		public override ZString Country => organization.MainAddress.CountryName;

		#endregion

		#region Email

		public override ZString Email => ZString.Empty;

		#endregion

		#endregion
	}
}
