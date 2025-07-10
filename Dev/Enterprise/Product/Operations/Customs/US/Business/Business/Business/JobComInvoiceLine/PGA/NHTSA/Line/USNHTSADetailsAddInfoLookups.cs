//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSADetailsAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSNHTSADetailsAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USNHTSADetailsAddInfoLookups : AutoUSNHTSADetailsAddInfoLookups
	{
		public USNHTSADetailsAddInfoLookups(AutoUSNHTSADetailsAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ModelYearList
		{
			get { return Factory.GetCachedValue("USNHTSAModelYearList", () => GetYearList(ZDateTime.Today.Year + 1)); }
		}

		public CodeDescriptionPairList ManufacturerYearList
		{
			get { return Factory.GetCachedValue("USNHTSAManufacturerYearList", () => GetYearList(ZDateTime.Today.Year)); }
		}

		CodeDescriptionPairList GetYearList(ZInt firstYearToShow)
		{
			var yearList = new CodeDescriptionPairList();

			var currentYear = firstYearToShow;
			while (currentYear >= 1885)
			{
				ZString yearString = currentYear.ToString();
				yearList.AddPair(yearString, yearString);
				currentYear--;
			}

			return yearList;
		}

		public MonthList MonthList
		{
			get { return Factory.GetCachedValue<MonthList>(); }
		}

		public CodeDescriptionPairList NumberTypes
		{
			get
			{
				return Factory.GetCachedValue("NHTSANumberTypes",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(ItemIdentityNumberQualifierList.Codes.EngineNumber, ItemIdentityNumberQualifierList.Descriptions.EngineNumber);
						result.AddPair(ItemIdentityNumberQualifierList.Codes.ModelNumber, ItemIdentityNumberQualifierList.Descriptions.ModelNumber);
						result.AddPair(ItemIdentityNumberQualifierList.Codes.SerialNumber, ItemIdentityNumberQualifierList.Descriptions.SerialNumber);
						result.AddPair(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, ItemIdentityNumberQualifierList.Descriptions.VehicleIdentificationNumberVIN);
						return result;
					});
			}
		}

		public CodeDescriptionPairList CategoryCodes
		{
			get
			{
				var parent = ((USNHTSADetailsAddInfo)Parent).Details;
				return Factory.GetCachedValue("ImportCodeList" + parent.US_NHTCategoryType,
					delegate
					{
						var result = new CodeDescriptionPairList();
						switch (parent.US_NHTCategoryType)
						{
							case CategoryTypeCodeList.Codes.NHTSACategoryTypeCodeMVSTYP:
								result = new NHTSACategoryCode_MVSTYPList();
								break;
							case CategoryTypeCodeList.Codes.NHTSACategoryTypeCodeREITYP:
								result = new NHTSACategoryCode_REITYPList();
								break;
							case CategoryTypeCodeList.Codes.NHTSACategoryTypeCodeTPETYP:
								result = new NHTSACategoryCode_TPETYPList();
								break;
							case CategoryTypeCodeList.Codes.NHTSACategoryTypeCodeOEITYP:
								result = new NHTSACategoryCode_OEITYPList();
								break;
							case CategoryTypeCodeList.Codes.NHTSACategoryTypeCodeOFFTYP:
								result = new NHTSACategoryCode_OFFTYPList();
								break;
						}

						return result;
					});
			}
		}

		public DriverSideList DriveSides
		{
			get { return Factory.GetCachedValue<DriverSideList>(); }
		}

		public NHTSALPCOTypeList LPCOTypes
		{
			get { return Factory.GetCachedValue<NHTSALPCOTypeList>(); }
		}

		public LPCODateQualifierList LPCODateTypes
		{
			get { return Factory.GetCachedValue<LPCODateQualifierList>(); }
		}
	}
}
