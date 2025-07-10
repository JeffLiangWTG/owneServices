using System.Linq;
using System.Reflection;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	class OrderManagerRequestMappingCodeTest : TestCase
	{
		public void TestRequestMappingCode()
		{
			var codes = typeof(RequestMappingCodes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(x => x.IsLiteral && !x.IsInitOnly).Select(x => x.GetRawConstantValue() as string).ToArray();

			AssertArrayEqualsByElements(
				"RequestMappingCodes constant must have 16 fixed codes",
				new string[] { "CDS", "CDE", "CDR", "BKQ", "TRM", "POL", "OVW", "MIV", "MAV", "PAR", "FBO", "DSL", "PQE", "BQP", "UNC", "MRS" },
				codes);
		}
	}
}
