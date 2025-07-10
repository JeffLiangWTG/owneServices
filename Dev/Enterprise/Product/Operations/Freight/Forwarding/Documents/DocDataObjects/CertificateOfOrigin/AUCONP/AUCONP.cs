using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class AUCONP : CertificateOfOriginDocDataObject<AUCONPLineItem>
	{
		public AUCONP(ZString sourceType, ZString sourceID) : base(sourceType, sourceID) { }

		#region ExporterReference

		public ZString ExporterReference
		{
			get => exporterReference;
			set
			{
				if (SetNonPersistentPropertyValue(ExporterReferenceInfo, ref exporterReference, value))
				{
					Validate(ExporterReferenceInfo);
				}
			}
		}
		ZString exporterReference;

		public ZPropertyInfo ExporterReferenceInfo => GetZPropertyInfo(nameof(ExporterReference));

		#endregion
	}
}
