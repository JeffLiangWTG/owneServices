using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclaration()
		{
			AssertEquals(Declaration.Lookups.Declaration, Declaration);
		}

		public void TestMessageTypeList()
		{
			var list = Lookups.MessageTypeList;
			var list2 = Lookups.MessageTypeList;
			AssertEquals(true, object.ReferenceEquals(list2, list));
			Assert(list.ContainsCode(MessageTypeCodeList.Codes.IPT));
			Assert(list.ContainsCode(MessageTypeCodeList.Codes.INP));
			Assert(list.ContainsCode(MessageTypeCodeList.Codes.OUT));
			Assert(list.ContainsCode(MessageTypeCodeList.Codes.TNP));
			Assert(list.ContainsCode(MessageTypeCodeList.Codes.COO));
		}

		public void TestCPCCollection()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "11", "11", "111", "One", "IPT", group: "GTR");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "", "22", "22", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure("AU", "", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "", "44", "44", "444", "Four", "IPT", group: "GTR");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			var cpcs = declaration.CPCCollection;
			AssertEquals("CPC Collection should have procedure codes filtered by MessageType & MessageSubType", 2, cpcs.Count);
			Assert("CPC Collection", cpcs.Contains(procedure1));
			Assert("CPC Collection", cpcs.Contains(procedure4));
		}

		public void TestSGApplicationCodeList()
		{
			var list = Lookups.ApplicationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "4.1, ITF", list.CodesAsString);
				AssertSame("Cached", list, Lookups.ApplicationCodeList);
				AssertEquals("4.1 Description", "TradeNet V4.1", list.GetDescriptionFromCode(JobApplicationCodeList.Codes.TradeNet41));
			});
		}

		//CargoIdTypeList
		public void TestPackingTypeList()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertEquals(typeof(CargoPackingTypeCodeList), Lookups.CargoIdTypeList.GetType());
			Assert(Lookups.CargoIdTypeList.ContainsCode(CargoPackingTypeCodeList.Codes.PackingType3));
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals(typeof(CargoPackingCodeList), Lookups.CargoIdTypeList.GetType());
			AssertEquals(false, Lookups.CargoIdTypeList.ContainsCode(CargoPackingTypeCodeList.Codes.PackingType3));
			Assert(Lookups.CargoIdTypeList.ContainsCode(CargoPackingCodeList.Codes.PackingType9));
		}

		public void TestJE_TotalNoOfPacksPackType_List()
		{
			AssertNotNull(Lookups.JE_TotalNoOfPacksPackType_List);
			Assert(Lookups.JE_TotalNoOfPacksPackType_List is UnitOfQuantityCodeList);
		}

		public void TestSupplyIndicators()
		{
			AssertNotNull(Lookups.SupplyIndicators);
			Assert(Lookups.SupplyIndicators.ContainsCode(SupplyIndicatorCodeList.Codes.Y));
		}

		public void TestExtensionOfPermitValidityCodes()
		{
			AssertNotNull(Lookups.ExtensionOfPermitValidityCodes);
			Assert(Lookups.ExtensionOfPermitValidityCodes.ContainsCode(CodeForExtensionOfPermitValidityCodeList.Codes.Y));
		}

		public void TestApplicationProductTypes()
		{
			AssertNotNull(Lookups.ApplicationProductTypes);
			Assert(Lookups.ApplicationProductTypes.ContainsCode(ApplicationProductTypeCodeList.Codes.NH));
		}

		public void TestCertificateTypes()
		{
			AssertNotNull(Lookups.CertificateTypes);
			Assert(Lookups.CertificateTypes.Count > 10);
		}

		public void TestSGLocos()
		{
			AssertNotNull(Lookups.SGLocoList);
			Assert(Lookups.SGLocoList is ZZRefCusCodeListCombinedCollection);
		}

		public void TestGetRelatedDeclarationsFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			SetDeclarationForMatch(declaration);
			var matchDeclaration = GetNewMatchingDec(declaration);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			AssertEquals(1, declaration.RelatedDeclarations.Count);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			SetDeclarationForMatch(declaration);
			matchDeclaration = GetNewMatchingDec(declaration);
			matchDeclaration.JE_HouseBill = ZString.Empty;
			matchDeclaration.JE_MasterBill = ZString.Empty;
			matchDeclaration.SG_OutwardHAWB = ZString.Empty;
			matchDeclaration.SG_OutwardMAWB = ZString.Empty;
			Factory.Save();
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration));
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			SetDeclarationForMatch(declaration);
			matchDeclaration = GetNewMatchingDec(declaration);
			var matchDeclaration1 = GetNewMatchingDec(declaration);
			matchDeclaration1.JE_HouseBill = ZString.Empty;
			matchDeclaration1.JE_MasterBill = ZString.Empty;
			matchDeclaration1.SG_OutwardHAWB = "HAWB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			SetDeclarationForMatch(declaration);
			matchDeclaration = GetNewMatchingDec(declaration);
			matchDeclaration1 = GetNewMatchingDec(declaration);
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_MasterBill = ZString.Empty;
			matchDeclaration.JE_HouseBill = ZString.Empty;
			matchDeclaration.JE_MasterBill = ZString.Empty;
			matchDeclaration1.SG_OutwardHAWB = "HAWB002";
			matchDeclaration1.SG_OutwardMAWB = "MAWB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			SetDeclarationForMatch(declaration);
			matchDeclaration = GetNewMatchingDec(declaration);
			matchDeclaration1 = GetNewMatchingDec(declaration);
			matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-11);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration1));
		}

		public void TestGlobalManifestStatusList()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(object.ReferenceEquals(declaration.Lookups.GlobalManifestStatusList, Lookups.GlobalManifestStatusList));
			AssertEquals("CN, CR, IP, NS", Lookups.GlobalManifestStatusList.CodesAsString);
		}

		JobDeclarationLookups Lookups => Declaration.Lookups;

		static void SetDeclarationForMatch(JobDeclaration declaration)
		{
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_HouseBill = "HB001";
			declaration.JE_MasterBill = "MB001";
			declaration.SG_OutwardHAWB = "HAWB001";
			declaration.SG_OutwardMAWB = "MAWB001";
		}

		JobDeclaration GetNewMatchingDec(JobDeclaration dec)
		{
			var newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_SystemCreateTimeUtc = dec.JE_SystemCreateTimeUtc.AddDays(4);
			newDeclaration.JE_MessageType = dec.JE_MessageType;
			newDeclaration.JE_HouseBill = dec.JE_HouseBill;
			newDeclaration.JE_MasterBill = dec.JE_MasterBill;
			newDeclaration.SG_OutwardHAWB = dec.SG_OutwardHAWB;
			newDeclaration.SG_OutwardMAWB = dec.SG_OutwardMAWB;
			return newDeclaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SGCertificateTypeHelper.CreateCertificateTypes(Factory);
			Factory.Save();
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}
