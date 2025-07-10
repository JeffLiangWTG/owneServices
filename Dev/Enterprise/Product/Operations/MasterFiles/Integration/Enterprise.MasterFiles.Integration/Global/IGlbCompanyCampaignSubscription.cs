using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaignSubscription : IBusiness
	{
		ZGuid PK { get; }

		ZGuid GCS_OH { get; set; }
		ZPropertyInfo GCS_OHInfo { get; }

		ZString GCS_Email { get; set; }
		ZPropertyInfo GCS_EmailInfo { get; }

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		ZGuid GCS_G0 { get; set; }
		ZPropertyInfo GCS_G0Info { get; }

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		ZString GCS_MediaCategory { get; set; }
		ZPropertyInfo GCS_MediaCategoryInfo { get; }

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		ZString GCS_MediaCategoryWithAll { get; set; }
		ZWrappedPropertyInfo GCS_MediaCategoryWithAllInfo { get; }

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		ZString GCS_MediaType { get; set; }
		ZPropertyInfo GCS_MediaTypeInfo { get; }

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		ZString GCS_MediaTypeWithAll { get; set; }
		ZWrappedPropertyInfo GCS_MediaTypeWithAllInfo { get; }

		ZBool GCS_IsSubscribed { get; set; }
		ZPropertyInfo GCS_IsSubscribedInfo { get; }

		ZDateTime GCS_SystemCreateTimeUtc { get; }
		ZPropertyInfo GCS_SystemCreateTimeUtcInfo { get; }

		ZString GCS_SystemCreateUser { get; }
		ZPropertyInfo GCS_SystemCreateUserInfo { get; }

		ZDateTime GCS_SystemLastEditTimeUtc { get; }
		ZPropertyInfo GCS_SystemLastEditTimeUtcInfo { get; }

		ZString GCS_SystemLastEditUser { get; }
		ZPropertyInfo GCS_SystemLastEditUserInfo { get; }

		ZBool GCS_IsOrgLevel { get; }
		ZWrappedPropertyInfo GCS_IsOrgLevelInfo { get; }
	}
}