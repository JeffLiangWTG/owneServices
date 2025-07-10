using System;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentCartageLegEventArgs : EventArgs
	{
		public DocumentCartageLegEventArgs(DocumentCartageLegOptions documentCartageLegOptions)
		{
			DocumentCartageLegOptions = documentCartageLegOptions;
			ContinueToPrint = true;
		}

		public DocumentCartageLegOptions DocumentCartageLegOptions;
		public bool ContinueToPrint;
	}
}
