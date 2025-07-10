//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSVehicleAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSVehicleAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USVehicleAddInfoLookups : AutoUSVehicleAddInfoLookups
	{
		public USVehicleAddInfoLookups(AutoUSVehicleAddInfo parent) : base(parent)
		{
		}

		public EPAVNEDocumentIdentifierList FormTypeList
		{
			get { return Factory.GetCachedValue<EPAVNEDocumentIdentifierList>(); }
		}

		public IndustryCodesList IndustryCodeList
		{
			get { return Factory.GetCachedValue<IndustryCodesList>(); }
		}

		public CodeDescriptionPairList ImportCodeList
		{
			get
			{
				var parent = ((USVehicleAddInfo)Parent).Parent;
				return Factory.GetCachedValue("ImportCodeList" + parent.DocumentIdentifier,
					delegate
					{
						var result = new CodeDescriptionPairList();
						if (parent.US_FormType == EPAVNEDocumentIdentifierList.Codes.EPA3520_21)
						{
							result = new ImportCodesForm3520_21List();
						}
						else
						{
							result = new ImportCodesForm3520_1List();
						}
						return result;
					});
			}
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public CodeDescriptionPairList BodyTypeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("BodyTypeList",
					delegate
					{
						var result = new CommodityVehicleQualifierCodesList();
						result.RemoveCode(CommodityVehicleQualifierCodesList.Codes.V01);
						result.RemoveCode(CommodityVehicleQualifierCodesList.Codes.V03);
						result.RemoveCode(CommodityVehicleQualifierCodesList.Codes.V04);
						result.RemoveCode(CommodityVehicleQualifierCodesList.Codes.V05);
						result.RemoveCode(CommodityVehicleQualifierCodesList.Codes.V06);
						return result;
					});
			}
		}

		public CodeDescriptionPairList BodyCodeList
		{
			get
			{
				var parent = ((USVehicleAddInfo)Parent).Parent;
				return Factory.GetCachedValue("BodyCodeList" + parent.US_BodyType,
					delegate
					{
						var result = new CodeDescriptionPairList();
						if (parent.US_BodyType == CommodityVehicleQualifierCodesList.Codes.V00)
						{
							result = new BodyTypeOver1TonList();
						}
						else
						{
							result = new BodyTypeUnder1TonList();
						}
						return result;
					});
			}
		}

		public CodeDescriptionPairList USStatesList
		{
			get { return Factory.GetCachedValue<USStatesList>(); }
		}

		public CodeDescriptionPairList YesNoList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("YesNoList",
				delegate
				{
					var result = new YesNoDefaultList();
					result.RemoveCode(YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		public EnginePowerUQList EnginePowerUQ
		{
			get { return Factory.GetCachedValue<EnginePowerUQList>(); }
		}

		public DriverSideList DrvSideList
		{
			get
			{
				return Factory.GetCachedValue("DrvSideList",
				delegate
				{
					var result = new DriverSideList();
					result.RemoveCode(DriverSideList.Codes.N);
					return result;
				});
			}
		}

		public CodeDescriptionPairList VNECertifyingIndividualList
		{
			get { return PartyTypeList.GetListForVNECertifyingIndividual(Factory); }
		}
	}
}
