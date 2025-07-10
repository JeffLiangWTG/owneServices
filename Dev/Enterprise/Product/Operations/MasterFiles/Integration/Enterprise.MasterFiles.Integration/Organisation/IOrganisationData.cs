using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrganisationData
	{
		ZGuid PK { get; }
		ZString Code { get; set; }
		ZString FullName { get; set; }
		ZString Address1 { get; set; }
		ZString Address2 { get; set; }
		ZString UNLOCO { get; set; }
		ZString Phone { get; set; }
		ZString Mobile { get; set; }
		ZString Fax { get; set; }
		ZString Email { get; set; }
		ZString Web { get; set; }
		ZString City { get; set; }
		ZString State { get; set; }
		ZString Postcode { get; set; }
	}
}