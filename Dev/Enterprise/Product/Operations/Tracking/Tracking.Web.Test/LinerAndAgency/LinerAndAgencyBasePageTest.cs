using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	class LinerAndAgencyBasePageTest : TestCaseWithFactory
	{
		#region TestBookedContainersGrid

		public void TestBookedContainersGrid()
		{
			using (ILinerAndAgenycBasePageForTest testPage = GetNewTestPage())
			{
				testPage.SetupBookedContainersGridForTest();
				int i = 0;

				AssertColumnIsInGrid("Number", i++, typeof(ZTextEditColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Type", i++, typeof(ZGuidDropDownListColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Count", i++, typeof(ZCalcEditColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Net Wt.", i++, typeof(ZCalcEditColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Tare Wt.", i++, typeof(ZCalcEditColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Gross Wt.", i++, typeof(ZCalcEditColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("WQ", i++, typeof(ZDropDownListColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Commodity", i++, typeof(ZCodeFindBoxColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Is Shipper Owned", i++, typeof(ZCheckBoxColumn), testPage.BookedContainersGridForTest);
				AssertColumnIsInGrid("Seal #", i++, typeof(ZTextEditColumn), testPage.BookedContainersGridForTest);
			}
		}

		#endregion

		#region TesPacksGrid

		[HttpContextEnabledTest]
		public void TestPacksGrid()
		{
			using (ILinerAndAgenycBasePageForTest testPage = GetNewTestPage())
			{
				testPage.SetupPackLinesGridForTest(Core.Constants.ContainerModes.FCL);
				int i = 0;

				AssertColumnIsInGrid("Container", i++, typeof(ZGuidDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Packs", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Pack Type", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Weight", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UW", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Length", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Width", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Height", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UD", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Volume", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UV", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Description", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Commodity", i++, typeof(ZCodeFindBoxColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Dangerous Goods", i++, typeof(ZFindBoxColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Harmonized Code", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);

				testPage.SetupPackLinesGridForTest(Core.Constants.ContainerModes.Bulk);
				i = 0;

				AssertColumnIsInGrid("Packs", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Pack Type", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Weight", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UW", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Length", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Width", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Height", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UD", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Volume", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UV", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Description", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Commodity", i++, typeof(ZCodeFindBoxColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Dangerous Goods", i++, typeof(ZFindBoxColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Harmonized Code", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);

				testPage.SetupPackLinesGridForTest(Core.Constants.ContainerModes.RollOnRollOff);
				i = 0;

				AssertColumnIsInGrid("VIN/Serial", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Count", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Description", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Marks And Numbers", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Gross Wt.", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UW", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Volume", i++, typeof(ZCalcEditColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("UV", i++, typeof(ZDropDownListColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Commodity", i++, typeof(ZCodeFindBoxColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Dangerous Goods", i++, typeof(ZFindBoxColumn), testPage.PacksGridForTest);
				AssertColumnIsInGrid("Harmonized Code", i++, typeof(ZTextEditColumn), testPage.PacksGridForTest);
			}
		}

		[HttpContextEnabledTest]
		public void TestPacksGridQuickViewUser()
		{
			using (ILinerAndAgenycBasePageForTest testPage = GetNewTestPage())
			{
				((ZPage)testPage).SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

				Assert(((ZPage)testPage).SiteUser != null);
				testPage.SetupPackLinesGridForTest(Core.Constants.ContainerModes.FCL);
				AssertColumnIsNotInGrid("Commodity", testPage.PacksGridForTest);
				AssertColumnIsNotInGrid("Dangerous Goods", testPage.PacksGridForTest);
				AssertColumnIsNotInGrid("Harmonized Code", testPage.PacksGridForTest);
			}
		}

		#endregion

		#region Implementation

		protected virtual ILinerAndAgenycBasePageForTest GetNewTestPage()
		{
			return new TestLinerAndAgencyBasePage();
		}

		void AssertColumnIsInGrid(ZString headerText, int columnIndex, Type columnType, ZDataGrid gridForTesting)
		{
			if (columnIndex < gridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (gridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, gridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		void AssertColumnIsNotInGrid(ZString headerText, ZDataGrid gridForTesting)
		{
			for (int i = 0; i < gridForTesting.Columns.Count; i++)
			{
				if (gridForTesting.Columns[i].HeaderText.Equals(headerText))
				{
					Fail(ZString.Format("Column '{0}' is in the grid at index {1} and should NOT be in grid.", headerText, i));
				}
			}
		}

		#endregion
	}
}
