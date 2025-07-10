using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class SalesRelationModel : SimpleTreeModel<IRelatableActivity>, ISalesRelationModel
	{
		public SalesRelationModel(IRelatableActivity master)
			: base(master)
		{
		}

		#region Properties

		#region RecentActivityDate

		public ZDateTime RecentActivityDate
		{
			get
			{
				var data = SalesRelationActivityData;
				if (data == null)
				{
					return Master.SystemLastEditTimeUtc.ToLocalBranchTime();
				}
				else
				{
					return data.VSR_RecentActivityDate.ToLocalBranchTime();
				}
			}
		}

		#endregion

		#region HasSalesRelation

		public ZBool HasSalesRelation
		{
			get { return SalesRelationActivityData != null; }
		}

		#endregion

		#endregion

		#region Master Node

		protected override ZNode<IRelatableActivity> CreateMasterNode()
		{
			return CreateNewNode(Master);
		}

		#endregion

		#region New Node

		protected override ZNode<IRelatableActivity> CreateNewNodeCore(ZTreeModel<IRelatableActivity> treeModel, IRelatableActivity bizOj)
		{
			return new SalesRelationNode((SalesRelationModel)treeModel, bizOj);
		}

		#endregion

		#region SalesRelationActivityData

		ViewSalesRelationActivityData SalesRelationActivityData
		{
			get { return Factory.Load<ViewSalesRelationActivityData>(Master.PK); }
		}

		#endregion
	}
}
