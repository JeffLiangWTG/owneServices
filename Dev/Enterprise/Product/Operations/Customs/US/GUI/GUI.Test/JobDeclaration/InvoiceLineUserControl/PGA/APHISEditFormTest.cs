using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(APHISEditForm))]
	sealed class APHISEditFormTest : ZFormBasherTest
	{
		public void TestVisibilityProgramTypeAPQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var header = invoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			using (APHISEditForm form = new APHISEditForm(header))
			{
				form.Show();
				AssertEquals("form.FormCaption", "APHIS - " + APHISProgramCodeList.Descriptions.APQ, form.FormCaption);
				AssertEquals("No. of columns", 11, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Genus is available", APHISProduct.Schema.US_Genus, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Species is available", APHISProduct.Schema.US_Species, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_Variety is available", APHISProduct.Schema.US_Variety, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_SourceTypeCode is available", APHISProduct.Schema.US_SourceTypeCode, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_CountryCode is available", APHISProduct.Schema.US_CountryCode, form.ProductsGrid.Columns[4].ColumnName);
				AssertEquals("Column US_GeographicLocation is available", APHISProduct.Schema.US_GeographicLocation, form.ProductsGrid.Columns[5].ColumnName);
				AssertEquals("Column US_ProcessingStartDate is available", APHISProduct.Schema.US_ProcessingStartDate, form.ProductsGrid.Columns[6].ColumnName);
				AssertEquals("Column US_ProcessingEndDate is available", APHISProduct.Schema.US_ProcessingEndDate, form.ProductsGrid.Columns[7].ColumnName);
				AssertEquals("Column US_ProcessingTypeCode is available", APHISProduct.Schema.US_ProcessingTypeCode, form.ProductsGrid.Columns[9].ColumnName);
				AssertEquals("Column US_ProcessingDescription is available", APHISProduct.Schema.US_ProcessingDescription, form.ProductsGrid.Columns[8].ColumnName);
				AssertEquals("Column US_Specific is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[10].ColumnName);
			}
		}

		public void TestVisibilityAndAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var header = invoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			using (APHISEditForm form = new APHISEditForm(header))
			{
				form.Show();
				AssertEquals("form.FormCaption", "APHIS - " + APHISProgramCodeList.Descriptions.AVS, form.FormCaption);
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", false, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);

				header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 9, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_ShowBreed is available", APHISProduct.Schema.US_ShowBreed, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_BreedVariety is available", APHISProduct.Schema.US_BreedVariety, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_Age is available", APHISProduct.Schema.US_Age, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_AgeRangeDesc is available", APHISProduct.Schema.US_AgeRangeDesc, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_Gender is available", APHISProduct.Schema.US_Gender, form.ProductsGrid.Columns[4].ColumnName);
				AssertEquals("Column US_Color is available", APHISProduct.Schema.US_Color, form.ProductsGrid.Columns[5].ColumnName);
				AssertEquals("Column US_IsFertilizedPregnantGestating is available", APHISProduct.Schema.US_IsFertilizedPregnantGestating, form.ProductsGrid.Columns[6].ColumnName);
				AssertEquals("Column US_GestationalAgeIfPregnant is available", APHISProduct.Schema.US_GestationalAgeIfPregnant, form.ProductsGrid.Columns[7].ColumnName);
				AssertEquals("Column US_IsProtectedSpecies is available", APHISProduct.Schema.US_IsProtectedSpecies, form.ProductsGrid.Columns[8].ColumnName);

				header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", false, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);
				header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSAnimalImportCenter;
				header.US_CategoryCode = Business.APHIS.ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation;
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", true, form.US_OA_PermittedAddressControl.Visible);
				header.US_ProcessingCode = ZString.Empty;
				header.US_CategoryCode = ZString.Empty;

				header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", false, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", true, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);

				header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", false, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);

				header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", false, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);
				header.US_CategoryCode = Enterprise.Customs.US.Business.APHIS.ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", true, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);

				header.US_CategoryType = APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", false, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", false, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);

				header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
				AssertEquals("form.US_OA_ApplicantAddressControl.Visible", true, form.US_OA_ApplicantAddressControl.Visible);
				AssertEquals("form.US_OA_ApplicantAddressControl.Enabled", true, form.US_OA_ApplicantAddressControl.Enabled);
				AssertEquals("form.US_OA_ShipperAddressControl.Visible", false, form.US_OA_ShipperAddressControl.Visible);
				AssertEquals("form.US_OA_CropGrowerAddressControl.Visible", false, form.US_OA_CropGrowerAddressControl.Visible);
				AssertEquals("form.US_OA_PermittedAddressControl.Visible", false, form.US_OA_PermittedAddressControl.Visible);
				AssertEquals("form.US_OA_USDAGrowerAddressControl.Visible", true, form.US_OA_USDAGrowerAddressControl.Visible);
				AssertEquals("No. of columns", 5, form.ProductsGrid.Columns.Count);
				AssertEquals("Column US_Type is available", APHISProduct.Schema.US_Type, form.ProductsGrid.Columns[0].ColumnName);
				AssertEquals("Column US_Origin is available", APHISProduct.Schema.US_Origin, form.ProductsGrid.Columns[1].ColumnName);
				AssertEquals("Column US_SpecificName is available", APHISProduct.Schema.US_SpecificName, form.ProductsGrid.Columns[2].ColumnName);
				AssertEquals("Column US_GeneralName is available", APHISProduct.Schema.US_GeneralName, form.ProductsGrid.Columns[3].ColumnName);
				AssertEquals("Column US_TypeDesc is available", APHISProduct.Schema.US_TypeDesc, form.ProductsGrid.Columns[4].ColumnName);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			using (var form = new APHISEditForm())
			{
				form.Show();
				Assert("form.US_ProductIngredientTypeDropEdit.Visible", form.US_ProductIngredientTypeDropEdit.Visible);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			using (var form = new APHISEditForm())
			{
				form.Show();
				Assert("form.US_ProductIngredientTypeDropEdit.Visible", !form.US_ProductIngredientTypeDropEdit.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new APHISEditForm(Factory.New<APHISHeader>());
	}
}
