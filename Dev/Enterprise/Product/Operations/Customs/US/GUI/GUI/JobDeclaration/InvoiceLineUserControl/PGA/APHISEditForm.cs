using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class APHISEditForm : ZChildForm
	{
		public APHISEditForm()
		{
		}

		public APHISEditForm(APHISHeader header)
			: base(header)
		{
			HookCategoryTypeChange();
			saveProductsGridGridId = ProductsGrid.GridId;
			SetProductsGridGridId();
			US_StockKeepingUnitNumberTextBox.Enabled = header.IsAPQProgramType;
			US_StockKeepingUnitNumberTextBox.Visible = US_StockKeepingUnitNumberTextBox.Enabled;
		}

		public override string FormCaption
		{
			get
			{
				return Res.GetString("62BD626C-7528-4E48-9DE6-7DE51F41D5DA", "APHIS - {0}", CurrentDataItem == null ? ZString.Empty : CurrentDataItem.US_ProgramTypeDesc);
			}
		}
		public new APHISHeader CurrentDataItem
		{
			get { return (APHISHeader)base.CurrentDataItem; }
		}

		void SetProductsGridGridId()
		{
			ProductsGrid.GridId = CurrentCategoryType + saveProductsGridGridId;
			ProductsGrid.CurrentColumnLayout = null;
		}
		readonly string saveProductsGridGridId;

		string CurrentCategoryType
		{
			get
			{
				var result = "";
				var header = CurrentDataItem;
				var lookups = header != null ? header.AddInfoLookups : null;
				if (lookups != null && lookups.CategoryTypes.ContainsCode(header.US_CategoryType))
				{
					result = header.US_CategoryType;
				}
				return result;
			}
		}

		void HookCategoryTypeChange()
		{
			var header = CurrentDataItem;
			if (header != null)
			{
				header.US_CategoryTypeInfo.ValueChanged -= US_CategoryTypeInfo_ValueChanged;
				header.US_CategoryTypeInfo.ValueChanged += US_CategoryTypeInfo_ValueChanged;
				header.US_CategoryCodeInfo.ValueChanged -= US_CategoryTypeInfo_ValueChanged;
				header.US_CategoryCodeInfo.ValueChanged += US_CategoryTypeInfo_ValueChanged;
				US_CategoryTypeInfo_ValueChanged(this, EventArgs.Empty);
			}
		}

		void UnHookCategoryTypeChange()
		{
			var header = CurrentDataItem;
			if (header != null)
			{
				header.US_CategoryTypeInfo.ValueChanged -= US_CategoryTypeInfo_ValueChanged;
				header.US_CategoryCodeInfo.ValueChanged -= US_CategoryTypeInfo_ValueChanged;
			}
		}

		void US_CategoryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!this.IsDesignMode())
			{
				var header = CurrentDataItem;
				if (header != null)
				{
					US_OA_ShipperAddressControl.Visible = !header.IsShipperNotApplicable;
					US_OA_CropGrowerAddressControl.Visible = !US_OA_ShipperAddressControl.Visible && !header.IsCropGrowerNotApplicable;
					US_OA_PermittedAddressControl.Visible = !header.IsPermittedNotApplicable;
					US_OA_USDAGrowerAddressControl.Visible = !header.IsUSDAAPHISGrowerNotApplicable;
					US_BouquetGroupingNumberTextBox.Visible = false;
					NonCutFlowerPanel.Visible = true;
					CutFlowePanel.Visible = false;
					if (header.IsAPQProgramType)
					{
						SetProductsGridColumnsLayout(
									new[]
									{
									APHISProduct.Schema.US_Genus, APHISProduct.Schema.US_Species, APHISProduct.Schema.US_Variety,
									APHISProduct.Schema.US_SourceTypeCode, APHISProduct.Schema.US_CountryCode,  APHISProduct.Schema.US_GeographicLocation,
									APHISProduct.Schema.US_ProcessingStartDate,  APHISProduct.Schema.US_ProcessingEndDate,
									APHISProduct.Schema.US_ProcessingDescription, APHISProduct.Schema.US_ProcessingTypeCode,
									APHISProduct.Schema.US_SpecificName
									},
									new[]
									{
									APHISProduct.Schema.US_Type, APHISProduct.Schema.US_Origin,
									APHISProduct.Schema.US_SpecificName, APHISProduct.Schema.US_GeneralName, APHISProduct.Schema.US_TypeDesc,
									APHISProduct.Schema.US_Age, APHISProduct.Schema.US_AgeRangeDesc, APHISProduct.Schema.US_BreedVariety,
									APHISProduct.Schema.US_Color, APHISProduct.Schema.US_Gender, APHISProduct.Schema.US_GestationalAgeIfPregnant, APHISProduct.Schema.US_IsFertilizedPregnantGestating,
									APHISProduct.Schema.US_IsProtectedSpecies, APHISProduct.Schema.US_ShowBreed
									},
									new[]
									{
									APHISProduct.Schema.US_Genus, APHISProduct.Schema.US_Species, APHISProduct.Schema.US_Variety,
									APHISProduct.Schema.US_SourceTypeCode, APHISProduct.Schema.US_CountryCode,  APHISProduct.Schema.US_GeographicLocation,
									APHISProduct.Schema.US_ProcessingStartDate,  APHISProduct.Schema.US_ProcessingEndDate,
									APHISProduct.Schema.US_ProcessingDescription, APHISProduct.Schema.US_ProcessingTypeCode,
									APHISProduct.Schema.US_SpecificName
									}
								);
					}
					else
					{
						switch (header.US_CategoryType)
						{
							case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
								SetProductsGridColumnsLayout(
									new[]
									{
									APHISProduct.Schema.US_Type, APHISProduct.Schema.US_Origin,
									APHISProduct.Schema.US_SpecificName, APHISProduct.Schema.US_GeneralName, APHISProduct.Schema.US_TypeDesc
									},
									new[]
									{
									APHISProduct.Schema.US_Age, APHISProduct.Schema.US_AgeRangeDesc, APHISProduct.Schema.US_BreedVariety,
									APHISProduct.Schema.US_Color, APHISProduct.Schema.US_Gender, APHISProduct.Schema.US_GestationalAgeIfPregnant, APHISProduct.Schema.US_IsFertilizedPregnantGestating,
									APHISProduct.Schema.US_IsProtectedSpecies, APHISProduct.Schema.US_ShowBreed,
									APHISProduct.Schema.US_Genus, APHISProduct.Schema.US_Species, APHISProduct.Schema.US_Variety,
									APHISProduct.Schema.US_SourceTypeCode, APHISProduct.Schema.US_CountryCode,  APHISProduct.Schema.US_GeographicLocation,
									APHISProduct.Schema.US_ProcessingStartDate,  APHISProduct.Schema.US_ProcessingEndDate,
									APHISProduct.Schema.US_ProcessingDescription, APHISProduct.Schema.US_ProcessingTypeCode,
									APHISProduct.Schema.US_SpecificName
									},
									new[]
									{
									APHISProduct.Schema.US_Type, APHISProduct.Schema.US_Origin,
									APHISProduct.Schema.US_SpecificName, APHISProduct.Schema.US_GeneralName, APHISProduct.Schema.US_TypeDesc
									}
								);
								break;
							case APHISCategoryTypeCodeList.Codes.LiveAnimals:
								SetProductsGridColumnsLayout(
									new[]
									{
									APHISProduct.Schema.US_Age, APHISProduct.Schema.US_AgeRangeDesc, APHISProduct.Schema.US_BreedVariety,
									APHISProduct.Schema.US_Color, APHISProduct.Schema.US_Gender, APHISProduct.Schema.US_GestationalAgeIfPregnant,
									APHISProduct.Schema.US_IsFertilizedPregnantGestating, APHISProduct.Schema.US_IsProtectedSpecies, APHISProduct.Schema.US_ShowBreed
									},
									new[]
									{
									APHISProduct.Schema.US_Type, APHISProduct.Schema.US_Origin,
									APHISProduct.Schema.US_SpecificName, APHISProduct.Schema.US_GeneralName, APHISProduct.Schema.US_TypeDesc,
									APHISProduct.Schema.US_Genus, APHISProduct.Schema.US_Species, APHISProduct.Schema.US_Variety,
									APHISProduct.Schema.US_SourceTypeCode, APHISProduct.Schema.US_CountryCode,  APHISProduct.Schema.US_GeographicLocation,
									APHISProduct.Schema.US_ProcessingStartDate,  APHISProduct.Schema.US_ProcessingEndDate,
									APHISProduct.Schema.US_ProcessingDescription, APHISProduct.Schema.US_ProcessingTypeCode,
									APHISProduct.Schema.US_SpecificName
									},
									new[]
									{
									APHISProduct.Schema.US_ShowBreed, APHISProduct.Schema.US_BreedVariety,
									APHISProduct.Schema.US_Age, APHISProduct.Schema.US_AgeRangeDesc, APHISProduct.Schema.US_Gender,
									APHISProduct.Schema.US_Color, APHISProduct.Schema.US_IsFertilizedPregnantGestating,
									APHISProduct.Schema.US_GestationalAgeIfPregnant, APHISProduct.Schema.US_IsProtectedSpecies
									}
								);
								break;
							case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
								US_BouquetGroupingNumberTextBox.Visible = true;
								NonCutFlowerPanel.Visible = false;
								CutFlowePanel.Visible = true;
								break;
							default:
								SetProductsGridColumnsLayout(
									new[]
									{
									APHISProduct.Schema.US_Type, APHISProduct.Schema.US_Origin,
									APHISProduct.Schema.US_SpecificName, APHISProduct.Schema.US_GeneralName, APHISProduct.Schema.US_TypeDesc
									},
									new[]
									{
									APHISProduct.Schema.US_Age, APHISProduct.Schema.US_AgeRangeDesc, APHISProduct.Schema.US_BreedVariety,
									APHISProduct.Schema.US_Color, APHISProduct.Schema.US_Gender, APHISProduct.Schema.US_GestationalAgeIfPregnant, APHISProduct.Schema.US_IsFertilizedPregnantGestating,
									APHISProduct.Schema.US_IsProtectedSpecies, APHISProduct.Schema.US_ShowBreed,
									APHISProduct.Schema.US_Genus, APHISProduct.Schema.US_Species, APHISProduct.Schema.US_Variety,
									APHISProduct.Schema.US_SourceTypeCode, APHISProduct.Schema.US_CountryCode,  APHISProduct.Schema.US_GeographicLocation,
									APHISProduct.Schema.US_ProcessingStartDate,  APHISProduct.Schema.US_ProcessingEndDate,
									APHISProduct.Schema.US_ProcessingDescription, APHISProduct.Schema.US_ProcessingTypeCode,
									APHISProduct.Schema.US_SpecificName
									},
									new[]
									{
									APHISProduct.Schema.US_Type, APHISProduct.Schema.US_Origin,
									APHISProduct.Schema.US_SpecificName, APHISProduct.Schema.US_GeneralName, APHISProduct.Schema.US_TypeDesc
									}
								);
								break;
						}
					}
				}
			}
		}

		void SetProductsGridColumnsLayout(string[] availableColumnNames, string[] nonAvailableColumnNames, string[] columnsOrder)
		{
			var hasProductsGridDataSource = ProductsGrid.DataSource != null;
			if (hasProductsGridDataSource)
			{
				ProductsGrid.SaveUserLayoutSettings();
			}
			using (ProductsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				SetProductsGridGridId();
				ProductsGrid.SetAvailability(false, nonAvailableColumnNames);
				ProductsGrid.SetAvailability(true, availableColumnNames);
				ProductsGrid.ReOrderColumns(columnsOrder);
			}
			if (hasProductsGridDataSource)
			{
				ProductsGrid.LoadUserLayoutSettings();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookCategoryTypeChange();
			}
			base.Dispose(disposing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			US_ProductIngredientTypeDropEdit.Visible = ZZCustomsFunctionality.IsAPHIS2024Effective;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
