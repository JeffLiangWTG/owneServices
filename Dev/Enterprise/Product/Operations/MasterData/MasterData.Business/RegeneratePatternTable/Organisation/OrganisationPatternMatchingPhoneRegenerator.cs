using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrganisationPatternMatchingPhoneRegenerator : PatternMatchingPhoneRegenerator<OrgHeader>
	{
		OrgAddress[] allOrgAddressArray;
		OrgContact[] allOrgContactArray;

		OrgAddress[] addressNeedAdd;
		OrgContact[] contactNeedAdd;
		OrgContact[] contactNeedUpdate;
		OrgAddress[] addressNeedUpdate;

		readonly List<string> propAddressList = new List<string> { OrgAddressSchema.Constants.OA_Phone, OrgAddressSchema.Constants.OA_Fax, OrgAddressSchema.Constants.OA_Mobile };
		readonly List<string> propContactList = new List<string> { OrgContactSchema.Constants.OC_Phone, OrgContactSchema.Constants.OC_Fax, OrgContactSchema.Constants.OC_Mobile, OrgContactSchema.Constants.OC_OtherPhone, OrgContactSchema.Constants.OC_HomePhone };

		public OrganisationPatternMatchingPhoneRegenerator(PatternMatchingRecalculator<OrgHeader> recalculator)
			: base(recalculator)
		{
		}

		int GetNewMatchingPhoneFromAddressCount(OrgAddress orgAddress)
		{
			var count = 0;

			foreach (var prop in propAddressList)
			{
				var propValue = GetObjectPropertyValue(orgAddress, prop);

				if (!propValue.IsEmpty && !TextStandardizerHelper.IsPlaceholderPhone(propValue))
				{
					count++;
				}
			}

			return count;
		}

		int GetNewMatchingPhoneFromContactCount(OrgContact orgContact)
		{
			var count = 0;

			foreach (var prop in propContactList)
			{
				var propValue = GetObjectPropertyValue(orgContact, prop);

				if (!propValue.IsEmpty && !TextStandardizerHelper.IsPlaceholderPhone(propValue))
				{
					count++;
				}
			}

			return count;
		}

		int CreatNewMatchingPhoneFromAddress(OrgAddress orgAddress, BusinessObjectFactory factory)
		{
			return AddCore(propAddressList, orgAddress, factory, orgAddress.Header.PK, orgAddress.OA_RN_NKCountryCode, orgAddress.OA_IsActive);
		}

		int CreatNewMatchingPhoneFromContact(OrgContact orgContact, BusinessObjectFactory factory)
		{
			return AddCore(propContactList, orgContact, factory, orgContact.Header.PK, orgContact.Header.CountryCode, orgContact.OC_IsActive);
		}

		void UpdateMatchingPhoneFromAddress(OrgAddress orgAddress, BusinessObjectFactory factory)
		{
			UpdateCore(propAddressList, orgAddress, factory, orgAddress.Header.PK, orgAddress.OA_RN_NKCountryCode, orgAddress.OA_IsActive);
		}

		void UpdateMatchingPhoneFromContact(OrgContact orgContact, BusinessObjectFactory factory)
		{
			UpdateCore(propContactList, orgContact, factory, orgContact.Header.PK, orgContact.Header.CountryCode, orgContact.OC_IsActive);
		}

		protected override int GetActualAddCount()
		{
			var counter = 0;

			addressNeedAdd.ForEach((item) =>
			{
				int affectedAddressRecords = GetNewMatchingPhoneFromAddressCount(item);
				counter += affectedAddressRecords;
			});

			contactNeedAdd.ForEach((item) =>
			{
				int affectedContactRecords = GetNewMatchingPhoneFromContactCount(item);
				counter += affectedContactRecords;
			});

			return counter;
		}

		protected override List<string> TablesPrefixList
		{
			get
			{
				return new List<string>()
				{
					OrgAddressSchema.Constants.Prefix,
					OrgContactSchema.Constants.Prefix
				};
			}
		}

		protected override int AddCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			var count = 0;

			addressNeedAdd.ForEach((item) =>
			{
				count += CreatNewMatchingPhoneFromAddress(item, factory);
			});

			contactNeedAdd.ForEach((item) =>
			{
				count += CreatNewMatchingPhoneFromContact(item, factory);
			});

			return count;
		}

		protected override int InitializeDataCountCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			var query = new ZQuery(OrgAddressSchema.OA_OH, bizo.PK);
			allOrgAddressArray = factory.Load<OrgAddress>(query);

			query = new ZQuery(OrgContactSchema.OC_OH, bizo.PK);
			allOrgContactArray = factory.Load<OrgContact>(query);

			query = new ZQuery(PatternMatchingPhoneSchema.PMP_OH, bizo.PK);
			query.AddToFilter(PatternMatchingPhoneSchema.PMP_ParentTableCode, TablesPrefixList);
			allPatternMatchingPhoneArray = factory.Load<PatternMatchingPhone>(query);

			var matchPhoneParentIds = allPatternMatchingPhoneArray.Select(a => a.ParentId).Distinct();

			addressNeedAdd = allOrgAddressArray.Where(s => !matchPhoneParentIds.Contains(s.PK)).ToArray();
			addressNeedUpdate = allOrgAddressArray.Where(s => matchPhoneParentIds.Contains(s.PK)).ToArray();
			contactNeedAdd = allOrgContactArray.Where(s => !matchPhoneParentIds.Contains(s.PK)).ToArray();
			contactNeedUpdate = allOrgContactArray.Where(s => matchPhoneParentIds.Contains(s.PK)).ToArray();

			var orgAddressIds = allOrgAddressArray.Select(a => a.PK).ToList();
			var orgContactIds = allOrgContactArray.Select(a => a.PK).ToList();

			needDelete = allPatternMatchingPhoneArray.Where(s => !orgAddressIds.Contains(s.ParentId) && !orgContactIds.Contains(s.ParentId)).ToArray();

			return GetActualAddCount() + needDelete.Length + addressNeedUpdate.Length + contactNeedUpdate.Length;
		}

		protected override int UpdateCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			var counter = 0;

			contactNeedUpdate.ForEach((item) =>
			{
				UpdateMatchingPhoneFromContact(item, factory);
				counter++;
				ReportProgress();
			});

			addressNeedUpdate.ForEach((item) =>
			{
				UpdateMatchingPhoneFromAddress(item, factory);
				counter++;
				ReportProgress();
			});

			return counter;
		}

		protected override void SetUltimateParentPK(PatternMatchingPhone patternPhone, ZGuid pk, ZGuid parentPK)
		{
			patternPhone.PMP_OH = pk;
		}
	}
}
