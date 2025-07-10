using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class SalesRelationModelView : ZTreeModelView<IRelatableActivity>, IObsoleteValidation
	{
		public SalesRelationModelView(SalesRelationModel inner)
			: base(inner)
		{
		}

		protected internal SalesRelationModel SalesRelationModel
		{
			get { return (SalesRelationModel)base.InnerModel; }
		}

		#region Properties

		#region ShowCommunication

		public ZBool ShowCommunication
		{
			get { return showCommunicaton; }
			set
			{
				SetNonPersistentPropertyValue(ShowCommunicationInfo, ref showCommunicaton, value);
				RefreshView();
			}
		}
		ZBool showCommunicaton;

		public ZPropertyInfo ShowCommunicationInfo
		{
			get { return GetZPropertyInfo(nameof(ShowCommunication)); }
		}

		#endregion

		#endregion

		#region GetChildren

		protected override IEnumerable<ZNode<IRelatableActivity>> GetFiltedChildren(IEnumerable<ZNode<IRelatableActivity>> children)
		{
			var result = base.GetFiltedChildren(children);
			result = result.Where(ShouldIncludeChildInView);
			return result;
		}

		bool ShouldIncludeChildInView(ZNode<IRelatableActivity> node)
		{
			if (!ShowCommunication && node.BizObj.ActivityType == RelatableActivityTypeList.Codes.Communication && node.FindDescendantNodeInclusive(SalesRelationModel.Master) == null)
			{
				return false;
			}

			return true;
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpointForEdit
		{
			get { return Env.Security.SalesRelationsEdit; }
		}

		#endregion
	}
}
