using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestJZ_IncoTerm_List()
		{
			AssertEquals(typeof(IncoTermList), Lookups.JZ_IncoTerm_List.GetType());
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.CarriageAndInsurancePaidTo));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.CarriagePaidTo));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.CostAndFreight));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.CostInsuranceAndFreight));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.DeliveredAtPlace));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.DeliveredAtTerminal));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.DeliveredAtPlaceUnloaded));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.DeliveredDutyPaid));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.ExWorks));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.FreeAlongsideShip));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.FreeCarrier));
			Assert(Lookups.JZ_IncoTerm_List.ContainsCode(IncoTermList.Codes.FreeOnBoard));
		}

		public void TestJZ_ExchangeRateIndicator_List()
		{
			AssertEquals(typeof(ExchangeRateIndicatorList), Lookups.JZ_ExchangeRateIndicator_List.GetType());
		}

		public void TestJZ_RelationshipIndicator_List()
		{
			AssertEquals(typeof(RelationshipIndicatorList), Lookups.JZ_RelationshipIndicator_List.GetType());
		}

		public void TestCountryList()
		{
			AssertEquals(typeof(RefCountryCollection), Lookups.CountryList.GetType());
		}

		public void TestQualifiesForPreferentialDutyList()
		{
			AssertEquals(typeof(QualifiesForPreferentialDutyList), Lookups.QualifiesForPreferentialDutyList.GetType());
		}

		public void TestYesNoList()
		{
			AssertEquals(typeof(YesNoList), Lookups.YesNoList.GetType());
		}

		#region Implementation
		protected JobComInvoiceHeaderLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = Factory.New<JobComInvoiceHeader>().Lookups;
				}
				return fLookups;
			}
		}
		JobComInvoiceHeaderLookups fLookups;
		#endregion
	}
}
