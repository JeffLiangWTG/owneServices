using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.General;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GlbGroupModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public GlbGroupModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var items = base.GetNewActionMenuItems().ToList();
			var menuItem = ObjectFactory.Get<IADActionsMenuItemProvider>().GetModuleMenuItem(this, Factory);
			if (menuItem != null)
			{
				items.Add((MenuItem)menuItem);
			}

			return items.ToArray();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Groups; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbGroup; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbGroup);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbGroupFilterControl(GridCollection, (GlbGroupFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbGroupCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbGroupFilterBusinessObject();
		}

		protected override ZQuery GetDisplayResultsQuery()
		{
			var query = base.GetDisplayResultsQuery();
			query.AddToFilter(GlbGroupSchema.GG_Type, SQLComparisonOperator.NotEqual, GlbGroupTypeList.Codes.Organisation);
			return query;
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var groupFilterBusinessObject = (GlbGroupFilterBusinessObject)FilterBusinessObject;
			var securityRightFilters = groupFilterBusinessObject.ActiveModuleFilters.OfType<StaffSecurityModuleFilter>()
				.Where(filter => !filter.SecurityFilterContainer.LookupKey.IsEmpty)
				.ToList();

			if (securityRightFilters.Count > 0)
			{
				const int VeryLargeValueSoWeCanFilterLocally = int.MaxValue / 2;
				SearchManager.MaxRowsToLoad = VeryLargeValueSoWeCanFilterLocally;

				query.MaximumRows = null;
			}

			var result = factory.Load(type, query);

			if (securityRightFilters.Count > 0)
			{
				SearchManager.MaxRowsToLoad = MaxRowsToLoad;

				// We load all security rows related to the checkpoints being queried (and their parents etc).
				// We could further refine the filter to only the groups in the rest of the filter, but we don't know what those are yet.

				var deniedGroup = new HashSet<ZGuid>();
				var allGlbSecurity = new GlbSecurityCollection(Factory);
				var securityQuery = new ZQuery(GlbSecuritySchema.GU_SecurityRight, GlbStaffGroupHelper.KeysForSecurityRightFilters(securityRightFilters));
				allGlbSecurity.Load(securityQuery);
				GlbStaffGroupHelper.LoadCollectionWithCategory(result, securityRightFilters, allGlbSecurity, deniedGroup);
				result = result.Where(r => !deniedGroup.Contains(r.PK)).ToArray();
			}

			return PerformSearchResult.Success(factory, query, result, permitActiveCollectionUpdates: false);
		}

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.GlbGroupWorkflowDescriptorCode;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.GlbGroup };

		public OperationalActionSupporter OperationalActionSupporter => new GlbGroupOperationalActionSupporter();
	}
}
