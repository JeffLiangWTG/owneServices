using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SupportingDocSendingObjectForTest : SupportingDocSendingObject
	{
		public SupportingDocSendingObjectForTest(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString DocumentTypeCode => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocument;
	}
}
