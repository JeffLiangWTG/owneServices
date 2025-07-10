using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using Constants = Enterprise.Core.Constants;
using Loader = Enterprise.MasterFiles.Business.OrgSupplierPart.Loader;

namespace Enterprise.MasterFiles.Business.Testing
{
	#region VolumeCalculatorTest

	sealed class VolumeCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var product = Factory.New<OrgSupplierPart>();

			var calculator = new VolumeCalculator(
					(ZPropertyInfoDecimal)product.OP_DepthInfo,
					(ZPropertyInfoDecimal)product.OP_WidthInfo,
					(ZPropertyInfoDecimal)product.OP_HeightInfo,
					(ZPropertyInfoString)product.OP_MeasureUQInfo,
					(ZPropertyInfoString)product.OP_CubicUQInfo);
			AssertEquals(0m, calculator.Calculate());

			product.OP_Depth = 2m;
			product.OP_Width = 3;
			product.OP_Height = 4m;
			AssertEquals("Should use default UQs.", 24m, calculator.Calculate());

			product.OP_CubicUQ = Constants.Volume.CubicInches;
			AssertEquals("Should have calculated from Metres to Cubic Inches.", 1464569.858m, calculator.Calculate());

			product.OP_MeasureUQ = Constants.Length.Inches;
			product.OP_CubicUQ = Constants.Volume.CubicInches;
			AssertEquals("Should have calculated from Inches to Cubic Inches.", 24m, calculator.Calculate());

