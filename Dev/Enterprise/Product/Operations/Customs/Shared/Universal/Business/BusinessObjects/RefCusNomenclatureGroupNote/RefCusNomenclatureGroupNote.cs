using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	[CodeAlive("Will be used in the future")]
	public sealed class RefCusNomenclatureGroupNote : AutoRefCusNomenclatureGroupNote
	{
		public RefCusNomenclatureGroupNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
