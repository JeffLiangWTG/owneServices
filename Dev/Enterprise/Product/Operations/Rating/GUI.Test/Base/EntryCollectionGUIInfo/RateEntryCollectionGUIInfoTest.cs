using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateEntryCollectionGUIInfoTest : RatingTestCase
	{
		public void TestAllEntryCollectionsHaveGUIInfo()
		{
			Type rateEntryCollectionType = typeof(RateEntryCollection);
			Type rateEntryCollectionGUIInfoType = typeof(RateEntryCollectionGUIInfo);
			var bizAssembly = rateEntryCollectionType.Assembly;
			var guiAssembly = rateEntryCollectionGUIInfoType.Assembly;
			foreach (Type type in bizAssembly.GetTypes())
			{
				if (type.IsSubclassOf(rateEntryCollectionType) && !type.Name.StartsWith("Tst"))
				{
					Type gUIInfoType = Type.GetType("Enterprise.Rating.GUI." + type.Name + "GUIInfo, " + guiAssembly.FullName);
					AssertNotNull(type.Name, gUIInfoType);
					Assert(gUIInfoType.Name + " is subclass of RateEntryCollectionGUIInfoType", gUIInfoType.IsSubclassOf(rateEntryCollectionGUIInfoType));
				}
			}
		}

		public void TestIfEachEntryCategoryHasPropertyInRatingHeader()
		{
			var ratingHeaderType = typeof(RatingHeader);

			var entryCategories = RatingConstants.RateCategory.RateCategories;
			foreach (var category in entryCategories)
			{
				var propInfo = ratingHeaderType.GetProperty(category + "RateEntriesForBinding");
				AssertNotNull(category + "RateEntriesForBinding", propInfo);
				AssertEquals(category + "RateEntriesForBinding Type", "RateEntryCollection", propInfo.PropertyType.Name);
			}
		}

		public void TestServiceLevelOptionalValidation()
		{
			AssertServiceLevelIsMandatory(Helper.NewQuote(Helper.NewOrgHeader()), typeof(Quote), RatingDataRegistry.Instance.QuotationsRequiredFields);
			AssertServiceLevelIsMandatory(Helper.NewClientRate(Helper.NewOrgHeader()), typeof(ClientRate), RatingDataRegistry.Instance.ClientRatesRequiredFields);
			AssertServiceLevelIsMandatory(Helper.NewCosting(Helper.NewOrgHeader()), typeof(Costing), RatingDataRegistry.Instance.CostsRequiredFields);
			AssertServiceLevelIsMandatory(Helper.NewCompanyTariff(), typeof(CompanyTariff), RatingDataRegistry.Instance.CompanyTariffsRequiredFields);
		}

		public void AssertServiceLevelIsMandatory(RatingHeader ratingHeader, Type rateType, AutoRatingRequiredFieldsRegistryItem registryItem)
		{
			var requiredFields = new AutoRatingRequiredFields();
			requiredFields.RequireServiceLevel = true;
			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);

			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var entryForTest = ratingHeader.AddRateEntry(category);
				entryForTest.MarkAsNeedingValidation();
				entryForTest.TI_RS_NKServiceLevel_NI = "";

				var serviceLevelColumn = GetColumnFromCollection(RateEntryCollectionGUIInfo.GetInfo(category, rateType, false), AutoRateEntry.Schema.TI_RS_NKServiceLevel_NI);
				if (serviceLevelColumn != null)
				{
					AssertHasErrors(entryForTest.TI_RS_NKServiceLevel_NIInfo);
				}
			}
		}

		public void TestClientRate_IsPublisherColumnVisible()
		{
			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var localInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), false);
				var globalInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), true);
				var localPublisherColumn = GetColumnFromCollection(localInfo, AutoRateEntry.Schema.TI_GC_Publisher);
				var globalPublisherColumn = GetColumnFromCollection(globalInfo, AutoRateEntry.Schema.TI_GC_Publisher);

				AssertNull("Column should not exist for a Local Client Rate. Category: " + category, localPublisherColumn);
				AssertNotNull("Column should exist for a Global Client Rate. Category: " + category, globalPublisherColumn);
			}
		}

		public void TestCanShowIsPublishedColumn()
		{
			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var localClientRateInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), false);
				var globalClientRateInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), true);
				var localClientRateColumn = GetColumnFromCollection(localClientRateInfo, RateEntry.Schema.IsPublished);
				var globalClientRateColumn = GetColumnFromCollection(globalClientRateInfo, RateEntry.Schema.IsPublished);

				var localCostingInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Costing), false);
				var globalCostingInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Costing), true);
				var localCostingColumn = GetColumnFromCollection(localCostingInfo, RateEntry.Schema.IsPublished);
				var globalCostingColumn = GetColumnFromCollection(globalCostingInfo, RateEntry.Schema.IsPublished);

				var companyTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(CompanyTariff), false);
				var globalTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(GlobalTariff), true);
				var companyTariffColumn = GetColumnFromCollection(companyTariffInfo, RateEntry.Schema.IsPublished);
				var globalTariffColumn = GetColumnFromCollection(globalTariffInfo, RateEntry.Schema.IsPublished);

				var quoteInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Quote), false);
				var quoteColumn = GetColumnFromCollection(quoteInfo, RateEntry.Schema.IsPublished);

				CombineAssertions(() =>
					{
						AssertNotNull("Column should exist for a local Client Rate. Category: " + category, localClientRateColumn);
						AssertNull("Column should NOT exist for a global Client Rate. Category: " + category, globalClientRateColumn);

						AssertNotNull("Column should exist for a local Costing. Category: " + category, localCostingColumn);
						AssertNull("Column should NOT exist for a global Costing. Category: " + category, globalCostingColumn);

						AssertNotNull("Column should exist for a Company Tariff. Category: " + category, companyTariffColumn);
						AssertNull("Column should NOT exist for a Global Tariff. Category: " + category, globalTariffColumn);

						AssertNull("Column should never exist for a Quote. Category: " + category, quoteColumn);
					}
				);
			}
		}

		public void TestCaptionForContractNumberColumn_Common()
		{
			var assertedCategory = 0;
			var categoriesToTest = RatingConstants.RateCategory.RateCategories.Except(RateEntryCollectionGUIInfo.CategoriesShowingCarrierContractNumberInRevenue);

			foreach (var category in categoriesToTest)
			{
				var localClientRateInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), false);
				var globalClientRateInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), true);
				var localClientRateColumn = GetColumnFromCollection(localClientRateInfo, RateEntry.Schema.TI_ContractNumber);
				var globalClientRateColumn = GetColumnFromCollection(globalClientRateInfo, RateEntry.Schema.TI_ContractNumber);

				var localCostingInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Costing), false);
				var globalCostingInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Costing), true);
				var localCostingColumn = GetColumnFromCollection(localCostingInfo, RateEntry.Schema.TI_ContractNumber);
				var globalCostingColumn = GetColumnFromCollection(globalCostingInfo, RateEntry.Schema.TI_ContractNumber);

				var companyTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(CompanyTariff), false);
				var globalTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(GlobalTariff), true);
				var companyTariffColumn = GetColumnFromCollection(companyTariffInfo, RateEntry.Schema.TI_ContractNumber);
				var globalTariffColumn = GetColumnFromCollection(globalTariffInfo, RateEntry.Schema.TI_ContractNumber);

				var interCompanyTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(IntercompanyTariff), true);
				var interCompanyTariffColumn = GetColumnFromCollection(interCompanyTariffInfo, RateEntry.Schema.TI_ContractNumber);

				var companyMultimodalSearchInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(WiseRatingHeaderView), false);
				var globalMultimodalSearchInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(WiseRatingHeaderView), true);
				var companyMultimodalSearchColumn = GetColumnFromCollection(companyMultimodalSearchInfo, RateEntry.Schema.TI_ContractNumber);
				var globalMultimodalSearchColumn = GetColumnFromCollection(globalMultimodalSearchInfo, RateEntry.Schema.TI_ContractNumber);

				var quoteInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Quote), false);
				var quoteColumn = GetColumnFromCollection(quoteInfo, RateEntry.Schema.TI_ContractNumber);

				CombineAssertions(() =>
					{
						// the column does not appear in every category, only test if it exists
						if (localClientRateColumn != null)
						{
							AssertEquals("Client Contract Number", localClientRateColumn.CaptionResourceString.Caption);
							assertedCategory |= 1;
						}
						if (globalClientRateColumn != null)
						{
							AssertEquals("Client Contract Number", globalClientRateColumn.CaptionResourceString.Caption);
							assertedCategory |= 2;
						}

						if (localCostingColumn != null)
						{
							AssertEquals("Carrier Contract Number", localCostingColumn.CaptionResourceString.Caption);
							assertedCategory |= 4;
						}
						if (globalCostingColumn != null)
						{
							AssertEquals("Carrier Contract Number", globalCostingColumn.CaptionResourceString.Caption);
							assertedCategory |= 8;
						}

						if (companyTariffColumn != null)
						{
							AssertEquals("Client Contract Number", companyTariffColumn.CaptionResourceString.Caption);
							assertedCategory |= 16;
						}
						if (globalTariffColumn != null)
						{
							AssertEquals("Client Contract Number", globalTariffColumn.CaptionResourceString.Caption);
							assertedCategory |= 32;
						}

						if (interCompanyTariffColumn != null)
						{
							AssertEquals("Client Contract Number", interCompanyTariffColumn.CaptionResourceString.Caption);
							assertedCategory |= 64;
						}

						if (quoteColumn != null)
						{
							AssertEquals("Client Contract Number", quoteColumn.CaptionResourceString.Caption);
							assertedCategory |= 128;
						}

						if (companyMultimodalSearchColumn != null)
						{
							AssertEquals("Carrier Contract Number", companyMultimodalSearchColumn.CaptionResourceString.Caption);
							assertedCategory |= 256;
						}

						if (globalMultimodalSearchColumn != null)
						{
							AssertEquals("Carrier Contract Number", globalMultimodalSearchColumn.CaptionResourceString.Caption);
							assertedCategory |= 512;
						}
					}
				);
			}

			AssertEquals("Contract Number columns appearing in categories should have been tested at least once", 1023, assertedCategory);
		}

		public void TestCaptionForContractNumberColumn_ExceptionalCategories()
		{
			var assertedCategory = 0;

			foreach (var category in RateEntryCollectionGUIInfo.CategoriesShowingCarrierContractNumberInRevenue)
			{
				var localClientRateInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), false);
				var globalClientRateInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(ClientRate), true);
				var localClientRateColumn = GetColumnFromCollection(localClientRateInfo, RateEntry.Schema.TI_ContractNumber);
				var globalClientRateColumn = GetColumnFromCollection(globalClientRateInfo, RateEntry.Schema.TI_ContractNumber);

				var localCostingInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Costing), false);
				var globalCostingInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Costing), true);
				var localCostingColumn = GetColumnFromCollection(localCostingInfo, RateEntry.Schema.TI_ContractNumber);
				var globalCostingColumn = GetColumnFromCollection(globalCostingInfo, RateEntry.Schema.TI_ContractNumber);

				var companyTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(CompanyTariff), false);
				var globalTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(GlobalTariff), true);
				var companyTariffColumn = GetColumnFromCollection(companyTariffInfo, RateEntry.Schema.TI_ContractNumber);
				var globalTariffColumn = GetColumnFromCollection(globalTariffInfo, RateEntry.Schema.TI_ContractNumber);

				var interCompanyTariffInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(IntercompanyTariff), true);
				var interCompanyTariffColumn = GetColumnFromCollection(interCompanyTariffInfo, RateEntry.Schema.TI_ContractNumber);

				var companyMultimodalSearchInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(WiseRatingHeaderView), false);
				var globalMultimodalSearchInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(WiseRatingHeaderView), true);
				var companyMultimodalSearchColumn = GetColumnFromCollection(companyMultimodalSearchInfo, RateEntry.Schema.TI_ContractNumber);
				var globalMultimodalSearchColumn = GetColumnFromCollection(globalMultimodalSearchInfo, RateEntry.Schema.TI_ContractNumber);

				var quoteInfo = RateEntryCollectionGUIInfo.GetInfo(category, typeof(Quote), false);
				var quoteColumn = GetColumnFromCollection(quoteInfo, RateEntry.Schema.TI_ContractNumber);

				CombineAssertions(() =>
					{
						// the column does not appear in every category, only test if it exists
						if (localClientRateColumn != null)
						{
							AssertEquals("Carrier Contract Number", localClientRateColumn.CaptionResourceString.Caption);
							assertedCategory |= 1;
						}
						if (globalClientRateColumn != null)
						{
							AssertEquals("Carrier Contract Number", globalClientRateColumn.CaptionResourceString.Caption);
							assertedCategory |= 2;
						}

						if (localCostingColumn != null)
						{
							AssertEquals("Carrier Contract Number", localCostingColumn.CaptionResourceString.Caption);
							assertedCategory |= 4;
						}
						if (globalCostingColumn != null)
						{
							AssertEquals("Carrier Contract Number", globalCostingColumn.CaptionResourceString.Caption);
							assertedCategory |= 8;
						}

						if (companyTariffColumn != null)
						{
							AssertEquals("Carrier Contract Number", companyTariffColumn.CaptionResourceString.Caption);
							assertedCategory |= 16;
						}
						if (globalTariffColumn != null)
						{
							AssertEquals("Carrier Contract Number", globalTariffColumn.CaptionResourceString.Caption);
							assertedCategory |= 32;
						}

						if (interCompanyTariffColumn != null)
						{
							AssertEquals("Carrier Contract Number", interCompanyTariffColumn.CaptionResourceString.Caption);
							assertedCategory |= 64;
						}

						if (quoteColumn != null)
						{
							AssertEquals("Carrier Contract Number", quoteColumn.CaptionResourceString.Caption);
							assertedCategory |= 128;
						}

						if (companyMultimodalSearchColumn != null)
						{
							AssertEquals("Carrier Contract Number", companyMultimodalSearchColumn.CaptionResourceString.Caption);
							assertedCategory |= 256;
						}

						if (globalMultimodalSearchColumn != null)
						{
							AssertEquals("Carrier Contract Number", globalMultimodalSearchColumn.CaptionResourceString.Caption);
							assertedCategory |= 512;
						}
					}
				);
			}

			AssertEquals("Contract Number columns appearing in categories should have been tested at least once", 1023, assertedCategory);
		}

		public void TestContractNumberColumn_WhenContractAllocationModuleNo_AndIsCarrierContractYes_ExpectTextbox()
		{
			TestContractNumberColumnType(
				enableClientContractRegistry: false,
				enableContractAllocationRegistry: true,
				(type) => false
			);
		}

		public void TestContractNumberColumn_WhenContractAllocationModuleNo_AndIsCarrierContractNo_ExpectTextbox()
		{
			TestContractNumberColumnType(
				enableClientContractRegistry: false,
				enableContractAllocationRegistry: false,
				(type) => false
			);
		}

		public void TestContractNumberColumn_WhenContractAllocationModuleYes_AndIsCarrierContractNo_ExpectTextbox()
		{
			TestContractNumberColumnType(
				enableClientContractRegistry: true,
				enableContractAllocationRegistry: false,
				(type) =>
					!IsWinzor &&
					(type == typeof(Costing) || type == typeof(ClientRate))
			);
		}

		public void TestContractNumberColumn_WhenContractAllocationModuleYes_AndIsCarrierContractYes_ExpectFindboxForCostingOnly()
		{
			TestContractNumberColumnType(
				enableClientContractRegistry: true,
				enableContractAllocationRegistry: true,
				(type) =>
					(type == typeof(Costing)) || (!IsWinzor && type == typeof(ClientRate))
			);
		}

		/// <summary>
		/// For those ContractNumber columns that have a FindBox (see TestContractNumberColumn_ColumnStyleKind)
		/// expect to have a ContractNumberLinked column shown as a ZCheckboxColumnStyleInfo. For the others, it should
		/// not be present.
		/// </summary>
		public void TestContractNumberLinked_ColumnStyleKind()
		{
			var rateKindsAvailable = new[]
			{
				new { Type = typeof(ClientRate), IsGlobal = false },
				new { Type = typeof(ClientRate), IsGlobal = true },
				new { Type = typeof(Costing), IsGlobal = false },
				new { Type = typeof(Costing), IsGlobal = true },
				new { Type = typeof(CompanyTariff), IsGlobal = false },
				new { Type = typeof(GlobalTariff), IsGlobal = true },
				new { Type = typeof(IntercompanyTariff), IsGlobal = true },
				new { Type = typeof(WiseRatingHeaderView), IsGlobal = false },
				new { Type = typeof(WiseRatingHeaderView), IsGlobal = true },
				new { Type = typeof(Quote), IsGlobal = false }
			};

			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var categoryMightHaveContractLinkedColumn = rateCategoriesSupportingContractNumberLookup.Contains(category);

				foreach (var kind in rateKindsAvailable)
				{
					var isCosting = kind.Type == typeof(Costing);
					var isContractNumberLinkedExpected = categoryMightHaveContractLinkedColumn && isCosting;

					var info = RateEntryCollectionGUIInfo.GetInfo(category, kind.Type, kind.IsGlobal);
					var column = GetColumnFromCollection(info, RateEntry.Schema.TI_ContractNumberLinked);

					if (isContractNumberLinkedExpected)
					{
						AssertNotNull(column);
						AssertType<ZCheckBoxColumnStyleInfo>(column);
					}
					else
					{
						AssertNull(column);
					}
				}
			}
		}

		void TestContractNumberColumnType(bool enableClientContractRegistry, bool enableContractAllocationRegistry, Func<Type, bool> expectFindbox)
		{
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableContractAllocationRegistry))
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableClientContractRegistry))
			{
				var rateKindsAvailable = new[]
				{
					new { Type = typeof(ClientRate), IsGlobal = false },
					new { Type = typeof(ClientRate), IsGlobal = true },
					new { Type = typeof(Costing), IsGlobal = false },
					new { Type = typeof(Costing), IsGlobal = true },
					new { Type = typeof(CompanyTariff), IsGlobal = false },
					new { Type = typeof(GlobalTariff), IsGlobal = true },
					new { Type = typeof(IntercompanyTariff), IsGlobal = true },
					new { Type = typeof(WiseRatingHeaderView), IsGlobal = false },
					new { Type = typeof(WiseRatingHeaderView), IsGlobal = true },
					new { Type = typeof(Quote), IsGlobal = false }
				};

				foreach (var category in RatingConstants.RateCategory.RateCategories)
				{
					foreach (var kind in rateKindsAvailable)
					{
						var info = RateEntryCollectionGUIInfo.GetInfo(category, kind.Type, kind.IsGlobal);
						var column = GetColumnFromCollection(info, RateEntry.Schema.TI_ContractNumber);

						if (column != null) // Because some rate categories do not have the TI_ContractNumber column
						{
							var isFindboxExpected =
								expectFindbox(kind.Type) &&
								rateCategoriesSupportingContractNumberLookup.Contains(category);

							if (isFindboxExpected)
							{
								Assert($"Column {column.Caption} should have a FindBox", column is ContractNumberFindBoxColumnStyleInfo);
							}
							else
							{
								Assert($"Column {column.Caption} should have a Textbox", column is ZTextBoxColumnStyleInfo);
							}
						}
					}
				}
			}
		}

		ZGridColumnInfo GetColumnFromCollection(RateEntryCollectionGUIInfo info, string columnName)
		{
			return info.Columns.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
		}

		// These are the rate categories for which there may be a FindBox (depending on rate kind)
		// on the TI_ContractNumber column that lets the user pick a value from a GLOW popup
		// Similarly, these same categories also support the TI_ContractNumberLinked checkbox to appear
		readonly string[] rateCategoriesSupportingContractNumberLookup = new[]
		{
			RatingConstants.RateCategory.AIR,
			RatingConstants.RateCategory.FCL,
			RatingConstants.RateCategory.LCL,
			RatingConstants.RateCategory.ORG,
			RatingConstants.RateCategory.DST,
			RatingConstants.RateCategory.CAI,
			RatingConstants.RateCategory.CFC,
			RatingConstants.RateCategory.CLC,
			RatingConstants.RateCategory.COR,
			RatingConstants.RateCategory.CDS
		};

#if WINZOR
		protected bool IsWinzor => true;
#else
		protected bool IsWinzor => false;
#endif
	}
}
