using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationPartyConfigCollection : BusinessObjectCollection<EDICommunicationPartyConfig>
	{
		public EDICommunicationPartyConfigCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EDICommunicationPartyConfigCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
