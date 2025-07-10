using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class PersonAssociationsTreeModelView : ZTreeModelView<PersonAssociationsTreeBizObjWrapper>
	{
		public PersonAssociationsTreeModelView(PersonAssociationsTreeModel treeModel)
			: base(treeModel)
		{
		}

		#region Properties

		ZBool showInactive;

		public ZBool ShowInactive
		{
			get { return showInactive; }
			set
			{
				SetNonPersistentPropertyValue(ShowInactiveInfo, ref showInactive, value);
				BuildTree();
			}
		}

		public ZPropertyInfo ShowInactiveInfo
		{
			get { return GetZPropertyInfo(nameof(ShowInactive)); }
		}

		#endregion

		#region Build Tree

		public void BuildTree(bool needRefreshCollection = true)
		{
			InnerModel.BuildTree(showInactive, needRefreshCollection);
			RefreshView();
		}

		#endregion

		#region Inner Model

		protected new PersonAssociationsTreeModel InnerModel
		{
			get { return (PersonAssociationsTreeModel)base.InnerModel; }
		}

		#endregion
	}
}
