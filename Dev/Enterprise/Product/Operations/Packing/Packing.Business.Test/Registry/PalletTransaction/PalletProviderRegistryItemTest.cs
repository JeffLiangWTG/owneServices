using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PalletProviderRegistryItem))]
	public class PalletProviderRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<PalletTypeParent>
	{
		protected override StronglyTypedRegistryItem<PalletTypeParent, PalletTypeParent> GetNewRegistryItem()
		{
			return new PalletProviderRegistryItem("", null, null, null, new PalletTypeParent());
		}

		protected override PalletTypeParent ValidValue
		{
			get
			{
				var result = new PalletTypeParent();
				var item = result.Types.AddNew();
				item.Code = "ABC";
				item.Description = (NoResString)"Hello";
				item.ProviderCode = "XYZ";
				item.EquipmentCode = "DEF";
				return result;
			}
		}
	}
}
