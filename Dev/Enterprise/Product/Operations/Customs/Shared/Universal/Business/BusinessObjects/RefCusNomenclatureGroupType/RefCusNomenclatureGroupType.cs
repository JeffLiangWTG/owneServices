using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	[CodeAlive("Will be used in the future")]
	public sealed class RefCusNomenclatureGroupType : AutoRefCusNomenclatureGroupType
	{
		public RefCusNomenclatureGroupType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
