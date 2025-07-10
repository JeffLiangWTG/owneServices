using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class RefLanguageTypeCollection : ActiveBusinessObjectCollection<RefLanguageType>
	{
		public RefLanguageTypeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
