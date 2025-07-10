using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PatternMatchingAddress))]
	sealed class PatternMatchingAddressTest : EnterpriseBusinessObjectTestCase
	{
		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var address = factory.NewWithValidTestData<PatternMatchingAddress>();
			address.ParentTableCode = "GS";
			address.PMA_ParentId = ZGuid.NewZGuid();
			address.PMA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			factory.Save();

			return address;
		}
	}
}
