using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.GUI.Testing;

[TestedType(typeof(ExitControlLayoutProvider))]
public class ExitControlLayoutProviderTest : EU.ExitControl.GUI.Testing.ExitControlLayoutProviderAbstractTest<ExitControlLayoutProvider>
{
	public void TestProviderType()
	{
		AssertType<ExitControlLayoutProvider>("Exit Control Layout Provider Type", EU.ExitControl.GUI.ExitControlLayoutProvider.GetLayoutProvider(CountryOrGroupingCode));
	}

	public override void TestReportAdditionalDocumentsGridLayout()
	{
		AssertType<ReportAdditionalDocumentUcc6GridColumnLayout>(Provider.ReportAdditionalDocumentsGridLayout);
	}

	public override void TestContainersOrEquipmentsGridLayout()
	{
		AssertType<ContainersOrEquipmentsUcc6GridColumnLayout>(Provider.ContainersOrEquipmentsGridLayout);
	}

	public override void TestSealsGridLayout()
	{
		AssertType<SealsUcc6GridColumnLayout>(Provider.SealsGridLayout);
	}

	public override void TestReportItemAdditionalDocumentsGridLayout()
	{
		AssertType<ReportItemAdditionalDocumentUcc6GridColumnLayout>(Provider.ReportItemAdditionalDocumentsGridLayout);
	}

	public override void TestConsignmentItemPackingDetailsGridLayout()
	{
		AssertType<ConsignmentItemPackingDetailsUcc6GridColumnLayout>(Provider.ConsignmentItemPackingDetailsGridLayout);
	}

	public override void TestConsignmentItemsGridLayout()
	{
		AssertType<ConsignmentItemsUcc6GridColumnLayout>(Provider.ConsignmentItemsGridLayout);
	}

	public override void TestAuthorizationGridColumnLayout()
	{
		AssertType<AuthorizationGridColumnLayout>(Provider.AuthorizationGridColumnLayout);
	}

	protected override IEnumerable<Type> ExpectedAdditionalDeclarationTabPageTypes => Enumerable.Empty<Type>();

	protected override IEnumerable<ZString> ExpectedRemovableDeclarationTabPageNames => Enumerable.Empty<ZString>();

	protected override Type ExpectedHeaderDetailsPanelLayoutType => typeof(HeaderDetailsLayout);

	protected override IEnumerable<Type> ExpectedAdditionalConsignmentTabPageTypes => Enumerable.Empty<Type>();

	protected override IEnumerable<ZString> ExpectedRemovableConsignmentTabPageNames => Enumerable.Empty<ZString>();

	protected override Type ExpectedConsignmentsGridUserControlType => typeof(ConsignmentsGridUserControl);

	protected override Type ExpectedConsignmentItemPanelLayoutWithGridType => typeof(ConsignmentItemUcc6LayoutWithGrid);

	protected override IEnumerable<Type> ExpectedAdditionalReportTabPageTypes
	{
		get
		{
			yield return typeof(ReportAdditionalDocumentsTabPage);
			yield return typeof(ReportAuthorizationsTabPage);
		}
	}

	protected override IEnumerable<ZString> ExpectedRemovableReportTabPageNames => Enumerable.Empty<ZString>();

	protected override Type ExpectedReportsGridUserControlType => typeof(ReportsUcc6GridUserControl);

	protected override Type ExpectedReportItemsUserControlType => typeof(ReportItemsUserControl);

	protected override Type ExpectedReportsMessagesUserControlType => typeof(MessagesTabUserControl);

	protected override Type ExpectedContainersOrEquipmentsAndSealsUserControlType => typeof(ContainersOrEquipmentsAndSealsUserControl);

	protected override Type ExpectedDetailsReportsGridUserControlType => typeof(HeaderExitReportStatusUcc6GridUserControl);

	protected override bool ExpectedAuthorizationIsActive => true;

	protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Poland;
}
