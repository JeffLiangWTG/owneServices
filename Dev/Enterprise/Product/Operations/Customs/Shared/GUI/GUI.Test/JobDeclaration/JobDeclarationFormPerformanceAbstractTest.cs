using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class JobDeclarationFormPerformanceAbstractTest : TestCaseWithFactory
	{
		public JobDeclarationFormPerformanceAbstractTest()
		{
			var typeFullName = GetType().FullName;
		}

		protected virtual Dictionary<string, int> LoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> loadEditableChildObjectsExpectedHitsBase = new Dictionary<string, int>
		{
		};

		protected virtual Dictionary<string, int> ValidateAllExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> validateAllExpectedHitsBase = new Dictionary<string, int>
		{
			{ GenCustomAddOnValueSchema.Constants.TableName, 6 },
			{ ProcessTaskTemplateSchema.Constants.TableName, 8 },
			{ StmDataSchema.Constants.TableName, 5 },
		};

		protected virtual Dictionary<string, int> LightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> lightFormValidationAndSaveExpectedHitsBase = new Dictionary<string, int>
		{
			{ ProcessTaskTemplateSchema.Constants.TableName, 8 },
			{ StmDataSchema.Constants.TableName, 5 },
		};

		protected virtual Dictionary<string, int> FormMergeExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> formMergeExpectedHitsBase = new Dictionary<string, int>();

		protected virtual Dictionary<string, int> UniversalXMLExportExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> universalXMLExportExpectedHitsBase = new Dictionary<string, int>
		{
			{ GenCustomAddOnValueSchema.Constants.TableName, 60 },
			{ ProcessTaskTemplateSchema.Constants.TableName, 13 },
			{ StmNoteSchema.Constants.TableName, 7 },
			{ OrgAddressAdditionalInfoSchema.Constants.TableName, 8 },
		};

		protected virtual Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> universalXMLImportUpdateExpectedHitsBase = new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ OrgContactSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 9 },
			{ OrgMiscServSchema.Constants.TableName, 8 },
			{ OrgPartRelationSchema.Constants.TableName, 2 },
			{ StmDocDataOverrideSchema.Constants.TableName, 6 },
			{ ProcessJobTriggerLinkSchema.Constants.TableName, 66 },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 14 },
			{ OrgRelatedPartySchema.Constants.TableName, 6 },
		};

		protected virtual Dictionary<string, int> UniversalXMLAddExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> universalXMLAddExpectedHitsBase = new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ OrgContactSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 9 },
			{ OrgMiscServSchema.Constants.TableName, 8 },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 13 },
			{ OrgRelatedPartySchema.Constants.TableName, 8 },
		};

		protected virtual Dictionary<string, int> DeleteExpectedHits => new Dictionary<string, int>();
		readonly Dictionary<string, int> deleteExpectedHitsBase = new Dictionary<string, int>
		{
			{ GenCustomAddOnValueSchema.Constants.TableName, 7 },
			{ JobDocumentExclusionSchema.Constants.TableName, 5 },
			{ ProcessJobTriggerLinkSchema.Constants.TableName, 68 },
			{ ProcessTasksSchema.Constants.TableName, 7 },
			{ StmDocDataOverrideSchema.Constants.TableName, 6 },
		};

		List<string> TablesToIgnore => new List<string>
		{
			StmUniversalCopySchema.Constants.TableName,
			RefLanguageTextSchema.Constants.TableName,
		};

		protected Dictionary<string, int> ZipDictionaries(IDictionary<string, int> first, IDictionary<string, int> second)
		{
			var dict = first.ToDictionary(e => e.Key, e => e.Value);
			foreach (var item in second)
			{
				dict[item.Key] = item.Value;
			}

			return dict;
		}

		const int DbHitTolerance = 5;
		const bool UseOnlyNewFactories = false;
		const int AcceptableVariance = 1;
		protected virtual int AcceptableVarianceForXMLImport => 1;

		protected virtual ZString MessageTypeForFormBashing => ZString.Empty;
		protected virtual bool DeclarationIsCancelled => false;

		[SnailTest]
		public void TestFetchHints()
		{
			Factory.NameForDebugging = "Data Creation Factory";
			var declaration = CreateFullyPopulatedObject(Factory, 6, 10);
			Factory.Save();

			string[] tablesToCollectQueriesFor = !TestingState.IsRunningOnDAT ? GetTablesToGatherHitQuery() : null;
			string ignoreStackTraceBeforeThis = tablesToCollectQueriesFor != null && tablesToCollectQueriesFor.Any() ? System.Environment.StackTrace.SplitByLine().Last() : null;

			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			void assertData()
			{
				var typeName = GetType().FullName;
				ZZCustomsFunctionalityEffectiveDate.ClearDictionary();
				RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(Factory);
				BusinessObjectFactory factory = null;
				using (AssertDbHitsForAllFactories(typeName + " - LoadChildEditableObjects", ZipDictionaries(loadEditableChildObjectsExpectedHitsBase, LoadEditableChildObjectsExpectedHits),
					hitTolerance: DbHitTolerance,
					stackTraceToIgnore: ignoreStackTraceBeforeThis,
					tablesToCollectQueriesFor: tablesToCollectQueriesFor,
					useOnlyNewFactories: UseOnlyNewFactories,
					tablesToIgnore: TablesToIgnore,
					acceptableVariance: AcceptableVariance))
				{
					factory = new BusinessObjectFactory { NameForDebugging = "LoadChildEditableObjects" };
					var dec = factory.Load<BaseJobDeclaration>(declaration.PK);
					using (var form = GetForm(dec))
					{
						form.Show();
						form.Update();
						factory.ResetDatabaseLoadCount();
						dec.LoadChildEditableObjects();
					}
				}
				RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);

				using (AssertDbHitsForAllFactories(typeName + " - Validate All", ZipDictionaries(validateAllExpectedHitsBase, ValidateAllExpectedHits),
					hitTolerance: DbHitTolerance,
					stackTraceToIgnore: ignoreStackTraceBeforeThis,
					tablesToCollectQueriesFor: tablesToCollectQueriesFor,
					useOnlyNewFactories: UseOnlyNewFactories,
					tablesToIgnore: TablesToIgnore,
					acceptableVariance: AcceptableVariance))
				{
					factory = new BusinessObjectFactory { NameForDebugging = "Validate All" };
					var dec = factory.Load<BaseJobDeclaration>(declaration.PK);
					using (var form = GetForm(dec))
					{
						form.Show();
						form.Update();
						factory.ResetDatabaseLoadCount();
						form.Menu.MenuItems.FindByText("&Validate All", true).PerformClick();
					}
				}
				RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);

				using (AssertDbHitsForAllFactories(typeName + " - Light Form Validation And Save", ZipDictionaries(lightFormValidationAndSaveExpectedHitsBase, LightFormValidationAndSaveExpectedHits),
					hitTolerance: DbHitTolerance,
					stackTraceToIgnore: ignoreStackTraceBeforeThis,
					tablesToCollectQueriesFor: tablesToCollectQueriesFor,
					useOnlyNewFactories: UseOnlyNewFactories,
					tablesToIgnore: TablesToIgnore,
					acceptableVariance: AcceptableVariance))
				{
					factory = new BusinessObjectFactory { NameForDebugging = "Light Form Validation And Save" };
					var dec = factory.Load<BaseJobDeclaration>(declaration.PK);
					using (var form = GetForm(dec))
					{
						form.Show();
						form.Update();
						factory.ResetDatabaseLoadCount();
						form.FireSaveButton();
					}
				}
				RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);

				using (AssertDbHitsForAllFactories(typeName + " - Form Merge", ZipDictionaries(formMergeExpectedHitsBase, FormMergeExpectedHits),
					hitTolerance: DbHitTolerance,
					stackTraceToIgnore: ignoreStackTraceBeforeThis,
					tablesToCollectQueriesFor: tablesToCollectQueriesFor,
					useOnlyNewFactories: UseOnlyNewFactories,
					tablesToIgnore: TablesToIgnore,
					acceptableVariance: AcceptableVariance))
				{
					factory = new BusinessObjectFactory { NameForDebugging = "Form Merge" };
					var dec = factory.Load<BaseJobDeclaration>(declaration.PK);
					dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					using (var form = GetForm(dec))
					{
						form.Show();
						form.Update();
						factory.ResetDatabaseLoadCount();
						dec.DoMerge();
					}
				}
				RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);

				UniversalShipment shipment;
				using (AssertDbHitsForAllFactories(typeName + " - Universal XML Export", ZipDictionaries(universalXMLExportExpectedHitsBase, UniversalXMLExportExpectedHits),
					hitTolerance: DbHitTolerance,
					stackTraceToIgnore: ignoreStackTraceBeforeThis,
					tablesToCollectQueriesFor: tablesToCollectQueriesFor,
					useOnlyNewFactories: UseOnlyNewFactories,
					tablesToIgnore: TablesToIgnore,
					acceptableVariance: AcceptableVariance))
				{
					factory = new BusinessObjectFactory { NameForDebugging = "Universal XML Export" };
					var dec = factory.Load<BaseJobDeclaration>(declaration.PK);
					dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					shipment = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, dec);
				}
				RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);

				if (!declaration.JE_IsCancelled)
				{
					var dataContext = DataContextFactory.New();
					dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
					dataContext.CodesMappedToTarget = true; // Required to import JobCosting
					dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);
					shipment.WayBillNumber = "MKD3232";
					shipment.DataContext = dataContext;
					eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					var expectedHits = ZipDictionaries(universalXMLImportUpdateExpectedHitsBase, UniversalXMLImportUpdateExpectedHits);

					using (AssertDbHitsForAllFactories(typeName + " - Universal XML Import Update", expectedHits,
						hitTolerance: DbHitTolerance,
						stackTraceToIgnore: ignoreStackTraceBeforeThis,
						tablesToCollectQueriesFor: tablesToCollectQueriesFor,
						useOnlyNewFactories: UseOnlyNewFactories,
						tablesToIgnore: TablesToIgnore,
						acceptableVariance: AcceptableVarianceForXMLImport))
					{
						IShipmentDataContextManager manager = new JobDeclarationDataContextManager();
						var objectFactory = new UniversalObjectFactory();
						using (factory.AddDisposableService())
						{
							factory = objectFactory.BOFactory;
							factory.NameForDebugging = "Universal XML Import Update";
							factory.ResetDatabaseLoadCount();
							AssertEquals("Universal XML Import", true, manager.UseIncomingShipmentData(shipment, new TestErrorLogger(), objectFactory));
							objectFactory.SaveForTesting();
						}
					}
					RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);

					dataContext = DataContextFactory.New();
					dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
					dataContext.CodesMappedToTarget = true; // Required to import JobCosting
					dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
					shipment.WayBillNumber = "JKD44332";
					shipment.DataContext = dataContext;
					SetUpDataForUniversalXmlImportAdd(shipment);
					using (AssertDbHitsForAllFactories(typeName + " - Universal XML Add", ZipDictionaries(universalXMLAddExpectedHitsBase, UniversalXMLAddExpectedHits),
						hitTolerance: DbHitTolerance,
						stackTraceToIgnore: ignoreStackTraceBeforeThis,
						tablesToCollectQueriesFor: tablesToCollectQueriesFor,
						useOnlyNewFactories: UseOnlyNewFactories,
						tablesToIgnore: TablesToIgnore,
						acceptableVariance: AcceptableVariance))
					{
						IShipmentDataContextManager manager = new JobDeclarationDataContextManager();
						var objectFactory = new UniversalObjectFactory();
						factory = objectFactory.BOFactory;
						using (factory.AddDisposableService())
						{
							factory.NameForDebugging = "Universal XML Add";
							factory.ResetDatabaseLoadCount();
							AssertEquals("Universal XML Add", true, manager.UseIncomingShipmentData(shipment, new TestErrorLogger(), objectFactory));
							objectFactory.SaveForTesting();
						}
					}
					RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);
				}

				using (AssertDbHitsForAllFactories(typeName + " - Delete", ZipDictionaries(deleteExpectedHitsBase, DeleteExpectedHits),
					hitTolerance: DbHitTolerance,
					stackTraceToIgnore: ignoreStackTraceBeforeThis,
					tablesToCollectQueriesFor: tablesToCollectQueriesFor,
					useOnlyNewFactories: UseOnlyNewFactories,
					tablesToIgnore: TablesToIgnore,
					acceptableVariance: AcceptableVariance))
				{
					factory = new BusinessObjectFactory { NameForDebugging = "Delete" };
					var dec = factory.Load<BaseJobDeclaration>(declaration.PK);
					dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					factory.ResetDatabaseLoadCount();
					dec.Delete();
				}
				RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);
			}

			if (!TestingState.IsRunningOnDAT)
			{
				CombineAssertions(assertData);
			}
			else
			{
				assertData();
			}
		}

		void RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(BusinessObjectFactory factory)
		{
			var factoryCacheManagerInstance = PersistentFactoryCacheManager.Instance;
			if (factoryCacheManagerInstance != null)
			{
				var weakReference = factoryCacheManagerInstance.persistentFactoryWeakReferences.Find(x => x.Target is object target && target.Equals(factory));
				factoryCacheManagerInstance.persistentFactoryWeakReferences.Remove(weakReference);
			}
			RegistryFactory.RenewFactory();
		}

		protected virtual string[] GetTablesToGatherHitQuery()
		{
			return new string[]
			{
				CusAddInfoSchema.Constants.TableName,
				CusClassPartPivotSchema.Constants.TableName,
				CusCodeDataSchema.Constants.TableName,
				CusContainerInvoiceLinePivotSchema.Constants.TableName,
				CusDispositionSchema.Constants.TableName,
				CusHouseContPackInvoiceLinePivotSchema.Constants.TableName,
				CusLineTariffDetailSchema.Constants.TableName,
				CusReferenceSchema.Constants.TableName,
				CusSupportingInfoSchema.Constants.TableName,
				CusUnderbondDecSchema.Constants.TableName,
				GenAddOnColumnSchema.Constants.TableName,
				GenCustomAddOnValueSchema.Constants.TableName,
				GenPivotSchema.Constants.TableName,
				JobComInvHeaderChargeSchema.Constants.TableName,
				JobComInvLineRefsSchema.Constants.TableName,
				JobComInvLineRefsSchema.Constants.TableName,
				JobComInvoiceHeaderRefsSchema.Constants.TableName,
				JobComInvoiceHeaderSchema.Constants.TableName,
				JobComInvoiceLineSchema.Constants.TableName,
				JobComInvoiceLineTaxSchema.Constants.TableName,
				JobDeclarationSchema.Constants.TableName,
				JobDocAddressSchema.Constants.TableName,
				JobDocumentDeliverySchema.Constants.TableName,
				JobDocumentExclusionSchema.Constants.TableName,
				OrgAddressCapabilitySchema.Constants.TableName,
				OrgAddressSchema.Constants.TableName,
				OrgHeaderSchema.Constants.TableName,
				OrgMiscServSchema.Constants.TableName,
				OrgSupplierBuyerLinkSchema.Constants.TableName,
				ProcessHeaderSchema.Constants.TableName,
				ProcessTasksSchema.Constants.TableName,
				ProcessTasksSchema.Constants.TableName,
				StmDataSchema.Constants.TableName,
				StmDocDataOverrideSchema.Constants.TableName,
				StmNoteSchema.Constants.TableName,
				StmUniversalCopySchema.Constants.TableName,
			};
		}

		protected virtual void SetUpDataForUniversalXmlImportAdd(UniversalShipment shipment)
		{
		}

		protected BusinessObject CreateFullyPopulatedObject(BusinessObjectFactory factory)
		{
			return CreateFullyPopulatedObject(factory, 3, 20);
		}

		BaseJobDeclaration CreateFullyPopulatedObject(BusinessObjectFactory factory, int numberOfInvoices, int numberOfInvoiceLines)
		{
			var declaration = BaseJobDeclaration.New(factory);
			declaration.JE_MessageType = MessageTypeForFormBashing;
			declaration.JE_IsCancelled = DeclarationIsCancelled;

			var declarationSupplier = OrgHeader.New(factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";
			declaration.JE_OH_Supplier = declarationSupplier.PK;

			var declarationImporter = OrgHeader.New(factory);
			declarationImporter.OH_Code = "IMPORTER";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.MainAddress.OA_Address1 = "Add1";
			declaration.JE_OH_Importer = declarationImporter.PK;

			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			DecorateDeclaration(declaration);
			AddAdditionalChildDataIfNeeded(declaration, 1);
			AddEntryInstructions(declaration, numberOfInvoices * numberOfInvoiceLines);

			var landedCostHeader = AddLandedCostInfos(declaration);
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			for (int invoiceLoop = 1; invoiceLoop <= numberOfInvoices; invoiceLoop++)
			{
				var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				var supplier = OrgHeader.New(factory);
				supplier.OH_IsConsignor = true;
				supplier.MainAddress.OA_Address1 = "Add1";
				supplier.OH_Code = "SUPPLIER" + invoiceLoop.ToString();
				invoice.JZ_OH_Supplier = supplier.PK;
				DecorateInvoiceHeader(invoice);
				AddAdditionalChildDataIfNeeded(invoice, invoiceLoop);
				var parentPK = ZGuid.Empty;
				for (int i = 0; i < numberOfInvoiceLines; i++)
				{
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();

					if (i % 3 == 1)
					{
						invoiceLine.JI_ParentID = ZGuid.Empty;
						var part = OrgSupplierPart.New(factory);
						part.OP_PartNum = "PART" + i;
						var relation = part.RelatedOrganisations.AddNew();
						relation.OU_OH = supplier.PK;
						relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

						var partUnit = part.PartUnits.AddNew();
						partUnit.OF_QuantityInParent = 24m;
						partUnit.OF_ParentPackType = "CTN";
						DecoratePart(part);

						invoiceLine.JI_PartNo = part.OP_PartNum;

						parentPK = invoiceLine.PK;
					}
					else
					{
						invoiceLine.JI_ParentID = parentPK;
					}
					DecorateInvoiceLine(invoiceLine);
					AddAdditionalChildDataIfNeeded(invoiceLine, i);
					AddLandedCostInfos(landedCostHeader, invoiceLine);
				}
			}

			if (!DeclarationIsCancelled)
			{
				Merge(declaration);
			}

			FinaliseCreateFullyPopulatedObject(declaration);
			return declaration;
		}

		protected virtual void FinaliseCreateFullyPopulatedObject(BaseJobDeclaration declaration)
		{
		}

		void AddAdditionalChildDataIfNeeded(BusinessObject bizObj, int index)
		{
			BaseJobDeclarationFormAbstractTest<BaseJobDeclaration>.AddAdditionalChildDataIfNeeded(bizObj, index);
		}

		protected virtual void DecorateDeclaration(BaseJobDeclaration declaration)
		{
		}

		BusinessObject AddLandedCostInfos(BaseJobDeclaration declaration)
		{
			BusinessObject landedCostHeader = null;
			if (((ILandedCostHeader)declaration).IsLCSupported)
			{
				var header = Factory.New<Integration.LandedCosting.ILandedCostHeader>();
				header.LT_ParentID = declaration.PK;
				header.LT_ParentTableCode = declaration.TablePrefix;
				header.LT_LandedCostType = MasterFiles.Business.LandedCostType.Actual;
				landedCostHeader = (BusinessObject)header;
			}

			return landedCostHeader;
		}

		void AddLandedCostInfos(BusinessObject landedCostHeader, BaseJobComInvoiceLine invoiceLine)
		{
			if (landedCostHeader != null)
			{
				var landedCostHistory = Factory.New<Integration.LandedCosting.ILandedCostHistory>();
				landedCostHistory.LH_LT = landedCostHeader.PK;
				landedCostHistory.LH_ParentID = invoiceLine.PK;
				landedCostHistory.LH_ParentTableCode = invoiceLine.TablePrefix;

				var landedCostItem1 = Factory.New<Integration.LandedCosting.ILandedLineCostItem>();
				landedCostItem1.LZ_LH = landedCostHistory.PK;
				landedCostItem1.LZ_CostType = "ST1";
				landedCostItem1.LZ_CostAmount = 0.5m;

				var landedCostItem2 = Factory.New<Integration.LandedCosting.ILandedLineCostItem>();
				landedCostItem2.LZ_LH = landedCostHistory.PK;
				landedCostItem2.LZ_CostType = "ST2";
				landedCostItem2.LZ_CostAmount = 0.6m;
			}
		}

		protected virtual void AddEntryInstructions(BaseJobDeclaration declaration, int numberOfEntryInstructions)
		{
		}

		protected virtual void DecorateInvoiceHeader(BaseJobComInvoiceHeader invoice)
		{
		}

		protected virtual void DecorateInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
		}

		protected virtual void DecoratePart(OrgSupplierPart part)
		{
		}

		protected virtual void Merge(BaseJobDeclaration declaration)
		{
		}

		protected virtual ZForm GetForm(BusinessObject bizO)
		{
			return new BaseJobDeclarationForm((BaseJobDeclaration)bizO);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var company = GlbCompany.CurrentCompany;
			var currentCountry = company.GC_RN_NKCountryCode;
			company.Reload();
			company.GC_RN_NKCountryCode = currentCountry;
			company.SetCountry(currentCountry);
			company.Factory.Save();
		}
	}
}
