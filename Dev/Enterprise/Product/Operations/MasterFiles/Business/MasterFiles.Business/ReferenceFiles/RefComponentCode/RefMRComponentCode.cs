using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefMRComponentCode for Container Yard project")]
	[CodeProperty(Schema.RCC_Code)]
	public class RefMRComponentCode : AutoRefMRComponentCode
	{
		public RefMRComponentCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
