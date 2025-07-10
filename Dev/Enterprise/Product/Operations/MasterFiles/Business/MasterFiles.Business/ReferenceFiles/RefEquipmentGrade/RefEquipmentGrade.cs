using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefEquipmentGrade for Container Yard project")]
	[CodeProperty(Schema.REG_Code)]
	public class RefEquipmentGrade : AutoRefEquipmentGrade
	{
		public RefEquipmentGrade(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
