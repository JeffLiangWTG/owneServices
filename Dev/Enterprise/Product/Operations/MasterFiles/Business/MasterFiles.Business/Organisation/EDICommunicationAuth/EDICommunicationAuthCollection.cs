using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationAuthCollection : BusinessObjectCollection<EDICommunicationAuth>
	{
		public EDICommunicationAuthCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
