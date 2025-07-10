using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105ConsignorWrapper))]
	sealed class NX5105ConsignorWrapperTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			consignorAddress.E2_GovRegNum = "123456789";
			NUnit.Framework.Assert.That(consignor.ID, NUnit.Framework.Is.EqualTo("123456789").Using(CustomComparers.TypeComparison));
			consignorAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			consignorAddress.E2_GovRegNum = "123456789";
			NUnit.Framework.Assert.That(consignor.ID, NUnit.Framework.Is.EqualTo("NO123456789").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			consignorAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
			NUnit.Framework.Assert.That(consignor.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			consignorAddress.E2_GovRegNumType = Constants.CCPPrefix;
			NUnit.Framework.Assert.That(consignor.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			consignorAddress.E2_GovRegNumType = OrgCusCode.TaiwanCodeTypes.PID;
			NUnit.Framework.Assert.That(consignor.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison));
			consignorAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			NUnit.Framework.Assert.That(consignor.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			consignorAddress = declaration.ConsignorDocumentaryAddress;
		}

		TWConsignorAddress consignorAddress;
		JobDeclaration declaration;
		IPartyDetails consignor => new NX5105ConsignorWrapper(declaration, consignorAddress);
	}
}
