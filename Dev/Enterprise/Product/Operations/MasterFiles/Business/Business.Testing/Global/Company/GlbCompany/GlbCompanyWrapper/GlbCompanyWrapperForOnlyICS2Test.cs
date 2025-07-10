using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapperForOnlyICS2))]
	class GlbCompanyWrapperForOnlyICS2Test : GlbCompanyWrapperTest<GlbCompanyWrapperForOnlyICS2>
	{
		protected override ZString CompanyCountryCode => Enterprise.Core.Constants.CountryCodes.France;
	}
}
