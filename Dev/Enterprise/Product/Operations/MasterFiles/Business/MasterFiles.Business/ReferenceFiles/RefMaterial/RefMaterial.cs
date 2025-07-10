using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefMaterial for Container Yard project")]
	[CodeProperty(Schema.RMC_Code)]
	public class RefMaterial : AutoRefMaterial
	{
		public RefMaterial(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
