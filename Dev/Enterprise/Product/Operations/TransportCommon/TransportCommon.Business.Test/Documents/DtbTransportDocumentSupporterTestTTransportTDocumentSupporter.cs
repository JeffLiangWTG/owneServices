using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportDocumentSupporterTest<TTransport, TDocumentSupporter> : DocumentSupporterTest
			where TTransport : DtbTransport
			where TDocumentSupporter : DocumentSupporter
	{
		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals(ExpectedBusinessContext, DocumentSupporter.BusinessContext);
		}

		protected abstract BusinessContext ExpectedBusinessContext { get; }

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(ExpectedSecurityCheckpoint, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		protected abstract SecurityCheckpoint ExpectedSecurityCheckpoint { get; }

		#endregion

		#region TestGetSupportedDataContexts

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
			TestGetSupportedDataContextsCore();
		}

		protected virtual void TestGetSupportedDataContextsCore()
		{
		}

		#endregion

		#region TestGetDocumentWrappersInternal

		public void TestGetDocumentWrappersInternal()
		{
			var supporter = DocumentSupporter;
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Some Document";

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			var wrappersForSomeGenericFreightJobDocument = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, stmMenuItem);
			AssertEquals("We print document for 1 Booking, so 1 document wrapper should be created.", 1, wrappersForSomeGenericFreightJobDocument.Length);

			var wrappersForCartageAdviceGenericFreightJobDocument = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, cartageAdviceMenuItem);
			AssertEquals("We print document for 1 Booking, so 1 document wrapper should be created.", 1, wrappersForCartageAdviceGenericFreightJobDocument.Length);

			TestGetDocumentWrappersInternalCore(supporter, stmMenuItem);
		}

		protected virtual void TestGetDocumentWrappersInternalCore(TDocumentSupporter documentSupporter, StmMenuItem menuItem)
		{
		}

		#endregion

		#region TestShowReasonForNotPrinting

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals("ShowReasonForNotPrinting", GetShowReasonForNotPrintingCore(), DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		protected virtual bool GetShowReasonForNotPrintingCore()
		{
			return false;
		}

		#endregion

		#region TestGetBODocDataProvidersNotFoundMessageReturnOneReason

		public void TestGetBODocDataProvidersNotFoundMessageReturnOneReason()
		{
			foreach (var dataContextAndMessage in SupportedDataContextAndNotFoundMessageReasonPairs)
			{
				var dataContext = dataContextAndMessage.Item1;
				var reasonMessage = dataContextAndMessage.Item2;
				var notFoundMessage = DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(dataContext.ToString()), null);
				if (DocumentSupporter.ShowReasonForNotPrinting(dataContext, null))
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

		protected virtual IEnumerable<Tuple<Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs => Enumerable.Empty<Tuple<Constants.DataContext, string>>();

		#endregion

		#region Implementation

		#region GetDocumentSupportableBusinessObject

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return ExpectedBizO;
		}

		#endregion

		#region DocumentSupporter

		protected TDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = (TDocumentSupporter)((IDocumentSupportable)ExpectedBizO).DocumentSupporter); }
		}
		TDocumentSupporter documentSupporter;

		#endregion

		#region ExpectedBizO

		protected TTransport ExpectedBizO
		{
			get { return expectedBizO ?? (expectedBizO = GetExpectedBizOWithValidData()); }
		}
		TTransport expectedBizO;

		protected abstract TTransport GetExpectedBizOWithValidData();

		#endregion

		#endregion
	}
}
