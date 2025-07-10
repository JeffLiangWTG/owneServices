using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	public sealed class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.Taiwan;
			}
		}
	}
}