			AssertEquals("The ad-hoc Calculate() method Should have calculated from CM to M3.", 0.125m,
				VolumeCalculator.Calculate(50m, 50m, 50m, Constants.Length.Centimetres, Constants.Volume.CubicMetres));
		}
	}

	#endregion

	#region AuditParentTest

	sealed class AuditParentTest : TestCaseWithFactory
	{
		public void TestRelatedAuditChildren()
		{
			var product = Factory.New<OrgSupplierPart>();

			AssertContainsExactElementsInAnyOrder([
				new AuditChildInfo(CusClassPartPivotSchema.CI_OP, null),
				new AuditChildInfo(OrgPartRelationSchema.OU_OP, null),
				new AuditChildInfo(OrgPartUnitSchema.OF_OP, null),
				new AuditChildInfo(OrgSupplierPartBarcodeSchema.PH_OP, OrgSupplierPartBarcodeSchema.PH_Barcode),
				new AuditChildInfo(UNDGDataItemSchema.DI_ParentID, null)
			], product.RelatedAuditChildren);
		}
	}

	#endregion

	#region OrgSupplierPartWorkflowProviderTest

	[TestedType(typeof(OrgSupplierPart))]
	class OrgSupplierPartWorkflowProviderTest : WorkflowProviderTest<OrgSupplierPart, OrgSupplierPartProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode; }
		}
	}

	#endregion

	#region OrgSupplierPartTest

	[TestedType(typeof(OrgSupplierPart))]
	public class OrgSupplierPartTest : BusinessObjectWithCustomLabelsTestCase, IDataRefreshBusSubscriber
	{
		public void TestSetterSuspender()
		{
			var bizObj = Factory.New<OrgSupplierPart>();
			ISetterSuspenderSupporter supporter = bizObj;
			var setter = supporter.SetterSuspender;
			AssertNotNull(setter);
			Assert(setter is SetterSuspender);
			AssertSame(setter, supporter.SetterSuspender);
		}

		public void TestISetterSuspenderSupporter_SupportedFields()
		{
			var bizObj = Factory.New<OrgSupplierPart>();
			ISetterSuspenderSupporter supporter = bizObj;
			var supportedFields = supporter.SupportedFields.ToArray();
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements(ExpectedISetterSuspenderSupporter_SupportedFields, supportedFields);
				foreach (var supportedField in supportedFields)
				{
					var info = bizObj.ZPropertyInfoHash.GetPropertySafe(supportedField);
					var defaultValue = info.Value.Default;
					info.Value = defaultValue;
					using (bizObj.SetterSuspender.SuspendSetting(supportedField))
					{
						ZPropertyInfoTestHelper.SetValue(info);
						AssertEquals(supportedField + " should not be set", defaultValue, info.Value);
					}
					ZPropertyInfoTestHelper.SetValue(info);
					AssertNotEquals(supportedField + " should be set", defaultValue, info.Value);
				}
			});
		}

		protected virtual string[] ExpectedISetterSuspenderSupporter_SupportedFields => new[]
		{
			OrgSupplierPart.Schema.OP_Brand, OrgSupplierPart.Schema.OP_Model
		};

		public void TestHumanReadableName()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "123";
			AssertEquals("Part 123", part.HumanReadableName);
		}

		public void TestSetOPDescToMaxLengthAndReadback()
		{
			var part = Factory.New<OrgSupplierPart>();
			var maxLength = OrgSupplierPart.Schema.OP_DescMaxLength;

			AssertEquals("Max length of description should be 128", 128, maxLength);
			AssertNoExceptionThrown(() => part.OP_Desc = new string('a', maxLength));

			var description = part.OP_Desc;

			AssertEquals("Description can be set to max length and read back as max number of chars", maxLength, description.Length);
		}

		#region TestConstruction_HasChange_WhenNMFCChanged

		public void TestConstruction_HasChange_WhenNMFCChanged()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1PK = helper.CreateClient("C1");
			var product = (OrgSupplierPart)helper.CreateProduct(client1PK, "Product");

			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "COM1";
			commodity1.RH_Description = "COM1";
			commodity1.RH_IsShipping = true;

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "COM2";
			commodity2.RH_Description = "COM2";
			commodity2.RH_IsShipping = true;

			var nmfc1 = Factory.New<RefNMFC>();
			nmfc1.FN_Code = "100";
			nmfc1.FN_Class = "1";

			var nmfc2 = Factory.New<RefNMFC>();
			nmfc2.FN_Code = "200";
			nmfc2.FN_Class = "2";
			commodity2.RH_FN_NKNMFC = "200|2";

			product.OP_RH_NKCommodityCode = "COM1";
			Factory.Save();

			AssertEquals("Precondition:", false, product.HasChanges);

			commodity1.RH_FN_NKNMFC = "100|1";
			AssertEquals("Should trigger Product.HasChange", true, product.HasChanges);
			Factory.Save();

			AssertNoExceptionThrown("Remove CommodityCode should not cause exception", () => product.OP_RH_NKCommodityCode = "");

			product.OP_RH_NKCommodityCode = "COM2";
			AssertEquals("200|2", product.CommodityCode.RH_FN_NKNMFC);
			Factory.Save();

			AssertEquals("Precondition:", false, product.HasChanges);

			commodity2.RH_FN_NKNMFC = "100|1";
			AssertEquals("Should trigger Product.HasChange", true, product.HasChanges);
		}

		#endregion

		#region TestAllProductCategories

		public void TestAllProductCategories()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var client1PK = helper.CreateClient("C1");
			var client1 = Factory.Load<OrgHeader>(client1PK);
			var client2PK = helper.CreateClient("C2");
			var client2 = Factory.Load<OrgHeader>(client2PK);

			var productCategoryClient1 = helper.CreateProductCategory("1CCate", "Client1 Category", ZGuid.Empty);
			var productCategoryClient2 = helper.CreateProductCategory("2CCate", "Client2 Category", ZGuid.Empty);

			var product = (OrgSupplierPart)helper.CreateProduct(client1PK, "Product");
			var relationClient1 = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client1PK, OrgPartRelation.RelationshipTypes.Owner);
			relationClient1.OU_OPC_Category = productCategoryClient1.PK;

			var relationClient2 = product.RelatedOrganisations.AddNew();
			relationClient2.OU_OH = client2PK;
			relationClient2.OU_OPC_Category = productCategoryClient2.PK;

			AssertEquals("1CCate(C1) 2CCate(C2)", product.AllProductCategories);
		}

		#endregion

		public void TestOP_PartNum_TriggersValidationFor_OP_IsActive()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTORG";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.RelatedOrganisations.AddOwner(org);
			part1.OP_PartNum = "DUPTEST";
			part1.OP_Desc = "Active Duplicate";
			part1.OP_IsActive = true;

			var part2 = Factory.New<OrgSupplierPart>();
			part2.RelatedOrganisations.AddOwner(org);
			part2.OP_PartNum = "DUPTEST";
			part2.OP_Desc = "Inactive Duplicate";
			part2.OP_IsActive = false;

			part1.RunPreSaveValidation();
			part2.RunPreSaveValidation();

			const string duplicateMessage = "Duplicate Product detected: Owner = TSTORG";

			AssertNoError("no errors about duplicates because one product is inactive", part1.OP_PartNumInfo, duplicateMessage);
			AssertNoError("no errors about duplicates because one product is inactive", part2.OP_PartNumInfo, duplicateMessage);

			Factory.Save();

			part2.OP_IsActive = true;
			AssertHasError("change of OP_IsActive should trigger validation for OP_PartNum, otherwise light validation can entirely skip validation for product", part2.OP_PartNumInfo, duplicateMessage);
		}

		public void TestDuplicateValidationWithMultipleProductChanges()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TSTORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTORG2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TSTORG3";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "DUPTEST";
			part1.OP_Desc = "Duplicate Product";
			part1.RelatedOrganisations.AddSupplier(org1);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "DUPTEST";
			part2.OP_Desc = "Duplicate Product";
			part2.RelatedOrganisations.AddSupplier(org2);

			Factory.Save();

			AssertNoErrorContaining(part2.OP_PartNumInfo, "Duplicate Product detected");

			part1.RelatedOrganisations.AddOrganisationIfNotExist(org3.PK, OrgPartRelation.RelationshipTypes.Both);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org3.PK, OrgPartRelation.RelationshipTypes.Both);

			AssertHasErrorContaining(part2.OP_PartNumInfo, "Duplicate Product detected");
		}

		public void TestDuplicateValidationWithMultipleProductChanges_NoCollisionBetweenPartsWithDifferentCodes()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TSTORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTORG2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TSTORG3";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "TESTPART1";
			part1.OP_Desc = "Product #1";
			part1.RelatedOrganisations.AddSupplier(org1);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "Product #2";
			part2.OP_Desc = "Duplicate Product";
			part2.RelatedOrganisations.AddSupplier(org2);

			Factory.Save();

			part1.RelatedOrganisations.AddOrganisationIfNotExist(org3.PK, OrgPartRelation.RelationshipTypes.Both);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org3.PK, OrgPartRelation.RelationshipTypes.Both);

			AssertNoErrorContaining(part2.OP_PartNumInfo, "Duplicate Product detected");

			part2.OP_PartNum = "TESTPART1";

			AssertHasErrorContaining(part2.OP_PartNumInfo, "Duplicate Product detected");
		}

		public void TestDuplicateValidationWithMultipleProductChanges_MovingRelation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TSTORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTORG2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TSTORG3";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "DUPTEST";
			part1.OP_Desc = "Duplicate Product";
			part1.RelatedOrganisations.AddSupplier(org1);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(org3.PK, OrgPartRelation.RelationshipTypes.Both);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "DUPTEST";
			part2.OP_Desc = "Duplicate Product";
			part2.RelatedOrganisations.AddSupplier(org2);

			Factory.Save();

			part2.RelatedOrganisations.AddOrganisationIfNotExist(org3.PK, OrgPartRelation.RelationshipTypes.Both);

			AssertHasErrorContaining(part2.OP_PartNumInfo, "Duplicate Product detected");

			part1.RelatedOrganisations.Remove(part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == org3.PK));
			part2.Validation.ValidateOP_PartNum();

			AssertNoErrorContaining(part2.OP_PartNumInfo, "Duplicate Product detected");
		}

		#region TestIJobNumberForWorkflow_JobNumber

		public void TestIJobNumberForWorkflow_JobNumber()
		{
			Part.OP_PartNum = "DANIEL";
			AssertEquals("DANIEL", Part.JobNumber);
		}

		#endregion

		#region TestOperationalActionsFieldToShowVisibility

		public void TestOperationalActionsFieldToShowVisibility()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(OrgSupplierPart).GetProperty(OrgSupplierPartSchema.OP_IsActive.Name)).ReadOnly);

			AssertEquals(true, ActionFieldFollowAttribute.ShouldFollow(typeof(OrgSupplierPart).GetProperty("Warehouse_Specific")));
			AssertEquals(ObjectFactory.GetType<IWhsProduct>(), ActionFieldFollowAttribute.GetReturnType(typeof(OrgSupplierPart).GetProperty("Warehouse_Specific")));

			AssertEquals(true, ActionFieldFollowAttribute.ShouldFollow(typeof(OrgSupplierPart).GetProperty("RelatedOrganisations")));
		}

		#endregion

		#region TestOP_StockKeepingUnit_OperationalActionsField

		public void TestOP_StockKeepingUnit_OperationalActionsField()
		{
			var product = Factory.New<OrgSupplierPart>();
			var actionAttrib = (ActionFieldAttribute)product.OP_StockKeepingUnitInfo.PropertyDescriptor.Attributes[typeof(ActionFieldAttribute)];
			AssertEquals(typeof(OrgSupplierPart.StockUnitPairList), actionAttrib.CollectionType);

			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(new BusinessObjectFactory()).GetAsCodeDescriptionPairWithStandardUnits(), new OrgSupplierPart.StockUnitPairList());
		}

		#endregion

		#region TestStockUnitPairList

		public void TestStockUnitPairList()
		{
			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(new BusinessObjectFactory()).GetAsCodeDescriptionPairWithStandardUnits(), new OrgSupplierPart.StockUnitPairList());
		}

		#endregion

		#region TestIsClassifiedFor

		public void TestIsClassifiedFor()
		{
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Canadian Wine";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			AssertEquals(false, product.IsClassifiedFor(Core.Constants.CountryCodes.Australia, new ZString[] { "IMP" }));

			GlbCompany.CurrentCompany.SetCountry("AU");
			BusinessObject classification = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusClassification>());
			classification[CusClassificationSchema.CC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;
			classification[CusClassificationSchema.CC_TariffNum] = "2203.00.39 26";
			classification[CusClassificationSchema.CC_LookupCode] = "Wine";
			classification[CusClassificationSchema.CC_Description] = "Wine";
			classification[CusClassificationSchema.CC_ClassificationType] = "IMP";

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.Australia;
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_AddInfo] = "GSTE=FOOD*PST=CA";

			Factory.Save();

			AssertEquals(false, product.IsClassifiedFor(Core.Constants.CountryCodes.NewZealand, new ZString[] { "IMP" }));
			AssertEquals(true, product.IsClassifiedFor(Core.Constants.CountryCodes.Australia, new ZString[] { "IMP" }));
			AssertEquals(false, product.IsClassifiedFor(Core.Constants.CountryCodes.Australia, new ZString[] { "EXP" }));
		}

		#endregion

		#region TestChildCollectionsWhenRolledBack

		public void TestChildCollectionsWhenRolledBack()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Canadian Wine";

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			product.Delete();

			Assert("deleted", product.IsDeleted);
			AssertEquals("No relations", 0, product.RelatedOrganisations.Count);

			((IBusinessObjectFactoryInternals)Factory).Rollback();
			Assert("not deleted", !product.IsDeleted);
			AssertEquals("rolledback and populated again", 2, product.RelatedOrganisations.Count);
			Assert("rolledback and populated again", !product.RelatedOrganisations[0].IsDeleted);
		}

		#endregion

		#region TestSmallestStockKeepingUnitSize

		public void TestSmallestStockKeepingUnitSize()
		{
			var product = Factory.New<OrgSupplierPart>();
			AssertEquals(1m, product.SmallestStockKeepingUnitSize);

			product.OP_CountDecimalPlaces = 2;
			AssertEquals(0.01m, product.SmallestStockKeepingUnitSize);

			product.OP_CountDecimalPlaces = 0;
			AssertEquals(1m, product.SmallestStockKeepingUnitSize);

			product.OP_CountDecimalPlaces = 1;
			AssertEquals(0.1m, product.SmallestStockKeepingUnitSize);

			product.OP_CountDecimalPlaces = 10;
			AssertEquals(1m, product.SmallestStockKeepingUnitSize);
		}

		#endregion

		#region SetDefaultValues

		public void TestSetDefaultValues()
		{
			AssertEquals(Env.Registry.DefaultStockUnit, Part.OP_StockKeepingUnit);
			AssertEquals(true, Part.OP_IsActive);
			AssertEquals(Env.Registry.PackageWeightUnit, Part.OP_WeightUQ);
			AssertEquals(Env.Registry.PackageVolumeUnit, Part.OP_CubicUQ);
			AssertEquals(true, Part.OP_CanDisassembleKit);
		}

		#endregion

		#region TestTemplateCopy

		public virtual void TestITemplateCopyable()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgHeader header2 = Factory.New<OrgHeader>();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();

			using (part.GetValidationSuspender())
			{
				part.OP_Cubic = 52m;
				part.OP_CubicUQ = "M3";
				part.OP_Department = "001";
				part.OP_StockKeepingUnit = "SM";
				part.OP_Depth = 33m;
				part.OP_Division = "OP_Division";
				part.OP_Height = 99m;
				part.OP_IsActive = true;
				part.OP_MeasureUQ = "HA";
				part.OP_OrderMultipleQty = 78m;
				part.OP_Desc = "DESC";
				part.OP_OrderMultipleUnit = "ORD";
				part.OP_RH_NKCommodityCode = "COMM";
				part.OP_Width = 6.2m;
				part.OP_WeightUQ = "KG";
				part.OP_Weight = 78m;

				OrgPartRelation partRelation0 = part.RelatedOrganisations.AddOrganisationIfNotExist(header.PK, OrgPartRelation.RelationshipTypes.Owner);
				OrgPartRelation partRelation1 = part.RelatedOrganisations.AddOrganisationIfNotExist(header2.PK, OrgPartRelation.RelationshipTypes.Owner);

				SubclassSetupForTestITemplateCopyable(part);
			}

			OrgSupplierPart partClone = (OrgSupplierPart)((ITemplateCopyable)part).TemplateCopy();

			AssertEquals(part.OP_Cubic, partClone.OP_Cubic);
			AssertEquals(part.OP_CubicUQ, partClone.OP_CubicUQ);
			AssertEquals(part.OP_StockKeepingUnit, partClone.OP_StockKeepingUnit);
			AssertEquals(part.OP_Department, partClone.OP_Department);
			AssertEquals(part.OP_Depth, partClone.OP_Depth);
			AssertEquals(part.OP_Division, partClone.OP_Division);
			AssertEquals(part.OP_Height, partClone.OP_Height);

			AssertEquals(part.OP_IsActive, partClone.OP_IsActive);
			AssertEquals(part.OP_MeasureUQ, partClone.OP_MeasureUQ);

			AssertEquals(part.OP_OrderMultipleQty, partClone.OP_OrderMultipleQty);
			AssertEquals(part.OP_Desc, partClone.OP_Desc);
			AssertEquals(part.OP_OrderMultipleUnit, partClone.OP_OrderMultipleUnit);
			AssertEquals(part.OP_RH_NKCommodityCode, partClone.OP_RH_NKCommodityCode);

			AssertEquals(part.OP_Width, partClone.OP_Width);
			AssertEquals(part.OP_WeightUQ, partClone.OP_WeightUQ);
			AssertEquals(part.OP_Weight, partClone.OP_Weight);

			ZQuery partRelationsfilter = new ZQuery(OrgPartRelationSchema.OU_OP, partClone.PK);
			OrgPartRelation[] partRelations = Factory.Load<OrgPartRelation>(partRelationsfilter);
			AssertEquals(2, partRelations.Length);

			SubclassAssertForTestITemplateCopyable(partClone);
		}

		protected virtual void SubclassSetupForTestITemplateCopyable(OrgSupplierPart part)
		{
		}

		protected virtual void SubclassAssertForTestITemplateCopyable(OrgSupplierPart part)
		{
		}

		#endregion

		#region TestCopyPickModeFromOriginalProductInCopy

		public void TestCopyPickModeFromOriginalProductInCopy()
		{
			var part = Factory.New<OrgSupplierPart>();
			var org = Factory.New<OrgHeader>();
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);

			SetAndAssertPickMode(org, relation, part, WhsPickMode.Codes.AttributeNeutral, WhsPickMode.Codes.AttributeNeutral);
			SetAndAssertPickMode(org, relation, part, WhsPickMode.Codes.AttributeNeutral, WhsPickMode.Codes.AttributeSpecified);
			SetAndAssertPickMode(org, relation, part, WhsPickMode.Codes.AttributeSpecified, WhsPickMode.Codes.AttributeNeutral);
			SetAndAssertPickMode(org, relation, part, WhsPickMode.Codes.AttributeSpecified, WhsPickMode.Codes.AttributeSpecified);
		}

		static void SetAndAssertPickMode(OrgHeader org, OrgPartRelation relation, OrgSupplierPart part, string defaultWarehousePickMode, string productPickMode)
		{
			org.MiscServ.OM_WhsDefaultWarehousePickMode = defaultWarehousePickMode;
			relation.OU_PickMode = productPickMode;
			var copyPart = (OrgSupplierPart)((ITemplateCopyable)part).TemplateCopy();
			AssertEquals("Should be Pick Mode from original part", productPickMode, copyPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(org.PK, OrgPartRelation.RelationshipTypes.Owner).OU_PickMode);
		}

		#endregion

		#region TestDataRefreshBusUpdatesCollectionsBothOnAddAndEdit
		public void TestDataRefreshBusUpdatesCollectionsBothOnAddAndEdit()
		{
			BusinessObjectFactory factoryForCollection = new BusinessObjectFactory();
			OrgSupplierPartCollection partCollection = new OrgSupplierPartCollection(factoryForCollection);
			partCollection.Load();
			new DataRefreshManager().StartManaging(partCollection);
			AssertEquals("Precondition: PartCollection.Count", 0, partCollection.Count);

			Part.FillWithValidTestData();
			Part.OP_PartNum = "PART NUM";
			Part.OP_Desc = "INITIAL PART DESCRIPTION (ROTTING DOGS IN SPECIAL SAUCE)";
			Factory.Save();
			AssertEquals("PartCollection.Count after record added in another factory", 1, partCollection.Count);
			AssertEquals("PartCollection[0].OP_Desc matches the record added in another factory", Part.OP_Desc, partCollection[0].OP_Desc);

			Part.OP_Desc = "UPDATED PART DESCRIPTION (ROTTING DOGS IN HERRING SAUCE)";
			Factory.Save();
			AssertEquals("PartCollection.Count after record updated in another factory", 1, partCollection.Count);
			AssertEquals("PartCollection[0].OP_Desc matches the record added in another factory", Part.OP_Desc, partCollection[0].OP_Desc);
		}
		#endregion

		#region TestSavingNewObjectFiresDataRefresh
		public void TestSavingNewObjectFiresDataRefresh()
		{
			var newFactory = new BusinessObjectFactory();
			var part = newFactory.NewWithValidTestData<OrgSupplierPart>();
			new DataRefreshManager().StartManaging(OrgSupplierPartSchema.Constants.TableName, this);
			newFactory.Save();
			AssertEquals("Update Called", true, UpdateCalled);
		}
		#endregion

		#region TestSavingExistingObjectDoesFireDataRefresh
		public void TestSavingExistingObjectDoesFireDataRefresh()
		{
			var newFactory = new BusinessObjectFactory();
			var part = newFactory.NewWithValidTestData<OrgSupplierPart>();
			newFactory.Save();

			new DataRefreshManager().StartManaging(OrgSupplierPartSchema.Constants.TableName, this);
			part.OP_Desc = "RARARA HENRY WEARS A BRA";
			newFactory.Save();
			AssertEquals("Update Called", true, UpdateCalled);
		}
		#endregion

		#region TestOP_PartNumAlwaysConvertsToUpper
		public void TestOP_PartNumAlwaysConvertsToUpper()
		{
			Part.OP_PartNum = "abcdef";
			AssertEquals("TestPart.OP_PartNum", "ABCDEF", Part.OP_PartNum);
			Part.OP_PartNum = "ABCDEF";
			AssertEquals("TestPart.OP_PartNum", "ABCDEF", Part.OP_PartNum);
		}
		#endregion

		#region OP_RX_NKLastWeightedCostCurr

		public void TestOP_RX_NKLastWeightedCostCurr()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			AssertEquals("", part.OP_RX_NKLastWeightedCostCurr);

			part.OP_RX_NKLastWeightedCostCurr = "AAA";
			AssertEquals("AAA", part.OP_RX_NKLastWeightedCostCurr);
		}

		#endregion

		#region OP_StockKeepingUnitPerPallet

		public void TestOP_StockKeepingUnitForPalletProxy()
		{
			OrgSupplierPart product = OrgSupplierPart.New(Factory);
			product.OP_StockKeepingUnit = "";
			AssertEquals("(Units)", product.OP_StockKeepingUnitForPalletProxy);
			product.OP_StockKeepingUnit = "BOX";
			AssertEquals("(Boxes)", product.OP_StockKeepingUnitForPalletProxy);
			product.OP_StockKeepingUnit = "BAG";
			AssertEquals("(Bags)", product.OP_StockKeepingUnitForPalletProxy);
			product.OP_StockKeepingUnit = "XXX";
			AssertEquals("", product.OP_StockKeepingUnitForPalletProxy);
		}

		public void TestOP_StockKeepingUnitPerPalletInfo()
		{
			OrgSupplierPart product = OrgSupplierPart.New(Factory);
			AssertEquals("OP_StockKeepingUnitPerPallet", product.OP_StockKeepingUnitPerPalletInfo.Name);
			AssertEquals(true, product.OP_StockKeepingUnitPerPalletInfo.ReadOnly);
		}

		public void TestOP_StockKeepingUnitForPalletProxyInfo()
		{
			OrgSupplierPart product = OrgSupplierPart.New(Factory);
			AssertEquals("OP_StockKeepingUnitForPalletProxy", product.OP_StockKeepingUnitForPalletProxyInfo.Name);
		}

		#endregion

		#region TestOP_IsBarcoded

		public void TestOP_IsBarcoded()
		{
			var product = Factory.New<OrgSupplierPart>();
			AssertEquals(true, product.OP_IsBarcoded);

			product.OP_IsBarcoded = false;
			AssertEquals(false, product.OP_IsBarcoded);

			product.OP_IsBarcoded = true;
			AssertEquals(true, product.OP_IsBarcoded);
		}

		#endregion

		#region Local Part Code / Desc

		public void TestGetLocalPartNum()
		{
			OrgHeader relatedOrg = Factory.New<OrgHeader>();
			Part.OP_PartNum = "PRODUCTCODE";
			AssertEquals("PRODUCTCODE", Part.GetLocalPartNum(relatedOrg, OrgPartRelation.RelationshipTypes.Supplier));

			OrgPartRelation orgPart = Part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = relatedOrg.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			orgPart.OU_LocalPartNumber = "LOCALCODE";
			AssertEquals("Should equal Local Product Code", "LOCALCODE", Part.GetLocalPartNum(relatedOrg, OrgPartRelation.RelationshipTypes.Supplier));
			AssertEquals("Should fall back to Product Number if different related type", "PRODUCTCODE", Part.GetLocalPartNum(relatedOrg, OrgPartRelation.RelationshipTypes.WarehouseConsignee));
		}

		public void TestGetLocalPartNumForWarehouseConsignee()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			Part.OP_PartNum = "PRODUCTCODE";
			AssertEquals("PRODUCTCODE", Part.GetLocalPartNumForWarehouseConsignee(consignee));

			OrgPartRelation orgPart = Part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			orgPart.OU_LocalPartNumber = "LOCALCODE";
			AssertEquals("Should equal Local Product Code", "LOCALCODE", Part.GetLocalPartNumForWarehouseConsignee(consignee));
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertEquals("Should fall back to Product Number if different related type", "PRODUCTCODE", Part.GetLocalPartNumForWarehouseConsignee(consignee));
		}

		public void TestGetLocalPartDesc()
		{
			OrgHeader relatedOrg = Factory.New<OrgHeader>();
			Part.OP_Desc = "PRODUCT DESCRIPTION";
			AssertEquals("PRODUCT DESCRIPTION", Part.GetLocalPartDesc(relatedOrg, OrgPartRelation.RelationshipTypes.Supplier));

			OrgPartRelation orgPart = Part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = relatedOrg.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			orgPart.OU_LocalPartDescription = "LOCAL DESCRIPTION";
			AssertEquals("Should equal Local Product Description", "LOCAL DESCRIPTION", Part.GetLocalPartDesc(relatedOrg, OrgPartRelation.RelationshipTypes.Supplier));
			AssertEquals("Should fall back to Part Description if different related type", "PRODUCT DESCRIPTION", Part.GetLocalPartDesc(relatedOrg, OrgPartRelation.RelationshipTypes.WarehouseConsignee));
		}

		public void TestGetLocalPartDescForWarehouseConsignee()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			Part.OP_Desc = "PRODUCT DESCRIPTION";
			AssertEquals("PRODUCT DESCRIPTION", Part.GetLocalPartDescForWarehouseConsignee(consignee));

			OrgPartRelation orgPart = Part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			orgPart.OU_LocalPartDescription = "LOCAL DESCRIPTION";
			AssertEquals("Should equal Local Product Description", "LOCAL DESCRIPTION", Part.GetLocalPartDescForWarehouseConsignee(consignee));
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertEquals("Should fall back to Part Description if different related type", "PRODUCT DESCRIPTION", Part.GetLocalPartDescForWarehouseConsignee(consignee));
		}

		#endregion

		#region TestILandedCostHistoryMaster
		public void TestILandedCostHistoryMaster()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			AssertEquals("PK", part.PK, ((ILandedCostHistoryMaster)part).PK);
			AssertEquals("FKSchemaColumnInLandedCostHistory", LandedCostHistorySchema.LH_OP, ((ILandedCostHistoryMaster)part).FKSchemaColumnInLandedCostHistory);
			AssertEquals("ShouldMarginPercentagesReadOnly", true, ((ILandedCostHistoryMaster)part).ShouldMarginPercentagesReadOnly);
		}
		#endregion

		#region TestIsLogged
		public void TestIsLogged()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = "PartNum";
			Factory.Save();
			Assert("Is logged", part.Logs.DatabaseCount > 0);
		}
		#endregion

		#region TestDefaultValues
		public void TestDefaultValues()
		{
			Env.Registry.PackageVolumeUnit = Constants.Volume.CubicDecimetres;
			Env.Registry.PackageWeightUnit = Constants.Weight.Ounces;
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			AssertEquals("UNT", part.OP_StockKeepingUnit);
			AssertEquals(true, part.OP_IsActive);
			AssertEquals("Cubic UQ", Constants.Volume.CubicDecimetres, part.OP_CubicUQ);
			AssertEquals("Weight UQ", Constants.Weight.Ounces, part.OP_WeightUQ);
			part.IsImportedFromXML = true;
			AssertEquals(true, part.IsImportedFromXML);
		}
		#endregion

		#region TestDelete
		[ExpectNoExceptions]
		public void TestDelete()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgSupplierPart newPart = OrgSupplierPart.New(factory);
			newPart.OP_PartNum = "PartNumDeleteTest";
			OrgPartRelation relation = newPart.RelatedOrganisations.AddNew();
			OrgHeader organisation = factory.LoadTop1<OrgHeader>(new ZQuery());
			relation.OU_OH = organisation.PK;

			factory.Save();
			newPart.Delete();
			factory.Save();
		}
		#endregion

		#region TestUpdateWeightedCostFromLocations
		public void TestUpdateWeightedCostFromLocations()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartLocation location1 = part.Locations.AddNew();
			OrgPartLocation location2 = part.Locations.AddNew();

			location1.OR_WeightCostThisLocation = 2;
			location2.OR_WeightCostThisLocation = 3;
			location1.OR_InStock = 3;
			location2.OR_InStock = 1;

			part.UpdateWeightedCostFromLocations();
			AssertEquals("(2*3+3*1)/4=2.25", 2.25m, part.OP_WeightedCost);
		}
		#endregion

		#region TestBusinessObjectsWithRelatedEvents
		public virtual void TestBusinessObjectsWithRelatedEvents()
		{
			OrgSupplierPart product = OrgSupplierPart.New(Factory);
			OrgPartLocation location = product.Locations.AddNew();
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain Location.", location, product.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain Relation.", relation, product.BusinessObjectsWithRelatedEvents);
		}
		#endregion

		#region TestLocations
		public void TestLocations()
		{
			OrgSupplierPart bO = (OrgSupplierPart)GetNewBusinessObject();
			AssertEquals(0, bO.Locations.Count);
		}
		#endregion

		#region TestOP_OH_FormLayoutController
		public void TestOP_OH_FormLayoutController()
		{
			OrgSupplierPart part = (OrgSupplierPart)this.GetNewBusinessObject();
			part.OP_OH_FormLayoutControllerChanged += new EventHandler(OnPart_OP_OH_FormLayoutControllerChanged);
			try
			{
				foreach (OrgPartRelation relatedOrg in part.RelatedOrganisations)
				{
					relatedOrg.OU_FormLayoutController = false;
				}
				AssertEquals("Empty initially", true, part.OP_OH_FormLayoutController.IsEmpty);

				ZGuid orgPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
				OrgPartRelation relation = part.RelatedOrganisations.AddOrganisationIfNotExist(orgPK, OrgPartRelation.RelationshipTypes.Owner);
				fOP_OH_FormLayoutControllerEventFired = false;

				relation.OU_FormLayoutController = true;
				AssertEquals("Change event should fire", true, fOP_OH_FormLayoutControllerEventFired);
				fOP_OH_FormLayoutControllerEventFired = false;

				relation.OU_FormLayoutController = false;
				AssertEquals("Change event should fire", true, fOP_OH_FormLayoutControllerEventFired);
				fOP_OH_FormLayoutControllerEventFired = false;

				relation.OU_FormLayoutController = true;
				AssertEquals("Change event should fire", true, fOP_OH_FormLayoutControllerEventFired);
				fOP_OH_FormLayoutControllerEventFired = false;

				relation.Delete();
				AssertEquals("Change event should fire", true, fOP_OH_FormLayoutControllerEventFired);
				fOP_OH_FormLayoutControllerEventFired = false;
			}
			finally
			{
				part.OP_OH_FormLayoutControllerChanged -= new EventHandler(OnPart_OP_OH_FormLayoutControllerChanged);
			}
		}

		bool fOP_OH_FormLayoutControllerEventFired;
		void OnPart_OP_OH_FormLayoutControllerChanged(object sender, EventArgs e)
		{
			fOP_OH_FormLayoutControllerEventFired = true;
		}
		#endregion

		#region TestWeightPerStockUnitSavedIntoProductUnit
		public void TestWeightPerStockUnitSavedIntoProductUnit()
		{
			AssertEquals("PreCondition:TestPart doesnt have a unit record now", 0, Part.PartUnits.Count);

			Part.OP_StockKeepingUnit = "BAG";
			Part.OP_Weight = 10;
			Part.OP_WeightUQ = "KG";
			Part.OP_PartNum = "PartNum";
			Factory.Save();

			AssertEquals("TestPart has a unit record now", 1, Part.PartUnits.Count);
			OrgPartUnit partUnit = Part.PartUnits[0];
			AssertEquals("Parent Unit", Part.OP_StockKeepingUnit, partUnit.OF_ParentPackType);
			AssertEquals("Child Unit", Part.OP_WeightUQ, partUnit.OF_PackType);
			AssertEquals("Unit Conversion Factor", 10m, partUnit.OF_QuantityInParent);
		}
		#endregion

		#region TestShouldPreventReceiveOfPartWithNoWeightOrDims

		public void TestShouldPreventReceiveOfPartWithNoWeightOrDims_SetWhsCheckPartWeightOrDimsOnReceive_NON()
		{
			var value = CreateProductAndProductUnit("NON", productHasWeightOrDims: true, productUnitHasWeightOrDims: false);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value.Product, value.Client, expectResult: false);

			var value1 = CreateProductAndProductUnit("NON", productHasWeightOrDims: true, productUnitHasWeightOrDims: true);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value1.Product, value1.Client, expectResult: false);

			var value2 = CreateProductAndProductUnit("NON", productHasWeightOrDims: false, productUnitHasWeightOrDims: true);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value2.Product, value2.Client, expectResult: false);

			var value3 = CreateProductAndProductUnit("NON", productHasWeightOrDims: false, productUnitHasWeightOrDims: false);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value3.Product, value3.Client, expectResult: false);
		}

		public void TestShouldPreventReceiveOfPartWithNoWeightOrDims_SetWhsCheckPartWeightOrDimsOnReceive_ALL()
		{
			var value = CreateProductAndProductUnit("ALL", productHasWeightOrDims: true, productUnitHasWeightOrDims: false);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value.Product, value.Client, expectResult: true);

			var value1 = CreateProductAndProductUnit("ALL", productHasWeightOrDims: true, productUnitHasWeightOrDims: true);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value1.Product, value1.Client, expectResult: false);

			var value2 = CreateProductAndProductUnit("ALL", productHasWeightOrDims: false, productUnitHasWeightOrDims: true);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value2.Product, value2.Client, expectResult: true);

			var value3 = CreateProductAndProductUnit("ALL", productHasWeightOrDims: false, productUnitHasWeightOrDims: false);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value3.Product, value3.Client, expectResult: true);
		}

		public void TestShouldPreventReceiveOfPartWithNoWeightOrDims_SetWhsCheckPartWeightOrDimsOnReceive_SKU()
		{
			var value = CreateProductAndProductUnit("SKU", productHasWeightOrDims: true, productUnitHasWeightOrDims: false);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value.Product, value.Client, expectResult: false);

			var value1 = CreateProductAndProductUnit("SKU", productHasWeightOrDims: true, productUnitHasWeightOrDims: true);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value1.Product, value1.Client, expectResult: false);

			var value2 = CreateProductAndProductUnit("SKU", productHasWeightOrDims: false, productUnitHasWeightOrDims: true);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value2.Product, value2.Client, expectResult: true);

			var value3 = CreateProductAndProductUnit("SKU", productHasWeightOrDims: false, productUnitHasWeightOrDims: false);
			AssertShouldPreventReceiveOfPartWithNoWeightOrDims(value3.Product, value3.Client, expectResult: true);
		}

		public void TestShouldPreventReceiveOfPartWithNoWeightOrDims_SetWhsCheckPartWeightOrDimsOnReceive_Default()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partRelation = part.RelatedOrganisations.AddNew();
			using (partRelation.GetValidationSuspender())
			{
				partRelation.FillWithValidTestData();
				partRelation.OU_OP = part.PK;
				partRelation.OU_OH = client.PK;
				partRelation.OU_Relationship = "OWN";

				AssertShouldPreventReceiveOfPartWithNoWeightOrDims(part, client, expectResult: false);
				AssertEquals("NON", client.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive);
			}
		}

		(OrgHeader Client, OrgSupplierPart Product) CreateProductAndProductUnit(string option, bool productHasWeightOrDims, bool productUnitHasWeightOrDims)
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive = option;

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partRelation = part.RelatedOrganisations.AddNew();
			partRelation.SuspendValidation();
			partRelation.FillWithValidTestData();
			partRelation.OU_OP = part.PK;
			partRelation.OU_OH = client.PK;
			partRelation.OU_Relationship = "OWN";
			part.OP_StockKeepingUnit = "BAG";
			part.OP_PartNum = "PartNum";

			if (productHasWeightOrDims)
			{
				part.OP_Weight = 10m;
			}

			var productUnit = part.PartUnits.AddNew();
			productUnit.OF_ParentPackType = "BOX";

			if (productUnitHasWeightOrDims)
			{
				part.PartUnits[0].OF_Weight = 10m;
			}

			Factory.Save();
			return (client, part);
		}

		void AssertShouldPreventReceiveOfPartWithNoWeightOrDims(OrgSupplierPart part, OrgHeader client, bool expectResult)
		{
			AssertEquals(expectResult, part.ShouldPreventReceiveOfPartWithNoWeightOrDims(client));
		}

		#endregion

		#region TestCubePerStockUnitSavedIntoProductUnit
		public void TestCubePerStockUnitSavedIntoProductUnit()
		{
			AssertEquals("PreCondition:TestPart doesnt have a unit record now", 0, Part.PartUnits.Count);

			Part.OP_StockKeepingUnit = "BAG";
			Part.OP_Cubic = 10;
			Part.OP_CubicUQ = "KG";
			Part.OP_PartNum = "PartNumInCudbPerStockUnit";
			Factory.Save();

			AssertEquals("TestPart has a unit record now", 1, Part.PartUnits.Count);
			OrgPartUnit partUnit = Part.PartUnits[0];
			AssertEquals("Parent Unit", Part.OP_StockKeepingUnit, partUnit.OF_ParentPackType);
			AssertEquals("Child Unit", Part.OP_CubicUQ, partUnit.OF_PackType);
			AssertEquals("Unit Conversion Factor", 10m, partUnit.OF_QuantityInParent);
		}
		#endregion

		#region TestDocManagerCode
		public void TestDocManagerCode()
		{
			OrgSupplierPart product = Factory.New(typeof(OrgSupplierPart)) as OrgSupplierPart;
			AssertEquals("Code should be PRD. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "PRD", ((IDocManagerSupport)product).DocManagerInfo.DocManagerCode);
		}
		#endregion

		#region TestParts
		public void TestParts()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgHeader header2 = Factory.New<OrgHeader>();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			OrgPartRelation partRelation = part.RelatedOrganisations.AddNew();
			OrgPartRelation partRelation2 = part.RelatedOrganisations.AddNew();
			using (partRelation.GetValidationSuspender())
			{
				partRelation.FillWithValidTestData();
				partRelation.OU_OP = part.PK;
				partRelation.OU_OH = header.PK;
				partRelation.OU_LocalPartNumber = "TEST1";
				partRelation.OU_LocalPartDescription = "THIS IS TO TEST THE STRING";

				using (partRelation2.GetValidationSuspender())
				{
					partRelation2.FillWithValidTestData();
					partRelation2.OU_OP = part.PK;
					partRelation2.OU_OH = header2.PK;
					partRelation2.OU_LocalPartNumber = "TEST2";
					partRelation2.OU_LocalPartDescription = "THIS IS TO TEST THE STRING JOIN";

					AssertEquals("All Parts", "TEST1, TEST2", part.AllLocalParts);
					AssertEquals("All Part Descriptions", "THIS IS TO TEST THE STRING, THIS IS TO TEST THE STRING JOIN", part.AllLocalPartDescriptions);
				}
			}
		}
		#endregion

		#region TestHasStockOnHandOrInTransit

		public void TestHasStockOnHandOrInTransit()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "P2");
			AssertEquals("Should *not* have Stock On Hand.", false, part1.HasStockOnHandOrInTransit);

			// Add some stock on hand
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			Factory.Save();

			var receiveLine = (IWhsDocketLine)helper.CreateStock(whsPK, client, part1.PK, 10m);
			Factory.Save();
			AssertEquals("Should have Stock On Hand.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Should *not* have Stock On Hand.", false, part2.HasStockOnHandOrInTransit);

			var orderPK = helper.CreateWhsOrder(client, whsPK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part1.PK, 10);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });

			var pickLine = helper.GetPickLines(pickPK).Single();
			AssertEquals("Precondition: Stock On Hand exists.", 10m, receiveLine.WE_StockOnHand);
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			AssertEquals("Precondition: Stock On Hand reduced.", 0m, receiveLine.WE_StockOnHand);
			Factory.Save();

			AssertEquals("Should have Stock In-Transit.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Should *not* have Stock In-Transit.", false, part2.HasStockOnHandOrInTransit);

			helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();

			AssertEquals("Should *not* have Stock On Hand or In-Transit.", false, part1.HasStockOnHandOrInTransit);
			AssertEquals("Should *not* have Stock On Hand or In-Transit.", false, part2.HasStockOnHandOrInTransit);
		}

		#endregion

		#region TestHasAsnLineOnUnfinalisedReceive

		public void TestHasAsnLineOnUnfinalisedReceive()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var whs = helper.CreateWarehouse("1", "A");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false when no Receive is referencing the product.", false, part1.HasAsnLineOnUnfinalisedReceive);
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part1.PK, 10m, "A");
			helper.CreateAsnLine(receivePk, part1.PK, 10m);
			Factory.Save();
			var receive = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receivePk);

			AssertNotEquals("Current status of the receive is not finalised.", "FIN", receive.WD_DocketStatus);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be true when unfinalised Receive is referencing the product.", true, part1.HasAsnLineOnUnfinalisedReceive);
			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();

			AssertEquals("Current status of the receive is finalised.", "FIN", receive.WD_DocketStatus);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false when finalised these Receive which is referencing the product.", false, part1.HasAsnLineOnUnfinalisedReceive);
		}

		public void TestHasAsnLineOnUnfinalisedReceive_Cancelled()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var whs = helper.CreateWarehouse("1", "A");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false when no Receive is referencing the product.", false, part1.HasAsnLineOnUnfinalisedReceive);
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part1.PK, 10m, "A");
			helper.CreateAsnLine(receivePk, part1.PK, 10m);
			Factory.Save();
			var receive = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receivePk);
			receive.WD_DocketStatus = "ERR";
			receive.WD_DocketStatus = "CAN";
			Factory.Save();
			AssertEquals("Current status of the receive is cancelled.", "CAN", receive.WD_DocketStatus);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false when cancelled these Receive which is referencing the product.", false, part1.HasAsnLineOnUnfinalisedReceive);
		}

		#endregion

		#region HasActiveOperatorTransaction

		public virtual void TestHasActiveOperatorTransaction()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var whsPK = helper.CreateWarehouse("IPW", "A").PK;
			var part = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			Factory.Save();

			AssertEquals("Should not have an active transaction", false, part.HasActiveOperatorTransaction);

			var warehouseMainAddressPK = OrgSupplierPartTestHelper.GetWarehouseMainAddressPK(Factory, whsPK);
			var importerPK = OrgSupplierPartTestHelper.GetImporterPK(Factory);
			var transactionPK = OrgSupplierPartTestHelper.GetNewTransactionPK(TestConnection, part, warehouseMainAddressPK, importerPK);

			AssertEquals("Should not have an active transaction", false, part.HasActiveOperatorTransaction);

			OrgSupplierPartTestHelper.UpdateTransactionStatus(TestConnection, transactionPK, "QUE");

			Assert("Should have an active transaction", part.HasActiveOperatorTransaction);

			OrgSupplierPartTestHelper.UpdateTransactionStatus(TestConnection, transactionPK, "VAL");

			Assert("Should have an active transaction", part.HasActiveOperatorTransaction);
		}

		#endregion

		#region TestHasStockOnHandOrInTransit_Staged

		public void TestHasStockOnHandOrInTransit_Staged()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			Factory.Save();

			var receive = helper.CreateWhsReceive(org.PK, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receive, part.PK, 10m, ZGuid.Empty);
			helper.WhsReceiveAllocateLocationsMock(receive);
			helper.FinaliseDocketWithoutUserConfirmation(receive);
			Factory.Save();

			var orderPK = helper.CreateWhsOrder(org.PK, whs.PK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part.PK, 10m);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });
			var transferLine = helper.PickAndMakeInTransitTransfer(helper.GetPickLines(pickPK).Single(), ZDateTime.Today);
			helper.FinaliseDocketLine(transferLine.PK);
			AssertEquals("Should be Staged.", "STA", transferLine.WE_CurrentInventoryStatus);
			Factory.Save();
			AssertEquals("Should return true as stock exists.", true, part.HasStockOnHandOrInTransit);

			helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();
			AssertEquals("Should return false as no stock exists.", false, part.HasStockOnHandOrInTransit);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var part = Factory.New<OrgSupplierPart>();
			AssertNotNull(part.Warehouse_Specific);
		}

		#endregion

		#region TestProduct

		public void TestOP_IsComponentPickedOnSalesOrder()
		{
			var part = Factory.New<OrgSupplierPart>();
			AssertEquals(false, part.OP_IsComponentPickedOnSalesOrder);
			part.OP_IsComponentPickedOnSalesOrder = true;
			AssertEquals(true, part.OP_IsComponentPickedOnSalesOrder);
		}

		#endregion

		#region TestBillOfMaterials

		public void TestBillOfMaterials()
		{
			OrgPartBOM bom = Factory.New<OrgPartBOM>();
			OrgSupplierPart subPart = Factory.New<OrgSupplierPart>();
			bom.OE_OP_MainProduct = Part.PK;
			bom.OE_OP_Component = subPart.PK;
			AssertNotNull(Part.BillOfMaterials);
			AssertEquals(1, Part.BillOfMaterials.Count);
			Assert(Part.BillOfMaterials.Contains(bom));
		}

		public void TestBillOfMaterialsView()
		{
			OrgPartBOM bom = Factory.New<OrgPartBOM>();
			OrgPartBOM bom1 = Factory.New<OrgPartBOM>();
			OrgSupplierPart subPart = Factory.New<OrgSupplierPart>();
			subPart.OP_Desc = "TEST PART NUMBER 1";
			OrgSupplierPart subPart1 = Factory.New<OrgSupplierPart>();
			subPart1.OP_Desc = "TEST PART NUMBER 2";
			bom.OE_OP_MainProduct = Part.PK;
			bom.OE_OP_Component = subPart.PK;
			bom1.OE_OP_MainProduct = subPart.PK;
			bom1.OE_OP_Component = subPart1.PK;

			AssertEquals(0, Part.BillOfMaterialsView.Count);

			Part.BillOfMaterialsView.Reload(bom);
			AssertEquals(1, Part.BillOfMaterialsView.Count);
			AssertEquals(subPart1.OP_Desc, Part.BillOfMaterialsView[0].ComponentDescription);

			Part.BillOfMaterialsView.Reload(bom1);
			AssertEquals(0, Part.BillOfMaterialsView.Count);
		}

		#endregion

		#region TestIsBOMProduct

		public void TestIsBOMProduct_True()
		{
			var product = Factory.New<OrgSupplierPart>();

			var bom = Factory.New<OrgPartBOM>();
			var subPart = Factory.New<OrgSupplierPart>();
			bom.OE_OP_MainProduct = product.PK;
			bom.OE_OP_Component = subPart.PK;

			AssertEquals(true, product.IsBOMProduct);
		}

		public void TestIsBOMProduct_False()
		{
			var product = Factory.New<OrgSupplierPart>();
			AssertEquals(false, product.IsBOMProduct);
		}

		#endregion

		#region TestSecondaryParts

		public void TestSecondaryParts()
		{
			var part = Factory.New<OrgSupplierPart>();
			AssertType<OrgSecondaryPartBOMCollection>(part.SecondaryParts);
			AssertEquals(true, part.IsRegisteredEditableChildObject(part.SecondaryParts));

			var newSecondaryPart = part.SecondaryParts.AddNew();
			AssertEquals(part.PK, newSecondaryPart.OSB_OP_MainProduct);
		}

		#endregion

		#region TestCubicCalculation
		public void TestCubicCalculation()
		{
			Part.OP_Depth = 1m;
			Part.OP_Height = 1m;
			Part.OP_Width = 1m;
			Part.OP_MeasureUQ = Constants.Length.Metres;
			AssertEquals(1m, Part.OP_Depth);
			AssertEquals(1m, Part.OP_Height);
			AssertEquals(1m, Part.OP_Width);
			AssertEquals(Constants.Length.Metres, Part.OP_MeasureUQ);
			AssertEquals(Constants.Volume.CubicMetres, Part.OP_CubicUQ);
			AssertEquals(1m, Part.OP_Cubic);

			Part.OP_Depth = 2m;
			AssertEquals(2m, Part.OP_Depth);
			AssertEquals(1m, Part.OP_Height);
			AssertEquals(1m, Part.OP_Width);
			AssertEquals(Constants.Length.Metres, Part.OP_MeasureUQ);
			AssertEquals(Constants.Volume.CubicMetres, Part.OP_CubicUQ);
			AssertEquals(2m, Part.OP_Cubic);

			Part.OP_Height = 2m;
			AssertEquals(2m, Part.OP_Depth);
			AssertEquals(2m, Part.OP_Height);
			AssertEquals(1m, Part.OP_Width);
			AssertEquals(Constants.Length.Metres, Part.OP_MeasureUQ);
			AssertEquals(Constants.Volume.CubicMetres, Part.OP_CubicUQ);
			AssertEquals(4m, Part.OP_Cubic);

			Part.OP_Width = 2m;
			AssertEquals(2m, Part.OP_Depth);
			AssertEquals(2m, Part.OP_Height);
			AssertEquals(2m, Part.OP_Width);
			AssertEquals(Constants.Length.Metres, Part.OP_MeasureUQ);
			AssertEquals(Constants.Volume.CubicMetres, Part.OP_CubicUQ);
			AssertEquals(8m, Part.OP_Cubic);

			Part.OP_MeasureUQ = Constants.Length.Feet;
			AssertEquals(2m, Part.OP_Depth);
			AssertEquals(2m, Part.OP_Height);
			AssertEquals(2m, Part.OP_Width);
			AssertEquals(Constants.Length.Feet, Part.OP_MeasureUQ);
			AssertEquals(Constants.Volume.CubicMetres, Part.OP_CubicUQ); // Should not change CubicUQ when MeasureUQ is changed.
			AssertEquals(0.227m, Part.OP_Cubic);

			Part.OP_Depth = 10m;
			Part.OP_Height = 10m;
			Part.OP_Width = 10m;
			Part.OP_MeasureUQ = Constants.Length.Centimetres;
			Part.OP_CubicUQ = Constants.Volume.Litre;
			AssertEquals(10m, Part.OP_Depth);
			AssertEquals(10m, Part.OP_Height);
			AssertEquals(10m, Part.OP_Width);
			AssertEquals(Constants.Length.Centimetres, Part.OP_MeasureUQ);
			AssertEquals(Constants.Volume.Litre, Part.OP_CubicUQ);
			AssertEquals(1m, Part.OP_Cubic);

			Part.OP_Cubic = 0.1m;
			AssertEquals(10m, Part.OP_Depth);
			AssertEquals(10m, Part.OP_Height);
			AssertEquals(10m, Part.OP_Width);
			AssertEquals(Constants.Length.Centimetres, Part.OP_MeasureUQ);
			AssertEquals(Constants.Volume.Litre, Part.OP_CubicUQ);
			AssertEquals(0.1m, Part.OP_Cubic);
		}

		#endregion

		#region TestCubicIsInvalid
		public void TestCubicIsInvalid()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();

			using (product.GetValidationSuspender())
			using (product.PostponedCubicUpdate())
			{
				product.OP_Depth = 1m;
				product.OP_Height = 1m;
				product.OP_Width = 1m;
				product.OP_MeasureUQ = Constants.Length.Metres;
				product.OP_CubicUQ = "XX";
				product.OP_Cubic = 1m;
			}

			AssertEquals(true, product.CubicIsInvalid());
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(Part.GetType()));
		}

		#endregion

		#region ExtendedCommercialDescription

		public void TestOP_ExtendedCommercialDescription()
		{
			Part.Notes.RemoveAndDeleteAll();
			AssertEquals("Part.OP_ExtendedCommercialDescription", ZString.Empty, Part.OP_ExtendedCommercialDescription);
			Part.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description, "Bob The Builder!!!");
			AssertEquals("Part.OP_ExtendedCommercialDescription", "Bob The Builder!!!", Part.OP_ExtendedCommercialDescription);
		}

		#endregion

		#region NoteTypes

		public void TestNoteTypes()
		{
			Assert("PredefinedNoteTypes.Instance.ExtendedCommercialDescription should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.ExtendedCommercialDescription, Part.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.ExtendedCommercialDescription should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.HandlingInstructions, Part.NoteTypes));
		}

		bool PredefinedNoteTypeExist(PredefinedNoteType noteTypeToCheck, NoteTypeCollection noteTypes)
		{
			bool result = false;

			foreach (PredefinedNoteType noteType in noteTypes)
			{
				if (noteTypeToCheck == noteType)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion

		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			foreach (OrgPartRelation relatedOrg in part.RelatedOrganisations)
			{
				relatedOrg.OU_FormLayoutController = false;
			}

			AssertEquals("Empty initially", true, part.OP_OH_FormLayoutController.IsEmpty);
			AssertEquals(1, ((IColumnValueRankerInternals)part.GetTemplateSelectionCriteria()).ColumnValues.Count());

			IWorkflowProviderCore provider = part;
			var criteria = (ColumnValueRanker)provider.GetTemplateSelectionCriteria();
			AssertEquals("P0_OH_Client is empty guid", ZGuid.Empty.ToString(), criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0].ToString());

			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(client.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation.OU_FormLayoutController = true;
			AssertEquals(1, ((IColumnValueRankerInternals)part.GetTemplateSelectionCriteria()).ColumnValues.Count());

			criteria = (ColumnValueRanker)provider.GetTemplateSelectionCriteria();
			AssertEquals("P0_OH_Client is client.PK", client.PK.ToString(), criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0].ToString());
		}

		#endregion

		#region IOrgSupplierPart Members

		public void TestIOrgSupplierPartMembers()
		{
			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = "ABC";
			part.OP_StockKeepingUnit = "PLT";
			part.OP_RH_NKCommodityCode = "123";
			part.OP_Weight = 3.14m;
			part.OP_WeightUQ = "KG";
			part.OP_Cubic = 9.81m;
			part.OP_CubicUQ = "M3";

			var iPart = (IOrgSupplierPart)part;
			AssertEquals(nameof(IOrgSupplierPart.PK), part.PK, iPart.PK);
			AssertEquals(nameof(IOrgSupplierPart.OP_PartNum), "ABC", iPart.OP_PartNum);
			AssertEquals(nameof(IOrgSupplierPart.OP_StockKeepingUnit), "PLT", iPart.OP_StockKeepingUnit);
			AssertEquals(nameof(IOrgSupplierPart.OP_RH_NKCommodityCode), "123", iPart.OP_RH_NKCommodityCode);
			AssertEquals(nameof(IOrgSupplierPart.OP_Weight), 3.14m, iPart.OP_Weight);
			AssertEquals(nameof(IOrgSupplierPart.OP_WeightUQ), "KG", iPart.OP_WeightUQ);
			AssertEquals(nameof(IOrgSupplierPart.OP_Cubic), 9.81m, iPart.OP_Cubic);
			AssertEquals(nameof(IOrgSupplierPart.OP_CubicUQ), "M3", iPart.OP_CubicUQ);
		}

		#endregion

		public void TestIUnitConverterDataProviderType()
		{
			var part = Factory.New<OrgSupplierPart>();
			AssertEquals("Pack Conversion Type should be All Areas", RPTypeList.Codes.AllAreas, ((IUnitConverterDataProvider)part).Type);
		}

		public void TestGetProductDefaultHoldCode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var client2 = helper.CreateClient("ABC2");
			var client3 = helper.CreateClient("ABC3");

			var relatedOrg2 = part1.RelatedOrganisations.AddNew();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relatedOrg2.OU_OH = client2;

			var damagedInventoryHeldCode = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Single();
			relatedOrg2.OU_WHC_DefaultInventoryHoldCode = damagedInventoryHeldCode.PK;
			Factory.Save();

			var relatedOrg1 = part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(relation => relation.OU_OH == client1);
			AssertEquals("Precondition", ZGuid.Empty, relatedOrg1.OU_WHC_DefaultInventoryHoldCode);
			AssertEquals("There should be no product default hold code, org part relation has no default hold code.", string.Empty, part1.GetProductDefaultHoldCode(client1));

			AssertEquals("Precondition", damagedInventoryHeldCode.PK, relatedOrg2.OU_WHC_DefaultInventoryHoldCode);
			AssertEquals("Product default hold code is correct.", "DAM", part1.GetProductDefaultHoldCode(client2));

			AssertEquals("Precondition", false, part1.RelatedOrganisations.Cast<OrgPartRelation>().Any(relation => relation.OU_OH == client3));
			AssertEquals("There should be no product default hold code, no org part relation between product and client.", string.Empty, part1.GetProductDefaultHoldCode(client3));
		}

		public void TestGetProductDefaultHoldCode_BasedOnCorrectRelationshipType()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var client2 = helper.CreateClient("ABC2");
			var client3 = helper.CreateClient("ABC3");
			var client4 = helper.CreateClient("ABC4");

			var damagedInventoryHeldCode = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Single();
			var relatedOrg1 = part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relatedOrg1.OU_WHC_DefaultInventoryHoldCode = damagedInventoryHeldCode.PK;

			var relatedOrg2 = part1.RelatedOrganisations.AddNew();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relatedOrg2.OU_OH = client2;
			relatedOrg2.OU_WHC_DefaultInventoryHoldCode = damagedInventoryHeldCode.PK;

			var relatedOrg3 = part1.RelatedOrganisations.AddNew();
			relatedOrg3.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relatedOrg3.OU_OH = client3;
			relatedOrg3.OU_WHC_DefaultInventoryHoldCode = damagedInventoryHeldCode.PK;

			var relatedOrg4 = part1.RelatedOrganisations.AddNew();
			relatedOrg4.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			relatedOrg4.OU_OH = client4;
			relatedOrg4.OU_WHC_DefaultInventoryHoldCode = damagedInventoryHeldCode.PK;

			Factory.Save();

			AssertEquals("Precondition", OrgPartRelation.RelationshipTypes.Owner, relatedOrg1.OU_Relationship);
			AssertEquals("Precondition", damagedInventoryHeldCode.PK, relatedOrg1.OU_WHC_DefaultInventoryHoldCode);
			AssertEquals("Product default hold code is returned for relationship type OWN.", "DAM", part1.GetProductDefaultHoldCode(client1));

			AssertEquals("Precondition", OrgPartRelation.RelationshipTypes.Both, relatedOrg2.OU_Relationship);
			AssertEquals("Precondition", damagedInventoryHeldCode.PK, relatedOrg2.OU_WHC_DefaultInventoryHoldCode);
			AssertEquals("Product default hold code is returned for relationship type BOTH.", "DAM", part1.GetProductDefaultHoldCode(client2));

			AssertEquals("Precondition", OrgPartRelation.RelationshipTypes.Supplier, relatedOrg3.OU_Relationship);
			AssertEquals("Precondition", damagedInventoryHeldCode.PK, relatedOrg3.OU_WHC_DefaultInventoryHoldCode);
			AssertEquals("Product default hold code is not returned for relationship type SUPPLIER.", string.Empty, part1.GetProductDefaultHoldCode(client3));

			AssertEquals("Precondition", OrgPartRelation.RelationshipTypes.WarehouseConsignee, relatedOrg4.OU_Relationship);
			AssertEquals("Precondition", damagedInventoryHeldCode.PK, relatedOrg4.OU_WHC_DefaultInventoryHoldCode);
			AssertEquals("Product default hold code is not returned for relationship type WAREHOUSE CONSIGNEE.", string.Empty, part1.GetProductDefaultHoldCode(client3));
		}

		#region Implementation

		#region Part
		protected OrgSupplierPart Part
		{
			get { return fPart ?? (fPart = OrgSupplierPart.New(Factory)); }
		}

		OrgSupplierPart fPart;
		#endregion

		#region GetNewCustomLabelsProvider
		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			return new OrgSupplierPart.CustomLabelsProvider((OrgSupplierPart)bO);
		}
		#endregion

		#region IDataRefreshBusSubscriber Members

		BusinessObjectFactory IDataRefreshBusSubscriber.Factory
		{
			get { return Factory; }
		}

		bool UpdateCalled;
		public void UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			UpdateCalled = true;
		}

		bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => false;

		#endregion

		#endregion
	}

	#endregion

	#region OrgSupplierPartDeletionTest

	public class OrgSupplierPartDeletionTest : TestCase
	{
		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestDeletionWithConversionToPLT()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var client = helper.CreateClient("ABC1");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var units = part.PartUnits;
			var unit1 = units.AddNew();
			unit1.OF_PackType = "UNT";
			unit1.OF_ParentPackType = "PLT";
			unit1.OF_QuantityInParent = 10;
			factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var partFromOtherFactory = otherFactory.Load<OrgSupplierPart>(part.PK);
			AssertEquals("Precondition", 10m, partFromOtherFactory.OP_UnitsPerPallet);
			partFromOtherFactory.Delete();
			otherFactory.Save();
			AssertNull(otherFactory.Load<OrgSupplierPart>(part.PK));
		}
	}

	#endregion

	#region OrgSupplierPartTriggerTest

	public class OrgSupplierPartTriggerTest : TestCaseWithFactory
	{
		#region TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			part2.OP_IsActive = false;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TCE";
			Factory.Save();

			part2.OP_IsActive = true;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when activating product has same barcode and same owner with other products.");
		}

		#endregion

		#region TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_InactivateProductWithDuplicatedBarcode

		public void TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_InactivateProductWithDuplicatedBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";
			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TCE";
			Factory.Save();

			var relatedOrg2 = part2.RelatedOrganisations.AddNew();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relatedOrg2.OU_OH = client1;
			part2.OP_IsActive = false;
			AssertNoExceptionThrown("Throw no excetpion when inactivating the product has same barcode and same owner with other products.",
				Factory.Save);
		}

		#endregion

		#region TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode = part.PartBarcodes.AddNew();
			barcode.PH_Barcode = "TestCode";
			barcode.PH_F3_NKPackType = "TCE";

			Factory.Save();

			part.OP_PartNum = "TestCode";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product code equal to barcode");
		}

		#endregion

		#region TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode_AnotherProduct

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode_AnotherProduct()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_F3_NKPackType = "TC2";
			Factory.Save();

			part1.OP_PartNum = "TestCode2";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product code equal to the barcode of another product");
		}

		#endregion

		#region TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode_DifferentClient

		public void TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode_DifferentClient()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";

			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_F3_NKPackType = "TC2";
			Factory.Save();

			part1.OP_PartNum = "TestCode2";
			AssertNoExceptionThrown("Throw no exception when products are in different client",
				Factory.Save);
		}

		#endregion

		#region TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode_HasNoBarcode

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode_ProductCodeEqualToBarcode_HasNoBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";

			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "TestCode");

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product code equal to the barcode of another product");
		}

		#endregion
	}

	#endregion

	#region LoaderTest
	[TestedType(typeof(Loader))]
	class OrgSupplierPartLoaderTest : LoaderTestCase
	{
		public void TestEmptyPartReturnsNull()
		{
			AssertNull(new Loader(Factory).Load(ZString.Empty, null, null));
		}

		public void TestLoadWithMatchingBuyerAndSupplier()
		{
			Loader loader = new Loader(Factory);
			AssertEquals(partData.Part1, loader.Load("ptcode", partData.Buyer, partData.Supplier));
		}

		public void TestBestMatch()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Loader loader = new Loader(Factory);
				AssertMatchProductWithCorrectPriority(loader);
			}
		}

		public void TestBestMatchDisableExactMatch()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				Loader loader = new Loader(Factory, false);
				AssertMatchProductWithCorrectPriority(loader);
			}
		}

		public void AssertMatchProductWithCorrectPriority(Loader loader)
		{
			AssertEquals("Correct priority", partData.Part1, loader.Load("ptcode", partData.Buyer, partData.Supplier));

			partData.Part1.Delete();
			AssertEquals("Correct priority", partData.Part3, loader.Load("ptcode", partData.Buyer, partData.Supplier));

			partData.Part3.Delete();
			AssertEquals("Correct priority", partData.Part2, loader.Load("ptcode", partData.Buyer, partData.Supplier));

			partData.Part2.Delete();
			AssertNull("None found", loader.Load("ptcode", partData.Buyer, partData.Supplier));
		}

		public void TestBestMatchWithInactive()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Loader loader = new Loader(Factory);

				partData.Part3.OP_IsActive = false;
				partData.Part3.Factory.Save();

				AssertEquals("Correct priority", partData.Part1, loader.Load("ptcode", partData.Buyer, partData.Supplier));

				partData.Part1.Delete();
				AssertEquals("Correct priority, but part3 is inactive, so going to part2 straight away", partData.Part2, loader.Load("ptcode", partData.Buyer, partData.Supplier));
			}
		}

		public void TestBestMatchWithBoth()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Loader loader = new Loader(Factory);
				AssertEquals("Correct priority", partData.Part5, loader.Load("ptcode5", partData.Buyer, partData.Supplier));

				partData.Part5.Delete();
				AssertEquals("Correct priority", partData.Part7, loader.Load("ptcode5", partData.Buyer, partData.Supplier));

				partData.Part7.Delete();
				AssertEquals("Correct priority", partData.Part6, loader.Load("ptcode5", partData.Buyer, partData.Supplier));

				partData.Part6.Delete();
				AssertNull("None found", loader.Load("ptcode5", partData.Buyer, partData.Supplier));
			}
		}

		public void TestLoadAndReturnMatchingCount()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				OrgHeader supplier = Factory.New<OrgHeader>();
				OrgHeader importer = Factory.New<OrgHeader>();
				OrgHeader supplier2 = Factory.New<OrgHeader>();
				OrgHeader importer2 = Factory.New<OrgHeader>();
				OrgHeader supplier3 = Factory.New<OrgHeader>();
				OrgHeader importer3 = Factory.New<OrgHeader>();
				OrgHeader supplier4 = Factory.New<OrgHeader>();
				OrgHeader importer4 = Factory.New<OrgHeader>();

				OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier4.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importer4.PK, OrgPartRelation.RelationshipTypes.Owner);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importer3.PK, OrgPartRelation.RelationshipTypes.Owner);

				OrgSupplierPart product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "Product";
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				OrgSupplierPart product3 = Factory.New<OrgSupplierPart>();
				product3.OP_PartNum = "Product";
				product3.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product3.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				Loader loader = new Loader(Factory);
				ProductLoadResult loadResult = loader.LoadAndReturnMatchingCount("Product", importer, supplier, false);
				Assert("load a product", loadResult.BestMatchingProduct == product2 || loadResult.BestMatchingProduct == product3);
				AssertEquals("LoadResult.TotalMatchCount", 2, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 3, loadResult.TotalNumerOfPartsCount);

				OrgSupplierPart product4 = Factory.New<OrgSupplierPart>();
				product4.OP_PartNum = "Product";
				product4.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product4.RelatedOrganisations.AddOrganisationIfNotExist(supplier4.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product4.RelatedOrganisations.AddOrganisationIfNotExist(importer4.PK, OrgPartRelation.RelationshipTypes.Owner);
				product4.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				loadResult = loader.LoadAndReturnMatchingCount("Product", importer, supplier, false);
				AssertEquals("load a product", product4, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 4, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("Product", importer, supplier, true);
				AssertEquals("load a product", product4, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 4, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct", importer, supplier, true);
				AssertNull("pre-condition", loadResult.BestMatchingProduct);
				AssertEquals("pre-condition", 0, loadResult.TotalMatchCount);
				AssertEquals("pre-condition", 0, loadResult.TotalNumerOfPartsCount);
				OrgPartRelation relationship = product4.RelatedOrganisations[0];
				relationship.OU_LocalPartNumber = "LocalProduct";
				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct", importer, supplier, true);
				AssertEquals("load a product", product4, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 1, loadResult.TotalNumerOfPartsCount);

				var product5 = Factory.New<OrgSupplierPart>();
				product5.OP_PartNum = "Product#";
				relationship = product5.RelatedOrganisations.AddSupplier(supplier);
				relationship.OU_LocalPartNumber = "LocalProduct#";
				product5.RelatedOrganisations.AddOwner(importer);
				product5.OP_IsActive = ZBool.False;

				var product6 = Factory.New<OrgSupplierPart>();
				product6.OP_PartNum = "Product#";
				relationship = product6.RelatedOrganisations.AddSupplier(supplier);
				relationship.OU_LocalPartNumber = "LocalProduct#";
				product6.RelatedOrganisations.AddOwner(importer);
				product6.OP_IsActive = ZBool.True;
				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct#", importer, supplier, true, allowInactive: true);
				AssertEquals("load a product", product6, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 2, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 2, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct#", importer, supplier, true, allowInactive: false);
				AssertEquals("load a product", product6, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 2, loadResult.TotalNumerOfPartsCount);

				relationship.OU_LocalPartNumber = "LocalProduct#2";
				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct#", importer, supplier, true, allowInactive: true);
				AssertEquals("load a product", product5, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 1, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct#", importer, supplier, true, allowInactive: false);
				AssertNull(loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 0, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 1, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("Product#", importer, supplier, true, allowInactive: true);
				AssertEquals("load a product", product6, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 2, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 2, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("Product#", importer, supplier, true, allowInactive: false);
				AssertEquals("load a product", product6, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 2, loadResult.TotalNumerOfPartsCount);

				product6.OP_PartNum = "Product#2";
				loadResult = loader.LoadAndReturnMatchingCount("Product#", importer, supplier, true, allowInactive: true);
				AssertEquals("load a product", product5, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 1, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("Product#", importer, supplier, true, allowInactive: false);
				AssertNull(loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 0, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 1, loadResult.TotalNumerOfPartsCount);
			}
		}

		public void TestLoadAndReturnMatchingCountWithBoth()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				OrgHeader supplier = Factory.New<OrgHeader>();
				OrgHeader importer = Factory.New<OrgHeader>();
				OrgHeader supplier2 = Factory.New<OrgHeader>();
				OrgHeader importer2 = Factory.New<OrgHeader>();
				OrgHeader supplier3 = Factory.New<OrgHeader>();
				OrgHeader importer3 = Factory.New<OrgHeader>();

				OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Both);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importer3.PK, OrgPartRelation.RelationshipTypes.Both);

				OrgSupplierPart product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "Product";
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Both);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Both);

				OrgSupplierPart product3 = Factory.New<OrgSupplierPart>();
				product3.OP_PartNum = "Product";
				product3.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Both);
				product3.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Both);

				Loader loader = new Loader(Factory);
				ProductLoadResult loadResult = loader.LoadAndReturnMatchingCount("Product", importer, supplier, false);
				Assert("load a product", loadResult.BestMatchingProduct == product2 || loadResult.BestMatchingProduct == product3);
				AssertEquals("LoadResult.TotalMatchCount", 2, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 3, loadResult.TotalNumerOfPartsCount);

				OrgSupplierPart product4 = Factory.New<OrgSupplierPart>();
				product4.OP_PartNum = "Product";
				product4.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Both);
				product4.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Both);

				loadResult = loader.LoadAndReturnMatchingCount("Product", importer, supplier, false);
				AssertEquals("load a product", product4, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 4, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("Product", importer, supplier, true);
				AssertEquals("load a product", product4, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 4, loadResult.TotalNumerOfPartsCount);

				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct", importer, supplier, true);
				AssertNull("pre-condition", loadResult.BestMatchingProduct);
				AssertEquals("pre-condition", 0, loadResult.TotalMatchCount);
				AssertEquals("pre-condition", 0, loadResult.TotalNumerOfPartsCount);
				OrgPartRelation relationship = product4.RelatedOrganisations[0];
				relationship.OU_LocalPartNumber = "LocalProduct";
				loadResult = loader.LoadAndReturnMatchingCount("LocalProduct", importer, supplier, true);
				AssertEquals("load a product", product4, loadResult.BestMatchingProduct);
				AssertEquals("LoadResult.TotalMatchCount", 1, loadResult.TotalMatchCount);
				AssertEquals("LoadResult.TotalNumerOfPartsCount", 1, loadResult.TotalNumerOfPartsCount);
			}
		}

		public void TestLoadWithSameImporterAndSupplier()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var importerAndSupplier = Factory.New<OrgHeader>();
				var unRelatedOrg = Factory.New<OrgHeader>();
				var product = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "Product";
				product.RelatedOrganisations.AddOrganisationIfNotExist(importerAndSupplier.PK, OrgPartRelation.RelationshipTypes.Both);
				product.RelatedOrganisations.AddOrganisationIfNotExist(unRelatedOrg.PK, OrgPartRelation.RelationshipTypes.Owner);

				var loader = new Loader(Factory);
				var loadResult = loader.LoadAndReturnMatchingCount("Product", importerAndSupplier, importerAndSupplier, false);
				AssertSame("Should have found the BTH Product.", product, loadResult.BestMatchingProduct);
			}
		}

		public void TestBestMatchWithoutOwnerOrSupplier()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Loader loader = new Loader(Factory);
				OrgSupplierPart result = loader.Load("ptcode4", null, null);
				AssertNull("BestMatch should return null", result);
			}
		}

		public void TestBestMatchOnSupplierOnlyWhenSupplierDoesntMatch()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Loader loader = new Loader(Factory);
				OrgSupplierPart result = loader.Load("ptcode4", null, OrgHeader.New(Factory));
				AssertNull("BestMatch null", result);
			}
		}

		public void TestWhenExactMatchIsOn()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				Loader loader = new Loader(Factory);

				OrgHeader supplier = Factory.New<OrgHeader>();
				OrgHeader importer = Factory.New<OrgHeader>();
				OrgHeader supplier2 = Factory.New<OrgHeader>();
				OrgHeader importer2 = Factory.New<OrgHeader>();
				OrgHeader supplier4 = Factory.New<OrgHeader>();
				OrgHeader importer4 = Factory.New<OrgHeader>();

				OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importer2.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("load product", null, loader.Load("Product", importer, supplier));
				AssertEquals("load product", product1, loader.Load("Product", null, supplier2));
				AssertEquals("load product", product1, loader.Load("Product", importer2, null));

				OrgSupplierPart product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "Product";
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(importer2.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("load product", null, loader.Load("Product", importer, supplier));

				OrgSupplierPart product3 = Factory.New<OrgSupplierPart>();
				product3.OP_PartNum = "Product";
				product3.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product3.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("load product", null, loader.Load("Product", importer, supplier));

				OrgSupplierPart product4 = Factory.New<OrgSupplierPart>();
				product4.OP_PartNum = "Product";
				product4.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
				AssertEquals("load product", product4, loader.Load("Product", importer, supplier));

				OrgSupplierPart product5 = Factory.New<OrgSupplierPart>();
				product5.OP_PartNum = "Product";
				product5.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("load product", product5, loader.Load("Product", importer, supplier));

				OrgSupplierPart product6 = Factory.New<OrgSupplierPart>();
				product6.OP_PartNum = "Product";
				product6.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product6.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product6.RelatedOrganisations.AddOrganisationIfNotExist(supplier4.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product6.RelatedOrganisations.AddOrganisationIfNotExist(importer4.PK, OrgPartRelation.RelationshipTypes.Owner);
				product6.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("load product", product6, loader.Load("Product", importer, supplier));
			}
		}

		public void TestWhenExactMatchIsOnWithBoth()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				Loader loader = new Loader(Factory);

				OrgHeader supplier = Factory.New<OrgHeader>();
				OrgHeader importer = Factory.New<OrgHeader>();
				OrgHeader supplier2 = Factory.New<OrgHeader>();
				OrgHeader importer2 = Factory.New<OrgHeader>();

				OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Both);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importer2.PK, OrgPartRelation.RelationshipTypes.Both);
				AssertEquals("load product", null, loader.Load("Product", importer, supplier));
				AssertEquals("load product", product1, loader.Load("Product", null, supplier2));
				AssertEquals("load product", product1, loader.Load("Product", importer2, null));

				OrgSupplierPart product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "Product";
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Both);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(importer2.PK, OrgPartRelation.RelationshipTypes.Both);
				AssertEquals("load product", null, loader.Load("Product", importer, supplier));

				OrgSupplierPart product3 = Factory.New<OrgSupplierPart>();
				product3.OP_PartNum = "Product";
				product3.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Both);
				product3.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Both);
				AssertEquals("load product", null, loader.Load("Product", importer, supplier));

				OrgSupplierPart product4 = Factory.New<OrgSupplierPart>();
				product4.OP_PartNum = "Product";
				product4.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Both);
				AssertEquals("load product", product4, loader.Load("Product", importer, supplier));

				OrgSupplierPart product5 = Factory.New<OrgSupplierPart>();
				product5.OP_PartNum = "Product";
				product5.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Both);
				AssertEquals("load product", product5, loader.Load("Product", importer, supplier));

				OrgSupplierPart product6 = Factory.New<OrgSupplierPart>();
				product6.OP_PartNum = "Product";
				product6.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Both);
				product6.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Both);
				product6.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Both);
				AssertEquals("load product", product6, loader.Load("Product", importer, supplier));
			}
		}

		public void TestBestMatchWithClassificationOrganisation()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var loader = new Loader(Factory);

				var supplier = Factory.New<OrgHeader>();
				var importer = Factory.New<OrgHeader>();
				var supplier2 = Factory.New<OrgHeader>();

				var product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				AssertEquals("CLS org match on buyer", product1, loader.Load("Product", importer, null));
				AssertEquals("CLS org match on supplier", null, loader.Load("Product", null, supplier));

				var product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "Product";
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
				AssertEquals("Buyer CLS match before supplier SUP", product1, loader.Load("Product", importer, supplier2));

				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("Buyer OWN match before Buyer CLS match", product2, loader.Load("Product", importer, supplier2));
			}
		}

		public void TestBestMatchWithClassificationOrganisation_Export()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var loader = new Loader(Factory);

				var supplier = Factory.New<OrgHeader>();
				var importer = Factory.New<OrgHeader>();

				var product1 = Factory.New<OrgSupplierPart>();
				var product2 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";
				product2.OP_PartNum = "Product";
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				AssertEquals("CLS match on buyer not considered", 2, loader.LoadAndReturnMatchingCount("Product", importer, supplier, false, isExportJob: true).TotalMatchCount);

				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
				AssertEquals("CLS match on buyer is considered", 1, loader.LoadAndReturnMatchingCount("Product", importer, supplier, false, isExportJob: true).TotalMatchCount);
				AssertEquals("Best match", product1, loader.Load("Product", importer, supplier));
			}
		}

		public void TestBestMatchWithParentOrg_Import()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var loader = new Loader(Factory);

				var supplier = Factory.New<OrgHeader>();
				var importer = Factory.New<OrgHeader>();
				var supplierParent = Factory.New<OrgHeader>();
				var importerParent = Factory.New<OrgHeader>();
				SetOrgParent(supplier, supplierParent);
				SetOrgParent(importer, importerParent);

				var product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";

				product1.RelatedOrganisations.AddOrganisationIfNotExist(importerParent.PK, OrgPartRelation.RelationshipTypes.Owner);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplierParent.PK, OrgPartRelation.RelationshipTypes.Supplier);
				AssertEquals("Buyer parent matched", product1, loader.Load("Product", importer, null));
				AssertEquals("Supplier parent is not considered", null, loader.Load("Product", null, supplier));

				var product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "Product";
				var importer2 = Factory.New<OrgHeader>();
				var importer2Parent = Factory.New<OrgHeader>();
				SetOrgParent(importer2, importer2Parent);
				product2.RelatedOrganisations.AddOrganisationIfNotExist(importer2Parent.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				AssertEquals("CLS org parent match on buyer", product2, loader.Load("Product", importer2, null));

				product2.RelatedOrganisations.AddOrganisationIfNotExist(importerParent.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				AssertEquals("Buyer OWN parent match before buyer CLS parent match", product1, loader.Load("Product", importer, null));

				product2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("Buyer OWN org match before buyer OWN parent match", product2, loader.Load("Product", importer, null));
			}
		}

		public void TestBestMatchWithParentOrg_Export()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var loader = new Loader(Factory);

				var supplier = Factory.New<OrgHeader>();
				var importer = Factory.New<OrgHeader>();
				var supplierParent = Factory.New<OrgHeader>();
				var importerParent = Factory.New<OrgHeader>();
				SetOrgParent(supplier, supplierParent);
				SetOrgParent(importer, importerParent);

				var product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "Product";

				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplierParent.PK, OrgPartRelation.RelationshipTypes.Supplier);
				product1.RelatedOrganisations.AddOrganisationIfNotExist(importerParent.PK, OrgPartRelation.RelationshipTypes.Owner);
				AssertEquals("Supplier parent matched", product1, loader.Load("Product", null, supplier, isExportJob: true));
				AssertEquals("Buyer parent is not considered", null, loader.Load("Product", importer, null, isExportJob: true));

				product1.RelatedOrganisations.AddOrganisationIfNotExist(supplierParent.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				AssertEquals("CLS org parent match on supplier", product1, loader.Load("Product", null, supplier, isExportJob: true));

				var product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "Product";
				product2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
				AssertEquals("CLS org match on supplier before CLS parent org match", product2, loader.Load("Product", null, supplier, isExportJob: true));
			}
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new Loader(Factory);
		}

		PartDataSetupHelper fPartData;
		PartDataSetupHelper partData
		{
			get
			{
				if (fPartData == null)
				{
					fPartData = new PartDataSetupHelper();
					fPartData.Setup(Factory);
				}
				return fPartData;
			}
		}

		void SetOrgParent(OrgHeader org, OrgHeader parent)
		{
			var orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			orgRelatedParty.PR_OH_RelatedParty = org.PK;
			orgRelatedParty.PR_OH_Parent = parent.PK;
		}
	}
	#endregion

	#region DeferrableTriggerTest

	[TestedType(typeof(OrgSupplierPart))]
	class OrgSupplierPartDeferrableTriggerTest : DeferrableTriggerTestCase<OrgSupplierPart>
	{
	}

	#endregion

	#region PartDataSetupHelper

	public class PartDataSetupHelper
	{
		public OrgSupplierPart Part1;
		public OrgSupplierPart Part2;
		public OrgSupplierPart Part3;
		public OrgSupplierPart Part4;
		public OrgSupplierPart Part5;
		public OrgSupplierPart Part6;
		public OrgSupplierPart Part7;

		public OrgHeader Supplier;
		public OrgHeader Buyer;

		public void Setup(BusinessObjectFactory factory)
		{
			Supplier = factory.New<OrgHeader>();
			Supplier.OH_Code = "sup";
			Supplier.MainAddress.OA_Address1 = "supaddr";

			Buyer = factory.New<OrgHeader>();
			Buyer.OH_Code = "buy";
			Buyer.MainAddress.OA_Address1 = "buyaddr";

			Part1 = OrgSupplierPart.New(factory);
			Part1.OP_PartNum = "ptcode";
			Part1.OP_Desc = "part1";
			Part1.RelatedOrganisations.AddOrganisationIfNotExist(Buyer.PK, OrgPartRelation.RelationshipTypes.Owner);
			Part1.RelatedOrganisations.AddOrganisationIfNotExist(Supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			Part2 = OrgSupplierPart.New(factory);
			Part2.OP_PartNum = "ptcode";
			Part2.OP_Desc = "part2";
			Part2.RelatedOrganisations.AddOrganisationIfNotExist(Supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			Part3 = OrgSupplierPart.New(factory);
			Part3.OP_PartNum = "ptcode";
			Part3.OP_Desc = "part3";
			Part3.RelatedOrganisations.AddOrganisationIfNotExist(Buyer.PK, OrgPartRelation.RelationshipTypes.Owner);

			Part4 = OrgSupplierPart.New(factory); // this part should be ignored as code does not match
			Part4.OP_PartNum = "ptcode4";
			Part4.OP_Desc = "part4";
			Part4.RelatedOrganisations.AddOrganisationIfNotExist(Supplier.PK, OrgPartRelation.RelationshipTypes.Owner);

			Part5 = OrgSupplierPart.New(factory);
			Part5.OP_PartNum = "ptcode5";
			Part5.OP_Desc = "part5";
			Part5.RelatedOrganisations.AddOrganisationIfNotExist(Buyer.PK, OrgPartRelation.RelationshipTypes.Both);
			Part5.RelatedOrganisations.AddOrganisationIfNotExist(Supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			Part6 = OrgSupplierPart.New(factory);
			Part6.OP_PartNum = "ptcode5";
			Part6.OP_Desc = "part6";
			Part6.RelatedOrganisations.AddOrganisationIfNotExist(Supplier.PK, OrgPartRelation.RelationshipTypes.Both);

			Part7 = OrgSupplierPart.New(factory);
			Part7.OP_PartNum = "ptcode5";
			Part7.OP_Desc = "part7";
			Part7.RelatedOrganisations.AddOrganisationIfNotExist(Buyer.PK, OrgPartRelation.RelationshipTypes.Both);

			// (Part1, Part3) and (Part5, Part7) are duplicate parts. It is no longer possible to create such parts by normal means,
			// but they can still present in customers databases until second wave transformation from WI00173045 is executed.
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				factory.Save();
			}
		}
	}

	#endregion
}
