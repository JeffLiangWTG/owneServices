using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbStaffManagementTreeBizObjWrapperBase : NonPersistentBusinessObject
	{
		protected GlbStaffManagementTreeBizObjWrapperBase(GlbStaffManagementTreeModel treeModel)
			: base(treeModel.Factory)
		{
			TreeModel = treeModel;
		}

		public GlbStaffManagementTreeModel TreeModel;

		#region Properties

		protected internal ZString ViewDeniedMessage
		{
			get { return Res.GetString("a43c6a89-f675-4b42-be96-f326889350df", "** View Denied **"); }
		}

		public virtual ZString Role { get; }
		public abstract ZString JobTitle { get; }
		public abstract ZString EffectiveDate { get; }
		public abstract ZString Branch { get; }

		#endregion

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "The arrays returned are not concerned with modifications.")]
		public abstract GlbStaffManagementTreeBizObjWrapperBase[] Children { get; }
	}
}
