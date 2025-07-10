using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.EDICommunicationParty)]
	public class EDICommunicationPartyCollection : BusinessObjectCollection<EDICommunicationParty>
	{
		public EDICommunicationPartyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EDICommunicationPartyCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
