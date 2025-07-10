using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class FWSExportPGABlocksCreatorTest : ExportPGABlocksCreatorTest
	{
		protected override void SetupData()
		{
			base.SetupData();

			invoiceLine.JI_Description = "CLEAR DESCRIPTION OF FWS";
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportFWS.US_ConfirmationNum = "2015CH1234567";
		}

		protected override ZString ExpectedResult
		{
			get
			{
				return
@"PGAFW72015CH1234567                                                             
PGAFW8CLEAR DESCRIPTION OF FWS                                                  ";
			}
		}

		public void TestExportFWSWithoutConfirmationNumber()
		{
			invoiceLine.JI_Description = "CLEAR DESCRIPTION OF FWS";
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportFWS.US_TaxonomicSerialNumber = "SN18383801234567890";
			invoiceLine.ExportFWS.US_PurposeCode = FWSPurposeCodeList.Codes.HuntingTrophy;
			invoiceLine.ExportFWS.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.CAR;
			invoiceLine.ExportFWS.US_SpeciesOrigin = Core.Constants.CountryCodes.Afghanistan;
			invoiceLine.ExportFWS.US_WildlifeSource = FWSWildlifeSourceList.Codes.F;
			invoiceLine.ExportFWS.US_CertificationCode = FWSCertificationCodeList.Codes.CertificationOfNoWildlife;
			invoiceLine.ExportFWS.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.OtherArthropods;
			invoiceLine.ExportFWS.US_USState = USStateList.Codes.Colorado;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var blocks = ExportPGABlocksCreator.BuildPGABlocks(entryLine);
			var messageBuilder = new ZStringBuilder();

			foreach (var block in blocks)
			{
				messageBuilder.AppendIfNotEmpty(block.Serialise());
			}

			var expectedResult =
@"PGAFW7              SN18383801234567890HCARAFFFW1APDCO                          
PGAFW8CLEAR DESCRIPTION OF FWS                                                  ";
			AssertEquals(expectedResult, messageBuilder.ToStringWithNewLineBetweenAppends());
		}
	}
}
