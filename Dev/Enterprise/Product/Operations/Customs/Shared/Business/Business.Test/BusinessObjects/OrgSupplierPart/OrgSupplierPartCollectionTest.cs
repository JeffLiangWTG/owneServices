using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	public class OrgSupplierPartCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollection(Factory);
		}

		public void TestGetNewRelationship()
		{
			CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ClientProductCreationTypeList.Codes.Never);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var orgSupplierPartCollection = new OrgSupplierPartCollectionExposed(Factory, invoiceLine, false);
			AssertEquals((supplier, OrgPartRelation.RelationshipTypes.Supplier), orgSupplierPartCollection.GetNewRelationshipExposed());
		}

		class OrgSupplierPartCollectionExposed : OrgSupplierPartCollection
		{
			public OrgSupplierPartCollectionExposed(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine, bool isExport)
				: base(factory, invoiceLine, isExport)
			{
			}

			public (OrgHeader Org, string Relationship)? GetNewRelationshipExposed() => GetNewRelationship();
		}

		public virtual void TestAdditionalAddNewByOrgHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.OH_RL_NKClosestPort = "AUSYD";
			importer.OH_IsConsignee = true;
			importer.OH_IsConsignor = true;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "Supplier";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.OH_IsConsignor = true;
			supplier.OH_IsConsignee = true;

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "Importer2";
			importer2.OH_IsConsignee = true;
			importer2.OH_IsConsignor = true;

			var declaration = Factory.New<BaseJobDeclaration>();
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification.PK;
			line1.JI_PartNo = "Test9";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var part1 = line1.Lookups.PartsList.AddNew();
			AssertEquals(part1.RelatedOrganisations.Count, 1);
			var part2 = line1.Lookups.PartsList.AdditionalAddNewByOrgHeader(null, importer2, true);
			AssertEquals(part2.RelatedOrganisations.Count, 1);
			AssertEquals(part2.RelatedOrganisations[0].OU_OH, importer2.PK);
			var part3 = line1.Lookups.PartsList.AddNew();
			AssertEquals(part3.RelatedOrganisations.Count, 1);
		}

		public virtual void TestAddingNewPart()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<BaseCusClassification>();
			cusClass.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "84314901";
			invoiceLine.JI_Tariff = "84314905";
			invoiceLine.JI_CC = cusClass.PK;
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1234";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = invoiceLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "ABC";
			invoiceLine.JI_RH_NKCommodity_Code = commodity.RH_Code;
			var part = (OrgSupplierPart)collection.AddNew();
			AssertEquals("RefCommodityCode", "ABC", part.OP_RH_NKCommodityCode);
			AssertEquals("UNDGSubstance", "1234", part.UNDGs[0].UNDGSubstance.DG_Code);
			AssertEquals("PivotCount", 1, part.PivotsForBinding.Count);
			var pivot = part.PivotsForBinding[0];
			AssertEquals("Child Type", ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			AssertEquals("Lookup", cusClass.PK, pivot.CI_CC);
			AssertEquals("Tariff Number", "", pivot.CI_TariffNum);

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "84314905";
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			part = (OrgSupplierPart)collection.AddNew();
			AssertEquals(1, part.PivotsForBinding.Count);
			pivot = part.PivotsForBinding[0];
			AssertEquals(ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			AssertEquals(ZGuid.Empty, pivot.CI_CC);
			AssertEquals("84314905", pivot.CI_TariffNum);
		}

		public void TestAddingNewPartWithCustomFields()
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var lineWorkflow = CreateWorkflowTemplate(WorkflowDescriptors.CommericalInvoiceLineWorkflowDescriptorCode);
			lineWorkflow.P0_OH_Client = importer.PK;
			DefineCustomField<ZString>(lineWorkflow, "String Value");
			DefineCustomField<ZInt>(lineWorkflow, "Integer Value");
			DefineCustomField<ZDecimal>(lineWorkflow, "Decimal Value");
			DefineCustomField<ZDateTime>(lineWorkflow, "DateTime Value");
			DefineCustomField<ZBool>(lineWorkflow, "Boolean Value");

			var partWorkflow = CreateWorkflowTemplate(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
			partWorkflow.P0_OH_Client = importer.PK;
			DefineCustomField<ZString>(partWorkflow, "String Value");
			DefineCustomField<ZInt>(partWorkflow, "Integer Value");
			DefineCustomField<ZDecimal>(partWorkflow, "Decimal Value");
			DefineCustomField<ZDateTime>(partWorkflow, "DateTime Value");
			DefineCustomField<ZBool>(partWorkflow, "Boolean Value");

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99990000";
			invoiceLine.JI_PartNo = "LINE WITH CUSTOM VALUES";
			invoiceLine.GetCustomBusinessObject(true);

			SetCustomField<ZString>(invoiceLine, "String Value", "AAA");
			SetCustomField<ZInt>(invoiceLine, "Integer Value", 111);
			SetCustomField<ZDecimal>(invoiceLine, "Decimal Value", 2.333);
			SetCustomField<ZDateTime>(invoiceLine, "DateTime Value", new DateTime(2024, 01, 02));
			SetCustomField<ZBool>(invoiceLine, "Boolean Value", true);

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);

			DataRegistry.Business.CustomsDataRegistry.Instance.CopyCustomFieldsFromInvoiceLineToProduct.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var product = (OrgSupplierPart)collection.AddNew();

			AssertHasNoCustomField<ZString>(product, "String Value");
			AssertHasNoCustomField<ZInt>(product, "Integer Value");
			AssertHasNoCustomField<ZDecimal>(product, "Decimal Value");
			AssertHasNoCustomField<ZDateTime>(product, "DateTime Value");
			AssertHasNoCustomField<ZBool>(product, "Boolean Value");

			DataRegistry.Business.CustomsDataRegistry.Instance.CopyCustomFieldsFromInvoiceLineToProduct.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			product = (OrgSupplierPart)collection.AddNew();

			AssertCustomField<ZString>(product, "String Value", "AAA");
			AssertCustomField<ZInt>(product, "Integer Value", 111);
			AssertCustomField<ZDecimal>(product, "Decimal Value", 2.333);
			AssertCustomField<ZDateTime>(product, "DateTime Value", new DateTime(2024, 01, 02));
			AssertCustomField<ZBool>(product, "Boolean Value", true);

			ProcessTaskTemplate CreateWorkflowTemplate(ZString processType)
			{
				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = processType;
				template.P0_IsActive = true;
				return template;
			}

			void DefineCustomField<T>(ProcessTaskTemplate template, ZString name)
				where T : IZType
			{
				var field = template.GenCustomColumnDefinitions.AddNew();
				field.XC_Name = name;
				field.XC_Type = AddOnColumnDataType.GetCodeFromType(typeof(T));
			}

			void SetCustomField<T>(ICustomFieldProvider bizo, ZString name, T value)
				where T : IZType
			{
				var identifier = CustomPropertyHelper.GeneratePropertyIdentifier(name, typeof(T));
				var cusBizo = bizo.GetCustomBusinessObject();
				cusBizo[identifier] = value;
			}

			void AssertCustomField<T>(ICustomFieldProvider bizo, ZString name, T value)
				where T : IZType
			{
				var identifier = CustomPropertyHelper.GeneratePropertyIdentifier(name, typeof(T));
				var cusBizo = bizo.GetCustomBusinessObject();
				AssertEquals(name, value, (T)cusBizo[identifier]);
			}

			void AssertHasNoCustomField<T>(ICustomFieldProvider bizo, ZString name)
				where T : IZType
			{
				var identifier = CustomPropertyHelper.GeneratePropertyIdentifier(name, typeof(T));
				var cusBizo = bizo.GetCustomBusinessObject();
				Assert(!cusBizo.GetOrderedCustomProperties().Any(x => x == identifier));
			}
		}
	}
}
