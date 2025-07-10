using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationSynchroniserTestNotCountrySpecific : JobDeclarationSynchroniserTest
	{
		public void TestDestinationAttachedOrders_CollectionCountChange_NullReferenceException()
		{
			var collectionCountChangeMethod = typeof(JobDeclarationSynchroniser).GetMethod(
				"DestinationAttachedOrders_CollectionCountChange", BindingFlags.Instance | BindingFlags.NonPublic);
			var args = new CollectionCountChangedEventArgs(false, null);
			AssertNoExceptionThrown(() => { collectionCountChangeMethod.Invoke(decSynchroniser, new object[] { declaration, args }); });

			args = new CollectionCountChangedEventArgs(false, Factory.New<BaseJobDeclaration>());
			AssertNoExceptionThrown(() => { collectionCountChangeMethod.Invoke(decSynchroniser, new object[] { declaration, args }); });
		}

		public void TestHandleMaximumJE_ContainerCount()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = Shipment.JS_RL_NKDestination;
			transport.JW_RL_NKDiscPort = Shipment.JS_RL_NKOrigin;

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";
			container1.JC_ContainerCount = short.MaxValue;
			var container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";
			container2.JC_ContainerCount = short.MaxValue;

			Shipment.JS_F3_NKPackType = "123";
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 12;
			packLine1.JL_F3_NKPackType = "AA";
			packLine1.JL_JC = container1.PK;

			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 14;
			packLine2.JL_F3_NKPackType = "BB";
			packLine2.JL_JC = container2.PK;
			Factory.Save();

			int maxValue = 2 * short.MaxValue;
			AssertEquals("Pre Condition", maxValue, Consol.JK_Calc_ContainerCount);
			AssertEquals("Pre Condition", maxValue, Shipment.JS_Calc_ContainerCount);
			AssertEquals("Pre Condition", (ZShort)0, declaration.JE_ContainerCount);
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", (ZShort)0, declaration.JE_ContainerCount);

			container1.JC_ContainerCount = 11;
			container2.JC_ContainerCount = 22;
			AssertEquals("Pre Condition", 33, Consol.JK_Calc_ContainerCount);
			AssertEquals("Pre Condition", 33, Shipment.JS_Calc_ContainerCount);
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", (ZShort)33, declaration.JE_ContainerCount);
		}

		public void TestSynchronise_OrderNumbersToOwnerRefInCorrectOrder()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "$#123";
			var shipment = CreateImportShipment();
			shipment.ConsigneePK = consignee.PK;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "KRANY";
			transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			shipment.JS_RL_NKOrigin = "KRASA";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			declaration.JE_JS = shipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("pre-condition", "", declaration.JE_OwnerRef);
			Factory.Save();

			shipment.AttachedOrders.ApplySort(Order.Schema.JD_OrderNumber, System.ComponentModel.ListSortDirection.Descending);
			var inv1Order = shipment.AttachedOrders.AddNew();
			inv1Order.JD_OA_BuyerAddress = consignee.MainAddress.PK;
			var inv2Order = shipment.AttachedOrders.AddNew();
			inv2Order.JD_OA_BuyerAddress = consignee.MainAddress.PK;
			inv2Order.JD_OrderNumberInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				var ve = (ValueChangedEventArgs)e;
				if (ve.NewValue.Equals(new ZString("P000002")))
				{
					ve.Info.Value = ZString.Empty;
				}
			};
			var inv3Order = shipment.AttachedOrders.AddNew();
			inv3Order.JD_OA_BuyerAddress = consignee.MainAddress.PK;

			var orderItem1 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "ORDINV67890";
			var orderItem2 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "";
			var orderItem3 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem3.JT_OrderReference = "ORDINV12345";
			var hasChangesStacksForDebugging = new List<StackTrace>();
			using (CaptureHasChangesStack(hasChangesStacksForDebugging))
			{
				Factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("1. Order Number", "ORDINV12345, ORDINV67890, P000001", declaration.JE_OwnerRef);
					Assert(FormatStackTraces(hasChangesStacksForDebugging), !declaration.HasChanges);

					orderItem1.JT_OrderReference = "ORDINA67890";
					AssertEquals("2. Order Number", "ORDINA67890, ORDINV12345, P000001", declaration.JE_OwnerRef);

					shipment.DocsAndCartage.OrderItems.Remove(orderItem3);
					AssertEquals("3. Order Number", "ORDINA67890, P000001, P000003", declaration.JE_OwnerRef);

					inv3Order.JD_OrderNumber = "A000003";
					AssertEquals("4. Order Number", "A000003, ORDINA67890, P000001", declaration.JE_OwnerRef);

					shipment.DocsAndCartage.OrderItems.Add(orderItem3);
					AssertEquals("5. Order Number", "A000003, ORDINA67890, ORDINV12345", declaration.JE_OwnerRef);

					shipment.AttachedOrders.RemoveFromRelationship(inv3Order);
					AssertEquals("6. Order Number", "ORDINA67890, ORDINV12345, P000001", declaration.JE_OwnerRef);

					shipment.AttachedOrders.Add(inv3Order);
					AssertEquals("7. Order Number", "A000003, ORDINA67890, ORDINV12345", declaration.JE_OwnerRef);
				});
			}
		}

		string FormatStackTraces(ICollection<StackTrace> stacks)
		{
			var sb = new StringBuilder();
			if (stacks.Count == 0)
			{
				sb.AppendLine("No stack traces were recorded. Either HasChanges was never false, or the object that caused the change was not being recorded.");
			}
			else
			{
				int i = 0;
				foreach (var stack in stacks)
				{
					sb.AppendLine("Stack #" + (++i).ToString(CultureInfo.InvariantCulture));
					sb.AppendLine(stack.ToString());
				}
			}

			return sb.ToString();
		}

		IDisposable CaptureHasChangesStack(ICollection<StackTrace> collector)
		{
			void AddStackIfChanged(object sender, HasChangesChangedEventArgs e)
			{
				if (e.ObjectJustWasChanged && declaration.HasChanges)
				{
					collector.Add(new StackTrace());
				}
			}

			declaration.HasChangesChanged += AddStackIfChanged;
			return new DisposableAction(() => declaration.HasChangesChanged -= AddStackIfChanged);
		}

		public void TestSynchroniseToShipmentWithAndWithoutConsol()
		{
			var localCountryCode = declaration.CountryCode;
			var sydney = localCountryCode + "SYD";
			var melbourne = localCountryCode + "MEL";
			var perth = localCountryCode + "PER";
			var dunstable = "GBDTE";
			var chesterfield = "GBCES";
			var heathrow = "GBLHR";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = sydney;
			consignor.OH_RL_NKClosestPort = chesterfield;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Shipment.Consols.RemoveAll();
			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPK = consignor.PK;
			Shipment.JS_RL_NKOrigin = dunstable;
			Shipment.JS_RL_NKDestination = perth;

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Consol-less shipment should make a dec whose transport ports match the shipment's CLIENTS' ports", chesterfield, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Consol-less shipment should make a dec whose transport ports match the shipment's CLIENTS' ports", sydney, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Consol-less shipment should make a dec whose shipment ports match the shipment's ports", dunstable, declaration.JE_RL_NKOrigin);
			AssertEquals("Consol-less shipment should make a dec whose shipment ports match the shipment's ports", perth, declaration.JE_RL_NKFinalDestination);

			Consol.JK_RL_NKLoadPort = heathrow;
			Consol.JK_RL_NKDischargePort = melbourne;
			Shipment.Consols.Add(Consol);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Consoled shipment should make a dec whose transport ports match the consol's ports", heathrow, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Consoled shipment should make a dec whose transport ports match the consol's ports", melbourne, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Consoled shipment should make a dec whose shipment ports match the shipment's ports", dunstable, declaration.JE_RL_NKOrigin);
			AssertEquals("Consoled shipment should make a dec whose shipment ports match the shipment's ports", perth, declaration.JE_RL_NKFinalDestination);
		}

		public void TestSynchroniseFirstArrivalDateAndPort()
		{
			Consol.JK_DatePortOfFirstArrival = ZDateTime.Today.AddDays(2);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
				declaration.JE_JS = Shipment.PK;
				TestHelper.MakeConsolRelevantToDeclaration(Consol, declaration);
				Consol.JK_DatePortOfFirstArrival = ZDateTime.BrettsBirthday;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals("Declaration.JE_DateOfFirstArrival", Consol.JK_DatePortOfFirstArrival, declaration.JE_DateOfFirstArrival);
			}
		}

		[ExpectNoExceptions]
		public void TestSynchronisationAfterConsolBecomesIrrelevantInAnotherFactory()
		{
			// Make consol, import into your country.  Set a carrier.  Make shipment into this country, open shipment, enter brokerage tab.  Save and close. From the consol, open the shipment again (to ensure factories are linked). Enter the brokerage tab again.  On consol, change destination port so that it's no longer into your country.  Save consol. On still-open shipment form, press documents menu. We hit GetCarrier. Destination.RelevantConsol is now null.
			var consolFactory1 = new BusinessObjectFactory();
			var consol = consolFactory1.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ARBUE";
			consol.JK_RL_NKDischargePort = consolFactory1.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			var declaration = consolFactory1.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			consolFactory1.Save();

			var consolFactory2 = new BusinessObjectFactory();
			var shipmentFactory = new BusinessObjectFactory();
			var consolReopened = consolFactory2.Load<ForwardingConsol>(consol.PK);
			var shipmentReopened = shipmentFactory.Load<ForwardingShipment>(shipment.PK);
			var declarationReopenedAndSynched = shipmentFactory.Load<BaseJobDeclaration>(declaration.PK);
			declarationReopenedAndSynched.ShipmentSynchroniser.Synchronise(true);

			// We're in a different factory so updating the discharge port will NOT fire the event handler to unhook the synchroniser
			consolReopened.JK_RL_NKDischargePort = consolFactory2.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			consolFactory2.Save();
			AssertNotNull("Simulate pressing documents menu. Should not explode on GetCarrier()", shipmentReopened.DeclarationForDocuments);
		}

		[ExpectNoExceptions]
		public void TestSynchronisationAfterConsolBecomesIrrelevantIsReHookedToNewConsol()
		{
			var consolFactory1 = new BusinessObjectFactory();
			var consolFromArbue = consolFactory1.New<ForwardingConsol>();
			consolFromArbue.JK_RL_NKLoadPort = "ARBUE";
			consolFromArbue.JK_RL_NKDischargePort = "ERASA";
			consolFromArbue.JK_MasterBillNum = "FROMBUE";

			var consolFromCaracas = consolFactory1.New<ForwardingConsol>();
			consolFromCaracas.JK_RL_NKLoadPort = "VECCS";
			consolFromCaracas.JK_RL_NKDischargePort = "GBLHR";
			consolFromCaracas.JK_MasterBillNum = "FROMCCS";

			var shipment = consolFromArbue.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = consolFromArbue.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consolFromArbue.JK_RL_NKDischargePort;

			shipment.Consols.Add(consolFromCaracas);

			var declaration = consolFactory1.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			consolFactory1.Save();

			var consolArbueFactory = new BusinessObjectFactory();
			var consolVeccsFactory = new BusinessObjectFactory();
			var shipmentFactory = new BusinessObjectFactory();
			var consolFromArbueReopened = consolArbueFactory.Load<ForwardingConsol>(consolFromArbue.PK);
			var consolFromCaracasReopened = consolVeccsFactory.Load<ForwardingConsol>(consolFromCaracas.PK);
			var shipmentReopened = shipmentFactory.Load<ForwardingShipment>(shipment.PK);
			var declarationReopenedAndSynched = shipmentFactory.Load<BaseJobDeclaration>(declaration.PK);
			declarationReopenedAndSynched.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("FROMBUE", declarationReopenedAndSynched.JE_MasterBill);

			consolFromArbueReopened.JK_RL_NKDischargePort = "FIHEL";
			consolFromCaracasReopened.JK_RL_NKDischargePort = "ERASA";
			consolArbueFactory.Save();
			consolVeccsFactory.Save();
			declarationReopenedAndSynched.ShipmentSynchroniser.Synchronise(true);  // Necessary?
			AssertEquals("FROMCCS", declarationReopenedAndSynched.JE_MasterBill);
		}

		ZString localPort;
		OrgHeader orgLocal;
		OrgAddress addressLocal;
		OrgAddress addressForeign;

		public void TestSynchWithOverridenOrgs_Import()
		{
			PrepareOrgs();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = localPort;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = localPort;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = addressForeign.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = addressLocal.PK;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = "IMP";
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Port is editable when source address overridden", false, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
			AssertEquals("JE_OH_Importer is blank when overridden", ZGuid.Empty, declaration.JE_OH_Importer);
			AssertEquals("JE_OH_Importer is editable when overridden", false, declaration.JE_OH_ImporterInfo.ReadOnly);
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Port is locked, without explicit re-synch, when override is changed", true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
			AssertEquals("JE_OH_Importer is NOT blank when overridden", orgLocal.PK, declaration.JE_OH_Importer);
			AssertEquals("JE_OH_Importer is locked when overridden", true, declaration.JE_OH_ImporterInfo.ReadOnly);

			shipment.Consols.Add(consol);
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port is locked now that we are able to calculate a port", true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
		}

		public void TestSynchWithOverridenOrgs_Export()
		{
			PrepareOrgs();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = localPort;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = localPort;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = addressForeign.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = addressLocal.PK;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = "EXP";
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Port is editable when source address overridden", false, declaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals("JE_OH_Supplier is blank when overridden", ZGuid.Empty, declaration.JE_OH_Supplier);
			AssertEquals("JE_OH_Supplier is editable when overridden", false, declaration.JE_OH_SupplierInfo.ReadOnly);
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Port is locked, without explicit re-synch, when override is changed", true, declaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals("JE_OH_Supplier is NOT blank when overridden", orgLocal.PK, declaration.JE_OH_Supplier);
			AssertEquals("JE_OH_Supplier is locked when overridden", true, declaration.JE_OH_SupplierInfo.ReadOnly);

			shipment.Consols.Add(consol);
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(localPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port is locked now that we are able to calculate a port", true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
		}

		void PrepareOrgs()
		{
			localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;

			orgLocal = Factory.New<OrgHeader>();
			addressLocal = orgLocal.MainAddress;
			addressLocal.OA_Address1 = "Daniel";
			addressLocal.OA_RL_NKRelatedPortCode = localPort;

			var orgForeign = Factory.New<OrgHeader>();
			addressForeign = orgForeign.MainAddress;
			addressForeign.OA_Address1 = "Daniel";
			addressForeign.OA_RL_NKRelatedPortCode = foreignPort;
		}

		[ExpectNoExceptions]
		public void TestSaveErrorOnDocumentEventSource_DocumentPrePrinted()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var overseaPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var menuItemQuery = new ZQuery();
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Bill Of Lading");
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");

			var menuItem = factory1.LoadTop1<StmMenuItem>(menuItemQuery);
			menuItem.SU_ContactType = ContactType.NotifyParty.Code;

			var consol = factory1.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MSCUS4255494";
			consol.JK_RL_NKLoadPort = overseaPort.Code;
			consol.JK_RL_NKDischargePort = localPort.Code;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HNLTZS18A03088";
			shipment.JS_RL_NKOrigin = overseaPort.Code;
			shipment.JS_RL_NKDischargePort = localPort.Code;

			var declaration = factory1.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.SetEnabled(true, false);

			AssertEquals("MSCUS4255494", declaration.JE_MasterBill);
			AssertEquals("HNLTZS18A03088", declaration.JE_HouseBill);

			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var declaration2 = factory2.Load<BaseJobDeclaration>(declaration.PK);
			declaration2.Bills.RemoveAndDeleteAll();
			factory2.Save();

			var events = new DocumentEventsForTest();
			var supporter = new CommonShipmentDocumentSupporter(shipment);
			supporter.Initialise(events);

			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));
			AssertEquals("HouseBill Issue Date will be set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
			AssertEquals("MSCUS4255494", declaration.PrimaryMasterBill.CU_BillNum);
			AssertEquals("HNLTZS18A03088", declaration.PrimaryHouseBill.CU_BillNum);
			AssertEquals(ZDateTime.Today, declaration.HouseBillIssuedDate);
		}
	}
}
