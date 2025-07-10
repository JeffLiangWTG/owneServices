using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.ExitControl.GUI
{
	public class ExitControlLayoutProvider : IExitControlLayoutProvider
	{
		IEnumerable<ITabPage> IExitControlLayoutProvider.AdditionalDeclarationTabPages => Enumerable.Empty<ITabPage>();

		IEnumerable<ZString> IExitControlLayoutProvider.RemovableDeclarationTabPageNames => Enumerable.Empty<ZString>();

		IPanelLayoutProvider IExitControlLayoutProvider.HeaderDetailsPanelLayout => new HeaderDetailsLayout();

		IDetailsReportsGridUserControl IExitControlLayoutProvider.CreateDetailsReportGridUserControl() => new HeaderExitReportStatusGridUserControl();

		IEnumerable<ITabPage> IExitControlLayoutProvider.AdditionalConsignmentTabPages => Enumerable.Empty<ITabPage>();

		IEnumerable<ZString> IExitControlLayoutProvider.RemovableConsignmentTabPageNames => Enumerable.Empty<ZString>();

		IConsignmentsGridUserControl IExitControlLayoutProvider.ConsignmentsGridUserControl => new ConsignmentsGridUserControl();

		IPanelLayoutWithGridProvider IExitControlLayoutProvider.ConsignmentItemPanelLayoutWithGrid => new ConsignmentItemLayoutWithGrid();

		IEnumerable<ITabPageWithVisibility<CusExitReport>> IExitControlLayoutProvider.AdditionalReportTabPages => Enumerable.Empty<ITabPageWithVisibility<CusExitReport>>();

		IEnumerable<ZString> IExitControlLayoutProvider.RemovableReportTabPageNames => Enumerable.Empty<ZString>();

		IReportsGridUserControl IExitControlLayoutProvider.CreateReportsGridUserControl() => new ReportsGridUserControl();

		IReportItemsUserControl IExitControlLayoutProvider.CreateReportItemsUserControl() => new ReportItemsUserControl();

		BaseMessagesTabUserControl IExitControlLayoutProvider.CreateReportsMessagesUserControl() => new MessagesTabUserControl();

		Type IExitControlLayoutProvider.ContainersOrEquipmentsAndSealsUserControlType => typeof(ContainersOrEquipmentsAndSealsUserControl);

		IGridColumnLayoutProvider IExitControlLayoutProvider.ReportAdditionalDocumentsGridLayout => new ReportAdditionalDocumentGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ContainersOrEquipmentsGridLayout => new ContainersOrEquipmentsGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.SealsGridLayout => new SealsGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ReportItemAdditionalDocumentsGridLayout => new ReportItemAdditionalDocumentGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ConsignmentItemPackingDetailsGridLayout => new ConsignmentItemPackingDetailsGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ConsignmentItemsGridLayout => new ConsignmentItemsGridColumnLayout();

		public IGridColumnLayoutProvider AuthorizationGridColumnLayout => new AuthorizationGridColumnLayout();

		public bool AuthorizationIsActive => false;
	}
}
