using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business
{
	public sealed class ReferenceFileUrlProvider
	{
		public ReferenceFileUrlProvider(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		const string configKey = "AURefURL";

		readonly BusinessObjectFactory factory;

		public ZString GetCustomsReferenceFileURL()
		{
			return SysConfigLoader.GetStringValue(configKey);
		}

		IRefSysConfigLoader SysConfigLoader
		{
			get
			{
				if (sysConfigLoader == null)
				{
					sysConfigLoader = ObjectFactory.Get<IRefSysConfigLoader>(nameof(IRefSysConfigLoader), factory);
				}
				return sysConfigLoader;
			}
		}
		IRefSysConfigLoader sysConfigLoader;
	}
}
