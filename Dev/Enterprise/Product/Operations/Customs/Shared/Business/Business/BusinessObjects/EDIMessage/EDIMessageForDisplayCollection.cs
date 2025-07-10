using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public class EDIMessageForDisplayCollection<T> : BusinessObjectCollection<T>, IBusinessObjectCollection
		where T : EDIMessage
	{
		public EDIMessageForDisplayCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
