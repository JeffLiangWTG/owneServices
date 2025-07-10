using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class PGARecapPrintSigned : ISEAdditionalData
	{
		public PGARecapPrintSigned(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				US_CertifyCargoRelease = true;
			}
		}

		public ZBool US_CertifyCargoRelease { get; set; }
		public ZBool US_AcknowledgeAndSign { get; set; }
		public ZDateTime US_DateOfDeclaration { get; set; }
		public ZBool CertifyTIB { get; set; }
		public ZString ContactName { get; }
		public ZString ContactPhone { get; }
		public ZString ReasonCode { get; }
		public ZString MultipleCargoDispositionsIndicator { get; }
		public ZString ReferenceIdentifier { get; }
		public ZString ReferenceIdentifierQualifier { get; }
		public ZBool DISIndicator { get; }
		public ZString DISIDRefNo { get; }
	}
}
