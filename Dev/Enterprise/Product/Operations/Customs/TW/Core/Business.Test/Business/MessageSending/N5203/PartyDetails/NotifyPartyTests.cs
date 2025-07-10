using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NotifyPartyTestsTest : TestCaseWithFactory
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(Notify.ID, NUnit.Framework.Is.EqualTo("N222222").Using(CustomComparers.TypeComparison));
			notifyParty.CustomsCodes.RemoveAndDelete(notifyParty.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			NUnit.Framework.Assert.That(Notify.ID, NUnit.Framework.Is.EqualTo("NON111111").Using(CustomComparers.TypeComparison));
			notifyParty.CustomsCodes.RemoveAndDelete(notifyParty.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			NUnit.Framework.Assert.That(Notify.ID, NUnit.Framework.Is.EqualTo("N333333").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(Notify.Name, NUnit.Framework.Is.EqualTo("Notify company name xxx.").Using(CustomComparers.TypeComparison));
			notifyParty.MainAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			NUnit.Framework.Assert.That(Notify.Name, NUnit.Framework.Is.EqualTo("Notify company name(OTA).").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(Notify.ChineseName, NUnit.Framework.Is.EqualTo("通知人公司名稱(OTA).").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(Notify.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			notifyParty.CustomsCodes.RemoveAndDelete(notifyParty.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			NUnit.Framework.Assert.That(Notify.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			notifyParty.CustomsCodes.RemoveAndDelete(notifyParty.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			NUnit.Framework.Assert.That(Notify.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(Notify.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(Notify.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void RoleCode()
		{
			NUnit.Framework.Assert.That(Notify.RoleCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void SubBoxID()
		{
			NUnit.Framework.Assert.That(Notify.SubBoxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(Notify.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCOAuthorizedParty)));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(Notify.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(Notify.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		#region IAddress
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(NotifyAddress.Line, NUnit.Framework.Is.EqualTo("903/50 CLARENCE ST, SYDNEY NSW 2000. TAIWAN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseLine()
		{
			NUnit.Framework.Assert.That(NotifyAddress.ChineseLine, NUnit.Framework.Is.EqualTo("台北市松山區民權東路四段342號").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(NotifyAddress.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionID()
		{
			NUnit.Framework.Assert.That(NotifyAddress.CountrySubDivisionID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionName()
		{
			NUnit.Framework.Assert.That(NotifyAddress.CountrySubDivisionName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			notifyParty = testHelper.CreateOrganizationForNotifyParty();
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			Factory.Save();
		}

		OrgHeader notifyParty;
		CusEntryHeader entryHeader;
		IPartyDetails Notify => new NotifyParty(entryHeader.Declaration, notifyParty);
		IAddress NotifyAddress => Notify.Address;
	}
}
