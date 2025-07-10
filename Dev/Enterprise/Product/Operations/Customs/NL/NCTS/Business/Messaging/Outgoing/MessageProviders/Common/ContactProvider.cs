using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class ContactProvider : IContact
{
	public static ContactProvider NewOrNull(JobDocAddress jobDocAddress)
	{
		return jobDocAddress != null ? new ContactProvider(jobDocAddress) : null;
	}
	readonly JobDocAddress jobDocAddress;

	ContactProvider(JobDocAddress jobDocAddress)
	{
		this.jobDocAddress = jobDocAddress;
	}

	public string Name
	{
		get
		{
			if (IsContactSelected)
			{
				return jobDocAddress.E2_Contact;
			}
			else
			{
				return ContactFromOrganisationWithAllocationCus?.OC_ContactName ?? ZString.Empty;
			}
		}
	}

	public string PhoneNumber
	{
		get
		{
			if (IsContactSelected)
			{
				if (jobDocAddress.E2_Phone.IsEmpty)
				{
					return ContactSelected.OC_Phone;
				}
				else
				{
					return jobDocAddress.E2_Phone;
				}
			}
			else
			{
				 return ContactFromOrganisationWithAllocationCus?.OC_Phone ?? ZString.Empty;
			}
		}
	}

	public string EMailAddress
	{
		get
		{
			if (IsContactSelected)
			{
				if (jobDocAddress.E2_Email.IsEmpty)
				{
					return ContactSelected.OC_Email;
				}
				else
				{
					return jobDocAddress.E2_Email;
				}
			}
			else
			{
				return ContactFromOrganisationWithAllocationCus?.OC_Email ?? ZString.Empty;
			}
		}
	}

	public IReadOnlyCollection<ICommunication> Communications => Array.Empty<ICommunication>();

	bool IsContactSelected => CachedValueHelper.GetValue(ref isContactSelected, () => ContactSelected != null);
	CachedValue<bool> isContactSelected;

	OrgContact ContactSelected => CachedValueHelper.GetValue(ref contactSelected, () => jobDocAddress.Contact);
	CachedValue<OrgContact> contactSelected;

	OrgContact ContactFromOrganisationWithAllocationCus => CachedValueHelper.GetValue(ref contactFromOrganisationWithAllocationCus, () => jobDocAddress.Organisation?.GetActiveContacts().Cast<OrgContact>().FirstOrDefault(c => c.Allocations.Cast<OrgContactAllocation>().Any(a => a.PC_Type.EqualsIgnoringCase(OrgConstants.ContactAllocationType.CUS))));
	CachedValue<OrgContact> contactFromOrganisationWithAllocationCus;
}
