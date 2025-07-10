using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class NMFS370Document : INMFSDocument
	{
		public NMFS370Document(ZString documentIdentifier, ZString documentNumber)
		{
			this.documentIdentifier = documentIdentifier;
			this.documentNumber = documentNumber;
		}

		readonly ZString documentIdentifier;
		readonly ZString documentNumber;

		#region INMFSDocument Members

		ZString INMFSDocument.DocumentIdentifier
		{
			get { return documentIdentifier; }
		}

		ZString INMFSDocument.DocumentNumber
		{
			get { return documentNumber; }
		}

		#endregion
	}
}
