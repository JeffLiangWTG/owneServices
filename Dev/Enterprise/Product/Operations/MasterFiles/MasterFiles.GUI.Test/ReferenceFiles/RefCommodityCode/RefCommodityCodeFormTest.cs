using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefCommodityCodeForm))]
	sealed class RefCommodityCodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefCommodityCodeForm(ComCode);
		}

		public void TestSetNMFCControlsVisibility()
		{
			RefCommodityCode commodity = Factory.New<RefCommodityCode>();
			RefCommodityCodeFormForTest form = null;
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				using (form = new RefCommodityCodeFormForTest(commodity))
				{
					form.Show();
					Assert(!form.NMFCCodeFindBox.Visible);
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
				using (form = new RefCommodityCodeFormForTest(commodity))
				{
					form.Show();
					Assert(form.NMFCCodeFindBox.Visible);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(country);
			}
		}

		[RequiresSTA]
		public void TestSetIsPersonalEffectsCheckBoxVisibility()
		{
			var commodity = Factory.New<RefCommodityCode>();
			RefCommodityCodeFormForTest form = null;

			using (ReferenceFilesDataRegistry.Instance.ShowPersonalEffects.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (form = new RefCommodityCodeFormForTest(commodity))
				{
					form.Show();
					Assert(!form.IsPersonalEffectsCheckBox.Visible);
				}
			}

			using (ReferenceFilesDataRegistry.Instance.ShowPersonalEffects.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (form = new RefCommodityCodeFormForTest(commodity))
				{
					form.Show();
					Assert(form.IsPersonalEffectsCheckBox.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestDegreesCaption()
		{
			RefCommodityCode commodity = Factory.New<RefCommodityCode>();
			using (RefCommodityCodeFormForTest form = new RefCommodityCodeFormForTest(commodity))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals((char)176 + " C", form.DegCLabel.Text);
				AssertEquals((char)176 + " C", form.DegCLabel2.Text);
			}
		}

		public void TestGridColumns()
		{
			var commodity = Factory.New<RefCommodityCode>();
			var commodityCodeMap = commodity.RefCommodityCodeMaps.AddNew();

			commodityCodeMap.LC_LocalCodeProvider = DELocalCommodityCodeProviderList.Codes.DE_DBH;
			commodityCodeMap.LC_RN_NKCountry = Core.Constants.CountryCodes.Germany;

			using (RefCommodityCodeFormForTest form = new RefCommodityCodeFormForTest(commodity))
			{
				form.Show();
				Application.DoEvents();
				form.LocalCodesTabPage.Show();
				AssertEquals(false, form.LocalMapsCountry.IsReadOnly);
				AssertEquals(false, form.LocalMapUsage.IsReadOnly);
				AssertEquals(false, form.LocalMapLocalCode.IsReadOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			using (RefCommodityCodeForm form = new RefCommodityCodeForm(ComCode))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		class RefCommodityCodeFormForTest : RefCommodityCodeForm
		{
			public RefCommodityCodeFormForTest(RefCommodityCode commodity)
				: base(commodity)
			{
			}

			public new ZTabPage MainTabPage
			{
				get { return base.MainTabPage; }
			}

			public new ZTabPage LocalCodesTabPage
			{
				get { return base.LocalCodesTabPage; }
			}

			public new ZCodeFindBox NMFCCodeFindBox
			{
				get { return base.NMFCCodeFindBox; }
			}

			public new ZCheckBox IsPersonalEffectsCheckBox
			{
				get { return base.IsPersonalEffectsCheckBox; }
			}

			public new ZLabel DegCLabel
			{
				get { return base.DegCLabel; }
			}

			public new ZLabel DegCLabel2
			{
				get { return base.DegCLabel2; }
			}

			public ZCodeFindBoxColumnStyleInfo LocalMapsCountry => base.RefCommodityCodeMapGrid.ColumnStyles[0] as ZCodeFindBoxColumnStyleInfo;
			public ZDropEditColumnStyleInfo LocalMapUsage => base.RefCommodityCodeMapGrid.ColumnStyles[1] as ZDropEditColumnStyleInfo;
			public ZTextBoxColumnStyleInfo LocalMapLocalCode => base.RefCommodityCodeMapGrid.ColumnStyles[2] as ZTextBoxColumnStyleInfo;
		}

		#region Implementation

		RefCommodityCode ComCode;

		protected override void SetUp()
		{
			base.SetUp();
			ComCode = Factory.New<RefCommodityCode>();
		}

		#endregion
	}
}
