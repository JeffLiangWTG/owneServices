using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveASNDocumentSupporter))]
	class WhsItemReceiveASNDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var receiveExpectedPackages = GetDocumentSupportableBusinessObject();
			var docSupporter = receiveExpectedPackages.DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", true, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		#endregion

		#region TestGetBODocDataProvidersNotFoundMessageReturnOneReason

		public void TestGetBODocDataProvidersNotFoundMessageReturnOneReason()
		{
			var receiveASN = GetDocumentSupportableBusinessObject();
			var docSupporter = receiveASN.DocumentSupporter;
			foreach (var dataContextAndMessage in SupportedDataContextAndNotFoundMessageReasonPairs)
			{
				var dataContext = dataContextAndMessage.Item1;
				var reasonMessage = dataContextAndMessage.Item2;
				var notFoundMessage = docSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(dataContext.ToString()), null);
				if (docSupporter.ShowReasonForNotPrinting(dataContext, null))
				{
					AssertEquals($"Should returns a reason when attempt to print without underlying data. (DataContext: {dataContext})", reasonMessage, notFoundMessage);
				}
				else
				{
					Assert(true);
				}
			}
			Assert(true);
		}

		List<Tuple<Core.Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs
		{
			get
			{
				var dataContextAndMessagePairs = new List<Tuple<Core.Constants.DataContext, string>>();
				dataContextAndMessagePairs.Add(new Tuple<Core.Constants.DataContext, string>(Core.Constants.DataContext.GenericFreightJob, "Cannot find Warehouse Receive ASNs."));

				return dataContextAndMessagePairs;
			}
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var docSupporter = ((IDocumentSupportable)asn).DocumentSupporter;
			AssertEquals(Env.Security.WhsItemReceiveASNCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestDocumentSupporter_SupportedChildBusinessContexts

		public void TestDocumentSupporter_SupportedChildBusinessContexts()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var docSupporter = ((IDocumentSupportable)asn).DocumentSupporter;
			AssertContainsExactElementsInAnyOrder(new BusinessContext[] { BusinessContext.TransitRcvConsignmnt, BusinessContext.TransitRecTranspUnt }, docSupporter.SupportedChildBusinessContexts);
		}

		#endregion

		#region TestGetChildCollection

		public void TestGetChildCollection()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);

			var rcnMenuItem = Factory.New<StmMenuItem>();
			rcnMenuItem.SU_MenuName = "RCN Menu";

			var asn = Helper.CreateReceiveASN("ASN1", data.Whs1.PK);
			var rcn1 = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", data.Whs1.PK);
			var p1 = Helper.CreatePackageState(rcn1, 1, "PKG", "", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			var p2 = Helper.CreatePackageState(rcn1, 2, "PKG", "", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			var p3 = Helper.CreatePackageState(rcn2, 3, "PLT", "", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			var docSupporter = ((IDocumentSupportable)asn).DocumentSupporter;
			var childCollection_2RCNs = docSupporter.GetChildCollection(rcnMenuItem, BusinessContext.TransitRcvConsignmnt, null);
			AssertContainsExactElementsInAnyOrder(new[] { rcn1, rcn2 }, childCollection_2RCNs);

			var childCollection_NoRCNs = Factory.New<WhsItemReceiveASN>().DocumentSupporter.GetChildCollection(rcnMenuItem, BusinessContext.TransitRcvConsignmnt, null);
			AssertEquals("There are no RCN's on this ASN.", 0, childCollection_NoRCNs.Length);
		}

		#endregion

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var asn = Helper.CreateReceiveASN("ASN1", data.Whs1.PK);
			var rcn1 = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK);
			var p1 = Helper.CreatePackageState(rcn1, 1, "PKG", "", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);

			return asn;
		}

		WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		#endregion
	}
}
