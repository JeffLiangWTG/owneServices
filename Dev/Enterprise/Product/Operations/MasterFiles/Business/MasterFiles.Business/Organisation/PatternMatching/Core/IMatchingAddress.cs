using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public interface IMatchingAddress
	{
		ZGuid PK { get; }
		ZString OA_Code { get; }
		ZString OA_Address1 { get; }
		ZString OA_Address2 { get; }
		ZString OA_City { get; }
		ZString OA_RL_NKRelatedPortCode { get; set; }
		bool HasChanges { get; }
		ZBool OA_RL_NKRelatedPortCodeInfoHasChanges { get; }
		ZBool OA_IsActive { get; }
		bool IsDeleted { get; }
		ZString OA_Language { get; }
		ZString OA_PostCode { get; }
		ZString OA_State { get; }
		ZString OA_Fax { get; }
		ZString OA_Phone { get; }
		ZString OA_Email { get; }
		ZString OA_CompanyNameOverride { get; }
		ZString OA_Mobile { get; }
		bool IsMainAddress { get; }
		void SetMainAddress();
		ZString PortName { get; }
		MultilingualString CountryName { get; }
		ZString OA_AdditionalAddressInformation { get; set; }
		ZString Contact { get; set; }
		ZString CountryCode { get; set; }

		IDisposable CachePortAndCountryNames();
	}
}
