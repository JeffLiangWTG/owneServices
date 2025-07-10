using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public class MasterDataProviderGUI : IMasterDataProviderGUI
	{
		public void CreateOrgMenu(ZForm parentForm, OrgHeader organisation)
		{
			new DeduplicationInitializer().CreateOrgMenu(parentForm, organisation);
		}

		public ZUserControl GetNewDeduplicationOrganizationsUserControl()
		{
			return new DeduplicationOrganizationsUserControl();
		}

		public ZUserControl GetNewDeduplicationPersonUserControl()
		{
			return new DeduplicationPersonsUserControl();
		}

		public ZUserControl SetDeduplicationPopupDataSource(object master, IEnumerable<ScoringResult> results, Action<ZGuid> openDetailsFormAction)
		{
			if (master is IDeduplicatable iMaster)
			{
				var dataSource = new DedupPopupBizoDataSource(iMaster, results, openDetailsFormAction);

				if (dataSource.ShouldCreatePopup)
				{
					if (dataSource.MasterIsOrgHeader)
					{
						var dedupPopupOrgHeadercontrol = new DedupPopupOrgHeaderControl();
						dedupPopupOrgHeadercontrol.SetDataBinding(dataSource, string.Empty);
						return dedupPopupOrgHeadercontrol;
					}
					else
					{
						var dedupePopupPersonControl = new DedupePopupPersonControl();
						dedupePopupPersonControl.SetDataBinding(dataSource, string.Empty);
						return dedupePopupPersonControl;
					}
				}
			}

			return null;
		}

		public Form ShowDeduplicationResultsViewerForm(DuplicationEventArgs e)
		{
			return new DeduplicationResultsViewerForm(e.Master as BusinessObject, e.TargetObjects as IEnumerable<object>, e.Results, e.ResultsModels, e.SelectedMasterPK);
		}

		public void OpenPersonMergeSummaryForm(GlbPerson retainedPerson, List<GlbPerson> dissolvedPersons)
		{
			ZFormModaliser.ShowDialogAndDispose(new PersonMergeSummaryForm(retainedPerson, dissolvedPersons));
		}

		public void ClearAdvancedFilterPanelCache()
		{
			DedupePanelAdvancedFilterHelper.ClearCache();
		}

		public void PersistenceFiltersValue(BusinessObjectFactory factory)
		{
			DedupePanelAdvancedFilterHelper.PersistencePersonFiltersValue(factory);
		}
	}
}
