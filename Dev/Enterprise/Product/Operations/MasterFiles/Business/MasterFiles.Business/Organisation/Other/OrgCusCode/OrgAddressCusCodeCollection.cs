using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressCusCodeCollection : ActiveBusinessObjectCollection<OrgCusCode>, IBODocDataProviderCollection
	{
		public OrgAddressCusCodeCollection(OrgAddress orgAddress)
			: base(orgAddress.Header, new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, orgAddress.PK))
		{
			this.orgAddress = orgAddress;
		}

		public ZString GetCustomsRegNo(ZString codeType, ZString countryCode)
		{
			OrgCusCode cusCode = GetOrgCusCodeObjectForCodeTypeAndCountry(codeType, countryCode);
			return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
		}

		public OrgCusCode GetOrgCusCodeObjectForCodeTypeAndCountry(ZString codeType, ZString countryCode)
		{
			OrgCusCode result = null;
			var codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);

			foreach (OrgCusCode element in Find(codeFilter))
			{
				result = element;
				break;
			}
			return result;
		}

		ZString GetCountryCodeFromPk(ZGuid countryPk)
		{
			var refCountry = Factory.Load<RefCountry>(countryPk);
			return refCountry != null ? refCountry.Code : ZString.Empty;
		}

		public OrgCusCode GetOrgCusCodeObjectMatching(params ZString[] codeTypes)
		{
			return GetOrgCusCodeObjectMatchingCountryAndCodes(GlbCompany.CurrentCompany.Country, codeTypes);
		}

		public OrgCusCode GetOrgCusCodeObjectMatchingCountryAndCodes(ICountry country, params ZString[] codeTypes)
		{
			return GetOrgCusCodeObjectMatchingCountryAndCodes(country == null ? ZString.Empty : GetCountryCodeFromPk(country.PK), codeTypes);
		}

		public OrgCusCode GetOrgCusCodeObjectMatchingCountryAndCodes(ZString countryCode, params ZString[] codeTypes)
		{
			foreach (var codeType in codeTypes)
			{
				var cusCode = GetOrgCusCodeObjectForCodeTypeAndCountry(codeType, countryCode);
				if (cusCode != null)
				{
					return cusCode;
				}
			}
			return null;
		}

		public ZString GetCustomsRegNoMatching(params ZString[] codeTypes)
		{
			return GetOrgCusCodeObjectMatching(codeTypes)?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public OrgCusCode[] GetOrgCusCodesForCodeIgnoringCountry(ZString codeType)
		{
			return Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType)).ToArray();
		}

		public ZString[] GetOrgCusCodeTypeCollectionForRegNoAndCountry(ZString regNo, ZString countryCode)
		{
			var codeTypeFilter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, regNo);
			codeTypeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			return Find(codeTypeFilter).Select(cusCodeType => cusCodeType.OK_CodeType).ToArray();
		}

		/// <summary>
		/// Add a reg number for the code for the current country
		/// </summary>
		public OrgCusCode AddNew(ZString codeType, ZString regNum)
		{
			return AddNew(codeType, regNum, GlbCompany.CurrentCompany.Country);
		}

		public OrgCusCode AddNew(ZString codeType, ZString customsRegNo, ICountry country)
		{
			var refCountry = Factory.Load<RefCountry>(country.PK);
			return AddNew(codeType, customsRegNo, refCountry != null ? refCountry.Code : ZString.Empty);
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
			OrgCusCode cusCode = GetOrgCusCodeObjectForCodeTypeAndCountry(codeType, countryCode);
			if (cusCode == null)
			{
				cusCode = AddNew();
				cusCode.OK_RN_NKCodeCountry = countryCode;
				cusCode.OK_CodeType = codeType;
			}
			cusCode.OK_CustomsRegNo = registrationNum;
			return cusCode;
		}

		#region Implementation
		protected readonly OrgAddress orgAddress;

		protected override void SetDefaultsForNewElementCore(OrgCusCode newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.OK_OA_PremisesAddress = orgAddress.PK;
		}

		protected override void OnAdded(OrgCusCode businessObject)
		{
			base.OnAdded(businessObject);
			if (((INeedRow)businessObject).Row.RowState != System.Data.DataRowState.Detached)
			{
				orgAddress.Header.CustomsCodes.Add(businessObject);
			}
		}

		#endregion

		#region IBODocDataProviderCollection Members

		IBODocDataProviderCollectionHelper Helper
		{
			get { return helper ?? (helper = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { this })); }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
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
			var countryCode = ZString.Empty;

			if (dividerIndex < 0)
			{
				codeType = index;
				countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			else
			{
				codeType = index.Left(dividerIndex);
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, index.SubstringSafe(dividerIndex + 1));
				if (country != null)
				{
					countryCode = country.Code;
				}
			}

			return BODocDataProvider.Get(GetOrgCusCodeObjectForCodeTypeAndCountry(codeType, countryCode))
				?? Helper[index];
		}

		public BusinessObject Find(ZString match)
		{
			return Helper.Find(match);
		}

		#endregion
	}
}
