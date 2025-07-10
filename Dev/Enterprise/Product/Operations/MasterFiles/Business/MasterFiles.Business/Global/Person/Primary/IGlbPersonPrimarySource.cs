using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IGlbPersonPrimarySource
	{
		ZGuid PK { get; }
		ZGuid PersonPK { get; }
		ZString City { get; }
		ZString State { get; }
		ZString Country { get; }
		ZString UNLOCO { get; }
		ZString CompanyName { get; }
		Guid CompanyPKForLogin { get; }
		ZString JobTitle { get; }
		string TableCode { get; }
		ZString Name { get; }
		ZString Email { get; }
		ZString WorkNumber { get; }
		ZString WorkExtension { get; }
		ZString MobileNumber { get; }
		ZString JobCategory { get; }
		ZString Code { get; }
	}
}
