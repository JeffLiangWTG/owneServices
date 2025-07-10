using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TariffFindBoxTestBaseOnly : TestCaseWithFactory
	{
		[TestDate(2010, 02, 24)]
		public void TestITariffFindBoxPopupSupport()
		{
			using (var findBox = new TariffFindBox())
			{
				findBox.SetDataBinding(new TariffObjectForTesting(), "TS_TariffCode");
				AssertEquals("BindToTariffPropertyInfo", null, findBox.BindToTariffPropertyInfo);
				TariffPropertyInfoTest.AssertTariffInfo(findBox.TariffInfo, TariffType.Export, ZDateTime.Today, string.Empty);
				AssertEquals("BoundItem", findBox.CurrentItemInternal, findBox.BoundItem);
			}
		}

		public void TestGetNewPopupForm()
		{
			using (var findBox = new TariffFindBox())
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

		public void TestGetBindingMembersForCompileTimeCheck()
		{
			using (var findBox = new TariffFindBox())
			{
				findBox.DataSourceType = typeof(TariffObjectForTesting);
				findBox.BindToTariffPropertyInfo = "TS_TariffCodeTariffInfo";
				var collection = findBox.GetBindingMembersForCompileTimeCheckInternal(findBox.DataSourceType, ((IDataBoundControl)findBox).DataMember);
				int countWithoutTariffInfo = collection.Count - 1;

				AssertEquals("BindingMember", findBox.BindToTariffPropertyInfo, collection[countWithoutTariffInfo].BindingMember);
				AssertEquals("DataSourceType", findBox.DataSourceType, collection[countWithoutTariffInfo].DataSourceType);
				AssertEquals("ControlPropertyType", findBox.TariffInfo.GetType(), collection[countWithoutTariffInfo].ControlPropertyType);

				findBox.BindToTariffPropertyInfo = string.Empty;
				collection = findBox.GetBindingMembersForCompileTimeCheckInternal(findBox.DataSourceType, ((IDataBoundControl)findBox).DataMember);
				AssertEquals("GetBindingMembersForCompileTimeCheck count", countWithoutTariffInfo, collection.Count);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}
	}
}
