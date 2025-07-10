using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface IAddInfoWithConcurrencyResolverSupporter : IAddInfoManager
	{
		BaseAddInfo NewAddInfoBizObj(ZPropertyInfo addInfoProperty);

		ZPropertyInfo NotificationAddInfo { get; }
	}
}
