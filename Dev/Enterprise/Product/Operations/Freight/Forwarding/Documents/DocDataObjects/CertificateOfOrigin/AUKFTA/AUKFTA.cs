using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class AUKFTA : CertificateOfOriginDocDataObject<AUKFTALineItem>
	{
		public AUKFTA(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}

		#region DeclarationCompletedBy

		public DeclarationCompletionFlag DeclarationCompletedBy
		{
			get => declarationCompletedBy;
			set => declarationCompletedBy = SetChild(declarationCompletedBy, value);
		}

		DeclarationCompletionFlag declarationCompletedBy;

		#endregion
	}
}
