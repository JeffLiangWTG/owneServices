using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class MessageSenderContactDetailsDefaultingHelper
	{
		public static GlbStaff GetBrokerContact(BusinessObjectFactory factory, ZGuid declarationPK, Guid companyPK, Guid branchPK)
		{
			var contactPk = USCustomsDataRegistry.Instance.CargoReleaseFTZContact.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			var contact = factory.Load<GlbStaff>(contactPk);

			if (contact == null)
			{
				contact = GlbStaff.CurrentUser;
				if ((contact == null || contact.GS_IsSystemAccount) && !declarationPK.IsEmpty)
				{
					var dec = factory.Load<JobDeclaration>(declarationPK);
					if (dec != null && dec.CusAgent is GlbStaff broker)
					{
						contact = broker;
					}
				}
			}

			return contact;
		}

		public static ZString GetPhoneNumber(GlbStaff staff)
		{
			var result = staff.GS_WorkPhone.IsEmpty && staff.HomeBranch is GlbBranch homeBranch ? homeBranch.GB_Phone : staff.GS_WorkPhone;
			return result.GetLocalPhoneNumber(staff.DefaultCountryCodeForPhoneNumbers);
		}
	}
}
