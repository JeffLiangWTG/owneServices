namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	class DA66DA63DocumentWrapperTest : DA63DocumentWrapperTest
	{
		public void TestMRN()
		{
			invoiceLine1.JI_PreviousEntryNumber = "EN1234";
			var wrapper = new DA66DA63DocumentWrapper(entryHeader);
			AssertEquals("EN1234", wrapper.MRN);
		}

		public void TestRefundDrawbackItemNumber()
		{
			invoiceLine1.CusLineTariffDetails.RemoveAndDeleteAll();
			{
				var wrapper = new DA66DA63DocumentWrapper(entryHeader);
				AssertNullOrEmpty(wrapper.RefundDrawbackItemNumber);
			}
			{
				var lineTariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
				lineTariffDetail.BZ_Type = "1P0";
				lineTariffDetail.BZ_Tariff = "80186008";
				var wrapper = new DA66DA63DocumentWrapper(entryHeader);
				AssertNullOrEmpty(wrapper.RefundDrawbackItemNumber);
			}
			{
				var lineTariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
				lineTariffDetail.BZ_Type = "5P1";
				lineTariffDetail.BZ_Tariff = "80286012";
				var wrapper = new DA66DA63DocumentWrapper(entryHeader);
				AssertEquals("80286", wrapper.RefundDrawbackItemNumber);
			}
			{
				var lineTariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
				lineTariffDetail.BZ_Type = "5P1";
				lineTariffDetail.BZ_Tariff = "80386020";
				var wrapper = new DA66DA63DocumentWrapper(entryHeader);
				AssertEquals("80286", wrapper.RefundDrawbackItemNumber);
			}
		}

		public void TestB3Values()
		{
			invoiceLine2.JI_Procedure = "00YY";
			invoiceLine1.JI_ImportDutyPaid = 1024.01m;
			invoiceLine1.JI_ImportVATPaid = 4096.01m;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12B", 1.01m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("12B", 2.02m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12A", 8.08m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("12A", 16.16m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("2P1", 32.01m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("2P2", 64.01m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("15B", 128.01m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("15A", 256.01m);

			var wrapper = new DA66DA63DocumentWrapper(entryHeader);
			CombineAssertions(() =>
			{
				AssertEquals("B3CustomsDuty", 1024.01m, wrapper.B3CustomsDuty);
				AssertEquals("B3ExciseDuty", 8.08m + 16.16m, wrapper.B3ExciseDuty);
				AssertEquals("B3AntiDumpingDuty", 32.01m + 64.01m, wrapper.B3AntiDumpingDuty);
				AssertEquals("B3DutySch1P2B", 1.01m + 2.02m, wrapper.B3DutySch1P2B);
				AssertEquals("B3VAT", 4096.01m, wrapper.B3VAT);
				AssertEquals("B3Other", 384.02m, wrapper.B3Other);
			});
		}

		public void TestDetailsOfAmounts()
		{
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12A", 1.01m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12B", 2.02m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("2P1", 3.03m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("15A", 4.04m);

			var wrapper = new DA66DA63DocumentWrapper(entryHeader);
			AssertEquals(4, wrapper.DetailsOfAmounts.Count);
			AssertEquals("12A", wrapper.DetailsOfAmounts[0].Code);
			AssertEquals("12B", wrapper.DetailsOfAmounts[1].Code);
			AssertEquals("2P1", wrapper.DetailsOfAmounts[2].Code);
			AssertEquals("15A", wrapper.DetailsOfAmounts[3].Code);
			AssertEquals(1.01m, wrapper.DetailsOfAmounts[0].Value);
			AssertEquals(2.02m, wrapper.DetailsOfAmounts[1].Value);
			AssertEquals(3.03m, wrapper.DetailsOfAmounts[2].Value);
			AssertEquals(4.04m, wrapper.DetailsOfAmounts[3].Value);
		}

		public void TestDA63EntryLines_DA63LineNumber()
		{
			invoiceLine3.JI_Procedure = "00YY";
			var wrapper = new DA66DA63DocumentWrapper(entryHeader);
			AssertEquals("Pre-requisite: DA63EntryLines.Count", 2, wrapper.DA63EntryLines.Count);
			AssertEquals("1", wrapper.DA63EntryLines[0].DA63LineNumber);
			AssertEquals("2", wrapper.DA63EntryLines[1].DA63LineNumber);
		}
	}
}
