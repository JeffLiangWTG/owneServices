using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(Consignor))]
	sealed class ConsignorTest : TestCaseWithFactory
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

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(consignor.Name, NUnit.Framework.Is.EqualTo("Consignor Company Name Override").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(consignor.ChineseName, NUnit.Framework.Is.EqualTo("發貨人 公司名稱").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(consignor.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(consignor.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(consignor.RoleCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(consignor.SubBoxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddress()
		{
			NUnit.Framework.Assert.That(consignor.Address, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IAddress)));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(consignor.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCOAuthorizedParty)));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(consignor.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(consignor.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			declaration = testHelper.CreateEntryHeaderForN5203().Declaration;
			var consignorOrgHeader = testHelper.CreateOrganizationForConsignor();
			consignorAddress = declaration.ConsignorDocumentaryAddress;
			consignorAddress.OrganisationPK = consignorAddress.PK;
			consignorAddress.E2_OA_Address = consignorOrgHeader.MainAddress.PK;
		}

		JobDeclaration declaration;
		TWConsignorAddress consignorAddress;
		IPartyDetails consignor => new Consignor(declaration, consignorAddress);
	}
}
