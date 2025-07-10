using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class will be implemented in the future.")]

	[CodeProperty(nameof(GST_Code))]
	[DescriptionProperty(nameof(GST_Name))]
	public class GlbTeam : AutoGlbTeam
	{
		public GlbTeam(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
