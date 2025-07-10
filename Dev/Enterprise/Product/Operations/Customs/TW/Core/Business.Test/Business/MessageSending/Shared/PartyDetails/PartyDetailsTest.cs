using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PartyDetailsTestsTest : TestCaseWithFactory
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(PartyDetails.ID, NUnit.Framework.Is.EqualTo("9569962").Using(CustomComparers.TypeComparison));
			var customsCodes = orgHeader.CustomsCodes;
			customsCodes.RemoveAndDelete(customsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			NUnit.Framework.Assert.That(PartyDetails.ID, NUnit.Framework.Is.EqualTo("G3429453").Using(CustomComparers.TypeComparison));
			customsCodes.RemoveAndDelete(customsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			NUnit.Framework.Assert.That(PartyDetails.ID, NUnit.Framework.Is.EqualTo("5342164").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(PartyDetails.Name, NUnit.Framework.Is.EqualTo("Valveo Group").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(PartyDetails.ChineseName, NUnit.Framework.Is.EqualTo("Valveo集團").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(PartyDetails.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			var customsCodes = orgHeader.CustomsCodes;
			customsCodes.RemoveAndDelete(customsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			NUnit.Framework.Assert.That(PartyDetails.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			customsCodes.RemoveAndDelete(customsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			NUnit.Framework.Assert.That(PartyDetails.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(PartyDetails.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(PartyDetails.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(PartyDetails.RoleCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(PartyDetails.SubBoxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(PartyDetails.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(PartyDetails.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		#region IAddress
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(PartyAddress.Line, NUnit.Framework.Is.EqualTo("4F., NO. 190, SONG JIANG RD., TAIPEI TAIWAN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseLine()
		{
			NUnit.Framework.Assert.That(PartyAddress.ChineseLine, NUnit.Framework.Is.EqualTo("190號4樓台北松江路").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(PartyAddress.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionID()
		{
			NUnit.Framework.Assert.That(PartyAddress.CountrySubDivisionID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionName()
		{
			NUnit.Framework.Assert.That(PartyAddress.CountrySubDivisionName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		#region ILPCOAuthorizedParty
		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyID()
		{
			NUnit.Framework.Assert.That(PartyDetails.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-F344463").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyName()
		{
			NUnit.Framework.Assert.That(PartyDetails.LPCOAuthorizedParty.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyTypeCode()
		{
			NUnit.Framework.Assert.That(PartyDetails.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "TWTPE";
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			orgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			orgAddress.OA_IsActive = true;
			orgAddress.OA_Language = Core.SharedConstants.Languages.English;
			orgAddress.OA_CompanyNameOverride = "Valveo Group";
			orgAddress.OA_Address1 = "4F., No. 190,";
			orgAddress.OA_Address2 = "Song Jiang Rd., Taipei";
			var translatedAddress = orgAddress.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			translatedAddress.Address1 = "190號4樓";
			translatedAddress.Address2 = "台北松江路";
			translatedAddress.CompanyName = "Valveo集團";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "G3429453", Core.Constants.CountryCodes.Taiwan);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "9569962", Core.Constants.CountryCodes.Taiwan);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "5342164", Core.Constants.CountryCodes.Taiwan);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.TPC, "7424324", Core.Constants.CountryCodes.Taiwan);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "F344463", Core.Constants.CountryCodes.Taiwan);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "D33431347", Core.Constants.CountryCodes.Taiwan);
		}

		IPartyDetails PartyDetails => new PartyDetails(declaration, orgHeader);
		IAddress PartyAddress => PartyDetails.Address;
		OrgHeader orgHeader;
		JobDeclaration declaration;
	}
}
