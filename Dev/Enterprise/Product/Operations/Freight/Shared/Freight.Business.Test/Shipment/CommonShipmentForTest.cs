using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonShipmentForTest : CommonShipment, IDocumentSupportable
	{
		public CommonShipmentForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Func<ZBool> BOLPrintingShouldBeConfirmedImplementation { get; set; }
		public override ZBool AWBOrHBLPrintingShouldBeConfirmed()
		{
			return BOLPrintingShouldBeConfirmedImplementation != null ? BOLPrintingShouldBeConfirmedImplementation() : ZBool.True;
		}

		public ContainerNonDependentCollection SelectedContainersToPrintForTest
		{
			get { return ((CommonShipmentDocumentSupporterForTest)DocumentSupporter).SelectedContainersToPrintTest; }
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return fDocumentSupporter ?? (fDocumentSupporter = new CommonShipmentDocumentSupporterForTest(this)); }
		}
		DocumentSupporter fDocumentSupporter;

		public void DocumentPrintedEvent(object sender, DocumentPrintedEventArgs e)
		{
			((CommonShipmentDocumentSupporterForTest)DocumentSupporter).DocumentEventSource_DocumentPrintedTestMethod(sender, e);
		}

		public IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlersForTest
		{
			get { return UniqueIndexFailureHandlers; }
		}

		public bool NeedsHouseBillForTest
		{
			get { return NeedsHouseBill; }
		}

		public void ProcessLog(IStmALog log)
		{
			ProcessLogCore(log);
		}

		protected override bool IsPropertyReadOnlyDueToPhaseCore(ZString propertyName)
		{
			return base.IsPropertyReadOnlyDueToPhaseCore(propertyName) || PropertiesForcedToReadOnlyDueToPhase.Contains(propertyName);
		}

		public List<string> PropertiesForcedToReadOnlyDueToPhase = new List<string>();
	}
}
