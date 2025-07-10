using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class NMFSExportPGABlocksCreatorTest : ExportPGABlocksCreatorTest
	{
		protected override void SetupData()
		{
			base.SetupData();

			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "THIS IS NATIONAL OCEANIC DESCRIPTIONS";
			var exportNMFS = invoiceLine.NMFSLines.AddNew();
			exportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			exportNMFS.US_ProcessingType = NMFSProductCategoryCodeList.Codes.Dressed;
			exportNMFS.US_IFTPPermitNumber = "SE52103";
			exportNMFS.US_DocumentType = "878";
			exportNMFS.US_DISDocumentID = "DOC34599";
			exportNMFS.US_HarvestedCountry = Core.Constants.CountryCodes.UnitedStates;
			exportNMFS.US_VesselCountry = Core.Constants.CountryCodes.Australia;
			exportNMFS.US_GeographicLocation = OceanGeographicAreaCodeList.Codes.A;
		}

		protected override ZString ExpectedResult
		{
			get
			{
				return
@"PGANM7THIS IS NATIONAL OCEANIC DESCRIPTIONS                                 HMS 
PGANM8NDR878DOC34599                      YUSAUA                                
PGANM9SE52103                                                                   ";
			}
		}
	}
}
