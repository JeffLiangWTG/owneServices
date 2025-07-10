using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class PersonAssociationsTreeNode : ZNode<PersonAssociationsTreeBizObjWrapper>
	{
		public PersonAssociationsTreeNode(PersonAssociationsTreeModel treeModel, PersonAssociationsTreeBizObjWrapper bizObj)
			: base(treeModel, bizObj)
		{
		}

		public new PersonAssociationsTreeModel TreeModel => (PersonAssociationsTreeModel)base.TreeModel;

		#region Properties

		#region Active

		public bool Active => BizObjForBinding.Active;

		#endregion

		#region City

		public ZString City => BizObjForBinding.City;

		#endregion

		#region Description

		public ZString Description => BizObjForBinding.Description;

		#endregion

		#region Grouping

		public ZString Grouping => BizObjForBinding.Grouping;

		#endregion

		#region State

		public ZString State => BizObjForBinding.State;

		#endregion

		#region CreatedTime

		public ZString CreatedTime => BizObjForBinding.CreatedTime;

		#endregion

		#region Country

		public ZString Country => BizObjForBinding.Country;

		#endregion

		#region Email

		public ZString Email => BizObjForBinding.Email;

		#endregion

		#region Primary Working Address

		public bool IsPrimary
		{
			get => BizObjForBinding.IsPrimary;
			set => BizObjForBinding.IsPrimary = value;
		}

		public ZBool IsPrimary_Visible => BizObjForBinding.IsPrimary_Visible;

		public ZBool IsPrimary_Editable => BizObjForBinding.IsPrimary_Editable;

		public ZString WorkingAddressUNLOCO => BizObjForBinding.WorkingAddressUNLOCO;

		#endregion

		#endregion

		#region Overrides

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(PersonAssociationsTreeBizObjWrapper previousParent,
			PersonAssociationsTreeBizObjWrapper newParent, bool checkValid)
		{
			throw new NotImplementedException("Not required as does not support re-ordering");
		}

		protected override IEnumerable<PersonAssociationsTreeBizObjWrapper> LoadChildBizObjs()
		{
			return BizObjForBinding.Children ?? Enumerable.Empty<PersonAssociationsTreeBizObjWrapper>();
		}

		protected override PersonAssociationsTreeBizObjWrapper LoadParentBizObj()
		{
			return null;
		}

		#endregion
	}
}
