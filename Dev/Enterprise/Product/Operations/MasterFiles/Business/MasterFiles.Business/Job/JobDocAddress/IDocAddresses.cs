using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	public interface IDocAddresses
	{
		JobDocAddressDependentCollection DocAddresses { get; }
		ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate);
		SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress);
		IReadOnlyList<DocAddressType> SupportedAddressTypes { get; }
		JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType);
		void DocAddressChanged(JobDocAddress docAddress);
		void OrgAddressBeforeChange(JobDocAddress docAddress);
		void OnBeforeDocAddressDeleted(JobDocAddress docAddress);
		void AnyAddressFieldBeforeChange(JobDocAddress docAddress);
		void OrgHeaderAfterChange(JobDocAddress docAddress);
		ZString HumanReadableName { get; }
		bool CanDeleteAddress(JobDocAddress docAddress);
		OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType);
	}

	public interface IDocAddressesCaption
	{
		ZString GetAddressCaption(JobDocAddress docAddress);
	}

	public interface IDocAddressesObsolete
	{
		JobDocAddress GetDocAddress(DocAddressType addressType);
	}
}


