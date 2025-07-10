using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	internal class TariffGridFindBoxTest : TestCaseWithFactory
	{
		[TestDate(2010, 02, 24)]
		public void TestITariffFindBoxPopupSupport()
		{
			using (var findBox = new TariffGridFindBox())
			{
				findBox.SetDataBinding(new TariffObjectForTesting(), "TS_TariffCode");
				AssertEquals("BindToTariffPropertyInfo", null, findBox.BindToTariffPropertyInfo);
				TariffPropertyInfoTest.AssertTariffInfo(findBox.TariffInfo, TariffType.Export, ZDateTime.Today, string.Empty);
				AssertEquals("BoundItem", findBox.CurrentItemInternal, findBox.BoundItem);
			}
		}

		public void TestGetNewPopupForm()
		{
			using (var findBox = new TariffGridFindBox())
			{
				AssertTypeOfPopupShown<ZCodeFindBoxPopup>();
				ErrorReporter.Clear();
				Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
				AssertTypeOfPopupShown<TariffFindBoxPopup>();
				void AssertTypeOfPopupShown<T>()
				{
					using (var popup = findBox.GetNewPopupFormInternal())
					{
						AssertType<T>(popup);
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}
	}
}
