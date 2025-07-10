using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusTWControllingMessageHeaderNX101LookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestProcessingUnitListWhenMessageTypeIsNX101()
		{
			NUnit.Framework.Assert.That(lookups.ProcessingUnitList, NUnit.Framework.Is.SameAs(TWRefCusCodeListTypes.GetProcessingUnitList(Factory, cusTWControllingMessageHeader.TW1_CertificateType, cusTWControllingMessageHeader.EntryInstruction?.DateOfValuation ?? ZDateTime.Today, true)));
		}

		[ExpectNoExceptions]
		public void TestCertificateTypeList()
		{
			NUnit.Framework.Assert.That(lookups.CertificateTypeList, NUnit.Framework.Is.SameAs(RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginType, ZDateTime.Today)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			declartion = Factory.NewWithValidTestData<JobDeclaration>();
			cusTWControllingMessageHeader = declartion.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			cusTWControllingMessageHeader.TW1_ControllingMessageType = "NX101";
			lookups = cusTWControllingMessageHeader.Lookups;
		}

		JobDeclaration declartion;
		CusTWControllingMessageHeaderLookups lookups;
		CusTWControllingMessageHeader cusTWControllingMessageHeader;
	}
}
