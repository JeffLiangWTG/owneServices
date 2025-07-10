//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusCodeListAttributeCombinedLookups
//
//    This class should be used for overriding collections in AutoZZRefCusCodeListAttributeCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListAttributeCombinedLookups : AutoZZRefCusCodeListAttributeCombinedLookups
	{
		public ZZRefCusCodeListAttributeCombinedLookups(AutoZZRefCusCodeListAttributeCombined parent)
			: base(parent)
		{
		}

		protected new ZZRefCusCodeListAttributeCombined Parent
		{
			get { return (ZZRefCusCodeListAttributeCombined)base.Parent; }
		}

		public IBusinessObjectCollection NameList
		{
			get
			{
				var codeType = ZString.Empty;
				var country = ZString.Empty;
				var codeList = Parent.CodeList;
				if (codeList != null)
				{
					codeType = codeList.ZZD_CodeType;
					country = codeList.ZZD_CountryOrGrouping;
				}
				return RefCusCodeListAttributeNameList.GetList(Factory, codeType, country);
			}
		}

		public ICodeDescriptionPairList ValueList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var codeListAttributeName = Parent.CodeListAttributeName;
				if (codeListAttributeName != null)
				{
					var valueDataType = codeListAttributeName.ZXE_ValueDataType.ToUpperInvariant();
					if (valueDataType.IsEmpty || valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.String)
					{
						var codeTypeForValueList = codeListAttributeName.ZXE_ZZK_NKCodeTypeForValueList;
						if (!codeTypeForValueList.IsEmpty)
						{
							result = RefCusCodeListTypes.GetCachedList(Factory, Parent.CodeList.ZZD_CountryOrGrouping, codeTypeForValueList, ZDateTime.Today);
						}
					}
					else if (valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean)
					{
						result = Factory.GetCachedValue<YesNoList>();
					}
				}
				return result;
			}
		}
	}
}
