using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefDamage for Container Yard project")]
	[CodeProperty(Schema.RFM_Code)]
	public class RefDamage : AutoRefDamage
	{
		public RefDamage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
