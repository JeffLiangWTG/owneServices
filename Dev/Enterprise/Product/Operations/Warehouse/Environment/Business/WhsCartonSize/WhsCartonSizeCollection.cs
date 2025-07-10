using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsCartonSize)]
	public class WhsCartonSizeCollection : ActiveBusinessObjectCollection<WhsCartonSize>
	{
		public WhsCartonSizeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsCartonSizeCollection(WhsCartonGroup master)
			: base(master, typeof(WhsCartonGroupSizeLink), null, WhsCartonGroupSizeLinkSchema.WCV_WCG, WhsCartonGroupSizeLinkSchema.WCV_WCS)
		{
			CartonGroup = Argument.NotNull(master, nameof(master));
			AddFetchHints(master);
		}

		WhsCartonGroup CartonGroup { get; }

		#region OnAddIntoRelationshipCore

		protected override void OnAddIntoRelationshipCore(BusinessObject businessObject)
		{
			base.OnAddIntoRelationshipCore(businessObject);

			var cartonSize = (WhsCartonSize)businessObject;

			if (CartonGroup != null && CartonGroup.OptimizationMode == CartonizationOptimizationModes.Codes.MinimizeVolume)
			{
				var linkQuery = new ZQuery(WhsCartonGroupSizeLinkSchema.WCV_WCS, cartonSize.PK);
				linkQuery.AddToFilter(WhsCartonGroupSizeLinkSchema.WCV_WCG, CartonGroup.PK);

				var link = Factory.LoadTop1<WhsCartonGroupSizeLink>(linkQuery);
				if (link != null)
				{
					link.WCV_OptimizationCost = link.GetOptimizationCostBasedOnCartonSizeVolume();
				}
			}
		}

		#endregion

		#region AddFetchHints

		void AddFetchHints(WhsCartonGroup master)
		{
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(WhsCartonGroupSizeLink), WhsCartonGroupSizeLinkSchema.WCV_WCS);
			pivotSubQuery.AddToFilter(WhsCartonGroupSizeLinkSchema.WCV_WCG, master.PK);

			var cartonSizeQuery = new ZDBOnlyQuery(typeof(WhsCartonSize));
			cartonSizeQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			Factory.AddFetchHint(WhsCartonSizeSchema.Instance, cartonSizeQuery);
		}

		#endregion
	}
}
