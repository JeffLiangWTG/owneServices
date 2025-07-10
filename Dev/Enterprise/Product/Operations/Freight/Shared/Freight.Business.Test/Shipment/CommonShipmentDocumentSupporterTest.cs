using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonShipmentDocumentSupporter))]
	sealed class CommonShipmentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocNumbersInPrintingRange()
		{
			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 6;

			var supporter = new CommonShipmentDocumentSupporterForTest(shipment);
			Factory.SetValue<ICommonShipmentDocumentSupporterQueryProvider, CommonShipmentDocumentSupporterQueryProviderForTest>();

			var documentShipment = new DocumentShipment(shipment, Constants.DataContext.GenericFreightJobBySelectedPackages);
			documentShipment.LabelRangeFrom = 2;
			documentShipment.LabelRangeTo = 4;
			var query = (CommonShipmentDocumentSupporterQueryProviderForTest)supporter.QueryProvider;
			query.SetDocumentOptionsOverride(documentShipment);

			var wrappers =
				supporter.GetGenericWrapperForPacksInRange(Constants.DataContext.GenericFreightJobBySelectedPackages, false);
			AssertEquals("Printed Document Shipment number for first document", 2, wrappers[0].DocNumber);
			AssertEquals("Printed Document Shipment number for second document", 3, wrappers[1].DocNumber);
			AssertEquals("Printed Document Shipment number for third document", 4, wrappers[2].DocNumber);
			AssertEquals("Printed Document Shipments count should be 3", 3, wrappers.Length);
		}

		public void TestDocIncludeConsigneeAndConsignorShouldBeTrueInWebEnvironment()
		{
			try
			{
				Globals.IsWeb = true;

				var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;

				var supporter = new CommonShipmentDocumentSupporterForTest(shipment);
				var wrappers = supporter.GetGenericWrapperForPacksFromDataContext(Constants.DataContext.GenericFreightJob, false);

				AssertEquals("Shipment wrapper should be created", 1, wrappers.Length);

				Assert(wrappers.All(c => c.DocIncludeConsignee));
				Assert(wrappers.All(c => c.DocIncludeConsignor));

				Globals.IsWeb = false;

				supporter = new CommonShipmentDocumentSupporterForTest(shipment);
				wrappers = supporter.GetGenericWrapperForPacksFromDataContext(Constants.DataContext.GenericFreightJob, false);

				AssertEquals("Shipment wrapper should be created", 1, wrappers.Length);

				Assert(!wrappers.All(c => c.DocIncludeConsignee));
				Assert(!wrappers.All(c => c.DocIncludeConsignor));
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestDocumentSupporterDataStateReturnsError()
		{
			var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Cartage Advice");
			var command = Factory.LoadTop1<DocumentCommand>(query);

			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			var supporter = new CommonShipmentDocumentSupporterForTest(shipment);
			var docData = supporter.GetDataStateBeforeRun(command);

			AssertEquals("ERROR", docData.ErrorMessage);
		}

		public void TestLocalPortSupportsLocalTransport()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "GBLON";

			AssertEquals("USLAX", shipment.DocumentSupporter.LocalPort(ContactType.LocalTransport, DocumentDirection.DEP));
			AssertEquals("GBLON", shipment.DocumentSupporter.LocalPort(ContactType.LocalTransport, DocumentDirection.ARV));
		}

		public void TestCustomsDocumentsAreCreditControlled()
		{
			using var dps = OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL");
			var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, Enterprise.Customs.Common.US.DocumentNames.FSISForm9540);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Customs");
			query.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, Core.Constants.CountryCodes.UnitedStates);

			var command = Factory.LoadTop1<DocumentCommand>(query);
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			command.SU_MenuName = CommonShipmentDocumentSupporter.USCustomsDocList.EntryPrint;

			var shipment = Factory.New<CreditControlledShipment>();
			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			((ICreditControlledDocumentDelivery)shipment).GetDocumentLogin += (sender, args) =>
			{
				args.IsAllowedToProceed = true;
			};
			((ICreditControlledDocumentDelivery)declaration).GetDocumentLogin += (sender, args) =>
			{
				Assert("Declaration should not be checked, because Shipment should be", false);
			};
			var dataState = new CommonShipmentDocumentSupporter(shipment).GetDataStateBeforeRun(command);

			Assert(dataState.IsValid);
			Assert(shipment.IsDPSFreightMovementRestrictedCalled);
			AssertEquals(nameof(DeliveryRestrictionType.CNH), command.SU_DeliveryRestrictionType);
			AssertEquals(false, command.HasChanges);
		}

		#region CreditControlledShipment

		class CreditControlledShipment : CommonShipment
		{
			public CreditControlledShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool IsDPSFreightMovementRestrictedCore()
			{
				IsDPSFreightMovementRestrictedCalled = true;
				return true;
			}

			public bool IsDPSFreightMovementRestrictedCalled;
		}

		#endregion

		public void TestGetContactOrganisation_NullShipmentJobHeader()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertNull(shipment.ShipmentJobHeader);
			AssertNoExceptionThrown(() => shipment.DocumentSupporter.GetContactOrganisation("Blah", ContactType.Payables, DocumentDirection.ANY));
			AssertNoExceptionThrown(() => shipment.DocumentSupporter.GetContactOrganisation("Blah", ContactType.LocalClient, DocumentDirection.ANY));
		}

		public void TestGetContactOrganisation_NullControllingAgent()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(shipment.DocAddresses.Count, 0);
			AssertNoExceptionThrown(() => shipment.DocumentSupporter.GetContactOrganisation("Blah", ContactType.ControllingAgent, DocumentDirection.ANY));
			AssertNull(shipment.DocumentSupporter.GetContactOrganisation("Blah", ContactType.ControllingAgent, DocumentDirection.ANY));
		}

		public void TestUSCustomsDocListShouldNotContainsDocumentThatHasSpecialWayOfHandling()
		{
			var list = new CommonShipmentDocumentSupporter.USCustomsDocList();

			foreach (var menuName in new[] { Enterprise.Customs.Common.US.DocumentNames.CustomsDeliveryOrder, Enterprise.Customs.Common.US.DocumentNames.FSISForm9540 })
			{
				var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, menuName);
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);
				query.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Customs");
				query.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, Core.Constants.CountryCodes.UnitedStates);
				var command = Factory.LoadTop1<DocumentCommand>(query);
				AssertEquals(menuName + " has a special way of handling so it should be be treated as Customs Doc", false, list.IsCustomsDoc(command));
			}

			foreach (var menuName in new[] {
				CommonShipmentDocumentSupporter.USCustomsDocList.EntryPrint,
				Enterprise.Customs.Common.US.DocumentNames.EntrySummary7501,
				Enterprise.Customs.Common.US.DocumentNames.FDARecap
			})
			{
				var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, menuName);
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);
				query.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Customs");
				query.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, Core.Constants.CountryCodes.UnitedStates);
				var command = Factory.LoadTop1<DocumentCommand>(query);
				AssertEquals(menuName, true, list.IsCustomsDoc(command));
			}
		}

		public void TestGetDocumentTitlesForPivot()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Bill Of Lading";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;
			pivot.SI_PrintCopyType = Enum.GetName(typeof(PrintCopyType), PrintCopyType.ALL);
			pivot.SI_DocumentTitle = "ORIGINAL";

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_NoOriginalBills = 5;
			shipment.JS_NoCopyBills = 5;
			var docSupporter = new CommonShipmentDocumentSupporterForTest(shipment);
			var result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertEquals((short)5, result.CopyCount);

			template.SO_Name = "dot-matrix";
			result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertEquals((short)1, result.CopyCount);

			pivot.SI_DocumentTitle = "COPY";
			template.SO_Name = "blah blah";
			result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertEquals((short)5, result.CopyCount);

			template.SO_Name = "dor-matrix";
			result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertEquals((short)5, result.CopyCount);

			menuItem.SU_MenuName = "Send Electronic Original Bill of Lading";
			pivot.SI_SU = menuItem.PK;
			pivot.SI_DocumentTitle = "ORIGINAL";
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertEquals("Title should be EXPRESS for Electronic EBL", "EXPRESS", result.Title);

			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReq;
			result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertNull("Title should not be overridden for non-express Electronic BOL", result);
		}

		public void TestGetDocumentTitlesForPivot_ForLegacyBillOfLading()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Legacy Bill Of Lading";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;
			pivot.SI_PrintCopyType = Enum.GetName(typeof(PrintCopyType), PrintCopyType.ALL);
			pivot.SI_DocumentTitle = "ORIGINAL";

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;
			var docSupporter = new CommonShipmentDocumentSupporterForTest(shipment);
			var result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertEquals((short)2, result.CopyCount);

			pivot.SI_DocumentTitle = "COPY";
			template.SO_Name = "blah blah";
			result = docSupporter.GetDocumentTitlesForPivot("", shipment, pivot);
			AssertEquals((short)3, result.CopyCount);
		}

		public void TestGetDocWrappersForCartageAdviceContext()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			var line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(container);

			var pickupPackline = Factory.New<PackLine>();
			pickupPackline.JL_JC = container.PK;
			pickupPackline.JL_JS = shipment.PK;

			var pickupConfirm = Factory.New<CommonPickupDeliveryConfirm>();
			pickupConfirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			pickupConfirm.PackLineType = typeof(PackLine);
			pickupConfirm.EU_JC = container.PK;

			var docSupporter = new CommonShipmentDocumentSupporterForTest(shipment);
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Confirm";
			menu.SU_DocumentDirection = "ARV";

			AssertEquals(1, docSupporter.GetDocWrappersForCartageAdviceContextForTest(menu).Length);
		}

		public void TestSaveOnPrintingCartageAdvice()
		{
			var previousIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = true;

				var shipment = Factory.NewWithValidTestData<CommonShipmentForTest>();
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "SGSIN";

				var savedCount = 0;

				shipment.OnShipmentSaved += (sender, e) => savedCount++;

				var filter = new DocumentZQuery("Shipment", "Cartage Advice");
				filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.DEP));
				var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem);
				shipment.DocumentPrintedEvent(this, eventArgs);
				Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);
				AssertEquals("Shipment should not be saved when previewing cartage advice", 0, savedCount);

				eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
				shipment.DocumentPrintedEvent(this, eventArgs);
				Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);
				AssertEquals("Shipment should be saved when printing document", 1, savedCount);

				Globals.IsUserInteractive = false;

				shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Empty;
				shipment.JS_GoodsDescription = "Has changes";
				eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
				shipment.DocumentPrintedEvent(this, eventArgs);
				Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);
				AssertEquals("Shipment should not be saved by document supporter when not user interactive", 1, savedCount);
			}
			finally
			{
				Globals.IsUserInteractive = previousIsUserInteractive;
			}
		}

		public void TestJobDocAddressUniqueIndexSaveExceptionSolvedOnPrintingCartageAdvice()
		{
			var previousIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = true;

				var ctoAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
				var shipment = Factory.NewWithValidTestData<CommonShipmentForTest>();
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "SGSIN";
				var dec = Factory.New(ObjectFactory.GetType<IBaseJobDeclaration>());
				dec[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_ParentID = dec.PK;
				docAddress.E2_ParentTableCode = "JE";
				docAddress.E2_AddressType = DocAddressTypes.Codes.CustomsContainerTerminalOperatorAddress;
				docAddress.E2_AddressSequence = 0;
				docAddress.E2_OA_Address = ctoAddress.PK;

				var savedCount = 0;
				shipment.OnShipmentSaved += (sender, e) => savedCount++;

				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var docAddress2 = factory2.New<JobDocAddress>();
				docAddress2.E2_ParentID = dec.PK;
				docAddress2.E2_ParentTableCode = "JE";
				docAddress2.E2_AddressType = DocAddressTypes.Codes.CustomsContainerTerminalOperatorAddress;
				docAddress2.E2_AddressSequence = 0;
				docAddress2.E2_OA_Address = ctoAddress.PK;
				factory2.Save();

				var filter = new DocumentZQuery("Shipment", "Cartage Advice");
				filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.DEP));
				var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
				AssertNoExceptionThrown(() => shipment.DocumentPrintedEvent(this, eventArgs));
				AssertEquals("Shipment should be saved when printing document", 1, savedCount);
			}
			finally
			{
				Globals.IsUserInteractive = previousIsUserInteractive;
			}
		}

		public void TestShipmentCheckIsCalledWithCorrectMenuName()
		{
			bool bolPrintingShouldBeConfirmedWasCalled = false;

			DocumentEventsForTest events = new DocumentEventsForTest();
			CommonShipmentForTest docSupport = Factory.New<CommonShipmentForTest>();
			docSupport.BOLPrintingShouldBeConfirmedImplementation = () =>
			{
				bolPrintingShouldBeConfirmedWasCalled = true;
				return ZBool.True;
			};

			CommonShipmentDocumentSupporter supporter = new CommonShipmentDocumentSupporter(docSupport);
			supporter.Initialise(events);

			StmMenuItem bolMenuItem = Factory.New<StmMenuItem>();
			bolMenuItem.SU_MenuName = "Bill Of Lading XYZ";
			events.FireDocumentPrintRequested(new DocumentCancelEventArgs(bolMenuItem));

			AssertEquals("Should call the method on CommonShipment for correct menu name", true, bolPrintingShouldBeConfirmedWasCalled);

			bolPrintingShouldBeConfirmedWasCalled = false;
			bolMenuItem = Factory.New<StmMenuItem>();
			bolMenuItem.SU_MenuName = "Neutral HAWB";
			events.FireDocumentPrintRequested(new DocumentCancelEventArgs(bolMenuItem));

			AssertEquals("Should call the method on CommonShipment for correct menu name", true, bolPrintingShouldBeConfirmedWasCalled);

			bolPrintingShouldBeConfirmedWasCalled = false;
			bolMenuItem = Factory.New<StmMenuItem>();
			bolMenuItem.SU_MenuName = "Laser HAWB";
			events.FireDocumentPrintRequested(new DocumentCancelEventArgs(bolMenuItem));

			AssertEquals("Should call the method on CommonShipment for correct menu name", true, bolPrintingShouldBeConfirmedWasCalled);

			bolPrintingShouldBeConfirmedWasCalled = false;
			bolMenuItem = Factory.New<StmMenuItem>();
			bolMenuItem.SU_MenuName = "Laser HAWB with Follow on Page";
			events.FireDocumentPrintRequested(new DocumentCancelEventArgs(bolMenuItem));

			AssertEquals("Should call the method on CommonShipment for correct menu name", true, bolPrintingShouldBeConfirmedWasCalled);
		}

		public void TestShipmentCheckIsNotCalledWithIncorrectMenuName()
		{
			bool bolPrintingShouldBeConfirmedWasCalled = false;

			DocumentEventsForTest events = new DocumentEventsForTest();
			CommonShipmentForTest docSupport = Factory.New<CommonShipmentForTest>();
			docSupport.BOLPrintingShouldBeConfirmedImplementation = () =>
			{
				bolPrintingShouldBeConfirmedWasCalled = true;
				return ZBool.True;
			};

			CommonShipmentDocumentSupporter supporter = new CommonShipmentDocumentSupporter(docSupport);
			supporter.Initialise(events);

			StmMenuItem bolMenuItem = Factory.New<StmMenuItem>();
			bolMenuItem.SU_MenuName = "doesnt start with BILL OF LADING";
			events.FireDocumentPrintRequested(new DocumentCancelEventArgs(bolMenuItem));

			AssertEquals("Should not call the method on CommonShipment for incorrect menu name", false, bolPrintingShouldBeConfirmedWasCalled);
		}

		public void TestGetDocWrappersForGenericFreightJobByContainerIfFCL()
		{
			CommonConsol consol = (CommonConsol)Factory.New<IForwardingConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			consol.Shipments.Add(shipment);

			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			shipment.OuterPackLines.AddNew().SetContainer(consol, container1);
			shipment.OuterPackLines.AddNew().SetContainer(consol, container2);

			CommonShipmentDocumentSupporterForTest docSupporter = new CommonShipmentDocumentSupporterForTest(shipment);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(1, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.DEP, null).Length);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(2, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.DEP, null).Length);
			AssertEquals(1, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.DEP, (CommonContainer x) => x.PK == container2.PK).Length);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Other;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(1, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.ARV, null).Length);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals(2, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.ARV, null).Length);
			AssertEquals(1, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.ARV, (CommonContainer x) => x.PK == container2.PK).Length);
		}

		public void TestGetDocWrappersForGenericFreightJobByContainerIfFCLWithoutSubscription()
		{
			CommonConsol consol = (CommonConsol)Factory.New<IForwardingConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			consol.Shipments.Add(shipment);

			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			shipment.OuterPackLines.AddNew().SetContainer(consol, container1);
			shipment.OuterPackLines.AddNew().SetContainer(consol, container2);

			CommonShipmentDocumentSupporterForTest docSupporter = new CommonShipmentDocumentSupporterForTest(shipment);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(1, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.DEP, null).Length);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(2, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.DEP, null).Length);
			AssertEquals(1, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.DEP, (CommonContainer x) => x.PK == container2.PK).Length);
		}

		public void TestGetDocWrappersForGenericFreightJobByContainerIfBCNWithoutArrivalConsol()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "BCN";
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "AUBNE";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = "SEA";
			consol1.JK_ConsolMode = "BCN";
			consol1.Transports[0].JW_RL_NKLoadPort = "SGSIN";
			consol1.Transports[0].JW_RL_NKDiscPort = "JPOSA";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = "SEA";
			consol2.JK_ConsolMode = "BCN";
			consol2.Transports[0].JW_RL_NKLoadPort = "JPOSA";
			consol2.Transports[0].JW_RL_NKDiscPort = "SGSIN";

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);

			var container = consol1.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);

			var docSupporter = new CommonShipmentDocumentSupporterForTest(shipment);
			AssertNull("Shipment has no arrival consol", shipment.ArrivalConsolForDocuments);
			AssertEquals(1, docSupporter.GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection.ARV, null).Length);
		}

		public void TestTransportMode()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AIR -> AIR", Core.Constants.TransportModes.Air, shipment.DocumentSupporter.TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("SEA -> SEA", Core.Constants.TransportModes.Sea, shipment.DocumentSupporter.TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("ROA -> ROA", Core.Constants.TransportModes.Road, shipment.DocumentSupporter.TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("RAI -> RAI", Core.Constants.TransportModes.Rail, shipment.DocumentSupporter.TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertEquals("FSA -> SEA", Core.Constants.TransportModes.Sea, shipment.DocumentSupporter.TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals("FAS -> AIR", Core.Constants.TransportModes.Air, shipment.DocumentSupporter.TransportMode);
		}

		public void TestContainerMode()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(Core.Constants.ContainerModes.FCL, shipment.DocumentSupporter.ContainerMode);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.LCL, shipment.DocumentSupporter.ContainerMode);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals(Core.Constants.ContainerModes.Bulk, shipment.DocumentSupporter.ContainerMode);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, shipment.DocumentSupporter.ContainerMode);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals(Core.Constants.ContainerModes.Liquid, shipment.DocumentSupporter.ContainerMode);
		}

		public void TestRecipientsForElectronicBillOfLading()
		{
			ZQuery menuItemQuery = new ZQuery();
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, "Send Electronic Original Bill of Lading");
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");

			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(menuItemQuery);

			CommonShipment shipment = Factory.New<CommonShipment>();
			OrgHeader consignor = Factory.New<OrgHeader>();

			shipment.ConsignorPK = consignor.PK;

			OrgContact contact1 = consignor.Contacts.AddNew();
			contact1.OC_ContactName = "Fred Bloggs";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			contact1Document.OD_SU_MenuItem = menuItem.PK;

			OrgContact contact2 = consignor.Contacts.AddNew();
			contact2.OC_ContactName = "John Doe";
			OrgDocument contact2Document = contact2.Documents.AddNew();
			contact2Document.OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact contact3 = consignor.Contacts.AddNew();
			contact3.OC_ContactName = "A. N. Other";
			OrgDocument contact3Document = contact3.Documents.AddNew();
			contact3Document.OD_DocumentGroup = ContactType.Warehouse.Code;

			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = shipment.DocumentSupporter;
			task.Add(pack);

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			AssertEquals("1 recipient added", 1, instructions.Recipients.Count);
			AssertEquals("Fred Bloggs is the only recipient", "Fred Bloggs", instructions.Recipients[0].Name);
		}

		public void TestRecipientFilterByForeignPort()
		{
			ZQuery menuItemQuery = new ZQuery();
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, "Arrival Notice");
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");

			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(menuItemQuery);
			menuItem.SU_ContactType = ContactType.NotifyParty.Code;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUBNE";

			OrgHeader notifyParty = Factory.New<OrgHeader>();

			OrgContact contact1 = notifyParty.Contacts.AddNew();
			contact1.OC_ContactName = "Fred Bloggs";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			contact1Document.OD_FilterForeignPort = "HK";
			contact1Document.OD_SU_MenuItem = menuItem.PK;

			OrgContact contact2 = notifyParty.Contacts.AddNew();
			contact2.OC_ContactName = "John Doe";
			OrgDocument contact2Document = contact2.Documents.AddNew();
			contact2Document.OD_FilterForeignPort = "SG";
			contact2Document.OD_SU_MenuItem = menuItem.PK;

			shipment.NotifyPartyContactPK = contact2.PK;

			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = shipment.DocumentSupporter;
			task.Add(pack);

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			AssertEquals("1 recipient added", 1, instructions.Recipients.Count);
			AssertEquals("Fred Bloggs is the only recipient", "Fred Bloggs", instructions.Recipients[0].Name);
		}

		public void TestConcurrencyErrorOnDocumentEventSource_DocumentPrePrinted()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var menuItemQuery = new ZQuery();
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Bill Of Lading");
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");

			var menuItem = factory1.LoadTop1<StmMenuItem>(menuItemQuery);
			menuItem.SU_ContactType = ContactType.NotifyParty.Code;

			var shipment1 = factory1.New<CommonShipment>();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_RL_NKOrigin = "NZAKL";
			shipment1.JS_RL_NKDestination = "AUSYD";
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var shipment2 = factory2.Load<CommonShipment>(shipment1.PK);
			shipment2.JS_TransportMode = "IBM";
			shipment2.JS_RL_NKOrigin = "KPFNJ";
			shipment2.JS_GoodsDescription = "Kim's super fast shipment via ICBM";

			factory2.Save();

			var events = new DocumentEventsForTest();
			var supporter = new CommonShipmentDocumentSupporter(shipment1);
			supporter.Initialise(events);

			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));
			AssertEquals("HouseBill Issue Date will be set", ZDateTime.Today, shipment1.JS_HouseBillIssueDate);
			AssertEquals("Latest information should have been reloaded in shipment1", "KPFNJ", shipment1.JS_RL_NKOrigin);
		}

		public void TestShipmentHouseBillIssueDateSetsAfterDocumentIsPreprinted()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = "Bill Of Lading";

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "SEA";
			Factory.Save();

			var events = new DocumentEventsForTest();
			var supporter = new CommonShipmentDocumentSupporter(shipment);
			supporter.Initialise(events);

			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem));
			AssertEquals(ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));
			AssertEquals(ZDateTime.Today, shipment.JS_HouseBillIssueDate);
			Assert(shipment.IsInDatabase && !shipment.HasChanges);
		}

		public void TestRecipientFilterByLocalPort()
		{
			ZQuery menuItemQuery = new ZQuery();
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, "Arrival Notice");
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");

			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(menuItemQuery);
			menuItem.SU_ContactType = ContactType.NotifyParty.Code;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUBNE";

			OrgHeader notifyParty = Factory.New<OrgHeader>();

			OrgContact contact1 = notifyParty.Contacts.AddNew();
			contact1.OC_ContactName = "Fred Bloggs";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			contact1Document.OD_FilterLocalPort = "AU";
			contact1Document.OD_SU_MenuItem = menuItem.PK;

			OrgContact contact2 = notifyParty.Contacts.AddNew();
			contact2.OC_ContactName = "John Doe";
			OrgDocument contact2Document = contact2.Documents.AddNew();
			contact2Document.OD_FilterLocalPort = "AUCNS";
			contact2Document.OD_SU_MenuItem = menuItem.PK;

			shipment.NotifyPartyContactPK = contact2.PK;

			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = shipment.DocumentSupporter;
			task.Add(pack);

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			AssertEquals("1 recipient added", 1, instructions.Recipients.Count);
			AssertEquals("Fred Bloggs is the only recipient", "Fred Bloggs", instructions.Recipients[0].Name);

			shipment.JS_RL_NKDestination = "AUCNS";

			instructions = new DeliveryInstructions(pack);
			AssertEquals("2 recipient added", 2, instructions.Recipients.Count);
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			base.SetUp();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CommonShipment>();
		}

		#region Filtering

		#region TestMODFilter

		public void TestMODFilter()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			AssertEquals(Constants.TransportModes.SeaAir, shipment.DocumentSupporter.GetFilterValue(DocumentFilters.MOD));

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = HomePort;
			AssertEquals(Constants.TransportModes.Air, shipment.DocumentSupporter.GetFilterValue(DocumentFilters.MOD));
		}

		static ZString HomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					return GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}
				return "AUSYD";
			}
		}

		#endregion

		#region TestHBLMenuTemplateFilter
		public void TestHBLMenuTemplateFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.HBL, null));

			aShipment.JS_HouseBillOfLadingType = "IAU";
			AssertEquals("IAU", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.HBL, null));
		}
		#endregion

		#region TestCNCTYMenuTemplateFilter
		public void TestCNCTYMenuTemplateFilter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("N", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNCTY, null));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			AssertEquals("Y", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNCTY, null));
		}
		#endregion

		#region TestCNAIRMenuTemplateFilter
		public void TestCNAIRMenuTemplateFilter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("N", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNAIR, null));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			AssertEquals("N", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNAIR, null));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Y", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNAIR, null));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("N", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNAIR, null));
		}
		#endregion

		#region TestCNSEAMenuTemplateFilter
		public void TestCNSEAMenuTemplateFilter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("N", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNSEA, null));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			AssertEquals("N", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNSEA, null));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Y", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNSEA, null));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("N", aShipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CNSEA, null));
		}
		#endregion

		#region TestRequiresContainerCartageAdvice
		public void TestRequiresContainerCartageAdvice()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("RequiresContainerCartageAdvice should be", ZBool.False, aShipment.RequiresContainerCartageAdvice(nameof(DocumentDirection.ARV)));

			aShipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("RequiresContainerCartageAdvice should be", ZBool.False, aShipment.RequiresContainerCartageAdvice(nameof(DocumentDirection.DEP)));

			aShipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("RequiresContainerCartageAdvice should be", ZBool.True, aShipment.RequiresContainerCartageAdvice(nameof(DocumentDirection.ARV)));

			aShipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("RequiresContainerCartageAdvice should be", ZBool.True, aShipment.RequiresContainerCartageAdvice(nameof(DocumentDirection.ARV)));

			aShipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("RequiresContainerCartageAdvice should be", ZBool.True, aShipment.RequiresContainerCartageAdvice(nameof(DocumentDirection.DEP)));
		}
		#endregion

		#region TestMYSRRFilter
		public void TestMYSRRFilter()
		{
			ZString currentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var mYPEN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "MYPEN"));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = mYPEN.RL_Code;
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(ZBool.False.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYPENSRR));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(ZBool.True.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYPENSRR));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals(ZBool.False.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYPENSRR));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(ZBool.True.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYPENSRR));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertEquals(ZBool.False.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYPENSRR));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals(ZBool.False.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYPENSRR));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(ZBool.True.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYPENSRR));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchPort;
		}
		#endregion

		#region TestCTYFilter
		public void TestCTYFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("Company's country code", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CTY));
		}
		#endregion

		#region TestCTYEGFilter
		public void TestCTYEGFilter()
		{
			RunCtyEgFilter("AUPER", "");
			RunCtyEgFilter("USATL", "");
			RunCtyEgFilter("GBLON", EconomicGroupList.Codes.EuropeanUnion);
			RunCtyEgFilter("DEHAM", EconomicGroupList.Codes.EuropeanUnion);
		}

		void RunCtyEgFilter(ZString portCode, string expectedValue)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = portCode.Right(3);
			company.GC_RN_NKCountryCode = portCode.Left(2);
			var branch = company.Branches.AddNew();
			branch.GB_Code = company.GC_Code;
			branch.GB_RL_NKHomePort = portCode;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("Country group code for " + portCode, expectedValue, shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CTYEG));
			}
		}
		#endregion

		#region TestCTYBKRFilter

		public void TestCTYBKRFilter()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("CA");
			try
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("CAN", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CTYBKR));

				var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
				AssertEquals("CAY", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CTYBKR));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
				AssertEquals("USY", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CTYBKR));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		#endregion

		#region TestDOMAIRORROADFilter

		public void TestDOMAIRORROADFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.JS_TransportMode = Constants.TransportModes.Air;
			aShipment.JS_RL_NKOrigin = "USORD";
			aShipment.JS_RL_NKDestination = "USJFK";
			AssertEquals("Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DOMAIRORROAD));

			aShipment.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DOMAIRORROAD));

			aShipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DOMAIRORROAD));

			aShipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DOMAIRORROAD));

			aShipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DOMAIRORROAD));
		}

		#endregion

		#region TestCNTFilter
		public void TestCNTFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("FCL", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CNT));

			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("BCN", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CNT));

			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertEquals("BLK", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CNT));

			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertEquals("LQD", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CNT));
		}
		#endregion

		#region TestFCLBCNFilter
		public void TestFCLBCNFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals(ZBool.True.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.FCLBCN));

			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals(ZBool.True.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.FCLBCN));

			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertEquals(ZBool.False.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.FCLBCN));

			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals(ZBool.False.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.FCLBCN));

			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertEquals(ZBool.False.ToString(), aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.FCLBCN));
		}
		#endregion

		#region TestBKRFilter
		public void TestBKRFilter()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("AU");
			try
			{
				CommonShipment aShipment = Factory.New<CommonShipment>();
				AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.BKR));

				BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
				AssertEquals("Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.BKR));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}
		#endregion

		#region TestMYDOFilter
		public void TestMYDOFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			aShipment.JS_TransportMode = Constants.TransportModes.Sea;
			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYDO));

			GlbCompany.CurrentCompany.SetCountry("MY");
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYDO));

			aShipment.JS_TransportMode = Constants.TransportModes.Sea;
			aShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MYDO));

			GlbCompany.CurrentCompany.SetCountry("AU");
		}
		#endregion

		#region TestAUBKRFilter
		public void TestAUBKRFilter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.AUBKR));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.AUBKR));

			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
			AssertEquals("Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.AUBKR));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			AssertEquals("N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.AUBKR));
		}
		#endregion

		#region TestDECTPFilter
		public void TestDECTPFilter()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

				CommonShipment aShipment = Factory.New<CommonShipment>();
				AssertEquals("DECTP Filter empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DECTP));

				BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
				AssertEquals("DECTP Filter AUEXP", "AUIMP", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DECTP));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
				AssertEquals("DECTP Filter for Export Declaration", "ZAEXP", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DECTP));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
				AssertEquals("DECTP Filter for Export Declaration", "USEXP", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.DECTP));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}
		#endregion

		#region TestCOFilter
		public void TestCOFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("DocumentFilters.CO", GlbCompany.CurrentCompany.GC_Code, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CO));
		}
		#endregion

		#region TestEBLFilter
		public void TestEBLFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.ConsignorPK = ZGuid.Empty;
			aShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("EBL Filter should return empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EBL));

			OrgHeader testCnor = OrgHeader.New(Factory);
			testCnor.MiscServ.OM_EXAllowedToPrintOriginalBL = ZBool.True;
			aShipment.ConsignorPK = testCnor.PK;
			AssertEquals("EBL Filter should return 'Y'", "Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EBL));

			testCnor.MiscServ.OM_EXAllowedToPrintOriginalBL = ZBool.False;
			AssertEquals("EBL Filter should return 'N'", "N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EBL));

			aShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("EBL Filter should return empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EBL));
		}
		#endregion

		#region TestMSGBKRFilter
		public void TestMSGBKRFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("Filter for MSGBKR is empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
			AssertEquals("Filter for MSGBKR is 'IMP'", "IMP", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXW";
			AssertEquals("Filter for MSGBKR is 'IMP'", "IMP", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
			AssertEquals("Filter for MSGBKR is 'EXP'", "EXP", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "DRW";
			AssertEquals("Filter for MSGBKR is 'DRW'", "DRW", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
		}
		#endregion

		#region TestMSGBKRCTYFilter
		public void TestMSGBKRCTYFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("Filter for MSGBKR is empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";

			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("Filter for MSGBKR is 'IMP'", "IMP" + customsCountry, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXW";
			AssertEquals("Filter for MSGBKR is 'IMP'", "IMP" + customsCountry, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
			AssertEquals("Filter for MSGBKR is 'EXP'", "EXP" + customsCountry, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "DRW";
			AssertEquals("Filter for MSGBKR is 'DRW'", "DRW" + customsCountry, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "DRW";
			AssertEquals("Filter for MSGBKR is 'DRW'", "DRW" + "US", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
		}
		#endregion

		#region TestMSGBKRLCOFilter

		public void TestMSGBKRLCOFilter()
		{
			var aShipment = Factory.New<CommonShipment>();
			AssertEquals("Filter for MSGBKR is empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));

			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
				AssertEquals("Import Declaration with Actual Landed Costing", "IMPACT", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXW";
				AssertEquals("Ex Warehouse Declaration with Actual Landed Costing", "IMPACT", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
				AssertEquals("Export Declaration with Actual Landed Costing", "EXPACT", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "DRW";
				AssertEquals("Drawback Declaration with Actual Landed Costing", "DRWACT", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
			}
		}

		#endregion

		#region TestMSGBKRCTYAPPFilter
		public void TestMSGBKRCTYAPPFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
			declaration[JobDeclarationSchema.Constants.JE_ApplicationCode] = "CMR";
			AssertEquals("Filter for MSGBKRCTYAPP is 'IMP'", "IMP" + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + "CMR", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals("Filter for MSGBKRCTYAPP is 'IMP'", "IMPUSCMR", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));
		}
		#endregion

		#region TestBKRCTYFilter
		public void TestBKRCTYFilter()
		{
			var shipment = Factory.New<CommonShipment>();
			var docSupport = new CommonShipmentDocumentSupporter(shipment);
			AssertEquals("BKRCTY Filter", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), docSupport.GetFilterValue(DocumentFilters.BKRCTY));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals("BKRCTY Filter", "US", docSupport.GetFilterValue(DocumentFilters.BKRCTY));
		}

		#endregion

		#region TestBKRCTYAPPFilter
		public void TestBKRCTYAPPFilter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var auShipment = Factory.New<CommonShipment>();
			var auDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			auDeclaration[JobDeclarationSchema.Constants.JE_JS] = auShipment.PK;
			auDeclaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
			auDeclaration[JobDeclarationSchema.Constants.JE_ApplicationCode] = "CMR";

			AssertEquals("Filter for BKRCTYAPP - AU CMR dec: 'AUCMR'", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + "CMR", auShipment.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			var sgShipment1 = Factory.New<CommonShipment>();
			var sgDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			sgDeclaration[JobDeclarationSchema.Constants.JE_JS] = sgShipment1.PK;
			sgDeclaration[JobDeclarationSchema.Constants.JE_MessageType] = "IPT";
			sgDeclaration[JobDeclarationSchema.Constants.JE_ApplicationCode] = "4.1";
			AssertEquals("Filter for BKRCTYAPP - SG 4.1 dec:'SG4.1'", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + "4.1", sgShipment1.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));

			var sgShipment2 = Factory.New<CommonShipment>();
			var sgTN4PointZeroDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			sgTN4PointZeroDeclaration[JobDeclarationSchema.Constants.JE_JS] = sgShipment2.PK;
			sgTN4PointZeroDeclaration[JobDeclarationSchema.Constants.JE_MessageType] = "OUT";
			sgTN4PointZeroDeclaration[JobDeclarationSchema.Constants.JE_ApplicationCode] = "SG4";
			AssertEquals("Filter for BKRCTYAPP - SG 4.0 dec:'SGSG4'", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + "SG4", sgShipment2.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var usShipment = Factory.New<CommonShipment>();
			var usDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			auDeclaration[JobDeclarationSchema.Constants.JE_JS] = usShipment.PK;
			auDeclaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
			auDeclaration[JobDeclarationSchema.Constants.JE_ApplicationCode] = "ACE";
			AssertEquals("Filter for BKRCTYAPP - US ACE dec: 'USACE'", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + "ACE", usShipment.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals("Filter for BKRCTYAPP - US ACE dec: 'USACE'", "USACE", usShipment.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));
		}
		#endregion

		#region TestBrokerageFiltersWithInvalidCountries
		public void TestBrokerageFiltersWithInvalidCountries()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			CommonShipment aShipment = Factory.New<CommonShipment>();
			AssertEquals("Filter for MSGBKR is empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
			AssertEquals("Filter for MSGBKRCTY is empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
			AssertEquals("Filter for EXPBKRLIC is 'N'", "N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EXPBKRLIC));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
			AssertEquals("Filter for MSGBKR is EXP", "EXP", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
			AssertEquals("Filter for MSGBKRCTY is EXP plus current country", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
			AssertEquals("Filter for EXPBKRLIC is 'Y'", "Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EXPBKRLIC));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Malaysia);
			AssertEquals("Filter for MSGBKR is empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
			AssertEquals("Filter for MSGBKRCTY is empty", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
			AssertEquals("Filter for EXPBKRLIC is 'N'", "N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EXPBKRLIC));
		}
		#endregion

		#region TestEXPBKRLICFilter
		public void TestEXPBKRLICFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("Filter for EXPBKRLIC is 'N'", "N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EXPBKRLIC));

			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = aShipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
			AssertEquals("Filter for EXPBKRLIC is 'N'", "N", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EXPBKRLIC));

			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
			AssertEquals("Filter for EXPBKRLIC is 'Y'", "Y", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.EXPBKRLIC));
		}
		#endregion

		#region TestCTYMODFilter
		public void TestCTYMODFilter()
		{
			CommonShipment aShipment = Factory.New<CommonShipment>();
			aShipment.JS_TransportMode = Constants.TransportModes.Sea;

			using (Env.SetTemporaryUserContext(null))
			{
				AssertEquals("Company's country code and TransportMode", string.Empty, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CTYMOD));
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AssertEquals("Company's country code and TransportMode", GlbBranch.CurrentBranch.Country.Code + aShipment.JS_TransportMode, aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CTYMOD));
		}
		#endregion

		#region TestCAIMP

		public void TestCAIMP()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "USORD";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				shipment.JS_RL_NKDestination = "USJFK";
				AssertEquals("Filter for CAIMP is 'N'", "N", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CAIMP));
				shipment.JS_RL_NKDestination = "CAVAN";
				AssertEquals("Filter for CAIMP is 'N'", "N", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CAIMP));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				shipment.JS_RL_NKDestination = "USJFK";
				AssertEquals("Filter for CAIMP is 'N'", "N", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CAIMP));
				shipment.JS_RL_NKDestination = "CAVAN";
				AssertEquals("Filter for CAIMP is 'Y'", "Y", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.CAIMP));
			}
		}

		#endregion

		#region TestAUMSGNXD

		public void TestAUMSGNXD()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<CommonShipment>();
				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "AQS";
				var invHeader = Factory.New<AU.IJobComInvoiceHeader>();
				invHeader.JZ_JE = declaration.PK;
				var exdocHeader = Factory.LoadTop1<AU.IQuarantineExdocHeader>(new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invHeader.PK)) as BusinessObject;

				exdocHeader[QuarantineExDocHeaderSchema.Constants.QH_ProduceType] = "DAI";
				AssertEquals("AQSY", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.AUMSGNXD));

				exdocHeader[QuarantineExDocHeaderSchema.Constants.QH_ProduceType] = "MEA";
				AssertEquals("AQSN", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.AUMSGNXD));

				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
				AssertEquals("IMPN", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.AUMSGNXD));
			}
		}

		#endregion

		#region PrintSSNTestWithSecurityRight

		public void TestPrintSSNTestWithSecurityRight()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Env.Security.USCustomsPrintSSN.IsAllowed = true;

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "FRCHI";
				shipment.JS_RL_NKDestination = "USCHI";

				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = "IMP";

				AssertEquals("Print SSN allowed", "Y", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.PrintSocialSecurityNumberAllowed));

				Env.Security.USCustomsPrintSSN.IsAllowed = false;
				AssertEquals("Print SSN not allowed", "N", shipment.DocumentSupporter.GetFilterValue(DocumentFilters.PrintSocialSecurityNumberAllowed));
			}
		}

		#endregion

		#region TestCAAsAccountedDataSupportAndCACurrentDataSupport

		public void TestCAAsAccountedDataSupportAndCACurrentDataSupport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var shipment = Factory.New<CommonShipment>();
				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
				var entryHeader = Factory.New<ICusEntryHeader>();
				entryHeader.CH_JE = declaration.PK;
				entryHeader.CH_MessageType = "B3C";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var newShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("B3C", newShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CAAsAccountedDataSupport));
				AssertEquals("B3C", newShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CACurrentDataSupport));

				entryHeader.CH_MessageType = "CAD";
				Factory.Save();
				newFactory = new BusinessObjectFactory();
				newShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("CAD", newShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CAAsAccountedDataSupport));
				AssertEquals("CAD", newShipment.DocumentSupporter.GetFilterValue(DocumentFilters.CACurrentDataSupport));
			}
		}

		#endregion

		#endregion

	}
}
