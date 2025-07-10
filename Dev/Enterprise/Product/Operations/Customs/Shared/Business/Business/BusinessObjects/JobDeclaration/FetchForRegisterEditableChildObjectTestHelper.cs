#region Test
#if DEBUG

using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class FetchForRegisterEditableChildObjectTestHelper<TDeclaration, TInvoiceHeader, TInvoiceLine>
		where TDeclaration : BaseJobDeclaration
		where TInvoiceHeader : BaseJobComInvoiceHeader
		where TInvoiceLine : BaseJobComInvoiceLine
	{
		public IEnumerable<(BusinessObject Object, string TableName, string PKColumnName)> SetupFetchHintForRegisterEditableChildObjectTestCase(
			BusinessObjectFactory factory,
			bool includeDeclarationObject = true,
			bool includeInvoiceHeaderObject = true,
			bool declarationSupportDeclarationRefs = false,
			bool declarationHasFetchForLoadChildEditableObjectsBeenCalled = false,
			bool declarationSupportsChcPivotBetweenInvoiceLineAndPackingCore = false,
			bool headerHasFetchForLoadChildEditableObjectsBeenCalled = false,
			bool lineSupportInvoiceLineRefs = false,
			bool lineSupportsJobComInvoiceLineTaxCore = false,
			bool lineSupportRulingConfigurations = false,
			bool lineSupportsAdditionalTariffs = false)
		{
			var declaration = factory.NewWithValidTestData<TDeclaration>();
			var invoice = factory.New<TInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			var invoice2 = factory.New<TInvoiceHeader>();
			invoice2.JZ_JE = declaration.PK;
			declaration.Invoices.Add(invoice);
			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "12345678";
			bill.CU_ClusterKey = declaration.JE_ClusterKey;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var jobContainer = factory.New<ForwardingContainer>();
			jobContainer.JC_ContainerNum = "123456";
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = jobContainer.PK;
			cusContainer.CO_JE = declaration.PK;
			declaration.PackingGroups.AddNew();
			declaration.PackingGroups[0].CR_ClusterKey = declaration.JE_ClusterKey;
			declaration.PackingGroups[0].CR_CO_Container = cusContainer.PK;
			declaration.PackingGroups[0].CR_CU_HouseBill = bill.PK;
			var basePackage = declaration.PackingGroups[0].Packages.AddNew();
			basePackage.CW_HouseBill = "MB:12345678";
			basePackage.CW_PackQty = 1;
			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "001";

			factory.Save();

			var invoiceLine = factory.New<TInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);
			invoiceLine.JI_ClusterKey = declaration.JE_ClusterKey;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_OP = part.PK;

			var objectsDic = new List<(BusinessObject, string, string)>();

			if (includeDeclarationObject)
			{
				objectsDic.Add((declaration, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.Constants.PK));
				objectsDic.Add((bill, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.Constants.PK));
				objectsDic.Add((entryHeader, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.Constants.PK));
				objectsDic.Add((cusContainer, CusContainerSchema.Constants.TableName, CusContainerSchema.Constants.PK));
				objectsDic.Add((basePackage, CusDecHouseContainerPackSchema.Constants.TableName, CusDecHouseContainerPackSchema.Constants.PK));
			}

			if (includeInvoiceHeaderObject)
			{
				objectsDic.Add((invoice, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.Constants.PK));
			}

			objectsDic.Add((invoiceLine, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.PK));

			if (includeDeclarationObject || includeInvoiceHeaderObject)
			{
				var parentLine = invoice2.InvoiceLines.AddNew();
				parentLine.JI_ClusterKey = declaration.JE_ClusterKey;
				invoiceLine.JI_ParentID = parentLine.PK;
				objectsDic.Add((parentLine, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.PK));
			}

			if (declarationSupportDeclarationRefs)
			{
				var jobDocRef = declaration.DeclarationRefs.AddNew();
				jobDocRef.J3_ClusterKey = declaration.JE_ClusterKey;
				objectsDic.Add((jobDocRef, JobDecRefsSchema.Constants.TableName, JobDecRefsSchema.Constants.PK));
			}

			var hasFetchForLoadChildEditableObjectsBeenCalled = declarationHasFetchForLoadChildEditableObjectsBeenCalled && headerHasFetchForLoadChildEditableObjectsBeenCalled;

			if (lineSupportRulingConfigurations)
			{
				var rulingConfig = invoiceLine.RulingConfigurations.AddNew();
				rulingConfig.ZZY_Type = "TST";
				rulingConfig.ZZY_Category = "GST";
				objectsDic.Add((rulingConfig, CusRulingConfigCombinedSchema.Constants.TableName, CusRulingConfigCombinedSchema.Constants.PK));
			}

			var undg = invoiceLine.UNDGs.AddNew();
			objectsDic.Add((undg, UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.PK));

			if (includeDeclarationObject)
			{
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_ClusterKey = declaration.JE_ClusterKey;
				invoiceLine.JI_CL = entryLine.PK;
				objectsDic.Add((entryLine, CusEntryLineSchema.Constants.TableName, CusEntryLineSchema.Constants.PK));
			}

			var inventory = invoiceLine.ComponentInventoryCollection.AddNew();
			inventory.JIV_ClusterKey = declaration.JE_ClusterKey;
			objectsDic.Add((inventory, JobComInvLineComponentInventorySchema.Constants.TableName, JobComInvLineComponentInventorySchema.Constants.PK));

			if (hasFetchForLoadChildEditableObjectsBeenCalled && lineSupportInvoiceLineRefs)
			{
				var lineRef = invoiceLine.InvoiceLineRefs.AddNew();
				lineRef.JG_ClusterKey = declaration.JE_ClusterKey;
				objectsDic.Add((lineRef, JobComInvLineRefsSchema.Constants.TableName, JobComInvLineRefsSchema.Constants.PK));
			}

			var workflowItem = invoiceLine.WorkflowItems.AddNew();
			objectsDic.Add((workflowItem, ProcessTasksSchema.Constants.TableName, ProcessTasksSchema.Constants.PK));

			var charge = invoiceLine.Charges.AddNew();
			objectsDic.Add((charge, JobComInvHeaderChargeSchema.Constants.TableName, JobComInvHeaderChargeSchema.Constants.PK));

			var apportionedCharge = invoiceLine.ApportionedCharges.AddNew();
			objectsDic.Add((apportionedCharge, JobComInvHeaderChargeSchema.Constants.TableName, JobComInvHeaderChargeSchema.Constants.PK));

			if (lineSupportsAdditionalTariffs)
			{
				var tariff = invoiceLine.CusLineTariffDetails.AddNew();
				objectsDic.Add((tariff, CusLineTariffDetailSchema.Constants.TableName, CusLineTariffDetailSchema.Constants.PK));
			}

			if (lineSupportsJobComInvoiceLineTaxCore)
			{
				var tax = factory.New<JobComInvoiceLineTax>();
				tax.JLT_JI = invoiceLine.PK;
				tax.JLT_ClusterKey = invoiceLine.JI_ClusterKey;
				tax.JLT_Type = "TST";
				objectsDic.Add((tax, JobComInvoiceLineTaxSchema.Constants.TableName, JobComInvoiceLineTaxSchema.Constants.PK));
			}

			if (declarationSupportsChcPivotBetweenInvoiceLineAndPackingCore)
			{
				var packagePivot = invoiceLine.PackagesPivot.AddNew();
				packagePivot.CHC_CW = basePackage.PK;
				packagePivot.CHC_ClusterKey = declaration.JE_ClusterKey;
				packagePivot.CHC_JE = declaration.PK;
				objectsDic.Add((packagePivot, CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, CusHouseContPackInvoiceLinePivotSchema.Constants.PK));
			}

			if (invoiceLine.ContainersPivot.Count == 0)
			{
				var containerPivot = invoiceLine.ContainersPivot.AddNew();
				containerPivot.C2_CO = cusContainer.PK;
				containerPivot.C2_ClusterKey = declaration.JE_ClusterKey;
				objectsDic.Add((containerPivot, CusContainerInvoiceLinePivotSchema.Constants.TableName, CusContainerInvoiceLinePivotSchema.Constants.PK));
			}

			factory.Save();

			return objectsDic;
		}
		public static IList<string> GetCommonTablesToCollectQueriesForRegisterEditableChildObject()
		{
			var result = new List<string>
			{
				CusEntryHeaderSchema.Constants.TableName,
				CusDecHouseBillSchema.Constants.TableName,
				CusContainerSchema.Constants.TableName,
				JobComInvoiceHeaderSchema.Constants.TableName,
				GlbBranchSchema.Constants.TableName,
				StmNoteSchema.Constants.TableName,
				EDIMessageSchema.Constants.TableName,
				JobDocAddressSchema.Constants.TableName,
				ProcessTasksSchema.Constants.TableName,
				ProcessHeaderSchema.Constants.TableName,
				JobDocsAndCartageSchema.Constants.TableName,
				CusEntryHeaderSchema.Constants.TableName,
				JobOrderHeaderSchema.Constants.TableName,
				CusEntryNumSchema.Constants.TableName,
				CusEquipmentSchema.Constants.TableName,
				CusDecHouseContainerPackSchema.Constants.TableName,
				JobComInvoiceLineSchema.Constants.TableName,
				JobComInvHeaderChargeSchema.Constants.TableName,
				JobComInvLineComponentInventorySchema.Constants.TableName,
				JobComInvHeaderChargeSchema.Constants.TableName,
				CusRulingConfigCombinedSchema.Constants.TableName,
				CusLineTariffDetailSchema.Constants.TableName,
				ProcessHeaderSchema.Constants.TableName,
				ProcessTasksSchema.Constants.TableName
			};

			return result;
		}

		public static void RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(BusinessObjectFactory factory)
		{
			var factoryCacheManagerInstance = PersistentFactoryCacheManager.Instance;
			if (factoryCacheManagerInstance != null)
			{
				var weakReference = factoryCacheManagerInstance.persistentFactoryWeakReferences.Find(x => x.Target is BusinessObjectFactory targetFactory && targetFactory.Equals(factory));
				factoryCacheManagerInstance.persistentFactoryWeakReferences.Remove(weakReference);
			}
			RegistryFactory.RenewFactory();
		}
	}
}

#endif
#endregion
