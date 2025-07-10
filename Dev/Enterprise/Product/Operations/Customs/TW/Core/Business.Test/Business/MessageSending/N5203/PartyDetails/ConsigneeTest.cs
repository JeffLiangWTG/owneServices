using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(Consignee))]
	sealed class ConsigneeTest : TestCaseWithFactory
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

		[ExpectNoExceptions]
		public void TestName()
		{
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			NUnit.Framework.Assert.That(consignee.Name, NUnit.Framework.Is.EqualTo("Consignee Company Name Override").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			NUnit.Framework.Assert.That(consignee.ChineseName, NUnit.Framework.Is.EqualTo("收貨人 公司名稱").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(consignee.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(consignee.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void RoleCode()
		{
			NUnit.Framework.Assert.That(consignee.RoleCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void SubBoxID()
		{
			NUnit.Framework.Assert.That(consignee.SubBoxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(consignee.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCOAuthorizedParty)));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(consignee.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(consignee.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#region IAddress
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(consignee.Address.Line, NUnit.Framework.Is.EqualTo("CONSIGNEE ADDRESS1 CONSIGNEE ADDRESS2 TAIWAN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseLine()
		{
			NUnit.Framework.Assert.That(consignee.Address.ChineseLine, NUnit.Framework.Is.EqualTo("收貨人 忠孝東路 地址三段232號 地址2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(consignee.Address.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionID()
		{
			NUnit.Framework.Assert.That(consignee.Address.CountrySubDivisionID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionName()
		{
			NUnit.Framework.Assert.That(consignee.Address.CountrySubDivisionName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			declaration = testHelper.CreateEntryHeaderForN5203().Declaration;
			entryInstruction = declaration.CusEntryInstruction;
			var consigneeOrgHeader = testHelper.CreateOrganizationForConsignee();
			consigneeAddress = declaration.ConsigneeDocumentaryAddress;
			consigneeAddress.OrganisationPK = consigneeOrgHeader.PK;
			consigneeAddress.E2_OA_Address = consigneeOrgHeader.MainAddress.PK;
		}

		TWConsigneeAddress consigneeAddress;
		CusEntryInstruction entryInstruction;
		JobDeclaration declaration;
		IPartyDetails consignee => new Consignee(declaration, consigneeAddress);
	}
}
