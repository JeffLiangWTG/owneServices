using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ShipmentPageTest : TestCaseWithFactory
	{
		public void TestGetNavigateURL()
		{
			var testPage = new DummyShipmentPage();
			testPage.IsCreateNewAppInstanceIfNullForTest = true;

			var testShipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			var testShipment2 = Factory.NewWithValidTestData<TrackingShipment>();
			testShipment2.JS_UniqueConsignRef = "S0123456789";
			var testDeclaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var testDeclaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			testDeclaration2.JE_DeclarationReference = "B012345678";
			var inactiveShipment = Factory.NewWithValidTestData<TrackingShipment>();
			inactiveShipment.JS_IsCancelled = true;
			var inactiveDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			inactiveDeclaration.JE_IsCancelled = true;

			Factory.Save();

			var testParams = new NameValueCollection();

			testParams.Add("Ref", testShipment1.PK.ToString());
			string expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.ShipmentDetailsPage, testShipment1.PK.ToString());
			AssertEquals("NavigateURL - Shipment by PK", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();

			testParams.Add("Number", testShipment2.JS_UniqueConsignRef);
			expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.ShipmentDetailsPage, testShipment2.PK.ToString());
			AssertEquals("NavigateURL - Shipment by Number", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();

			testParams.Add("Ref", testDeclaration1.PK.ToString());
			expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.DeclarationDetailsPage, testDeclaration1.PK.ToString());
			AssertEquals("NavigateURL - Declaration by PK", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();

			testParams.Add("Number", testDeclaration2.JE_DeclarationReference);
			expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.DeclarationDetailsPage, testDeclaration2.PK.ToString());
			AssertEquals("NavigateURL - Declaration by Number", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();

			testParams.Add("Ref", inactiveShipment.PK.ToString());
			expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.ShipmentDetailsPage, inactiveShipment.PK.ToString());
			AssertEquals("NavigateURL - Shipment by PK - Inactive", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();

			testParams.Add("Ref", inactiveDeclaration.PK.ToString());
			expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.DeclarationDetailsPage, inactiveDeclaration.PK.ToString());
			AssertEquals("NavigateURL - Declaration by PK - Inactive", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		public void TestGetNavigateURLWithTable()
		{
			DummyShipmentPage testPage = new DummyShipmentPage();
			testPage.IsCreateNewAppInstanceIfNullForTest = true;
			NameValueCollection testParams = new NameValueCollection();

			ZGuid testPK = ZGuid.NewZGuid();

			testParams.Add("Ref", testPK.ToString());
			testParams.Add("Table", TrackingShipment.Schema.TableName);
			string expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.ShipmentDetailsPage, testPK.ToString());
			AssertEquals("NavigateURL - Shipment by PK", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();

			testParams.Add("Ref", testPK.ToString());
			testParams.Add("Table", BaseJobDeclaration.Schema.TableName);
			expectedURL = String.Format("{0}?Ref={1}", testPage.AppInstance.DeclarationDetailsPage, testPK.ToString());
			AssertEquals("NavigateURL - Declaration by PK", expectedURL, testPage.GetNavigateURL(testParams));
			testParams.Clear();
		}
	}
}
