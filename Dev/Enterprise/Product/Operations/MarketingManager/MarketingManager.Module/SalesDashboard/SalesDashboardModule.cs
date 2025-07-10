using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public class SalesDashboardModule : RefreshableGridModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.SalesDashboard; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}
		
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.SalesDashboard);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new SalesDashboardFilterControl((SalesDashboardActivityCollection)GridCollection, (SalesDashboardFilterBusinessObject)FilterBusinessObject, showCurrentLoginUserInfo: true);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new SalesDashboardActivityCollection(Factory, this);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SalesDashboardFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override ZQuery GetCollectionQuery() => GetDisplayResultsQuery();

		public override string[] GetTableNamesToMonitor() => new[] {
			OrgOpportunitySchema.Constants.TableName,
			GlbCompanyCampaignSchema.Constants.TableName,
			OrgSalesCallSchema.Constants.TableName,
			OrgColdCallRegisterSchema.Constants.TableName,
			RatingHeaderSchema.Constants.TableName,
			WorkProjectSchema.Constants.TableName,
		};

		#endregion

		#region Security Checkpoint

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.SalesDashboard; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SalesDashboard; }
		}

		#endregion
	}
}
