using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefRepairCode for Container Yard project")]
	[CodeProperty(Schema.RRC_Code)]
	public class RefRepairCode : AutoRefRepairCode
	{
		public RefRepairCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
