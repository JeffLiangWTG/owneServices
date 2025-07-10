namespace Enterprise.Customs.NO.Business
{
	public sealed class ImportSupportingDocumentLookups : SupportingDocumentLookups
	{
		public ImportSupportingDocumentLookups(SupportingDocument parent) : base(parent)
		{
		}

		protected override string ListType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
	}
}
