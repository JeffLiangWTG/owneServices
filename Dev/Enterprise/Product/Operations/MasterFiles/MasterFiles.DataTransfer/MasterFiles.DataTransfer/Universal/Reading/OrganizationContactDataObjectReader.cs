using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganizationContactDataObjectReader : DataObjectReader<OrganizationContact>
	{
		public OrganizationContactDataObjectReader(OrganizationContact contactData, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(contactData, logger, factory)
		{
		}

		public OrgContact GetMatched()
		{
			OrgContact contact = null;
			var fullName = dataObject.FullName.GetValueOrDefault();
			var phone = dataObject.Phone.GetValueOrDefault();
			var email = dataObject.Email.GetValueOrDefault();
			if (!fullName.IsEmpty || !phone.IsEmpty || !email.IsEmpty)
			{
				var query = new ZQuery();
				if (!fullName.IsEmpty)
				{
					query.AddToFilter(OrgContactSchema.OC_ContactName, fullName);
				}

				if (!phone.IsEmpty)
				{
					var phoneQuery = new ZQuery();
					phoneQuery.AddToFilter(OrgContactSchema.OC_Phone, phone);
					phoneQuery.AddToFilter(JoinCondition.Or, OrgContactSchema.OC_Mobile, phone);

					var subPhoneQuery = new ZDBOnlyQuery(typeof(OrgContact));
					var sql = FormattableString.Invariant($@"{OrgContactSchema.OC_OH.Name} in
					(
					    SELECT oh.OH_PK FROM dbo.OrgHeader oh
					    JOIN dbo.OrgAddress oa on oa.OA_OH = oh.OH_PK
					    JOIN dbo.OrgAddressCapability pz on pz.PZ_OA = oa.OA_PK
					    WHERE pz.PZ_AddressType = 'OFC' 
					    AND pz.PZ_IsMainAddress = 1
					    AND (oa.OA_Phone = @OA_Phone or oa.OA_Mobile = @OA_Mobile)
					)");
					var parameters = new ZSqlParameterCollection(
						ZSqlParameter.New("@OA_Phone", phone, OrgAddressSchema.OA_Phone),
						ZSqlParameter.New("@OA_Mobile", phone, OrgAddressSchema.OA_Mobile));

					subPhoneQuery.AddFilterAndZSQLParameterCollection(sql, parameters);
					phoneQuery.AddToFilter(subPhoneQuery, JoinCondition.Or);
					query.AddToFilter(phoneQuery);
				}

				if (!email.IsEmpty)
				{
					var emailQuery = new ZQuery(OrgContactSchema.OC_Email, email);
					query.AddToFilter(emailQuery);
				}

				if (!query.IsEmpty)
				{
					contact = factory.LoadTop1<OrgContact>(query);
				}
			}
			return contact;
		}
	}
}

