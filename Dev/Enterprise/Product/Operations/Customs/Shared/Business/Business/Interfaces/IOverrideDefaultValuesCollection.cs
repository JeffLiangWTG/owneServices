using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface IOverrideDefaultValuesCollection
	{
		bool IsOverrideDefaultValuesEnabled { get; }
		ZPropertyInfoBool OverrideDefaultValuesInfo { get; }
	}
}
