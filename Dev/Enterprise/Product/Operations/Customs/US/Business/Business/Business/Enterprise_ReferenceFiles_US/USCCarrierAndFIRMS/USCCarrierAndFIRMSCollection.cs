using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.USCCarrierAndFIRMS)]
	public class USCCarrierAndFIRMSCollection : BusinessObjectCollection<USCCarrierAndFIRMS>
	{
		public USCCarrierAndFIRMSCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
