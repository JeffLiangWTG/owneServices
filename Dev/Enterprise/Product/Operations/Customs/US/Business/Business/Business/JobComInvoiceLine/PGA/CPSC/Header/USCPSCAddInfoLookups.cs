//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCPSCAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSCPSCAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USCPSCAddInfoLookups : AutoUSCPSCAddInfoLookups
	{
		public USCPSCAddInfoLookups(AutoUSCPSCAddInfo parent) : base(parent)
		{
		}

		public ConsignorCollection Organizations
		{
			get { return new ConsignorCollection(Factory); }
		}

		public CPSCProcessingCodeList ProcessingCodeList
		{
			get { return Factory.GetCachedValue<CPSCProcessingCodeList>(); }
		}

		public ProductIDTypeCodeList ProductIDTypeCodeList
		{
			get { return Factory.GetCachedValue<ProductIDTypeCodeList>(); }
		}

		public CodeDescriptionPairList IntendedUseCodeList
		{
			get
			{
				return Factory.GetCachedValue("USCPSCIntendedUseCodeList", delegate
				{
					var intendedUseCodeList = new CodeDescriptionPairList();
					ZZRefCusCodeListCombined.Loader.Load(
						Factory,
						Core.Constants.CountryCodes.UnitedStates,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode,
						ZDateTime.Today,
						new[] {
							new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PGAIUCAgency,  SQLComparisonOperator.Equal, new ZString[] { "CPS" })
						}).ForEach(x => intendedUseCodeList.AddPairIfNotExist(x.ZZD_Code, x.ZZD_Description));
					intendedUseCodeList.Sort();
					return intendedUseCodeList;
				});
			}
		}

		public CodeDescriptionPairList YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}
	}
}
