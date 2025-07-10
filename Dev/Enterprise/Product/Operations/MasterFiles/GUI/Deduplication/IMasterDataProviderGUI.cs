using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public interface IMasterDataProviderGUI
	{
		Form ShowDeduplicationResultsViewerForm(DuplicationEventArgs e);
		void CreateOrgMenu(ZForm parentForm, OrgHeader organisation);
		ZUserControl GetNewDeduplicationOrganizationsUserControl();

		ZUserControl GetNewDeduplicationPersonUserControl();

		ZUserControl SetDeduplicationPopupDataSource(object master, IEnumerable<ScoringResult> results, Action<ZGuid> openDetailsFormAction);

		void OpenPersonMergeSummaryForm(GlbPerson retainedPerson, List<GlbPerson> dissolvedPersons);

		void ClearAdvancedFilterPanelCache();

		void PersistenceFiltersValue(BusinessObjectFactory factory);
	}
}
