using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business
{
	public interface ICusUnderbondMessageSender
	{
		ICusUnderbondDependentCollectionParent GetProviderToAddUnderbondTo(ICusUnderbondDependentCollectionParent[] allPossibleParents);
	}
}
