//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenRegCertAccredMaintListLookups
//
//    This class should be used for overriding collections in AutoGenRegCertAccredMaintListLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;

namespace Enterprise.MasterFiles.Business
{
	using System.ComponentModel;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;

	public class GenRegCertAccredMaintListLookups : AutoGenRegCertAccredMaintListLookups
	{
		public GenRegCertAccredMaintListLookups(AutoGenRegCertAccredMaintList parent)
			: base(parent)
		{
		}

		#region Certificate Types

		public ICodeDescriptionPairList CertificateTypes
		{
			get
			{
				return Factory.GetCachedValue(
					FormattableString.Invariant($"GenRegCertAccredMaintListLookups.CertificateTypes+{((BusinessObject)Parent.MasterParent)?.TablePrefix}"),
					() => { return Parent.MasterParent != null ? Parent.MasterParent.GetCertificateTypeList() : new CodeDescriptionPairList(); });
			}
		}

		public ICodeDescriptionPairList CertificateTypes_ActiveList
		{
			get
			{
				return Factory.GetCachedValue(
					FormattableString.Invariant($"GenRegCertAccredMaintListLookups.CertificateTypes_ActiveList+{((BusinessObject)Parent.MasterParent)?.TablePrefix}"),
					() => { return Parent.MasterParent != null ? Parent.MasterParent.GetActiveCertificateTypeList() : new CodeDescriptionPairList(); });
			}
		}

		#endregion

		#region States Or Provinces

		public IBusinessObjectCollection StatesOrProvinces
		{
			get
			{
				var countryCode = Parent.XZ_RN_NKCountryOfIssuance;
				if (countryCode.IsEmpty || Parent.XZ_RN_NKCountryOfIssuanceInfo.HasErrors())
				{
					statesOrProvinces = new RefCountryStatesCollection(Factory, new ZQuery { IsNoResultQuery = true });
				}
				else if (statesOrProvinces == null || previousCountryCode != countryCode)
				{
					var country = Parent.CountryOfIssuance;
					var query = country == null ? new ZQuery { IsNoResultQuery = true } : new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, country.RN_Code);

					var collection = new RefCountryStatesCollection(Factory, query);
					collection.ApplySort(RefCountryStates.Schema.RW_Code, ListSortDirection.Ascending);
					statesOrProvinces = collection;
					previousCountryCode = countryCode;
				}
				return statesOrProvinces;
			}
		}

		IBusinessObjectCollection statesOrProvinces;
		ZString previousCountryCode;

		#endregion

		public new GenRegCertAccredMaintList Parent
		{
			get { return (GenRegCertAccredMaintList)base.Parent; }
		}
	}
}
