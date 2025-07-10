using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobConShipLink))]
	sealed class JobConShipLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsSavedByFactory()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment otherShipment = Factory.New<CommonShipment>();
			CommonConsol otherConsol = Factory.New<CommonConsol>();

			JobConShipLink pivot = Factory.New<JobConShipLink>();
			pivot.JN_JS = shipment.PK;
			pivot.JN_JK = consol.PK;
			AssertEquals("no duplicate in db", true, pivot.IsSavedByFactory);

			Factory.Save();
			AssertEquals("no duplicate in db", true, pivot.IsSavedByFactory);

			JobConShipLink pivotNew = Factory.New<JobConShipLink>();
			pivotNew.JN_JS = pivot.JN_JS;
			pivotNew.JN_JK = pivot.JN_JK;

			try
			{
				Factory.Save();
			}
			catch (ZSaveException)
			{
				// Expected exception
			}

			// This is because BusinessObject.DuplicateOfMeInDatabase doesn't exclude audit columns. Is there a way of determining what the audit columns are? (Presumably, but what is it?)
			AssertEquals("duplicate of other pivot", false, pivotNew.IsSavedByFactory);

			pivotNew.JN_JS = otherShipment.PK;
			pivotNew.JN_JK = pivot.JN_JK;
			AssertEquals("should not be a duplicate of other pivot", true, pivotNew.IsSavedByFactory);

			pivotNew.JN_JK = pivot.JN_JS;
			pivotNew.JN_JK = pivot.JN_JK;

			Factory.Save(); // should be able to save

			pivotNew.JN_JS = pivot.JN_JS;
			pivotNew.JN_JK = otherConsol.PK;
			AssertEquals("should not be a duplicate of other pivot", true, pivotNew.IsSavedByFactory);

			pivotNew.JN_JS = otherShipment.PK;
			pivotNew.JN_JK = otherConsol.PK;
			AssertEquals("should not be a duplicate of other pivot", true, pivotNew.IsSavedByFactory);
		}

		public void TestDetachingShipmentFromConsolWillDeReferenceJobContainerOnCusContainer()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT3234";
			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var cusContainer = Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer.CO_ContainerNumber = "CONT3234";
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_JE = declaration.PK;
			Factory.Save();
			consol.Shipments.Remove(shipment);
			AssertEquals(ZGuid.Empty, cusContainer.CO_JC);
			consol.Shipments.Add(shipment);
			AssertEquals(ZGuid.Empty, cusContainer.CO_JC);
			cusContainer.CO_JC = container.PK;
			var link = Factory.New<JobConShipLink>();
			link.JN_JK = consol.PK;
			link.JN_JS = shipment.PK;
			consol.Shipments.Remove(shipment);
			consol.Shipments.Load();
			AssertCollectionContains(shipment, consol.Shipments);
			AssertEquals("Should stay link because there another link", container.PK, cusContainer.CO_JC);

			var consol2 = Factory.New<CommonConsol>();
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT3234";
			link.JN_JK = consol2.PK;
			AssertEquals(ZGuid.Empty, cusContainer.CO_JC);
		}

		public void TestOnSaving_IsForwardRegistered()
		{
			var consol1 = Factory.New<CommonConsol>();
			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_IsForwardRegistered = ZBool.False;

			var link1 = Factory.New<JobConShipLink>();
			link1.JN_JK = consol1.PK;
			link1.JN_JS = shipment1.PK;

			AssertEquals(false, shipment1.JS_IsForwardRegistered);
			AssertEquals(false, shipment1.JS_IsBooking);
			Factory.Save();
			AssertEquals(false, shipment1.JS_IsForwardRegistered);

			var consol2 = Factory.New<CommonConsol>();
			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_IsForwardRegistered = ZBool.False;
			shipment2.JS_IsBooking = ZBool.True;

			var link2 = Factory.New<JobConShipLink>();
			link2.JN_JK = consol2.PK;
			link2.JN_JS = shipment2.PK;

			AssertEquals(false, shipment2.JS_IsForwardRegistered);
			AssertEquals(true, shipment2.JS_IsBooking);
			Factory.Save();
			AssertEquals(true, shipment2.JS_IsForwardRegistered);
		}

		public void TestOnSaving_IsCFSRegistered()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_IsCFS = ZBool.True;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_IsCFSRegistered = ZBool.False;

			var link = Factory.New<JobConShipLink>();
			link.JN_JK = consol.PK;
			link.JN_JS = shipment.PK;

			AssertEquals(false, shipment.JS_IsCFSRegistered);
			Factory.Save();
			AssertEquals(true, shipment.JS_IsCFSRegistered);

			ErrorReporter.Clear(); // suppress the TheCFSShipmentHasNotBeenSetAsCFS DeveloperNotificationException
		}

		public void TestRemovingAndAttaching()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			Factory.Save();

			consol.Shipments.Remove(shipment);
			consol.Shipments.Add(shipment);
			JobConShipLink link = (JobConShipLink)consol.Shipments.GetRelationshipBusinessObject(shipment);
			AssertEquals("This link should be saved as the previous one is currently deleted in our factory", true, link.IsSavedByFactory);
		}
	}
}
