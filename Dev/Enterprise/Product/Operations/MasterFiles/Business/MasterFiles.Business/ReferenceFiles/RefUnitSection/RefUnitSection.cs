using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject RefUnitSection for Container Yard project")]
	[CodeProperty(Schema.RUS_Code)]
	public class RefUnitSection : AutoRefUnitSection
	{
		public RefUnitSection(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString GroupCode
		{
			get { return RUS_Code.Substring(0, 2); }
		}
	}
}
