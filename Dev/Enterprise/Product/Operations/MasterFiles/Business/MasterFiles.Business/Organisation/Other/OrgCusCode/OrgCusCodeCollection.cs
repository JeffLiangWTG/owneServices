using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusCodeCollection : DependentBusinessObjectCollection<OrgCusCode, OrgHeader>, IBODocDataProviderCollection
	{
		public OrgCusCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCusCodeCollection(OrgHeader parentOrganisation, BusinessObjectFactory factory)
			: base(parentOrganisation, factory)
		{
		}

		public OrgCusCodeCollection(OrgHeader parentOrganisation, ZQuery additionalFilter)
			: base(parentOrganisation, additionalFilter)
		{
		}

		/// <summary>
		/// Retrieves Customs Registration No of the passed code type for current country
		/// </summary>
		public ZString GetCustomsRegNo(ZString codeType)
		{
			return GetCustomsRegNo(codeType, GlbCompany.CurrentCompany.Country);
		}

		public OrgCusCode GetOrgCusCode(ZString code, RefCountry country)
		{
			return GetOrgCusCodeObjectForCodeAndCountry(code, country);
		}

		public OrgCusCode GetOrgCusCode(ZString codeType, ZString countryCode, ZGuid addressPK)
		{
			var results = FindCusCodeLinkToPremiseAddress(codeType, countryCode, ref addressPK);
			OrgCusCode result = null;
			foreach (OrgCusCode cusCode in results)
			{
				result = cusCode;
				if (result.OK_OA_PremisesAddress == addressPK)
				{
					break;
				}
			}
			return result;
		}

		public ZString GetCustomsRegNo(ZString code, RefCountry country)
		{
			OrgCusCode cusCode = GetOrgCusCodeObjectForCodeAndCountry(code, country);
			return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
		}

		public ZString GetCustomsRegNo(ZString code, ZString countryCode)
		{
			OrgCusCode cusCode = GetOrgCusCodeObjectForCodeAndCountry(code, countryCode);
			return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
		}

		public ZString GetCustomsRegNo(ZString codeType, ZString countryCode, ZGuid addressPK)
		{
			var cusCode = GetOrgCusCode(codeType, countryCode, addressPK);
			return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
		}

		public ZString GetCustomsRegNoPremiseAddressOnly(ZString codeType, ZString countryCode, ZGuid addressPK)
		{
			return GetOrgCusCodeForPremiseAddress(codeType, countryCode, addressPK)?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public OrgCusCode GetOrgCusCodeForPremiseAddress(ZString codeType, ZString countryCode, ZGuid addressPK)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			if (countryCode.IsValid && !countryCode.IsEmpty)
			{
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			}

			query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, addressPK.IsValid ? addressPK : null);

			var results = (OrgCusCode[])Find(query);
			return results.FirstOrDefault();
		}

		OrgCusCode[] FindCusCodeLinkToPremiseAddress(ZString codeType, ZString countryCode, ref ZGuid addressPK)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			if (countryCode.IsValid && !countryCode.IsEmpty)
			{
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			}
			var addressSubQuery = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, null);
			if (addressPK.IsValid)
			{
				addressSubQuery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_OA_PremisesAddress, addressPK);
			}
			query.AddToFilter(addressSubQuery);
			OrgCusCode[] results = (OrgCusCode[])Find(query);
			return results;
		}

		/// <summary>
		/// GetCustomsCode in the order of codeTypes passed in 
		/// </summary>
		public ZString GetCustomsRegNoMatching(params ZString[] codeTypes)
		{
			OrgCusCode cusCode = GetOrgCusCodeObjectMatching(codeTypes);
			return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
		}

		/// <summary>
		/// Get OrgCusCode in the order of codeTypes passed in for the current country
		/// </summary>
		public OrgCusCode GetOrgCusCodeObjectMatching(params ZString[] codeTypes)
		{
			return GetOrgCusCodeObjectMatchingCountryAndCodes(GlbCompany.CurrentCompany.Country, codeTypes);
		}

		public ZString GetCountryCodeFromPk(ZGuid countryPk)
		{
			var refCountry = Factory.Load<RefCountry>(countryPk);
			return refCountry != null ? refCountry.Code : ZString.Empty;
		}

		public OrgCusCode GetOrgCusCodeObjectMatchingCountryAndCodes(ICountry country, params ZString[] codeTypes)
		{
			return GetOrgCusCodeObjectMatchingCountryAndCodes(country == null ? ZString.Empty : GetCountryCodeFromPk(country.PK), codeTypes);
		}

		public OrgCusCode GetOrgCusCodeObjectMatchingCountryAndCodes(ZString countryCode, params ZString[] codeTypes)
		{
			foreach (ZString codeType in codeTypes)
			{
				OrgCusCode cusCode = GetOrgCusCodeObjectForCodeAndCountry(codeType, countryCode);
				if (cusCode != null)
				{
					return cusCode;
				}
			}
			return null;
		}

		public ZString GetCustomsRegNoMatchingCountryAndCodes(ZString countryCode, params ZString[] codeTypes)
		{
			var cusCode = GetOrgCusCodeObjectMatchingCountryAndCodes(countryCode, codeTypes);
			return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
		}

		public OrgCusCode GetOrgCusCodeObjectForCodeAndCountry(ZString code, ICountry country)
		{
			return GetOrgCusCodeObjectForCodeAndCountry(code, country == null ? ZString.Empty : GetCountryCodeFromPk(country.PK));
		}

		public OrgCusCode GetOrgCusCodeObjectForCodeAndCountry(ZString code, ZString countryCode)
		{
			OrgCusCode result = null;
			var codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, code);
			if (countryCode.IsValid && !countryCode.IsEmpty)
			{
				codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			}

			BusinessObject[] arrayListCustomsCodes = Find(codeFilter);
			if (arrayListCustomsCodes.Length > 0)
			{
				result = (OrgCusCode)arrayListCustomsCodes[0];
			}
			return result;
		}

		public OrgCusCode[] GetOrgCusCodesForCodeAndCountry(ZString code, ZString countryCode)
		{
			var codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, code);
			if (countryCode.IsValid)
			{
				codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			}

			var arrayListCustomsCodes = (OrgCusCode[])Find(codeFilter);
			return arrayListCustomsCodes;
		}

		public OrgCusCode[] GetAllOrgCusCodesForCountryAndCodes(ZString countryCode, params ZString[] codeTypes)
		{
			var codeFilter = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			if (codeTypes.Length > 0)
			{
				codeFilter.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeTypes);
			}

			return (OrgCusCode[])Find(codeFilter);
		}

		public OrgCusCode[] GetOrgCusCodesForCodeIgnoringCountry(ZString code) => GetOrgCusCodesForMatchingCodesIgnoringCountry(code);

		public OrgCusCode[] GetOrgCusCodesForMatchingCodesIgnoringCountry(params ZString[] codeTypes)
		{
			return (OrgCusCode[])Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, codeTypes));
		}

		public ZString GetUNC()
		{
			return GetCustomsRegNo(OrgCusCode.CodeTypes.UniversalNettingCode, (ZString)null);
		}

		public ZString GetUOC()
		{
			return GetCustomsRegNo(OrgCusCode.CodeTypes.UniversalOfficeCode, (ZString)null);
		}

		#region Indexer + AddNew

		/// <summary>
		/// Add a reg number for the code for the current country
		/// </summary>
		public OrgCusCode AddNew(ZString codeType, ZString regNum)
		{
			return AddNew(codeType, regNum, GlbCompany.CurrentCompany.Country);
		}

		public OrgCusCode AddNew(ZString codeType, ZString customsRegNo, ICountry country)
		{
			return AddNew(codeType, customsRegNo, GetCountryCodeFromPk(country.PK));
		}

		public OrgCusCode AddNew(ZString codeType, ZString customsRegNo, ZString countryCode)
		{
			OrgCusCode result = AddNew();
			result.OK_CodeType = codeType;
			result.OK_CustomsRegNo = customsRegNo;
			result.OK_RN_NKCodeCountry = countryCode;
			return result;
		}

		public OrgCusCode UpdateOrAddCustomsCodesIfNoneExists(ZString codeType, ZString registrationNum, ZString countryCode)
		{
			OrgCusCode cusCode = GetOrgCusCodeObjectForCodeAndCountry(codeType, countryCode);
			if (cusCode == null)
			{
				cusCode = AddNew();
				cusCode.OK_RN_NKCodeCountry = countryCode;
				cusCode.OK_CodeType = codeType;
			}
			cusCode.OK_CustomsRegNo = registrationNum;
			return cusCode;
		}

		public OrgCusCode this[string index]
		{
			get
			{
				var regex = new Regex(@"^\s*""(?<codeType>[A-Za-z0-9]{2,3})""(|\s*,\s*""(?<countryCode>[A-Za-z0-9][A-Za-z0-9])"")\s*$");
				var parameters = regex.Match(index);
				if (parameters.Success)
				{
					string codeType = parameters.Groups["codeType"].Value;
					string countryCode = parameters.Groups["countryCode"].Value;

					if (string.IsNullOrEmpty(countryCode))
					{
						countryCode = GlbCompany.CurrentCompany.Country.RN_Code;
					}

					return GetOrgCusCodeObjectForCodeAndCountry(codeType, countryCode);
				}

				return null;
			}
		}

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			if (!IsDeletingForDataRefresh)
			{
				MarkParentAsNeedingPatternMatchesRegen();

				if (!IsValidationSuspended && ((OrgCusCode)elementToRemove).OK_CodeType == OrgCusCode.CodeTypes.GS1)
				{
					var parent = Master;
					if (parent != null)
					{
						parent.OrgFountains.MarkAsNeedingValidation();
					}
				}

				if (!IsValidationSuspended)
				{
					var cusCodeToRemove = (OrgCusCode)elementToRemove;
					foreach (OrgCusCode cusCode in this)
					{
						if (string.Equals(cusCode.OK_RN_NKCodeCountry, cusCodeToRemove.OK_RN_NKCodeCountry, StringComparison.OrdinalIgnoreCase))
						{
							cusCode.MarkAsNeedingValidation();
						}
					}
				}
			}

			base.Remove(elementToRemove);
		}

		public bool AllowDeleteRegardlessOfSecurity
		{
			get { return fAllowDeleteRegardlessOfSecurity; }
			set { fAllowDeleteRegardlessOfSecurity = value; }
		}

		protected ISecurityCheckpoint FailingCheckpoint { get; set; }

		bool fAllowDeleteRegardlessOfSecurity;

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (!AllowDeleteRegardlessOfSecurity && CheckpointDeniesDelete((OrgCusCode)elementToDelete))
			{
				FailingCheckpoint.ShowError();
			}
			else
			{
				MarkParentAsNeedingPatternMatchesRegen();
				Master.FindDuplicates();
				base.RemoveAndDelete(elementToDelete);
			}
		}

		protected bool CheckpointDeniesDelete(OrgCusCode elementToDelete)
		{
			FailingCheckpoint = null;
			var isPrimaryCodeType = elementToDelete.IsOriginalCompanyCodeTypePrimary;
			if (elementToDelete.IsInDatabase)
			{
				FailingCheckpoint = Master.GetFailingCheckpointWhenModifyRegistrationNumber(isPrimaryCodeType);
			}

			return FailingCheckpoint != null;
		}

		void MarkParentAsNeedingPatternMatchesRegen()
		{
			var parent = Master;
			if (parent != null)
			{
				parent.PatternMatchRequiresRegen = true;
			}
		}

		#endregion

		#region IBODocDataProviderCollection Members

		IBODocDataProviderCollectionHelper Helper
		{
			get { return helper ?? (helper = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { this })); }
		}
		IBODocDataProviderCollectionHelper helper;

		IBODocDataProvider IBODocDataProviderCollection.this[string index]
		{
			get { return GetRow(index); }
		}

		IBODocDataProvider IBODocDataProviderCollection.this[int index]
		{
			get { return Helper[index]; }
		}

		int IBODocDataProviderCollection.Count
		{
			get { return Helper.Count; }
		}

		ZString IBODocDataProviderCollection.Format(ZString formatString, ZString delimiter, ZString filterString, ZString groupByParameters, ZInt maxItems)
		{
			return Helper.Format(formatString, delimiter, filterString, groupByParameters, maxItems);
		}

		object IBODocDataProviderCollection.Total(ZString fieldName, ZString decimalPlaces, ZString filter)
		{
			return Helper.Total(fieldName, decimalPlaces, filter);
		}

		IBODocDataProvider GetRow(ZString index)
		{
			const char divider = ':';

			int dividerIndex = index.IndexOf(divider);
			string codeType;
			RefCountry country;

			if (dividerIndex < 0)
			{
				codeType = index;
				country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
			else
			{
				codeType = index.Left(dividerIndex);
				country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, index.SubstringSafe(dividerIndex + 1));
			}

			var cusCode = GetOrgCusCode(codeType, country);
			IBODocDataProvider provider = BODocDataProvider.Get(cusCode)
				?? Helper[index];

			return provider ?? BODocDataProvider.Get(Factory.New<OrgCusCode>());
		}

		public BusinessObject Find(ZString match)
		{
			return Helper.Find(match);
		}

		#endregion
	}
}
