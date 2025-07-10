
using System;

namespace Enterprise.Customs.Business
{
	public delegate void CusContainersToPrintEventHandler(object sender, CusContainersToPrintEventArgs e);

	public class CusContainersToPrintEventArgs : EventArgs
	{
		public CusContainersToPrintEventArgs(DocumentCusContainerCollectionHeader documentCusContainerCollectionHeader)
		{
			this.DocumentCusContainerCollectionHeader = documentCusContainerCollectionHeader;
			ContinueToPrint = true;
		}

		public readonly DocumentCusContainerCollectionHeader DocumentCusContainerCollectionHeader;
		public bool ContinueToPrint;
	}
}
