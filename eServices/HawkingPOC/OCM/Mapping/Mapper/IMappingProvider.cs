using OcmPoc.Mapping.Interface;

namespace OcmPoc.Mapping.Mapper
{
	interface IMappingProvider
	{
		IMapping GetMapping(string name);
	}
}