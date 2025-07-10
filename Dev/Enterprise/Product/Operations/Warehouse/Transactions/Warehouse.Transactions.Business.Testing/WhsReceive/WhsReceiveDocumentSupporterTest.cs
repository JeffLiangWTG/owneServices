using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveDocumentSupporter))]
	internal class WhsReceiveDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<WhsReceive>();
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName.StartsWith("Cartage Advice") ||
				documentCommand.SU_MenuName.StartsWith("Detailed Label") ||
				documentCommand.SU_MenuName.StartsWith("Summary Label");
		}

		#region GetDocumentWrappers

		#region TestGetDocumentWrappers_ForPalletIDLabels

		public void TestGetDocumentWrappers_ForPalletIDLabels()
		{
			Receive.OnPrint += OnWhsReceiveToPrint;
			ContinueToPrint = false;
			DocumentWrapper[] wrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsPalletIDLabels, null);
			AssertEquals("Wrapper should be null", null, wrappers);

			ContinueToPrint = true;
			wrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsPalletIDLabels, null);
			AssertEquals("Wrapper should be created", 1, wrappers.Length);
			AssertEquals("Wrapper type should be DocWhsReceive", "DocWhsReceive", wrappers[0].GetType().Name);
			AssertEquals(typeof(WhsReceive), wrappers[0].WrappedObject.GetType());
			AssertEquals(Receive, wrappers[0].WrappedObject);
		}

		#endregion

		#region TestGetDocumentWrappers_ForPalletIDLabels_DocStripDocument

		public void TestGetDocumentWrappers_ForPalletIDLabels_DocStripDocument()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT-2");
			receive.InventoryToPrintPalletLabelFor = inventory1; // just to ensure that inventory to print is cleared when we go printing document.

			var receiveDocumentSupporter = new WhsReceiveDocumentSupporter(receive);
			var wrappers = receiveDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Wrapper should be created", 1, wrappers.Length);
			AssertEquals("Wrapper type should be DocWhsReceive", "FreightWrapperFromWhsBO", wrappers[0].GetType().Name);
			AssertNull("When printing a doc strip Pallet Label Document from Receive, no single Pallet should be specified.", receive.InventoryToPrintPalletLabelFor);
		}

		#endregion

		#region TestGetDocumentWrappers_ForOtherDocuments

		public void TestGetDocumentWrappers_ForOtherDocuments()
		{
			WhsInventoryView inventory = Factory.New<WhsInventoryView>();

			DocumentWrapper[] wrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsReceive, null);
			AssertEquals(1, wrappers.Length);
			AssertEquals("DocWhsReceive", wrappers[0].GetType().Name);
			AssertNull(Receive.InventoryToPrintPalletLabelFor);

			Receive.InventoryToPrintPalletLabelFor = inventory;
			wrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsWorkOrder, null); // doesn't matter what DataContext as long as it's not WhsPalletIDLabels
			AssertEquals(1, wrappers.Length);
			AssertEquals("DocWhsReceive", wrappers[0].GetType().Name);
			AssertNull(Receive.InventoryToPrintPalletLabelFor);

			Receive.InventoryToPrintPalletLabelFor = inventory;
			wrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsPalletIDLabels, null);
			AssertEquals(1, wrappers.Length);
			AssertEquals("DocWhsReceive", wrappers[0].GetType().Name);
			AssertNotNull("Run of PalleIDLabels document shouldn't change InventoryToPrintPalletLabelFor property.", Receive.InventoryToPrintPalletLabelFor);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabels

		public void TestGetDocumentWrappers_ProductLabels()
		{
			var inv1 = Receive.Lines.AddNew().Inventory[0];
			var inv2 = Receive.Lines.AddNew().Inventory[0];
			var inv3 = Receive.Lines.AddNew().Inventory[0];
			inv1.WI_TotalUnits = 1;
			inv2.WI_TotalUnits = 1;
			Receive.OnInventoryPrint += OnWhsDocumentInventoryToPrint;
			ContinueToPrint = false;
			DocumentWrapper[] wrappers = DocSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be null", 0, wrappers.Length);

			ContinueToPrint = true;
			wrappers = DocSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be created", 2, wrappers.Length);
			AssertEquals("Wrapper type should be DocWhsReceive", "FreightWrapperFromWhsBO", wrappers[0].GetType().Name);
			AssertEquals(typeof(WhsReceive), wrappers[0].WrappedObject.GetType());
			AssertEquals(Receive, wrappers[0].WrappedObject);
		}

		#endregion

		#endregion

		#region TestGetContactOrganisation

		public override void TestGetContactOrganisation()
		{
			var receive = (WhsReceive)BusinessObject;
			var client = Factory.NewWithValidTestData<OrgHeader>();
			receive.WD_OH_Client = client.PK;

			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			receive.TransportCoPK = transportCo.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var contact1 = receive.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the client", receive.Client.PK, contact1.OrgHeader.PK);

			var contact2 = receive.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("The contact should be the contact of TransportCo", transportCo.PK, contact2.OrgHeader.PK);

			var contact3 = receive.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY);
			AssertEquals("The contact should be the contact of Supplier", supplier.PK, contact3.OrgHeader.PK);
		}

		#endregion

		public void TestGetEDocsProviderSupport()
		{
			IEDocsProvider receive = (IEDocsProvider)BusinessObject;
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), receive.GetEDocsProviderSupporter().GetType());
		}

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsInwards, DocSupporter.BusinessContext);
		}

		public void TestDataContext()
		{
			AssertEquals("Core.Constants.DataContext.WhsReceive is Supported", true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.WhsReceive)));
			AssertEquals("Core.Constants.DataContext.WhsPalletLabels is Supported", true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.WhsPalletIDLabels)));
		}

		#region TestSupportedChildBusinessContexts

		public void TestSupportedChildBusinessContexts()
		{
			AssertEquals("SupportedChildBusinessContexts", 1, DocSupporter.SupportedChildBusinessContexts.Length);
			AssertEquals("SupportedChildBusinessContexts", BusinessContext.DtbBooking, DocSupporter.SupportedChildBusinessContexts[0]);
		}

		#endregion

		#region TestGetBODocDataProvidersNotFoundMessageReturnOneReason

		protected override IEnumerable<Tuple<Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs
		{
			get
			{
				var dataContextAndMessagePairs = new List<Tuple<Constants.DataContext, string>>();

				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.WhsReceive, "Cannot find Warehouse Receive."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.GenericFreightJob, "Cannot find Warehouse Receive."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.GenericProductLabel, "Cannot find Product Label to print."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.WhsPalletIDLabels, "Cannot find Pallet ID Label to print."));

				return dataContextAndMessagePairs;
			}
		}

		#endregion

		#region Implementation

		protected WhsReceive Receive
		{
			get { return (WhsReceive)BusinessObject; }
		}

		bool ContinueToPrint;

		void OnWhsReceiveToPrint(object sender, WhsReceiveToPrintEventArgs e)
		{
			e.ContinueToPrint = ContinueToPrint;
		}

		void OnWhsDocumentInventoryToPrint(object sender, WhsDocumentInventoryEventArgs e)
		{
			e.ContinueToPrint = ContinueToPrint;
		}

		protected override Constants.DataContext DataContext
		{
			get { return Enterprise.Core.Constants.DataContext.WhsReceive; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsReceive>();
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsReceiveCustomiseDocuments;

		#endregion
	}
}
