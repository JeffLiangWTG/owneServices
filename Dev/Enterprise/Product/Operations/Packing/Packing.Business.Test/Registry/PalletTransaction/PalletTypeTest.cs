using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PalletType))]
	internal class PalletTypeTest : RegistryBusinessObjectTestCaseBase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new PalletType();
			result.Code = "ABC";
			result.Description = (NoResString)"Hello";
			result.ProviderCode = "XYZ";
			result.EquipmentCode = "DEF";
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
