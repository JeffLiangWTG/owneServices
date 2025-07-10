using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	class DocumentEventsForTesting : IDocumentEvents
	{
		public void RaiseDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(sender, e);
			}
		}

		public void RaiseDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
			DocumentPrePreviewed?.Invoke(sender, e);
		}

		public void RaiseDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (DocumentPrePrinted != null)
			{
				DocumentPrePrinted(sender, e);
			}
		}

		public void RaiseDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (DocumentPrinted != null)
			{
				DocumentPrinted(sender, e);
			}
		}

		public event DocumentCancelEventHandler DocumentPrintRequested;
		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public event DocumentPrintedEventHandler DocumentPrePrinted;
		public event DocumentPrintedEventHandler DocumentPrinted;
	}
}
