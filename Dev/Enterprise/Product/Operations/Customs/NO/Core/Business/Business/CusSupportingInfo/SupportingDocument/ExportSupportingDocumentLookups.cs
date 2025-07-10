namespace Enterprise.Customs.NO.Business
{
	public sealed class ExportSupportingDocumentLookups : SupportingDocumentLookups
	{
		public ExportSupportingDocumentLookups(SupportingDocument parent) : base(parent)
		{
		}

		protected override string ListType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
	}
}
