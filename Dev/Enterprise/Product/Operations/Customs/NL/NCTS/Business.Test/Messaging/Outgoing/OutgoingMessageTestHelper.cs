using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

public static class OutgoingMessageTestHelper
{
	public static T CreateCusReference<T>(this BusinessObjectFactory factory, string code, string type, string reference = null, BusinessObject parent = null) where T : CusReference
	{
		var cusReference = factory.New<T>();
		if (parent != null)
		{
			cusReference.CFR_ParentID = parent.PK;
			cusReference.CFR_ParentTableCode = parent.TablePrefix;
		}
		cusReference.CFR_Type = type;
		cusReference.CFR_Reference = reference;
		cusReference.CFR_Code = code;
		return cusReference;
	}

	public static CusSupportingInfo CreateCusSupportingInfo(this BusinessObjectFactory factory, string type, string subtype, string reference = null, string reference2 = null, string code = null, BusinessObject parent = null)
	{
		var cusSupportingInfo = factory.New<CusSupportingInfo>();
		if (parent != null)
		{
			cusSupportingInfo.CSI_ParentID = parent.PK;
			cusSupportingInfo.CSI_ParentTableCode = parent.TablePrefix;
		}
		cusSupportingInfo.CSI_Type = type;
		cusSupportingInfo.CSI_SubType = subtype;
		cusSupportingInfo.CSI_ReferenceNumber = reference;
		cusSupportingInfo.CSI_ReferenceNumber2 = reference2;
		cusSupportingInfo.CSI_Code = code;
		return cusSupportingInfo;
	}

	public static JobDocAddress CreateJobDocAddress(this BusinessObjectFactory factory, string addressType = null, string orgHeaderFullName = null, string contactName = null, string contactPhone = null, string contactEmail = null
			, JobDocAddress jobDocAddress = null
			, OrgHeader orgHeader = null
			, OrgAddress orgAddress = null
			, IEnumerable<OrgCusCode> orgCusCodes = null, BusinessObject parent = null)
	{
		jobDocAddress = jobDocAddress ?? factory.New<JobDocAddress>();
		orgHeader = orgHeader ?? factory.New<OrgHeader>();
		var orgContact = factory.New<OrgContact>();

		orgHeader.OH_FullName = orgHeaderFullName;
		jobDocAddress.E2_AddressType = addressType;

		jobDocAddress.E2_Contact = contactName;
		jobDocAddress.E2_Phone = contactPhone;
		jobDocAddress.E2_Email = contactEmail;

		orgContact.OC_ContactName = contactName;
		orgContact.OC_Phone = contactPhone;
		orgContact.OC_Email = contactEmail;

		if (orgAddress != null)
		{
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_OH = orgHeader.PK;
		}

		jobDocAddress.OrganisationPK = orgHeader.PK;
		if (parent != null)
		{
			jobDocAddress.E2_ParentID = parent.PK;
			jobDocAddress.E2_ParentTableCode = parent.TablePrefix;
		}
		jobDocAddress.ContactPK = orgContact.PK;

		orgHeader.CustomsCodes.AddRange(orgCusCodes ?? Enumerable.Empty<OrgCusCode>());
		return jobDocAddress;
	}
}
