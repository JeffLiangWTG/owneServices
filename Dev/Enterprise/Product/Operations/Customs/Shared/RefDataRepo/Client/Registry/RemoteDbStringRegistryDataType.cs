using System;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class RemoteDbStringRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (registryItem.Name == "RemoteDatabaseServiceUri")
			{
				ValidateRemoteDatabaseServiceUri(registryItem, proposedValue);
			}
			if (registryItem.Name == "SingleRefDatabaseName")
			{
				ValidateSingleRefDatabaseName(registryItem, proposedValue);
			}
		}

		void ValidateRemoteDatabaseServiceUri(IRegistryItem registryItem, string proposedValue)
		{
			if (!proposedValue.Equals(registryItem.DefaultValue.ToString(), StringComparison.InvariantCultureIgnoreCase)
				&& RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.Value.Equals(RefDbTableNameResolver.DefaultSingleRefDbName, StringComparison.OrdinalIgnoreCase))
			{
				throw new RegistryValidationException((NoResString)@"Changing Service Uri while having SingleRefDatabaseName still point to default might result in non-production data downloaded to shared db Single Reference Database, which affects other CW1 systems on the same sql server instance.
Changing Service Uri is only allowed when this CW1 instance uses unique reference db other than Single Reference Database's default name");
			}
		}

		void ValidateSingleRefDatabaseName(IRegistryItem registryItem, string proposedValue)
		{
			var uriRegistryItem = RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri;
			if (!uriRegistryItem.Value.Equals(uriRegistryItem.DefaultValue, StringComparison.InvariantCultureIgnoreCase)
				&& proposedValue.Equals(RefDbTableNameResolver.DefaultSingleRefDbName, StringComparison.OrdinalIgnoreCase))
			{
				throw new RegistryValidationException((NoResString)@"Changing SingleRefDatabaseName to default while having Service Uri pointing not to default might result in non-production data downloaded to shared db Single Reference Database, which affects other CW1 systems on the same sql server instance.
Changing SingleRefDatabaseName to default is only allowed when Service Uri pointing to default");
			}
		}
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
