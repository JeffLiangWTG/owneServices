using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(ApplicationGUIProvider))]
sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
{
	protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

	protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => new[] { typeof(HeaderPartiesUserControl) };

	protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl), typeof(PreviousDocumentsUserControl) };

	protected override Type ExpectedBillLayoutType => typeof(BillLayout);

	protected override Type ExpectedBillPartiesLayoutType => typeof(NOBillPartiesLayout);

	protected override int MaxColumnsOfManifestLayout => 3;

	protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
	{
		var common = CommonManifestControlBag.Instance;
		var groups = new Dictionary<string, ControlReference[]>();
		groups.Add("Sea Vessel", new[] { common.VesselCodeFindBox, common.LloydsNumberTextBox, common.VoyageFlightTextBox, common.RadioCallSignTextBox, common.ConveyanceCountryCodeFindBox });
		groups.Add("Road Transport", new[] { common.VehicleRegistrationTextBox, common.Trailer1RegNoTextBox, common.Trailer2RegNoTextBox, common.Trailer1RegCountryCodeFindBox, common.Trailer2RegCountryCodeFindBox });
		return groups;
	}

	protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
	{
		AssertSequencesEqual(new string[]
		{
			"APA_PackQty"
		}, columnsOrder);
	}

	public void TestGetManifestLayoutCore()
	{
		AssertType<NOManifestLayout>(CreateNewGuiProvider().GetManifestLayout());
	}

	public void TestMainTabPageGroupBoxNameCore()
	{
		AssertEquals("Transport", CreateNewGuiProvider().MainTabPageGroupBoxName.Caption);
	}

	public void TestGetBillsGridColumnsOrder()
	{
		var provider = CreateNewGuiProvider();
		var expected = new[]
		{
		AsycudaBill.Schema.ABL_SequenceNumber,
		AsycudaBill.Schema.ABL_BillNumber,
		AsycudaBill.Schema.ABL_BolType,
		AsycudaBill.Schema.CustomsLevel,
		AsycudaBill.Schema.ABL_RL_NKOrigin,
		AsycudaBill.Schema.ABL_RL_NKFinalDestination,
		AsycudaBill.Schema.ABL_ConsigneeName,
		AsycudaBill.Schema.ABL_ConsigneeStreet1,
		AsycudaBill.Schema.ABL_ConsigneePostcode,
		AsycudaBill.Schema.ABL_ConsigneeCity,
		AsycudaBill.Schema.ABL_GoodsDescription,
		AsycudaBill.Schema.ABL_ManifestQty,
		AsycudaBill.Schema.ABL_ManifestUQ,
		AsycudaBill.Schema.ABL_GrossWeight,
		AsycudaBill.Schema.ABL_GrossWeightUQ,
		AsycudaBill.Schema.ABL_Volume,
		AsycudaBill.Schema.ABL_VolumeUQ,
		AsycudaBill.Schema.ABL_MarksAndNumbers,
		AsycudaBill.Schema.ABL_Remarks,
		AsycudaBill.Schema.ABL_UCRNumber,
		AsycudaBill.Schema.CustomsJobNumber,
		};

		AssertContainsExactElementsInAnyOrder(expected, provider.GetBillsGridColumnsOrder());
	}

	protected override void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsVisiblility, ASYCUDA.Business.AsycudaManifestHeader header)
	{
		AssertEquals(4, columnsVisiblility.Count);
	}

	public void TestBillsGridExtraColumns()
	{
		var manifest = CreateNewManifest();
		manifest.Bills.AddNew();

		using var form = new ManifestForm(manifest);
		form.Show();
		var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
		var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
		var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
		mainTabControl.SelectedTab = billsAndPacksTabPage;
		var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

		CombineAssertions(() =>
		{
			Assert("CustomsLevel IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.CustomsLevel).IsVisible);
		});
	}

	protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
	{
		AssertContainsExactElementsInExactOrder(new[] { "CustomsLevel" }, columnInfos.Select(s => s.ColumnName));
	}
}
