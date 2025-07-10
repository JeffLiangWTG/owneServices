using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ForwardingDocDataObjectProviderTest : TestCaseWithFactory
	{
		#region TestGetDocDataObject_FromNull

		public void TestGetDocDataObject_FromNull()
		{
			AssertGetDocDataObject_FromNull(DataContext.UXML);
			AssertGetDocDataObject_FromNull(DataContext.HouseBill);
			AssertGetDocDataObject_FromNull(DataContext.CGNExportNotification);
		}

		void AssertGetDocDataObject_FromNull(string dataContext)
		{
			var provider = new ForwardingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var dataObject = provider.GetDocDataObject(null, dataContext, parameters);

			AssertNull($"expected null for null parent and '{dataContext}' data context", dataObject);
		}

		#endregion

		#region TestGetDocDataObject_FromReceiveConsignment

		public void TestGetDocDataObject_FromReceiveConsignment()
		{
			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var provider = new ForwardingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var dataObject = provider.GetDocDataObject(consignment, DataContext.FRPortsGoodsReceivedCRESA, parameters);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a FR CRESA data object", typeof(Cresa), dataObject.GetType());
		}

		#endregion

		#region TestGetDocDataObject_FromDispatchConsignment

		public void TestGetDocDataObject_FromDispatchConsignment()
		{
			var consignment = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var provider = new ForwardingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var dataObject = provider.GetDocDataObject(consignment, DataContext.FRPortsGoodsReceivedCRESA, parameters);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a FR CRESA data object", typeof(Cresa), dataObject.GetType());
		}

		#endregion

		#region TestGetDocDataObject_FromShipment

		public void TestGetDocDataObject_FromShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var provider = new ForwardingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var dataObject = provider.GetDocDataObject(shipment, DataContext.HouseBill, parameters);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a HouseBill data object", typeof(HouseBill), dataObject.GetType());
		}

		#endregion

		#region TestGetDocDataObject_FromConsol

		public void TestGetDocDataObject_FromConsol_DraftHouseBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var provider = new ForwardingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "DraftHouseBill"
			};

			var dataObject = provider.GetDocDataObject(consol, DataContext.DraftHouseBill, parameters);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a DraftHouseBill data object", typeof(DraftHouseBill), dataObject.GetType());
		}

		#endregion

		#region TestProviderIsRegisteredWithObjectFactory

		public void TestProviderIsRegisteredWithObjectFactory()
		{
			var provider = ObjectFactory.Get<IForwardingDocDataObjectProvider>();

			AssertNotNull("ForwardingDocDataObjectProvider is accessible via ObjectFactory", provider);
		}

		#endregion

		#region TestGetDocDataObject_QuotedBooking

		public void TestGetDocDataObject_QuotedBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var provider = new ForwardingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var dataObject = provider.GetDocDataObject(quotedBooking, DataContext.HouseBill, parameters);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a HouseBill data object", typeof(HouseBill), dataObject.GetType());
		}

		#endregion

		#region TestGetDocDataObject_Language

		public void TestGetDocDataObject_Language()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_WorkingLanguage = "ZH-CN";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Direct;
				var provider = new ForwardingDocDataObjectProvider();

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var dataObject = provider.GetDocDataObject(consol, DataContext.ContainerLoadPlan, parameters);
				AssertEquals("should return English", "Direct", ((CN.ContainerLoadPlan)dataObject).ShipmentType.Description);
			}
		}

		#endregion

		#region TestGetDocDataObject_USATF6A
		public void TestGetDocDataObject_USATF6A()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;

				var parameters = new Mock<IDocDataObjectParameters>();
				parameters.SetupGet(x => x.Data).Returns(Array.Empty<ZString>());

				var provider = new ForwardingDocDataObjectProvider();
				var dataObject = provider.GetDocDataObject(shipment, DocumentVisualizer.Integration.DataContext.USATF6A, parameters.Object);
				AssertNotNull(dataObject);
			}
		}
		#endregion

		public void TestGetDocDataObject_DA306Document()
		{
			using var setCountry = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var parameters = new Mock<IDocDataObjectParameters>();
			parameters.SetupGet(x => x.Data).Returns(Array.Empty<ZString>());

			var provider = new ForwardingDocDataObjectProvider();
			var dataObject = provider.GetDocDataObject(shipment, DataContext.DA306Document, parameters.Object);
			AssertNotNull(dataObject);
		}
	}
}
