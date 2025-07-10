using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(EntryFiler))]
	sealed class EntryFilerTest : RegistryBusinessObjectTemplateTestCase<EntryFiler>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override EntryFiler GetBusinessObjectToClone()
		{
			EntryFiler result = new EntryFiler();
			result.IsABICertified = true;
			result.EntryFilerCode = "SV1";
			return result;
		}

		protected override EntryFiler GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
		#endregion
	}
}
