using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefRepairCode for Container Yard project")]
	[CodeProperty(Schema.RMM_Code)]
	public class RefMachineryMake : AutoRefMachineryMake
	{
		public RefMachineryMake(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
