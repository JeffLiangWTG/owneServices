//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFSISLotAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSFSISLotAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USFSISLotAddInfoLookups : AutoUSFSISLotAddInfoLookups
	{
		public USFSISLotAddInfoLookups(AutoUSFSISLotAddInfo parent) : base(parent)
		{
		}

		new USFSISLotAddInfo Parent
		{
			get { return (USFSISLotAddInfo)base.Parent; }
		}

		public ShippingOrPackingingUnitList PackingTypes
		{
			get { return Factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}

		public CodeDescriptionPairList WeightUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public FSISProductQualifierCodeList ProductQualifierCodes
		{
			get { return Factory.GetCachedValue<FSISProductQualifierCodeList>(); }
		}

		public FSISProductSpeciesNameList ProductSpeciesNames
		{
			get { return Factory.GetCachedValue<FSISProductSpeciesNameList>(); }
		}

		public CodeDescriptionPairList ProductCharacteristics
		{
			get
			{
				return Factory.GetCachedValue("ProductCharacteristics" + Parent.US_ProductQualifierCode, delegate
				{
					switch (Parent.US_ProductQualifierCode)
					{
						case FSISProductQualifierCodeList.Codes.EEP:
							return new EEPCharacteristicList();
						case FSISProductQualifierCodeList.Codes.FCNS:
							return new FCNSCharacteristicList();
						case FSISProductQualifierCodeList.Codes.HTSS:
							return new HTSSCharacteristicList();
						case FSISProductQualifierCodeList.Codes.NFC:
							return new NFCCharacteristicList();
						case FSISProductQualifierCodeList.Codes.NHTS:
							return new NHTSCharacteristicList();
						case FSISProductQualifierCodeList.Codes.PWSI:
							return new PWSICharacteristicList();
						case FSISProductQualifierCodeList.Codes.RPI:
							return new RPICharacteristicList();
						case FSISProductQualifierCodeList.Codes.RPNI:
							return new RPNICharacteristicList();
						case FSISProductQualifierCodeList.Codes.TPCS:
							return new TPCSCharacteristicList();
						default:
							return new CodeDescriptionPairList();
					}
				});
			}
		}

		public USCCountryCollection USCountryList
		{
			get { return new USCCountryCollection(Factory); }
		}
	}
}
