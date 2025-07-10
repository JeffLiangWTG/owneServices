using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	public sealed class ACEEntrySummaryMessageBuilderForTesting : ACEEntrySummaryMessageBuilder
	{
		public ACEEntrySummaryMessageBuilderForTesting(IACECusEntryHeader entryHeader, ZBool cargoReleaseCertified, ZBool signed, UpdateActionCode action)
			: base(entryHeader, GetIAcknowledgeAndSignActionObject(cargoReleaseCertified, signed), action)
		{
		}

		public ACEEntrySummaryMessageBuilderForTesting(IACECusEntryHeader entryHeader, ZBool certifyTIB)
			: base(entryHeader, GetIAcknowledgeAndSignActionObject(false, true, certifyTIB), UpdateActionCode.Add)
		{
		}

		static ISEAdditionalData GetIAcknowledgeAndSignActionObject(bool cargoReleaseCertified, bool signed, bool certifyTIB = false)
		{
			var mock = new Mock<ISEAdditionalData>();
			mock.Setup(m => m.US_CertifyCargoRelease).Returns(cargoReleaseCertified);
			mock.Setup(m => m.US_AcknowledgeAndSign).Returns(signed);
			mock.Setup(m => m.US_DateOfDeclaration).Returns(ZDateTime.Today);
			mock.Setup(m => m.ContactName).Returns("Support");
			mock.Setup(m => m.ContactPhone).Returns("123456");
			mock.Setup(m => m.ReasonCode).Returns(ZString.Empty);
			mock.Setup(m => m.MultipleCargoDispositionsIndicator).Returns(ZString.Empty);
			mock.Setup(m => m.ReferenceIdentifier).Returns(ZString.Empty);
			mock.Setup(m => m.ReferenceIdentifierQualifier).Returns(ZString.Empty);
			mock.Setup(m => m.DISIndicator).Returns(new ZBool(ZString.Empty));
			mock.Setup(m => m.DISIDRefNo).Returns(ZString.Empty);
			mock.Setup(m => m.CertifyTIB).Returns(certifyTIB);
			return mock.Object;
		}
	}
}
