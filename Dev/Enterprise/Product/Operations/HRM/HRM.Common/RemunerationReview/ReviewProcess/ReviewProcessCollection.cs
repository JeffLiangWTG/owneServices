using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Common
{
	[ModuleID(ModuleId.ReviewProcess)]
	public class ReviewProcessCollection : ActiveBusinessObjectCollection<ReviewProcess>
	{
		public ReviewProcessCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
