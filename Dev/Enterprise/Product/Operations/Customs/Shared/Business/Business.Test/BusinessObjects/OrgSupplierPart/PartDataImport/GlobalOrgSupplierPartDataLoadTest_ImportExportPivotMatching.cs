using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	public class GlobalOrgSupplierPartDataLoadTest_ImportExportPivotMatching : BaseGlobalOrgSupplierPartDataLoadTest
	{
		#region TestOnlySupportsSingleHTIorHTEPerProduct

		protected override ZString OnlySupportsSingleHTIorHTEPerProduct_Line1 => ",DESCR,,,,,070610,,ABIGAS  ,,";

		#endregion

		public void TestDoesNotRequiresPerOrganisationMatching()
		{
			OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = "SUP";
			var pivot1 = Factory.NewWithValidTestData<BaseCusClassPartPivot>();
			pivot1.CI_OP = product.PK;
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_OH = relation.OU_OH;

			Factory.Save();

			var dataLoad = GetNewDataLoader();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code     ,Description,UQ,Supplier,ImportTariff");
					sw.WriteLine(product.OP_PartNum + ",DESCR,,ABIGAS  ,070610");
				}

				dataLoad.ImportProductData(tempFile.Filename, true, false);

				var logText = string.Concat(dataLoad.Log.ToList());
				AssertContains($"PART NO: {product.OP_PartNum} - Part has been UPDATED", logText);
			}
		}

		#region TestImportingCountrySpecificFields

		protected override ZString ImportingCountrySpecificFields_Header => "Code   ,Description,UQ,Supplier,ImportClassification,StartDate,EndDate,ExportCountry,TaxType,OriginState,PrimaryPreference,SecondaryPreference,ValuationCode,ValuationMarkup,Colour,EngineCapacity,NewUsed,ROOCert,VehicleFormat,VehicleType,UsageComment,ClassificationDescription";
		protected override ZString ImportingCountrySpecificFields_Line1 => "PoolCue,Pool Cue   ,NO,ABIGAS  ,PoolCue             ,20161221,20171221,DE           ,VAT    ,CN         ,PREF1            ,PREF2              ,AB           ,5.0            ,Red   ,3000          ,N      ,CRT    ,FMT          ,TYP         ,UsageComment,ClassificationDescription";
		protected override ZString ImportingCountrySpecificFields_Line2 => "PoolCue,Pool Cue   ,NO,ABIGAS  ,PoolCue             ,20161221,20171221,DE           ,VAT    ,CN         ,PREF1            ,PREF2              ,AB           ,5.0            ,White ,3000          ,N      ,CRT    ,FMT          ,TYP         ,UsageComment,ClassificationDescription";

		#endregion

		#region TestSettingUQFromTariff

		protected override ZString SettingUQFromTariff_Line1 => "PoolCue1 ,Pool Cue   ,,                    ,IMPPOOLCUE          ,            ,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line2 => "PoolCue2 ,Pool Cue   ,,EXPPOOLCUE          ,                    ,            ,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line3 => "PoolCue3 ,Pool Cue   ,,                    ,                    ,            ,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line4 => "PoolCue4 ,Pool Cue   ,,                    ,                    ,            ,1020304051  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line5 => "PoolCue5 ,Pool Cue   ,,                    ,                    ,            ,1020304052  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line6 => "PoolCue6 ,Pool Cue   ,,                    ,                    ,            ,1020304053  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line7 => "PoolCue7 ,Pool Cue   ,,                    ,                    ,            ,1020304054  ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line8 => "PoolCue8 ,Pool Cue   ,,                    ,                    ,1020304051  ,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line9 => "PoolCue9 ,Pool Cue   ,,                    ,                    ,1020304052  ,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line10 => "PoolCue10 ,Pool Cue ,,                    ,                    ,1020304053  ,            ,,ABIGAS  ,,";
		protected override ZString SettingUQFromTariff_Line11 => "PoolCue11,Pool Cue  ,,                    ,                    ,1020304054  ,            ,,ABIGAS  ,,";

		#endregion

		#region TestSaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent()

		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line1 => "P1234,RUBBER GASKET,KG,,Test Lookup,,,,";
		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line2 => "P1234,RUBBER O RING,NO,,Changed Lookup,,,,";
		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithTariff => "P1234,RUBBER GASKET,KG,,,,07061009,,";
		protected override ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithAltTariff => "P1234,RUBBER O RING,NO,,,,07061011,,";

		#endregion

		#region TestSaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent

		protected override ZString SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line1 => "LEAFLETS,PAPER BOOKLETS,NO,Export Lookup,,,,,";
		protected override ZString SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line2 => "LEAFLETS,ONE PAGE BROCHURES,NO,Changed Lookup,,,,,";

		#endregion

		public void TestClassificationDoesNotAutoCreated()
		{
			fileHeader = "Code,Description,UQ,ImportTariff,ExportTariff,Supplier";

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,PAPER BOOKLETS,NO,4901.10.00,01010000," + testOrganisation.OH_Code);
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				OrgSupplierPart enterprisePart = LoadPart("LEAFLETS");

				var importPivot = enterprisePart.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNull(importPivot.Classification);
				AssertEquals(ExpectedTariffNumber, importPivot.CI_TariffNum);

				var exportPivot = enterprisePart.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNull(exportPivot.Classification);
				AssertEquals("01010000", exportPivot.CI_TariffNum);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			fileHeader = "Code,Description,UQ,ExportClassification,ImportClassification,ExportTariff,ImportTariff,Owner,Supplier,Division,QtyinStock";
			testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		protected virtual ZString ExpectedTariffNumber => "4901.10.00";

		protected override GlobalOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new GlobalOrgSupplierPartDataLoad_ForImportExportPivotMatching();
		}

		class GlobalOrgSupplierPartDataLoad_ForImportExportPivotMatching : GlobalOrgSupplierPartDataLoad
		{
			protected override bool UseOldClassificationFields => true;
		}

		#endregion
	}
}
