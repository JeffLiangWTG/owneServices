using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	partial class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedAsycudaItemSelectionDialogType => typeof(AsycudaItemSelectionDialog);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedBillLayoutType => typeof(SGBillLayouts);

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(23, columnsOrder.Length);
		}

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();
			header.AMA_ManifestType = SGManifestTypes.Codes.MGI;
			return header;
		}

		protected override void AssertGetPacksGridColumnAvailability(IReadOnlyDictionary<bool, string[]> result, AsycudaManifestHeader header)
		{
			AssertEquals(1, result.Count);
			AssertEquals("gstPaidDropEditColumnStyleInfo.ColumnName", "PackedItem+GSTPaid", result.Values.First()[0]);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] {
				"CycleDate",
				"CycleNumber",
				"ABL_MessageStatus",
				"ABL_BillStatus" }, columnInfos.Select(s => s.ColumnName));
		}

		protected override void SetUp()
		{
			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			accessEnableDisposable = accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			accessEnableDisposable.Dispose();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			var expectedColumns = new[]
			{   "PackedItem+API_FormattedTariff",
				"PackedItem+API_GoodsDescription",
				"PackedItem+API_CustomsQty",
				"PackedItem+API_CustomsUQ",
				"PackedItem+API_CustomsValue",
				"PackedItem+API_DutyAmount",
				"PackedItem+API_TaxAmount",
				"PackedItem+API_RN_NKGoodsOrigin",
				"PackedItem+API_MessageStatus",
				"PackedItem+API_PackStatus",
				"PackedItem+GoodsType",
				"PackedItem+GSTPaid"
			};
			AssertContainsExactElementsInExactOrder(expectedColumns, columnInfos.Select(s => s.ColumnName));
		}

		IDisposable accessEnableDisposable;
	}
}
