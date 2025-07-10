
namespace Enterprise.Customs.NZ.Business
{
	public partial class RelationshipIndicatorList
	{
		public RelationshipIndicatorList(bool isTSWDeclaration)
		{
			AddPair(Codes.NotRelated, Descriptions.NotRelated);
			AddPair(Codes.Related, Descriptions.Related);
			if (isTSWDeclaration)
			{
				AddPair(Codes.RelatedDoesNotAffectPrice, Descriptions.RelatedDoesNotAffectPrice);
			}
		}
	}
}
