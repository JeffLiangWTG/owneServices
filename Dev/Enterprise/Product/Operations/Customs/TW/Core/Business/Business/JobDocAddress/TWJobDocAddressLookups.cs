using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TWJobDocAddressLookups : JobDocAddressLookups
	{
		public TWJobDocAddressLookups(TWJobDocAddress parent) : base(parent)
		{
			AddressCodeTypes = Parent.AddressCodeTypes;
		}

		public new TWJobDocAddress Parent => (TWJobDocAddress)base.Parent;

		protected readonly IAddressCodeTypes AddressCodeTypes;

		public CodeDescriptionPairList IDCodeTypeList => Factory.GetCachedValue($"{nameof(TWJobDocAddressLookups)}.{nameof(AddressCodeTypes.IDCodeTypes)}.{string.Join(",", AddressCodeTypes.IDCodeTypes)}", () => Factory.GetCodeTypeList(AddressCodeTypes.IDCodeTypes.ToArray()));

		public CodeDescriptionPairList AEOCodeTypeList => Factory.GetCachedValue($"{nameof(TWJobDocAddressLookups)}.{nameof(AddressCodeTypes.AEOCodeTypes)}.{string.Join(",", AddressCodeTypes.AEOCodeTypes)}", () => Factory.GetCodeTypeList(AddressCodeTypes.AEOCodeTypes.ToArray()));

		public CodeDescriptionPairList TPCCodeTypeList => Factory.GetCachedValue($"{nameof(TWJobDocAddressLookups)}.{nameof(AddressCodeTypes.TPCCodeTypes)}.{string.Join(",", AddressCodeTypes.TPCCodeTypes)}", () => Factory.GetCodeTypeList(AddressCodeTypes.TPCCodeTypes.ToArray()));

		public CodeDescriptionPairList CBPCodeTypeList => Factory.GetCachedValue($"{nameof(TWJobDocAddressLookups)}.{nameof(AddressCodeTypes.CBPCodeTypes)}.{string.Join(",", AddressCodeTypes.CBPCodeTypes)}", () => Factory.GetCodeTypeList(AddressCodeTypes.CBPCodeTypes.ToArray()));

		public CodeDescriptionPairList FRICodeTypeList => Factory.GetCachedValue($"{nameof(TWJobDocAddressLookups)}.{nameof(AddressCodeTypes.FRICodeTypes)}.{string.Join(",", AddressCodeTypes.FRICodeTypes)}", () => Factory.GetCodeTypeList(AddressCodeTypes.FRICodeTypes.ToArray()));

		public override CodeDescriptionPairList GovRegNumTypes => Factory.GetCachedValue(
			"Enterprise.Customs.TW.Business.TWJobDocAddressLookups.CodeTypeList",
			() =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(IDCodeTypeList);
				result.AddRange(AEOCodeTypeList);
				result.AddRange(CBPCodeTypeList);
				result.AddRange(TPCCodeTypeList);
				return result;
			}
		);
	}
}
