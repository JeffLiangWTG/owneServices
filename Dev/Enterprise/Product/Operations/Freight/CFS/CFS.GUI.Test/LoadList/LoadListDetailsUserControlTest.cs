using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class LoadListDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestNumbersControlInvisibleWithCompanyNotCanada()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			var loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();

			using (CFSLoadListConsolFormForTesting form = new CFSLoadListConsolFormForTesting(loadList))
			{
				form.Show();

				NumbersControl referenceNumbersControl = (NumbersControl)form.LoadListDetailsUserControl.Controls.Find("referenceNumbersControl", true)[0];
				Assert("The ReferenceNumbersControl should be invisible", !referenceNumbersControl.Visible);
			}
		}

		public void TestReferenceNumbersBinding()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			var loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			CusEntryNumber additionalNumber = loadList.Numbers.AddNew();
			additionalNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			additionalNumber.CE_EntryNum = "~~~";
			additionalNumber.CE_IssueDate = ZDateTime.Today;
			additionalNumber.CE_EntryLineReference = "Reference";

			Factory.Save();

			using (CFSLoadListConsolFormForTesting form = new CFSLoadListConsolFormForTesting(loadList))
			{
				form.Show();

				NumbersControl referenceNumbersControl = (NumbersControl)form.LoadListDetailsUserControl.Controls.Find("referenceNumbersControl", true)[0];
				ZGrid numbersGrid = (ZGrid)referenceNumbersControl.Controls.Find("numbersGrid", true)[0];

				Assert("The ReferenceNumbersControl should be visible for a CA company", referenceNumbersControl.Visible);
				Assert("The NumbersGrid should be visible", numbersGrid.Visible);
				AssertEquals("There should be one CusEntryNumber in the NumbersGrid", 1, numbersGrid.ListManager.Count);
				AssertSame("Check binding of the NumbersGrid is correct", additionalNumber, numbersGrid.ListManager.List[0]);
			}
		}

		public void TestExternalColumnsInShipmentsGrid()
		{
			var loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			using (CFSLoadListConsolFormForTesting form = new CFSLoadListConsolFormForTesting(loadList))
			{
				form.Show();

				AssertNull("Column RNSReleaseStatus should not be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("RNSReleaseStatus"));
				AssertNull("Column RNSReleaseDate should not be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("RNSReleaseDate"));

				AssertNull("Column ArrivalCertificationStatus should not be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("ArrivalCertificationStatus"));
				AssertNull("Column ArrivalCertificationDate should not be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("ArrivalCertificationDate"));

				AssertNull("Column CanadaHouseCCN should be removed", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("CanadaHouseCCN"));
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			using (CFSLoadListConsolFormForTesting form = new CFSLoadListConsolFormForTesting(loadList))
			{
				form.Show();

				AssertNotNull("Column RNSReleaseStatus should be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("RNSReleaseStatus"));
				AssertNotNull("Column RNSReleaseDate should be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("RNSReleaseDate"));

				AssertNotNull("Column ArrivalCertificationStatus should be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("ArrivalCertificationStatus"));
				AssertNotNull("Column ArrivalCertificationDate should be added", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("ArrivalCertificationDate"));

				AssertNotNull("Column CanadaHouseCCN should not be removed", form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("CanadaHouseCCN"));
			}
		}

		class CFSLoadListConsolFormForTesting : CFSLoadListConsolForm
		{
			public CFSLoadListConsolFormForTesting(CFSLoadListConsol loadList)
				: base(loadList)
			{
			}

			public new LoadListDetailsUserControl LoadListDetailsUserControl
			{
				get
				{
					return base.LoadListDetailsUserControl;
				}
			}
		}
	}
}
