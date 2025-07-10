using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	public class DocumentEventsForTest : IDocumentEvents
	{
		public DocumentEventsForTest()
		{
			BOLMenu = new BusinessObjectFactory().New<StmMenuItem>();
			BOLMenu.SU_MenuName = "Bill Of Lading XYZ";
			EventArgs = new DocumentCancelEventArgs(BOLMenu);
		}

		public DocumentCancelEventArgs EventArgs;
		public StmMenuItem BOLMenu;

		public void FireDocumentPrintRequested(DocumentCancelEventArgs eventArgs)
		{
			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(this, eventArgs);
			}
		}

		public void FireDocumentPrinted(DocumentPrintedEventArgs eventArgs)
		{
			if (DocumentPrinted != null)
			{
				DocumentPrinted(this, eventArgs);
			}
		}

		public void FireDocumentPrePreviewed(DocumentPrintedEventArgs eventArgs)
		{
			DocumentPrePreviewed?.Invoke(this, eventArgs);
		}

		public void FireDocumentPrePrinted(DocumentPrintedEventArgs eventArgs)
		{
			if (DocumentPrePrinted != null)
			{
				DocumentPrePrinted(this, eventArgs);
			}
		}

		#region IDocumentEvents Members

		public event DocumentCancelEventHandler DocumentPrintRequested;
		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public event DocumentPrintedEventHandler DocumentPrePrinted;

		public event DocumentPrintedEventHandler DocumentPrinted;

		#endregion
	}
}
