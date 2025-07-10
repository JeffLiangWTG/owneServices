using System;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentContainerEventArgs : EventArgs
	{
		public DocumentContainerEventArgs(DocumentContainerOptions documentContainerOptions)
		{
			DocumentContainerOptions = documentContainerOptions;
			ContinueToPrint = true;
		}

		public DocumentContainerOptions DocumentContainerOptions;
		public bool ContinueToPrint;
	}
}
