using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public abstract class PersonAssociationsTreeBizObjWrapper : NonPersistentBusinessObject
	{
		protected PersonAssociationsTreeBizObjWrapper(PersonAssociationsTreeModel treeModel,
			IEnumerable<PersonAssociationsTreeBizObjWrapper> children)
			: base(treeModel.Factory)
		{
			this.TreeModel = treeModel;
			this.Children = children;
		}

		protected PersonAssociationsTreeBizObjWrapper(PersonAssociationsTreeModel treeModel)
			: base(treeModel.Factory)
		{
			this.TreeModel = treeModel;
		}

		public readonly PersonAssociationsTreeModel TreeModel;
		public readonly IEnumerable<PersonAssociationsTreeBizObjWrapper> Children;

		#region Properties

		protected internal ZString ViewDeniedMessage
		{
			get { return Res.GetString("6BC4E4C4-14F1-47A6-8A50-1F3F6CB6E3BC", "** View Denied **"); }
		}

		#region Active

		public virtual ZBool Active
		{
			get { return false; }
			set { }
		}

		#endregion

		#region City

		public abstract ZString City { get; }

		#endregion

		#region Description

		public abstract ZString Description { get; }

		#endregion

		#region Grouping

		public abstract ZString Grouping { get; }

		#endregion

		#region State

		public abstract ZString State { get; }

		#endregion

		#region CreatedTime

		public abstract ZString CreatedTime { get; }

		#endregion

		#region Country

		public abstract ZString Country { get; }

		#endregion

		#region Primary Working Address

		public virtual ZBool IsPrimary
		{
			get { return false; }
			set { }
		}

		public virtual ZBool IsPrimary_Visible => false;

		public ZBool IsPrimary_Editable => Env.Security.PersonIntelligencePrimaryWorkplace.IsAllowed;

		public virtual ZString WorkingAddressUNLOCO => ZString.Empty;

		#endregion

		#region Unknown

		public ZString Unknown =>
			Enterprise.MasterFiles.Business.Res.GetString("PersonAssociationsTreeBizObjWrapper|Unknown", "Unknown");

		#endregion

		#region Email

		public abstract ZString Email { get; }

		#endregion

		#endregion
	}
}
