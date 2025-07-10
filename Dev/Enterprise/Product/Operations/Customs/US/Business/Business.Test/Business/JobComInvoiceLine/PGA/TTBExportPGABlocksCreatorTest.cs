using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class TTBExportPGABlocksCreatorTest : ExportPGABlocksCreatorTest
	{
		protected override void SetupData()
		{
			base.SetupData();

			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var exportTTB = invoiceLine.TTBLines.AddNew();
			exportTTB.US_NumberForIRC = "DH-388-I9-974AI";
			exportTTB.US_Date = new ZDate(2016, 04, 22);
			exportTTB.US_SerialNumber = "09876543211";
		}

		protected override ZString ExpectedResult
		{
			get { return "PGATTBDH-388-I9-974AI2016042209876543211                                        "; }
		}

		public void TestDisclaimedTTB()
		{
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			var exportTTB = invoiceLine.TTBLines.AddNew();
			exportTTB.US_NumberForIRC = "DH-388-I9-974AI";
			exportTTB.US_Date = new ZDate(2016, 04, 22);
			exportTTB.US_SerialNumber = "09876543211";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var blocks = ExportPGABlocksCreator.BuildPGABlocks(entryLine);
			var messageBuilder = new ZStringBuilder();

			foreach (var block in blocks)
			{
				messageBuilder.AppendIfNotEmpty(block.Serialise());
			}

			var expectedResult = "PGATTBDH-388-I9-974AI20160422098765432111                                       ";
			AssertEquals(expectedResult, messageBuilder.ToStringWithNewLineBetweenAppends());
		}
	}
}
