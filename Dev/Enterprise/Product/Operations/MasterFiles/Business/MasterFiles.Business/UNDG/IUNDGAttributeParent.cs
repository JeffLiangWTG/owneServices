using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IUNDGAttributeParent
	{
		ZString TableCode { get; }
		ZGuid PK { get; }
		ZString DetailsLanguage { get; }
		ZPropertyInfo DetailsLanguageInfo { get; }
	}
}
