using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefRefrigerantType for Container Yard project")]
	[CodeProperty(Schema.RRT_Code)]
	public class RefRefrigerantType : AutoRefRefrigerantType
	{
		public RefRefrigerantType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
