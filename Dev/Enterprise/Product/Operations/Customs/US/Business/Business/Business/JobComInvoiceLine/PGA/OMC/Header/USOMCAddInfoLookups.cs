//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSOMCAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSOMCAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USOMCAddInfoLookups : AutoUSOMCAddInfoLookups
	{
		public USOMCAddInfoLookups(AutoUSOMCAddInfo parent) : base(parent)
		{
		}

		public USCCountryCollection USCountries
		{
			get
			{
				var declarationCode = Parent.US_DeclarationCode;
				var additionalFilter = Factory.GetCachedValue("USConformanceDeclarationCodeFilter" + declarationCode, () =>
				{
					CodeDescriptionPairList countries = null;
					switch (declarationCode)
					{
						case ConformanceDeclarationCodeList.Codes._7A2:
							countries = Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes._7A2);
							break;
						case ConformanceDeclarationCodeList.Codes._7A4:
							countries = Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes._7A4);
							break;
						case ConformanceDeclarationCodeList.Codes._7B:
							countries = Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes._7B);
							break;
					}

					var filter = new ZQuery();
					if (countries != null && countries.Count > 0)
					{
						filter.AddToFilter(ZArchitecture.Schema.USCCountrySchema.UC_Code, countries.GetAllCodesZString());
					}
					return filter;
				});
				var result = new USCCountryCollection(Factory, additionalFilter);
				return result;
			}
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public CodeDescriptionPairList UnitOfMeasureList
		{
			get
			{
				return Factory.GetCachedValue("UnitOfMeasureList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ABIUnitOfMeasureList.Codes.Kilograms, ABIUnitOfMeasureList.Descriptions.Kilograms);
					return result;
				});
			}
		}

		public ConformanceDeclarationCodeList DeclarationCodes
		{
			get { return Factory.GetCachedValue<ConformanceDeclarationCodeList>(); }
		}

		protected new OMCHeaderAddInfo Parent => (OMCHeaderAddInfo)base.Parent;
	}
}
