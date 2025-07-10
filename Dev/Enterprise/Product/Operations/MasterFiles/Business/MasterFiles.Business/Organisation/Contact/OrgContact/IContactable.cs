
using CargoWise.Types;
using Enterprise.Core.Environment;

namespace Enterprise.MasterFiles.Business
{
	public interface IContactable : IContactBase
	{
		ZGuid PK { get; }
		string Mobile { get; }
		bool IsActive { get; }
		IContactable[] GetNestedContacts(string parentContactDescription);
	}
}
