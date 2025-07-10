using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CustomsDocNamesTest : TestCaseWithFactory
	{
		public void TestIsCACustomsDoc()
		{
			var menuNames = new[]
								{
									CommonShipmentDocumentSupporterForTest.CACustomsDocList.B3CurrentData,
									CommonShipmentDocumentSupporterForTest.CACustomsDocList.B3AsLodged,
									CommonShipmentDocumentSupporterForTest.CACustomsDocList.ReleaseStatusDocument,
									CommonShipmentDocumentSupporterForTest.CACustomsDocList.CADCurrentData,
									CommonShipmentDocumentSupporterForTest.CACustomsDocList.CADAsLodged,
								};

			foreach (var menuName in menuNames)
			{
				AssertCustomsDocName(menuName, Constants.CountryCodes.Canada);
			}
		}

		public void TestIsNZCustomsDoc()
		{
			var menuNames = new[]
								{
									CommonShipmentDocumentSupporterForTest.NZCustomsDocList.EntryPrint,
									CommonShipmentDocumentSupporterForTest.NZCustomsDocList.CustomsCertificate,
									CommonShipmentDocumentSupporterForTest.NZCustomsDocList.DeliveryOrder,
									CommonShipmentDocumentSupporterForTest.NZCustomsDocList.DissectionReport,
									CommonShipmentDocumentSupporterForTest.NZCustomsDocList.MAFCoverSheet
								};

			foreach (var menuName in menuNames)
			{
				AssertCustomsDocName(menuName, Constants.CountryCodes.NewZealand);
			}
		}

		public void TestIsUSCustomsDoc()
		{
			foreach (var menuName in new[]
								{
									CommonShipmentDocumentSupporterForTest.USCustomsDocList.EntryPrint,
									DocumentNames.EntrySummary7501,
									DocumentNames.FDARecap
								})
			{
				AssertCustomsDocName(menuName, Constants.CountryCodes.UnitedStates);
			}
		}

		void AssertCustomsDocName(string menuName, string countryCode)
		{
			var documentSupporter = (CommonShipmentDocumentSupporterForTest)Factory.New<CommonShipmentForTest>().DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "Customs\\";
			menuItem.SU_FilterList = countryCode;
			menuItem.SU_MenuName = menuName;
			AssertEquals(menuName, true, documentSupporter.IsCustomsDoc_Exposed(menuItem));

			menuItem.SU_FilterList = ZString.Empty;
			AssertEquals(menuName, false, documentSupporter.IsCustomsDoc_Exposed(menuItem));

			menuItem.SU_MenuPath = ZString.Empty;
			menuItem.SU_FilterList = countryCode;
			AssertEquals(menuName, false, documentSupporter.IsCustomsDoc_Exposed(menuItem));
		}
	}
}
