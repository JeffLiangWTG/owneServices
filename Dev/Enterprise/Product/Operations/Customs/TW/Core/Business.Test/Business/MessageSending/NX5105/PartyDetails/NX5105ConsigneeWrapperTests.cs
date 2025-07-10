using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105ConsigneeWrapper))]
	sealed class NX5105ConsigneeWrapperTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			consigneeAddress.E2_GovRegNum = "123456789";
			NUnit.Framework.Assert.That(consignee.ID, NUnit.Framework.Is.EqualTo("123456789").Using(CustomComparers.TypeComparison));
			consigneeAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			consigneeAddress.E2_GovRegNum = "123456789";
			NUnit.Framework.Assert.That(consignee.ID, NUnit.Framework.Is.EqualTo("NO123456789").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			consigneeAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
			NUnit.Framework.Assert.That(consignee.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			consigneeAddress.E2_GovRegNumType = Constants.CCPPrefix;
			NUnit.Framework.Assert.That(consignee.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			consigneeAddress.E2_GovRegNumType = OrgCusCode.TaiwanCodeTypes.PID;
			NUnit.Framework.Assert.That(consignee.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison));
			consigneeAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			NUnit.Framework.Assert.That(consignee.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			consigneeAddress = declaration.ConsigneeDocumentaryAddress;
		}

		TWConsigneeAddress consigneeAddress;
		JobDeclaration declaration;
		IPartyDetails consignee => new NX5105ConsigneeWrapper(declaration, consigneeAddress);
	}
}
