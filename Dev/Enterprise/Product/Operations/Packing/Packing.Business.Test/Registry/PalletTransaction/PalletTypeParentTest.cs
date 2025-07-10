using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PalletTypeParent))]
	class PalletTypeParentTest : RegistryBusinessObjectTemplateTestCase<PalletTypeParent>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PalletTypeParent GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override PalletTypeParent GetBusinessObjectToSerialise()
		{
			var record = new PalletTypeParent();
			var result = record.Types.AddNew();
			result.Code = "ABC";
			result.Description = (NoResString)"Hello";
			result.ProviderCode = "XYZ";
			result.EquipmentCode = "DEF";

			return record;
		}
	}
}
