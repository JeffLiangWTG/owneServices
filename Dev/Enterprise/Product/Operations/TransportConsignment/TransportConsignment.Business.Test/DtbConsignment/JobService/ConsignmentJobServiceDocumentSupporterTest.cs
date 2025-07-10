using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(ConsignmentJobServiceDocumentSupporter))]
	sealed class ConsignmentJobServiceDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporter()
		{
			AssertEquals("Document Supporter should be of type", typeof(ConsignmentJobServiceDocumentSupporter), DocumentSupporter.GetType());
		}

		public void TestService()
		{
			var docSupporter = DocumentSupporter;
			AssertEquals("Document Supporter Service should be TestService", DocumentSupporter.PK, docSupporter.Service.PK);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("Core.Constants.DataContext.RequestForService is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LTConsignmentJobService)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobServices)));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.LTCngmntJobService, DocumentSupporter.BusinessContext);
		}

		public void TestGenericFreightJobDocumentWrapper()
		{
			AssertEquals(1, GetDocumentSupportableBusinessObject().DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null).Length);
		}

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

		List<Tuple<Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs
		{
			get
			{
				var dataContextAndMessagePairs = new List<Tuple<Constants.DataContext, string>>();
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.GenericFreightJob, "Cannot find Request For Service Parent."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.LTConsignmentJobService, "Cannot find Request For Service Parent."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.GenericFreightJobServices, "Cannot find Request For Service Parent."));

				return dataContextAndMessagePairs;
			}
		}

		#endregion

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var consignment = Factory.New<DtbConsignment>();
			return consignment.Services.AddNew();
		}

		ConsignmentJobServiceDocumentSupporter documentSupporter;

		public ConsignmentJobServiceDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new ConsignmentJobServiceDocumentSupporter(Factory.New<ConsignmentJobService>())); }
		}

		#endregion
	}
}
