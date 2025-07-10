using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IADLinkedEntity : IBusiness
	{
		bool IsADIntegrationEnabled { get; }
		bool IsActive { get; }

		ZBool IsADLinked { get; }
		ZPropertyInfo IsADLinkedInfo { get; }

		bool IsADLinkable { get; }

		ZString DomainName { get; set; }
		ZPropertyInfo DomainNameInfo { get; }

		ZDateTime SystemCreateTimeUtc { get; }
		ZDateTime SystemLastEditTimeUtc { get; }

		void SynchroniseWithAD();
		void DisconnectFromAD();
	}
}
