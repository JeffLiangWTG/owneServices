using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderForEnquiryMatching : OrgHeader
	{
		public OrgHeaderForEnquiryMatching(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override OrgPatternMatchCollection GetNewPatternMatchesForThisOrgCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
		{
			return new OrgPatternMatchCollectionForEnquiryMatching(factory, additionalFilter);
		}

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		public void PopulateFrom(OrgHeaderForEnquiryMatching orgForMatching)
		{
			OH_FullName = orgForMatching.OH_FullName;
			MainAddress.OA_Address1 = orgForMatching.MainAddress.OA_Address1;
			MainAddress.OA_Address2 = orgForMatching.MainAddress.OA_Address2;
			MainAddress.OA_City = MainAddress.OA_City;
			MainAddress.OA_State = orgForMatching.MainAddress.OA_State;
			MainAddress.OA_PostCode = orgForMatching.MainAddress.OA_PostCode;
			OH_RL_NKClosestPort = orgForMatching.OH_RL_NKClosestPort;
			PrimaryRegistrationNumber.Number = orgForMatching.PrimaryRegistrationNumber.Number;
			MainWebURL.PU_URL = orgForMatching.MainWebURL.PU_URL;
		}
	}
}
