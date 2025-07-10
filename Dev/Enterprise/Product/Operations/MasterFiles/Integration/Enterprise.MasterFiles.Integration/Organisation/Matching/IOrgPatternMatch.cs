using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgPatternMatch
	{
		ZGuid PK { get; }
		ZGuid OS_OH { get; set; }
		ZGuid OS_OA { get; set; }
		ZString OS_BusinessRegNo { get; set; }
		ZString OS_PostCode { get; set; }
		ZString OS_City { get; set; }
		ZString OS_State { get; set; }
		ZString OS_UNLOCO { get; set; }
		ZString OS_Phone { get; set; }
		ZString OS_FaxNum { get; set; }
		ZString OS_FullCompanyName { get; set; }
		ZString OS_CompanyName1 { get; set; }
		ZString OS_CompanyName2 { get; set; }
		ZString OS_CompanyName3 { get; set; }
		ZString OS_CompanyName4 { get; set; }
		ZBool OS_IsCorporation { get; set; }
		ZString OS_StreetNumber { get; set; }
		ZString OS_Address1 { get; set; }
		ZString OS_Address2 { get; set; }
		ZString OS_Address3 { get; set; }
		ZString OS_Address4 { get; set; }
		ZBool OS_IsPOBox { get; set; }
		ZString OS_POBoxNumber { get; set; }
		ZString OS_Email { get; set; }
		ZString OS_Domain { get; set; }
		void Delete();
		bool IsDeleted { get; }
	}
}
