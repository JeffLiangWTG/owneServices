//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISLicenseAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAPHISLicenseAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USAPHISLicenseAddInfoLookups : AutoUSAPHISLicenseAddInfoLookups
	{
		public USAPHISLicenseAddInfoLookups(AutoUSAPHISLicenseAddInfo parent)
			: base(parent)
		{
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public IBusinessObjectCollection StateList
		{
			get
			{
				IBusinessObjectCollection result = null;
				var licence = License;
				var country = licence == null ? null : licence.Country;
				if (country != null)
				{
					result = country.States;
				}
				return result ?? new RefCountryStatesCollection(Factory, ZQuery.NoResultQuery);
			}
		}

		public ICodeDescriptionPairList LicenseTypes
		{
			get { return APHISLicenseTypeList.GetListForProgram(Factory, License.ProgramType); }
		}

		public ICodeDescriptionPairList DateQualifiers
		{
			get { return LPCODateQualifierList.GetListForAPHIS(Factory); }
		}

		public CodeDescriptionPairList UnitOfMeasureList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("USPGAAPHISUnitOfMeasure",
				delegate
				{
					var result = new APHISLicensesUnitOfMeasureList();
					var appendixBUnitsMeasureList = new AppendixBUnitsMeasureList();
					foreach (ICodeDescription pair in appendixBUnitsMeasureList)
					{
						if (!result.ContainsCode(pair.Code))
						{
							result.AddPair(pair.Code, pair.Description);
						}
					}
					result.Sort();

					return result;
				});
			}
		}

		protected new USAPHISLicenseAddInfo Parent
		{
			get { return (USAPHISLicenseAddInfo)base.Parent; }
		}

		protected APHISLicense License
		{
			get { return Parent.Parent; }
		}

		public OrganisationsFindBoxCollection Organisations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}
	}
}
