using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.Testing
{
	abstract class OCBEventProcessorTest : TestCaseWithFactory
	{
		public void TestAddOCBEvent()
		{
			var consol = Consol;
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNull(ocb);

			AddLog(documentData, Events.MessageSent);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNotNull(ocb);
			AssertEquals(ExpectedOrgMessage, ocb.SL_Reference);

			UpdateConsol();
			AddLog(documentData, Events.MessageSent);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertEquals(ExpectedAmdMessage, ocb.SL_Reference);

			AddLog(documentData, Events.MessageWithdrawCancelRequest);
			supporter.GetMessageEventsProcessor(Document).OnMessageWithdrawalSent();

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertEquals(ExpectedWthMessage, ocb.SL_Reference);

			Thread.Sleep(100);
			Factory.Save();

			var parameters = new List<KeyValuePair<string, string>>();

			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "Test_Type"));
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, decimal.MaxValue.ToString()));
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, "1.0"));
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Maximum, decimal.MaxValue.ToString()));
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Quantity, "1.0"));
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, "ORG"));
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Company, "Test_CMP"));

			consol.Logs.CreateOrRecreateEventLog(Events.OceanCarrierBookingByTEU, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());
			Factory.Save();
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			CombineAssertions(() =>
			{
				AssertEquals(decimal.MaxValue.ToString(), ocb.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Maximum));
				AssertEquals(decimal.MaxValue.ToString(), ocb.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old));
			});
		}

		public void TestAddOCBEvent_DocumentDelivery()
		{
			var consol = Consol;
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetPrintEventsProcessor(Document).OnPrintJobsCreated(null);

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNull(ocb);

			AddLog(documentData, Events.MessageSent);
			supporter.GetPrintEventsProcessor(Document).OnPrintJobsCreated(null);

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNotNull(ocb);
			AssertEquals(ExpectedDocumentDeliveryMessage, ocb.SL_Reference);
		}

		#region Implementation

		protected ForwardingConsol Consol => consol ?? (consol = CreateConsol());
		ForwardingConsol consol;

		protected abstract ForwardingConsol CreateConsol();
		protected abstract void UpdateConsol();
		protected abstract DocDataObject DocDataObject { get; }
		protected abstract string Context { get; }
		protected abstract string DocumentName { get; }
		protected abstract string DataStoreName { get; }
		protected abstract string ExpectedOrgMessage { get; }
		protected abstract string ExpectedAmdMessage { get; }
		protected abstract string ExpectedWthMessage { get; }
		protected abstract string ExpectedDocumentDeliveryMessage { get; }

		protected IDocument Document => document ?? (document = GetDocumentData());
		IDocument document;

		protected IDocument GetDocumentData()
		{
			var dynamicData = DocDataObject.MakeDynamic();
			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(DocumentName);
			document.SetupGet(d => d.DataContext).Returns(Context);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			return document.Object;
		}

		public void AddLog(VisualizerDocumentData documentData, Event @event)
		{
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DocumentName)
			};

			documentData.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParameters);

			Thread.Sleep(100);
			Factory.Save();
		}

		#endregion
	}
}
