using eServices.Configuration.Schemas;

namespace eServices.Configuration.Framework
{
	public interface IConfigurationHandler
	{
		ConfigurationMessage Get(ConfigurationMessage configuration);
		bool TryGet(ConfigurationMessage criteria, out ConfigurationMessage configuration);
		void AddOrUpdate(ConfigurationMessage configuration);
        string SenderId { get; set; }
    }
}
