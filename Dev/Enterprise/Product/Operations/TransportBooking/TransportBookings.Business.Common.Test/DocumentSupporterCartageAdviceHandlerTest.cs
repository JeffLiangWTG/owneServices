using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Shared.Testing
{
	// This class is for tests that require a specific subclass of CartageAdviceDocumentEventsHandler, or a subclass of DocumentSupporter
	// For tests that are focused on CartageAdviceDocumentEventsHandler itself, see CartageAdviceDocumentEventsHandlerTest
	public abstract class DocumentSupporterCartageAdviceHandlerTest : TestCaseWithFactory
	{
		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_IfChildMenuItemDocTypeIndicatesCartageAdvice()
		{
			if (!ChecksChildMenuItem)
			{
				AssertEquals("This class does not check child menu item", false, ChecksChildMenuItem);
				return;
			}
			var originalIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;

				var documentSupporterParent = GetDocumentSupporterParent();

				Factory.Save();

				var menuItem = SetupMenuItemWithChildMenuItem(RefDocTypes.CartageAdvice);
				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
				var events = new IDocumentEventsMock();
				var supporter = documentSupporterParent.DocumentSupporter;
				supporter.Initialise(events);

				Factory.Save();
				var documentSupportables = supporter.GetChildCollection(menuItem, BusinessContext.DtbBooking, null);
				Assert("Precondition: date not set", ((IDtbBooking)documentSupportables.Single()).KM_BookingOfTransportRequestedDate.IsEmpty);
				events.NotifyDocumentPrePrinted(eventArgs);
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var bookingNewFactory = newFactory.Load<IDtbBooking>(((BusinessObject)documentSupportables.Single()).PK);
				AssertEquals("KM_BookingOfTransportRequestedDate should be set with cartage advice", ZDateTime.Today, bookingNewFactory.KM_BookingOfTransportRequestedDate);
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}
		}

		[ExpectNoExceptions]
		public void TestFireDocumentPrePrinted_DoesNotThrowIfGetChildCollectionCalledWithDifferentBusinessContext()
		{
			var originalIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;

				var documentSupporterParent = GetDocumentSupporterParent();

				Factory.Save();

				var menuItem = SetupMenuItemWithChildMenuItem(RefDocTypes.CartageAdvice);
				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
				var events = new IDocumentEventsMock();
				var supporter = documentSupporterParent.DocumentSupporter;
				supporter.Initialise(events);

				var documentSupportables = supporter.GetChildCollection(menuItem, BusinessContext.SubShipment, null);
				events.NotifyDocumentPrePrinted(eventArgs);
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}
		}

		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_ChangesDoneOnCorrectFactory_IfIsUserInteractiveFalse()
		{
			var originalIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				var documentSupporterParent = GetDocumentSupporterParent();
				Factory.Save();

				var menuItem = SetupMenuItem(RefDocTypes.CartageAdvice);
				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
				var events = new IDocumentEventsMock();
				var supporter = documentSupporterParent.DocumentSupporter;
				supporter.Initialise(events);
				Factory.Save();

				Globals.IsUserInteractive = false;

				var documentSupportables = supporter.GetChildCollection(menuItem, BusinessContext.DtbBooking, null);
				Assert("Precondition: date not set", ((IDtbBooking)documentSupportables.Single()).KM_BookingOfTransportRequestedDate.IsEmpty);
				if (DocumentSupportablesUseDifferentFactory)
				{
					AssertNotEquals("Precondition: bookings from documentSupportables have a different factory from document supporter parent", ((BusinessObject)documentSupporterParent).Factory, ((BusinessObject)documentSupportables.First()).Factory);
				}

				events.NotifyDocumentPrePrinted(eventArgs);

				// Simulate WorkflowEventTriggerProcessor factory saving
				Factory.Save();

				var changedObjects = ((BusinessObject)documentSupportables.Single()).Factory.GetChanges().GetChangedObjects();
				var newFactory = new BusinessObjectFactory();
				var bookingNewFactory = newFactory.Load<IDtbBooking>(((BusinessObject)documentSupportables.Single()).PK);
				CombineAssertions(() =>
				{
					AssertEquals("Should save change to KM_BookingOfTransportRequestedDate to database", ZDateTime.Today, bookingNewFactory.KM_BookingOfTransportRequestedDate);
					AssertEquals("Factory associated with document supportables should have no business objects with unsaved changes", 0, changedObjects.Length);
				});
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}
		}

		public void TestBookingsProperty_UsesSameFactoryAsDocumentSupporterParent()
		{
			var originalIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				var documentSupporterParent = GetDocumentSupporterParent();

				Factory.Save();

				var menuItem = SetupMenuItem(RefDocTypes.CartageAdvice);
				var supporter = documentSupporterParent.DocumentSupporter;

				Globals.IsUserInteractive = false;

				var documentSupportables = supporter.GetChildCollection(menuItem, BusinessContext.DtbBooking, null);
				var bookingFromDocumentSupportables = (BusinessObject)documentSupportables.First();
				if (DocumentSupportablesUseDifferentFactory)
				{
					AssertNotEquals("Precondition: bookings from documentSupportables have a different factory from document supporter parent", ((BusinessObject)documentSupporterParent).Factory, bookingFromDocumentSupportables.Factory);
				}

				var bookingFromBookingsProperty = (BusinessObject)GetBookingsFromBookingsProperty(supporter).First();
				AssertEquals("Bookings from document supporter Bookings property should have the same factory as the document supporter parent", ((BusinessObject)documentSupporterParent).Factory, bookingFromBookingsProperty.Factory);
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}
		}

		public void TestBookingsProperty_BookingDoesNotLoad()
		{
			if (!DocumentSupportablesUseDifferentFactory)
			{
				AssertEquals("This class does not use a different factory for document supportables", false, DocumentSupportablesUseDifferentFactory);
				return;
			}
			var originalIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				var factory = new LoadFailsBusinessObjectFactory();

				var documentSupporterParent = GetDocumentSupporterParent();
				Factory.Save();

				var documentSupporterParentLoadFailsFactory = factory.Load(documentSupporterParent.GetType(), ((BusinessObject)documentSupporterParent).PK);

				var menuItem = SetupMenuItem(RefDocTypes.CartageAdvice);
				var supporter = ((IDocumentSupportable)documentSupporterParentLoadFailsFactory).DocumentSupporter;

				Globals.IsUserInteractive = false;

				supporter.GetChildCollection(menuItem, BusinessContext.DtbBooking, null);

				factory.LoadNull = true;

				var bookingsFromBookingsProperty = GetBookingsFromBookingsProperty(supporter);
				var expectedErrorReporterKey = $"{supporter.GetType().Name}_NullBooking"; // For example ForwardingShipmentDocumentSupporter_NullBooking
				CombineAssertions(() =>
				{
					AssertEquals("Should not include null booking in Bookings property", 0, bookingsFromBookingsProperty.Length);
					AssertEquals("Should report error", expectedErrorReporterKey, ErrorReporter.LastKeyReported);
				});
				ErrorReporter.Clear();
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}
		}

		protected DocumentCommand SetupMenuItem(string rT_DocType)
		{
			var menuItem = Factory.New<DocumentCommand>();
			menuItem.SU_MenuName = "Uninformative menu name, no indication of what document type it is";
			menuItem.SU_DocumentDirection = "ARV";

			var stmTemplate = Factory.NewWithValidTestData<StmTemplate>();

			var pivot = menuItem.Documents.AddNew();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = stmTemplate.PK;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = rT_DocType;

			pivot.SI_RT_DocType = docType.PK;

			return menuItem;
		}

		protected DocumentCommand SetupMenuItemWithChildMenuItem(string rT_DocType)
		{
			var menuItem = Factory.New<DocumentCommand>();
			menuItem.SU_MenuName = "Uninformative menu name, no indication of what document type it is";
			menuItem.SU_DocumentDirection = "ARV";

			var childMenuItem = Factory.New<DocumentCommand>();
			childMenuItem.SU_MenuName = "Another uninformative menu name, no indication of what document type it is";

			var stmTemplate = Factory.NewWithValidTestData<StmTemplate>();

			var menuTemplatePivot = childMenuItem.Documents.AddNew();
			menuTemplatePivot.SI_SU = childMenuItem.PK;
			menuTemplatePivot.SI_SO = stmTemplate.PK;

			var stmMenuMenuPivot = menuItem.ChildMenus.AddNew();
			stmMenuMenuPivot.SF_SU_Outward = childMenuItem.PK;
			stmMenuMenuPivot.SF_SU_Inward = childMenuItem.PK;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = rT_DocType;

			menuTemplatePivot.SI_RT_DocType = docType.PK;

			return menuItem;
		}

		protected abstract IDocumentSupportable GetDocumentSupporterParent();
		protected abstract IDocumentSupportable[] GetBookingsFromBookingsProperty(DocumentSupporter documentSupporter);

		protected abstract bool ChecksChildMenuItem { get; }
		protected abstract bool DocumentSupportablesUseDifferentFactory { get; }

		protected class IDocumentEventsMock : IDocumentEvents
		{
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(this, e);
			}

			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(this, e);
				}
			}

			public event DocumentCancelEventHandler DocumentPrintRequested;
			public void NotifyDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				if (DocumentPrintRequested != null)
				{
					DocumentPrintRequested(this, e);
				}
			}

			public event DocumentPrintedEventHandler DocumentPrinted;
			public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(this, e);
				}
			}
		}

		class LoadFailsBusinessObjectFactory : BusinessObjectFactory
		{
			public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
			{
				return new LoadFailsBusinessObjectFactory();
			}

			public override BusinessObject Load(Type bizOType, ZGuid pK)
			{
				if (LoadNull)
				{
					return null;
				}
				else
				{
					return base.Load(bizOType, pK);
				}
			}

			public bool LoadNull { get; set; }
		}
	}
}
