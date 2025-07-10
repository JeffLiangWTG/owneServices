using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IMessageTemplateApplicationScopeEnforcerProvider
	{
		void Track(BusinessObjectFactory factory, ZGuid guid);
	}
}
