using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	public class GlobalOrgSupplierPartDataLoadTest_ClassificationTypePivotMatching : BaseGlobalOrgSupplierPartDataLoadTest
	{
		#region TestOnlySupportsSingleHTIorHTEPerProduct

		protected override ZString OnlySupportsSingleHTIorHTEPerProduct_Line1 => ",DESCR,,,HTI,070610,,ABIGAS  ,,";

		#endregion

		#region TestImportingCountrySpecificFields

		protected override ZString ImportingCountrySpecificFields_Header => "Code   ,Description,UQ,Supplier,ClassificationLookup,ClassificationType,StartDate,EndDate,ExportCountry,TaxType,OriginState,PrimaryPreference,SecondaryPreference,ValuationCode,ValuationMarkup,Colour,EngineCapacity,NewUsed,ROOCert,VehicleFormat,VehicleType,UsageComment,ClassificationDescription";
		protected override ZString ImportingCountrySpecificFields_Line1 => "PoolCue,Pool Cue   ,NO,ABIGAS  ,PoolCue              ,HTI               ,20161221,20171221,DE           ,VAT    ,CN         ,PREF1            ,PREF2              ,AB           ,5.0            ,Red   ,3000          ,N      ,CRT    ,FMT          ,TYP        ,UsageComment,ClassificationDescription";
		protected override ZString ImportingCountrySpecificFields_Line2 => "PoolCue,Pool Cue   ,NO,ABIGAS  ,PoolCue              ,HTI               ,20161221,20171221,DE           ,VAT    ,CN         ,PREF1            ,PREF2              ,AB           ,5.0            ,White ,3000          ,N      ,CRT    ,FMT          ,TYP        ,UsageComment,ClassificationDescription";

		#endregion

		#region TestSettingUQFromTariff

		protected override ZString SettingUQFromTariff_Line1 => "PoolCue1 ,Pool Cue   ,,IMPPOOLCUE      ,HTI,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line2 => "PoolCue2 ,Pool Cue   ,,EXPPOOLCUE      ,HTE,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line3 => "PoolCue3 ,Pool Cue   ,,                ,HTI,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line4 => "PoolCue4 ,Pool Cue   ,,                ,HTI,1020304051  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line5 => "PoolCue5 ,Pool Cue   ,,                ,HTI,1020304052  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line6 => "PoolCue6 ,Pool Cue   ,,                ,HTI,1020304053  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line7 => "PoolCue7 ,Pool Cue   ,,                ,HTI,1020304054  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line8 => "PoolCue8 ,Pool Cue   ,,                ,HTE,1020304051  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line9 => "PoolCue9 ,Pool Cue   ,,                ,HTE,1020304052  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line10 => "PoolCue10 ,Pool Cue ,,                ,HTE,1020304053  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line11 => "PoolCue11,Pool Cue  ,,                ,HTE,1020304054  ,,ABIGAS  ,,";

		#endregion

		#region TestSaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent

		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line1 => "P1234,RUBBER GASKET,KG,Test Lookup,HTI,,,";
		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line2 => "P1234,RUBBER O RING,NO,Changed Lookup,HTI,,,";
		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithTariff => "P1234,RUBBER GASKET,KG,,HTI,07061009,,";
		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithAltTariff => "P1234,RUBBER O RING,NO,,HTI,07061011,,";

		#endregion

		#region TestSaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent

		protected override ZString SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line1 => "LEAFLETS,PAPER BOOKLETS,NO,Export Lookup,HTE,,,";
		protected override ZString SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line2 => "LEAFLETS,ONE PAGE BROCHURES,NO,Changed Lookup,HTE,,,";

		#endregion

		public void TestClassificationDoesNotAutoCreated()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,PAPER BOOKLETS,NO,,HTI,4901.10.00,," + testOrganisation.OH_Code + ",,");
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				OrgSupplierPart enterprisePart = LoadPart("LEAFLETS");

				var importPivot = enterprisePart.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNull(importPivot.Classification);
				AssertEquals(ExpectedTariffNumber, importPivot.CI_TariffNum);
			}
		}

		#region Implementation

		protected virtual ZString ExpectedTariffNumber => "4901.10.00";

		protected override GlobalOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new GlobalOrgSupplierPartDataLoad();
		}

		protected override void SetUp()
		{
			base.SetUp();
			fileHeader = "Code,Description,UQ,ClassificationLookup,ClassificationType,Tariff,Owner,Supplier,Division,QtyinStock";
			testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		#endregion
	}
}
