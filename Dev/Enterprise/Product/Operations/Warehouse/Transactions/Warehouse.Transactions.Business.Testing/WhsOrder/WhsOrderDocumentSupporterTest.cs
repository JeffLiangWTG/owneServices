using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderDocumentSupporter))]
	public class WhsOrdersDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var client = Helper.CreateClient("CLNT");
			var transportCompany = Helper.CreateClient("TCO");
			var consignee = Helper.CreateClient("CNE");
			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = client.PK;
			order.TransportCoPK = transportCompany.PK;
			order.ConsigneePK = consignee.PK;
			var contact = order.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalTransport,
				DocumentDirection.ANY);
			order.WD_PackagesSent = 4;
			return order;
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			GetDocumentSupportableBusinessObject();
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName.StartsWith("Basic Label") ||
				   documentCommand.SU_MenuName.StartsWith("Carrier Label") ||
				   documentCommand.SU_MenuName.StartsWith("Product/Delivery") ||
				   documentCommand.SU_MenuName.StartsWith("Delivery Label") ||
				   documentCommand.SU_MenuName.StartsWith("Product Label – Bat/Exp") ||
				   documentCommand.SU_MenuName.StartsWith("Cartage Advice");
		}

		#region TestCustomisationSecurityCheckpoint

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsOrderCustomiseDocuments;

		#endregion

		#region TestGetContactOrganisation

		public override void TestGetContactOrganisation()
		{
			var order = (WhsOrder)BusinessObject;
			var client = Helper.CreateClient("CLNT");
			var transportCompany = Helper.CreateClient("TCO");
			var consignee = Helper.CreateClient("CNE");
			order.WD_OH_Client = client.PK;
			order.TransportCoPK = transportCompany.PK;
			order.ConsigneePK = consignee.PK;
			var contact = order.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalTransport,
				DocumentDirection.ANY);
			AssertEquals("The contact should be the transport company.", order.GetTransportCo().PK,
				contact.OrgHeader.PK);

			contact = order.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL,
				DocumentDirection.ANY);
			AssertEquals("The contact should be the client.", order.Client.PK, contact.OrgHeader.PK);
			AssertEquals("The contact related Organization should be the consignee.", order.Consignee.PK,
				contact.RelatedOrgHeader.PK);

			contact = order.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee,
				DocumentDirection.ANY);
			AssertEquals("The contact should be the Consignee.", order.Consignee.PK, contact.OrgHeader.PK);
			AssertNull(contact.RelatedOrgHeader);
		}

		#endregion

		#region TestGetEDocsProviderSupport

		public void TestGetEDocsProviderSupport()
		{
			IEDocsProvider order = (IEDocsProvider)BusinessObject;
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter),
				order.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region TestBusinessContext

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsOrder, DocSupporter.BusinessContext);
		}

		#endregion

		#region TestGetSupportedDataContexts

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true,
				DocSupporter.IsDataContextSupported(
					new DataContextValueForTesting(Core.Constants.DataContext.WhsOrder)));
			AssertEquals(true,
				DocSupporter.IsDataContextSupported(
					new DataContextValueForTesting(Core.Constants.DataContext.WhsPackageLabels)));
			AssertEquals(true,
				DocSupporter.IsDataContextSupported(
					new DataContextValueForTesting(Core.Constants.DataContext.WhsDeliveryLabels)));
			AssertEquals(true,
				DocSupporter.IsDataContextSupported(
					new DataContextValueForTesting(Core.Constants.DataContext.WhsPickableDocket)));
		}

		#endregion

		#region TestGetDocumentWrappers

		public void TestGetDocumentWrappers()
		{
			DocumentWrapper[] orderWrappers =
				DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsOrder, null);
			AssertEquals("DocWhsOrder", orderWrappers[0].GetType().Name);
			AssertEquals(Order, orderWrappers[0].WrappedObject);

			DocumentWrapper[] packageLabelWrappers =
				DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsPackageLabels, null);
			AssertEquals("DocWhsOrder", packageLabelWrappers[0].GetType().Name);
			AssertEquals(Order, orderWrappers[0].WrappedObject);

			DocumentWrapper[] deliveryLabelWrappers =
				DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsDeliveryLabels, null);
			AssertEquals("DocWhsOrder", deliveryLabelWrappers[0].GetType().Name);
			AssertEquals(Order, orderWrappers[0].WrappedObject);

			DocumentWrapper[] pickableDocketWrappers =
				DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsPickableDocket, null);
			AssertEquals("DocWhsPickableDocket", pickableDocketWrappers[0].GetType().Name);
			AssertEquals(Order, pickableDocketWrappers[0].WrappedObject);

			DocumentWrapper[] jobServiceWrappers =
				DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.Service, null);
			AssertEquals("DocWhsOrder", jobServiceWrappers[0].GetType().Name);
			AssertEquals(Order, jobServiceWrappers[0].WrappedObject);
		}

		#endregion

		#region TestDocumentSupporterGetChildCollection

		public void TestDocumentSupporterGetChildCollection()
		{
			#region SetUpData

			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "part3");
			var part4 = Helper.CreateProduct(data.Org1, "part4");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part4, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 5);
			var order1Line3 = Helper.CreateWhsOrderLine(order1, part3, 5);
			var order1Line4 = Helper.CreateWhsOrderLine(order1, part4, 5);
			Factory.Save();

			Helper.CreatePickNew(order1);

			// Package Job 1
			//    1x Box B1
			//    1x Box B2
			//       Part1
			//		 1x Box B4
			//          Part1
			//          Part2
			//          Part3
			//          Part4
			//    1x Box B3
			//       Part2
			//       Part3
			//       Part4

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1OuterPackWith0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");

			var order1OuterPackWith1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B2");
			order1OuterPackWith1.Pack(order1Line1.ReleaseLines[0], 1m);

			var order1OuterPackWith3 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B3");
			order1OuterPackWith3.Pack(order1Line2.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line3.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line4.ReleaseLines[0], 1);

			var order1InnerPackWith4 =
				PackingHelper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith4.Pack(order1Line1.ReleaseLines[0], 1);
			order1InnerPackWith4.Pack(order1Line2.ReleaseLines[0], 1);
			order1InnerPackWith4.Pack(order1Line3.ReleaseLines[0], 1);
			order1InnerPackWith4.Pack(order1Line4.ReleaseLines[0], 1);

			#endregion

			var packingCommand = Factory.New<DocumentCommand>();
			var docSupporter = order1.DocumentSupporter;
			var order1PackingDocuments = docSupporter.GetChildCollection(packingCommand, BusinessContext.Packing, null);
			AssertEquals("Selected Order 1", 1, order1PackingDocuments.Length);
			AssertEquals("Selected Order 1", order1.PackageJob, order1PackingDocuments[0]);
		}

		#endregion

		#region TestDocumentSupporterGetChildCollection_WithOrderThatHasNoPackageJob

		public void TestDocumentSupporterGetChildCollection_WithOrderThatHasNoPackageJob()
		{
			var order = Factory.New<WhsOrder>();
			var childCollection =
				order.DocumentSupporter.GetChildCollection(Factory.New<DocumentCommand>(), BusinessContext.Packing,
					null);
			AssertEquals(0, childCollection.Length);
		}

		#endregion

		#region TestSupportedChildBusinessContexts

		public void TestSupportedChildBusinessContexts()
		{
			AssertEquals("SupportedChildBusinessContexts", 2, DocSupporter.SupportedChildBusinessContexts.Length);
			AssertEquals("SupportedChildBusinessContexts", BusinessContext.Packing,
				DocSupporter.SupportedChildBusinessContexts[0]);
			AssertEquals("SupportedChildBusinessContexts", BusinessContext.DtbBooking,
				DocSupporter.SupportedChildBusinessContexts[1]);
		}

		#endregion

		#region TestGetBODocDataProvidersNotFoundMessageReturnOneReason

		protected override IEnumerable<Tuple<DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs
		{
			get
			{
				var dataContextAndMessagePairs = new List<Tuple<DataContext, string>>();

				dataContextAndMessagePairs.Add(new Tuple<DataContext, string>(Constants.DataContext.WhsOrder,
					"Cannot find Warehouse Order."));
				dataContextAndMessagePairs.Add(new Tuple<DataContext, string>(Constants.DataContext.Service,
					"Cannot find Warehouse Order."));
				dataContextAndMessagePairs.Add(
					new Tuple<DataContext, string>(Constants.DataContext.WhsPickableDocket,
						"Cannot find Warehouse Order."));
				dataContextAndMessagePairs.Add(
					new Tuple<DataContext, string>(Constants.DataContext.GenericFreightJob,
						"Cannot find Warehouse Order."));
				dataContextAndMessagePairs.Add(new Tuple<DataContext, string>(
					Constants.DataContext.WhsPackageLabels, "Cannot find Package Label to print."));
				dataContextAndMessagePairs.Add(new Tuple<DataContext, string>(
					Constants.DataContext.WhsDeliveryLabels, "Cannot find Delivery Label to print."));

				return dataContextAndMessagePairs;
			}
		}

		#endregion

		#region Implementation

		WhsOrder Order
		{
			get { return (WhsOrder)BusinessObject; }
		}

		#region Properties

		protected override DataContext DataContext
		{
			get { return Enterprise.Core.Constants.DataContext.WhsOrder; }
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			WhsOrder order = Factory.New<WhsOrder>();
			order.WD_PackagesSent = 4;
			return order;
		}

		#endregion
	}
}
