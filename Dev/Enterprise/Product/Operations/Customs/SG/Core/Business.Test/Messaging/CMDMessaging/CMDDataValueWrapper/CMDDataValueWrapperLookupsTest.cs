using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	class CMDDataValueWrapperLookupsTest : TestCaseWithFactory
	{
		public void TestEntryTypeList()
		{
			AssertEquals(17, CMDDataValueWrapper.Lookups.EntryTypeList.Count);
			AssertEquals(SGConstants.Permit, CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.Permit));
			AssertEquals("Certificate", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.Certificate));
			AssertEquals("Transhipment (includes re-documentation cargo)", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.TS));
			AssertEquals("Unaccompanied, non-controlled goods with total value not exceeding $400", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.UA));
			AssertEquals("Not prohibited under regulation 6 of the Imports and Exports Regulations 1995", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.PP));
			AssertEquals("Diplomatic correspondence", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.DP));
			AssertEquals("By joint defense force, excluding civilian motor vehicles", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.MD));
			AssertEquals("By the MFA, excluding motor vehicles", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.MF));
			AssertEquals("Used motor vehicles covered by Carnet de Passage endorsed by the Automobile Association of Singapore", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.CA));
			AssertEquals("Goods covered with an ATA Carnet", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.AT));
			AssertEquals("Bona fide trade samples not exceeding $400", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.SP));
			AssertEquals("Commercial, shipping or airline documents", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.CD));
			AssertEquals("Press photographs or negatives, news write-ups, news clippings, news films or news transcription tapes", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.PM));
			AssertEquals("Human corpses, human remains, human bones or cremated ashes", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.HC));
			AssertEquals("Human transplant materials", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.HT));
			AssertEquals("Pets", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.PT));
			AssertEquals("Others", CMDDataValueWrapper.Lookups.EntryTypeList.GetDescriptionFromCode(CustomsEntryTypeList.Singapore.SGExemption.Codes.ZZ));
		}

		CMDDataValueWrapper CMDDataValueWrapper
		{
			get
			{
				if (fCMDDataValueWrapper == null)
				{
					var cMDPermitNum = Factory.New<CMDPermitNumber>();
					fCMDDataValueWrapper = new CMDDataValueWrapper(cMDPermitNum);
				}

				return fCMDDataValueWrapper;
			}
		}

		CMDDataValueWrapper fCMDDataValueWrapper;
	}
}
