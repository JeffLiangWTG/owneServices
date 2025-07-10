using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.USCFIRMS)]
	[CodeAlive("This Business Object would delete in next workflow.")]
	public class USCFirmsCollection : ActiveBusinessObjectCollection<USCFIRMS>
	{
		public USCFirmsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
