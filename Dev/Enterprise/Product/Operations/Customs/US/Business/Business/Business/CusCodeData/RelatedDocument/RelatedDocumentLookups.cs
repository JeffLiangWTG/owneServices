namespace Enterprise.Customs.US.Business
{
	public class RelatedDocumentLookups : Customs.Business.CusCodeDataLookups
	{
		public RelatedDocumentLookups(RelatedDocument parent)
			: base(parent)
		{
		}

		public override ZArchitecture.Core.CodeDescriptionPairList CY_CodeList
		{
			get { return new RelatedDocumentIdentifierList(); }
		}
	}
}
