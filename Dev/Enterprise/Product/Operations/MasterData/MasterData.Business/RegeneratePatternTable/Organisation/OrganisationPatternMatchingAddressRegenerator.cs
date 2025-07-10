using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrganisationPatternMatchingAddressRegenerator : PatternMatchingAddressRegenerator<OrgHeader>
	{
		OrgAddress[] allOrgAddressArray;
		OrgAddress[] addressesToAdd;

		public OrganisationPatternMatchingAddressRegenerator(PatternMatchingRecalculator<OrgHeader> recalculator)
			: base(recalculator)
		{
		}

		protected override int AddCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			foreach (var item in addressesToAdd)
			{
				ZString valueToHash = GetValueToUpper(item);

				AddCore(factory, item.OA_OH, item.TablePrefix, item.PK, item.OA_RN_NKCountryCode, valueToHash, item.Address1, item.Address2);
			}

			return addressesToAdd.Length;
		}

		protected override void SetUltimateParentPK(PatternMatchingAddress patternAddress, ZGuid pk)
		{
			patternAddress.PMA_OH = pk;
		}

		protected override int InitializeDataCountCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			var query = new ZQuery(OrgAddressSchema.OA_OH, bizo.PK);
			allOrgAddressArray = factory.Load<OrgAddress>(query);

			query = new ZQuery(PatternMatchingAddressSchema.PMA_OH, bizo.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, TablesPrefixList);
			allPatternMatchingAddressArray = factory.Load<PatternMatchingAddress>(query);

			var matchAddressparentIds = allPatternMatchingAddressArray.Select(a => a.ParentId).ToList();
			Func<OrgAddress, bool> isNotPlaceHolder = addr => (!TextStandardizerHelper.IsPlaceholderAddress(addr.OA_Address1) || (addr.OA_Address1.IsEmpty && !TextStandardizerHelper.IsPlaceholderAddress(addr.OA_Address2)));

			addressesToAdd = allOrgAddressArray.Where(s => isNotPlaceHolder(s) && !string.IsNullOrEmpty(s.OA_Address1 + s.OA_Address2 + s.OA_City + s.OA_PostCode + s.OA_State) && !matchAddressparentIds.Contains(s.PK)).ToArray();

			var orgAddressIds = allOrgAddressArray.Select(a => a.PK).ToList();

			patternMatchingAddressesToDelete = allPatternMatchingAddressArray.Where(s => !orgAddressIds.Contains(s.ParentId)).ToArray();
			patternMatchingAddressesToUpdate = allPatternMatchingAddressArray.Where(s => orgAddressIds.Contains(s.ParentId)).ToArray();
			return addressesToAdd.Length + patternMatchingAddressesToDelete.Length + patternMatchingAddressesToUpdate.Length;
		}

		protected override int GetActualAddCount()
		{
			return addressesToAdd.Length;
		}

		protected override List<string> TablesPrefixList
		{
			get
			{
				return new List<string>()
				{
					OrgAddressSchema.Constants.Prefix
				};
			}
		}

		protected override void UpdateCore(OrgHeader bizo, PatternMatchingAddress patternAddress)
		{
			var orgAddress = allOrgAddressArray.Where(oa => oa.PK.Equals(patternAddress.ParentId)).FirstOrDefault();
			ZString valueToHash = GetValueToUpper(orgAddress);

			UpdateCore(patternAddress, orgAddress.PK, orgAddress.OA_Address1, orgAddress.OA_Address2, valueToHash, orgAddress.OA_IsActive, orgAddress.OA_RN_NKCountryCode);
		}
	}
}
