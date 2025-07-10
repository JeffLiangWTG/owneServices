using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconFlattenedDataLine))]
	sealed class ReconFlattenedDataLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSecondSPIMaxLength()
		{
			var dataLine = new ReconFlattenedDataLine();
			AssertEquals(1, dataLine.ReconSecondarySPIInfo.MaxLength);
		}

		public void TestEntryNumber()
		{
			ReconFlattenedDataLine dataLine = new ReconFlattenedDataLine();
			dataLine.EntryNumber = "XXX 12345-7";
			AssertEquals("non-alphanumeric letters are removed", "XXX123457", dataLine.EntryNumber);
		}

		public void TestTariffNumbers()
		{
			ReconFlattenedDataLine dataLine = new ReconFlattenedDataLine();
			dataLine.OriginalTariff = "8517.11.1234";
			dataLine.ReconTariff = "8517.12.5678";
			dataLine.OriginalSupTariff = "8517.11.9101";
			dataLine.ReconSupTariff = "8517.12.1213";
			AssertEquals("OriginalTariff trimmed", "8517111234", dataLine.OriginalTariff);
			AssertEquals("ReconTariff trimmed", "8517125678", dataLine.ReconTariff);
			AssertEquals("OriginalSupTariff trimmed", "8517119101", dataLine.OriginalSupTariff);
			AssertEquals("ReconSupTariff trimmed", "8517121213", dataLine.ReconSupTariff);
			AssertEquals("FormattedOriginalTariff trimmed", "8517.11.1234", dataLine.OriginalFormattedTariff);
			AssertEquals("FormattedReconTariff trimmed", "8517.12.5678", dataLine.ReconFormattedTariff);
			AssertEquals("FormattedOriginalSupTariff trimmed", "8517.11.9101", dataLine.OriginalFormattedSupTariff);
			AssertEquals("FormattedReconSupTariff trimmed", "8517.12.1213", dataLine.ReconFormattedSupTariff);
			dataLine.OriginalTariff = "8517.11.12341";
			dataLine.ReconTariff = "8517.12.56781";
			dataLine.OriginalSupTariff = "8517.11.910111";
			dataLine.ReconSupTariff = "8517.12.12131";
			AssertEquals("OriginalTariff trimmed", "8517111234", dataLine.OriginalTariff);
			AssertEquals("ReconTariff trimmed", "8517125678", dataLine.ReconTariff);
			AssertEquals("OriginalSupTariff trimmed", "8517119101", dataLine.OriginalSupTariff);
			AssertEquals("ReconSupTariff trimmed", "8517121213", dataLine.ReconSupTariff);
			AssertEquals("FormattedOriginalTariff trimmed", "8517.11.1234", dataLine.OriginalFormattedTariff);
			AssertEquals("FormattedReconTariff trimmed", "8517.12.5678", dataLine.ReconFormattedTariff);
			AssertEquals("FormattedOriginalSupTariff trimmed", "8517.11.9101", dataLine.OriginalFormattedSupTariff);
			AssertEquals("FormattedReconSupTariff trimmed", "8517.12.1213", dataLine.ReconFormattedSupTariff);
			dataLine.OriginalFormattedTariff = "8517.11.1234";
			dataLine.ReconFormattedTariff = "8517.12.5678";
			dataLine.OriginalFormattedSupTariff = "8517.11.9101";
			dataLine.ReconFormattedSupTariff = "8517.12.1213";
			AssertEquals("OriginalTariff trimmed", "8517111234", dataLine.OriginalTariff);
			AssertEquals("ReconTariff trimmed", "8517125678", dataLine.ReconTariff);
			AssertEquals("OriginalSupTariff trimmed", "8517119101", dataLine.OriginalSupTariff);
			AssertEquals("ReconSupTariff trimmed", "8517121213", dataLine.ReconSupTariff);
			AssertEquals("FormattedOriginalTariff trimmed", "8517.11.1234", dataLine.OriginalFormattedTariff);
			AssertEquals("FormattedReconTariff trimmed", "8517.12.5678", dataLine.ReconFormattedTariff);
			AssertEquals("FormattedOriginalSupTariff trimmed", "8517.11.9101", dataLine.OriginalFormattedSupTariff);
			AssertEquals("FormattedReconSupTariff trimmed", "8517.12.1213", dataLine.ReconFormattedSupTariff);
			dataLine.OriginalFormattedTariff = "8517.11.12341";
			dataLine.ReconFormattedTariff = "8517.12.56781";
			dataLine.OriginalFormattedSupTariff = "8517.11.910111";
			dataLine.ReconFormattedSupTariff = "8517.12.12131";
			AssertEquals("OriginalTariff trimmed", "8517111234", dataLine.OriginalTariff);
			AssertEquals("ReconTariff trimmed", "8517125678", dataLine.ReconTariff);
			AssertEquals("OriginalSupTariff trimmed", "8517119101", dataLine.OriginalSupTariff);
			AssertEquals("ReconSupTariff trimmed", "8517121213", dataLine.ReconSupTariff);
			AssertEquals("FormattedOriginalTariff trimmed", "8517.11.1234", dataLine.OriginalFormattedTariff);
			AssertEquals("FormattedReconTariff trimmed", "8517.12.5678", dataLine.ReconFormattedTariff);
			AssertEquals("FormattedOriginalSupTariff trimmed", "8517.11.9101", dataLine.OriginalFormattedSupTariff);
			AssertEquals("FormattedReconSupTariff trimmed", "8517.12.1213", dataLine.ReconFormattedSupTariff);
		}
	}
}
