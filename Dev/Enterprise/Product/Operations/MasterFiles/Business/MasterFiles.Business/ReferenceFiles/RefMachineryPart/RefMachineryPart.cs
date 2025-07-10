using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefMachineryPart for Container Yard project")]
	[CodeProperty(Schema.RMP_PartNumber)]
	[DescriptionProperty(Schema.RMP_Description)]
	public class RefMachineryPart : AutoRefMachineryPart
	{
		public RefMachineryPart(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
