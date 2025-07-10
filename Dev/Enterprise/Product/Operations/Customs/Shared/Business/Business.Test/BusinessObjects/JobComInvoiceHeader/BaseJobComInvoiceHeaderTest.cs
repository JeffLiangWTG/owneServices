using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseJobComInvoiceHeaderTest<TJobDeclaration, TInvoiceHeader, TInvoiceLine> : BaseJobComInvoiceHeaderAbstractTest<TInvoiceHeader, TInvoiceLine>
		where TJobDeclaration : BaseJobDeclaration
		where TInvoiceHeader : BaseJobComInvoiceHeader
		where TInvoiceLine : BaseJobComInvoiceLine
	{
		protected virtual (BaseJobDeclaration, BaseJobDeclaration) GetDeclarationsForAttach()
		{
			var factory01 = new BusinessObjectFactory();
			var factory02 = new BusinessObjectFactory();
			var currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			var declaration01 = factory01.New<BaseJobDeclaration>();
			declaration01.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration01.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration01.JE_TotalWeight = 100m;
			declaration01.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			var invoice11 = declaration01.Invoices.AddNew();
			invoice11.JZ_InvoiceAmount = 25m;
			invoice11.JZ_RX_NKInvoice_Currency = currency;
			var invoiceLine11 = invoice11.JobComInvoiceLines.AddNew();
			invoiceLine11.JI_LinePrice = 25m;
			invoiceLine11.JI_Tariff = "0000000011";
			var invoice12 = declaration01.Invoices.AddNew();
			invoice12.JZ_InvoiceAmount = 25m;
			invoice12.JZ_RX_NKInvoice_Currency = currency;
			var invoiceLine12 = invoice12.JobComInvoiceLines.AddNew();
			invoiceLine12.JI_LinePrice = 25m;
			invoiceLine12.JI_Tariff = "0000000012";
			var transport01 = declaration01.Transports.AddNew();
			transport01.JW_RL_NKLoadPort = "AUSYD";
			factory01.Save();

			var declaration02 = factory02.New<BaseJobDeclaration>();
			declaration02.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration02.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration02.JE_TotalWeight = 100m;
			declaration02.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			var invoice21 = declaration02.Invoices.AddNew();
			invoice21.JZ_InvoiceAmount = 50m;
			invoice21.JZ_RX_NKInvoice_Currency = currency;
			var invoiceLine21 = invoice21.JobComInvoiceLines.AddNew();
			invoiceLine21.JI_LinePrice = 50m;
			invoiceLine21.JI_Tariff = "0000000021";
			var transport02 = declaration02.Transports.AddNew();
			transport02.JW_RL_NKLoadPort = "AUSYD";
			factory02.Save();

			return (declaration01, declaration02);
		}

		public void TestIWorkflowTriggerEventSourceMembers()
		{
			var company1 = Factory.New<GlbCompany>();
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var company2 = Factory.New<GlbCompany>();
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company2.PK;

			var invoiceHeader = Factory.New<TInvoiceHeader>();
			invoiceHeader.JZ_GB = ZGuid.Empty;
			invoiceHeader.JZ_InvoiceNumber = "INV0001";
			var workflowTriggerEventSource = invoiceHeader as IWorkflowTriggerEventSource;
			AssertNotNull(workflowTriggerEventSource);
			AssertEquals(GlbCompany.CurrentCompany, workflowTriggerEventSource.JobHeaderCompany);

			invoiceHeader.JZ_GB = branch2.PK;
			AssertEquals(invoiceHeader.Branch.Company, workflowTriggerEventSource.JobHeaderCompany);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00000001";
			declaration.JE_GC = company1.PK;
			invoiceHeader.JZ_JE = declaration.PK;
			AssertEquals(declaration.Company, workflowTriggerEventSource.JobHeaderCompany);

			AssertContainsExactElementsInAnyOrder(new[] { declaration }, workflowTriggerEventSource.ParentWorkflowProviders);
		}

		public void TestDeletePackagesPivotWhenDetachAndAttach()
		{
			var invoice = Factory.New<TInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "TEST0001";
			invoice.JZ_InvoiceAmount = 50m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10m;
			invoiceLine.JI_Tariff = "000000001";
			Factory.Save();

			var (declaration01, declaration02) = GetDeclarationsForAttach();
			declaration01.Bills.AddNew();
			declaration01.Bills[0].PackingGroups.AddNew();
			var package1 = declaration01.Bills[0].PackingGroups[0].Packages.AddNew();
			package1.CW_PackQty = 1;
			var package2 = declaration01.Bills[0].PackingGroups[0].Packages.AddNew();
			package2.CW_PackQty = 2;
			var package3 = declaration01.Bills[0].PackingGroups[0].Packages.AddNew();
			package3.CW_PackQty = 3;
			var package4 = declaration01.Bills[0].PackingGroups[0].Packages.AddNew();
			package4.CW_PackQty = 4;
			declaration01.Factory.Save();

			invoice.JZ_JE = declaration01.PK;
			var pivot1 = invoice.PackagesPivot.AddPivotFor(package1);
			var pivot2 = invoice.PackagesPivot.AddPivotFor(package2);
			var pivot3 = invoiceLine.PackagesPivot.AddPivotFor(package3);
			var pivot4 = invoiceLine.PackagesPivot.AddPivotFor(package4);
			Factory.Save();
			AssertEquals(2, invoice.PackagesPivot.Count);
			AssertEquals(2, invoiceLine.PackagesPivot.Count);

			invoice.JZ_JE = ZGuid.Empty;
			Factory.Save();
			AssertEquals(0, invoice.PackagesPivot.Count);
			AssertEquals(0, invoiceLine.PackagesPivot.Count);
			AssertEquals(true, pivot1.IsDeleted);
			AssertEquals(true, pivot2.IsDeleted);
			AssertEquals(true, pivot3.IsDeleted);
			AssertEquals(true, pivot4.IsDeleted);

			invoice.JZ_JE = declaration01.PK;
			pivot1 = invoice.PackagesPivot.AddPivotFor(package1);
			pivot2 = invoice.PackagesPivot.AddPivotFor(package2);
			pivot3 = invoiceLine.PackagesPivot.AddPivotFor(package3);
			pivot4 = invoiceLine.PackagesPivot.AddPivotFor(package4);
			Factory.Save();
			AssertEquals(2, invoice.PackagesPivot.Count);
			AssertEquals(2, invoiceLine.PackagesPivot.Count);

			invoice.JZ_JE = declaration02.PK;
			Factory.Save();
			AssertEquals(0, invoice.PackagesPivot.Count);
			AssertEquals(0, invoiceLine.PackagesPivot.Count);
			AssertEquals(true, pivot1.IsDeleted);
			AssertEquals(true, pivot2.IsDeleted);
			AssertEquals(true, pivot3.IsDeleted);
			AssertEquals(true, pivot4.IsDeleted);
		}

		public void TestDetachInvoiceAfterAttachToAnotherDeclaration()
		{
			var invoice = Factory.New<TInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "TEST0001";
			invoice.JZ_InvoiceAmount = 50m;
			var invoiceLine01 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine01.JI_LinePrice = 10m;
			invoiceLine01.JI_Tariff = "000000001";
			var invoiceLine02 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine02.JI_LinePrice = 20m;
			invoiceLine02.JI_Tariff = "000000002";
			var invoiceLine03 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine03.JI_LinePrice = 20m;
			invoiceLine03.JI_Tariff = "000000003";
			var transport = invoice.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			Factory.Save();

			var (declaration01, declaration02) = GetDeclarationsForAttach();

			var currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			declaration01.Factory.Load<TInvoiceHeader>(invoice.PK).JZ_JE = declaration01.PK;
			declaration01.JE_AutoWeightApportion = true;
			declaration01.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration01.Invoices.ForEach(x => x.JZ_RX_NKInvoice_Currency = currency);
			var entryInstruction1 = declaration01.CustomsEntryInstructions?.AddNew();
			if (entryInstruction1 != null)
			{
				declaration01.InvoiceLines.Cast<BaseJobComInvoiceLine>().ForEach(x => x.JI_CEI = entryInstruction1.PK);
			}
			declaration01.DoMerge();

			declaration02.Factory.Load<TInvoiceHeader>(invoice.PK).JZ_JE = declaration02.PK;
			var invoice23 = declaration02.Invoices.AddNew();
			invoice23.JZ_InvoiceAmount = 50m;
			invoice23.InvoiceLines.AddNew();
			declaration02.JE_AutoWeightApportion = true;
			declaration02.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration02.Invoices.ForEach(x => x.JZ_RX_NKInvoice_Currency = currency);
			var entryInstruction2 = declaration02.CustomsEntryInstructions?.AddNew();
			if (entryInstruction2 != null)
			{
				declaration02.InvoiceLines.Cast<BaseJobComInvoiceLine>().ForEach(x => x.JI_CEI = entryInstruction2.PK);
			}
			declaration02.DoMerge();

			CombineAssertions("Declaration 01, Before saving Declaration 01", () =>
			{
				AssertEquals("SortedInvoiceList", 3, declaration01.SortedInvoiceList.Count);
				AssertEquals("Invoices", 3, declaration01.Invoices.Count);
				AssertEquals("InvoiceLines", 5, declaration01.InvoiceLines.Count);
				AssertEquals("TopGroupInvoice.AllJobComInvoiceLines", 5, declaration01.TopGroupInvoice.AllJobComInvoiceLines.Count);
				AssertEquals("Transports", 1, declaration01.Transports.Count);
				AssertEquals("ApportionmentDirty", false, declaration01.ApportionmentDirty);
				AssertEquals("Apportioned Weight", 50m, declaration01.Invoices[2].JZ_Weight);
				AssertEquals("MergedLines", 5, declaration01.CustomsEntryHeaders.SelectMany(entryHeader => entryHeader.InvoiceLines.Select(entryLine => entryLine.PK)).Distinct().Count());
			});

			CombineAssertions("Declaration 02, Before saving Declaration 01", () =>
			{
				AssertEquals("SortedInvoiceList", 3, declaration02.SortedInvoiceList.Count);
				AssertEquals("Invoices", 3, declaration02.Invoices.Count);
				AssertEquals("InvoiceLines", 5, declaration02.InvoiceLines.Count);
				AssertEquals("TopGroupInvoice.AllJobComInvoiceLines", 5, declaration02.TopGroupInvoice.AllJobComInvoiceLines.Count);
				AssertEquals("Transports", 1, declaration02.Transports.Count);
				AssertEquals("ApportionmentDirty", false, declaration02.ApportionmentDirty);
				AssertEquals("Apportioned Weight", 33.333m, declaration02.Invoices[1].JZ_Weight);
				AssertEquals("MergedLines", 5, declaration02.CustomsEntryHeaders.SelectMany(entryHeader => entryHeader.InvoiceLines.Select(entryLine => entryLine.PK)).Distinct().Count());
			});

			AssertNoExceptionThrown(() =>
			{
				declaration01.Factory.Save();
			});

			CombineAssertions("Declaration 01, After saving Declaration 01", () =>
			{
				AssertEquals("SortedInvoiceList", 3, declaration01.SortedInvoiceList.Count);
				AssertEquals("Invoices", 3, declaration01.Invoices.Count);
				AssertEquals("InvoiceLines", 5, declaration01.InvoiceLines.Count);
				AssertEquals("TopGroupInvoice.AllJobComInvoiceLines", 5, declaration01.TopGroupInvoice.AllJobComInvoiceLines.Count);
				AssertEquals("Transports", 1, declaration01.Transports.Count);
				AssertEquals("ApportionmentDirty", false, declaration01.ApportionmentDirty);
				AssertEquals("Apportioned Weight", 50m, declaration01.Invoices[2].JZ_Weight);
				AssertEquals("Invoice Sequence", new ZShort(3), declaration01.Invoices[2].JZ_InvoiceDisplaySequence);
				AssertEquals("MergedLines", 5, declaration01.CustomsEntryHeaders.SelectMany(entryHeader => entryHeader.InvoiceLines.Select(entryLine => entryLine.PK)).Distinct().Count());
			});

			CombineAssertions("Declaration 02, After saving Declaration 01", () =>
			{
				AssertEquals("SortedInvoiceList", 2, declaration02.SortedInvoiceList.Count);
				AssertEquals(false, declaration02.SortedInvoiceList.ContainsCode(invoice.JZ_InvoiceNumber));
				AssertEquals("Invoices", 2, declaration02.Invoices.Count);
				AssertEquals("InvoiceLines", 2, declaration02.InvoiceLines.Count);
				AssertEquals("TopGroupInvoice.AllJobComInvoiceLines", 2, declaration02.TopGroupInvoice.AllJobComInvoiceLines.Count);
				AssertEquals("Transports", 0, declaration02.Transports.Count);
				AssertEquals("ApportionmentDirty", true, declaration02.ApportionmentDirty);
				AssertEquals("Apportioned Weight", 50m, declaration02.Invoices[0].JZ_Weight);
				AssertEquals("Invoice Sequence", new ZShort(2), invoice23.JZ_InvoiceDisplaySequence);
				AssertEquals("MergedLines", 5, declaration02.CustomsEntryHeaders.SelectMany(entryHeader => entryHeader.InvoiceLines.Select(entryLine => entryLine.PK)).Distinct().Count());
			});

			AssertNoExceptionThrown(() =>
			{
				declaration02.ThrowAwayMerge();
				declaration02.DoMerge();
			});

			CombineAssertions("Declaration 02, After merging Declaration 02", () =>
			{
				AssertEquals("SortedInvoiceList", 2, declaration02.SortedInvoiceList.Count);
				AssertEquals(false, declaration02.SortedInvoiceList.ContainsCode(invoice.JZ_InvoiceNumber));
				AssertEquals("Invoices", 2, declaration02.Invoices.Count);
				AssertEquals("InvoiceLines", 2, declaration02.InvoiceLines.Count);
				AssertEquals("Transports", 0, declaration02.Transports.Count);
				AssertEquals("ApportionmentDirty", false, declaration02.ApportionmentDirty);
				AssertEquals("Apportioned Weight", 50m, declaration02.Invoices[0].JZ_Weight);
				AssertEquals("JZ_InvoiceDisplaySequence", new ZShort(2), invoice23.JZ_InvoiceDisplaySequence);
				AssertEquals("MergedLines", 2, declaration02.CustomsEntryHeaders.SelectMany(entryHeader => entryHeader.InvoiceLines.Select(entryLine => entryLine.PK)).Distinct().Count());
			});
		}

		public void TestDoNotAllowAttachNonStandAloneInvoice()
		{
			var invoice = Factory.NewWithValidTestData<TInvoiceHeader>();
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 500m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			Factory.Save();

			var (declaration1, declaration2) = GetDeclarationsForAttach();
			var newFactory1 = declaration1.Factory;
			var newFactory2 = declaration2.Factory;
			newFactory1.RefreshEnabled = false;
			newFactory2.RefreshEnabled = false;
			newFactory1.Save();
			newFactory2.Save();

			var invoice1 = newFactory1.Load<TInvoiceHeader>(invoice.PK);
			var invoice2 = newFactory2.Load<TInvoiceHeader>(invoice.PK);
			declaration1.Invoices.Add(invoice1);
			declaration2.Invoices.Add(invoice2);
			AssertEquals(4, declaration1.InvoiceLines.Count);
			AssertEquals(3, declaration2.InvoiceLines.Count);
			newFactory1.Save();
			var handler = new NotificationHandlerForTest();
			try
			{
				newFactory2.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertContains("Please cancel your changes and reload the form.", handler.ReportInformationMessage);
		}

		public void TestTypeOfGroupCharges()
		{
			AssertEquals("GroupCharges' type should be expected", ExpectedTypeOfGroupCharges, ((BaseJobComInvoiceHeader)GetNewBusinessObject()).GroupCharges.GetType());
		}

		public void TestTypeOfCharges()
		{
			AssertEquals("Charges' type should be expected", ExpectedTypeOfCharges, ((BaseJobComInvoiceHeader)GetNewBusinessObject()).Charges.GetType());
		}

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.BaseJobComInvoiceHeader);
			}
		}

		#endregion

		public virtual void TestITriggerActionProviderMembers()
		{
			var invoiceHeader = Factory.New<TInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "INV0001";
			Factory.Save();

			var triggerActionProvider = invoiceHeader as ITriggerActionProvider;
			AssertNotNull(triggerActionProvider);
			AssertEquals("Stand alone invoice header.", ZString.Empty, triggerActionProvider.ReasonForDoNotTriggerAction);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00000001";
			invoiceHeader.JZ_JE = declaration.PK;
			Factory.Save();

			AssertEquals("Invoice Header is attached to a customs declaration.", "Commercial Invoice INV0001 is attached to Customs Declaration B00000001.", triggerActionProvider.ReasonForDoNotTriggerAction);
		}

		protected virtual Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<BaseApportionedCharge>);

		protected virtual Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<BaseInvoiceCharge>);

		public virtual void TestGetNewJobComInvoiceHeaderProcessTaskCollection()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<BaseJobComInvoiceHeaderProcessTask, BaseJobComInvoiceHeader>", typeof(ProcessTaskCollection<BaseJobComInvoiceHeaderProcessTask, BaseJobComInvoiceHeader>), ((IWorkflowProvider)invoice).WorkflowItems);
		}

		public virtual void TestAllowNonWesternEuropeanCharacterForMarksAndNumbers()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals(false, invoice.AllowNonWesternEuropeanCharacterForMarksAndNumbers);
		}

		public void TestUniversalCopy()
		{
			AssertNotNull(typeof(BaseJobComInvoiceHeader).GetCustomAttribute<UniversalCopyAddInfoAttribute>());

			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])(typeof(BaseJobComInvoiceHeader).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false));
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertCollectionContains("GroupInvoiceFK", ignoreElementAttributes[0].ElementNames);

			var mappingKeysAttributes = (UniversalCopyMappingKeysAttribute[])(typeof(BaseJobComInvoiceHeader).GetCustomAttributes(typeof(UniversalCopyMappingKeysAttribute), false));
			AssertEquals(1, mappingKeysAttributes.Length);
			AssertEquals(BaseJobComInvoiceHeader.Schema.JZ_JZ_GroupInvoiceFK, mappingKeysAttributes[0].RelatedKeyPropertyName);
		}

		public void TestUniversalCopyWithExtendedEntities()
		{
			AssertNotNull(typeof(BaseJobComInvoiceHeader).GetCustomAttribute<UniversalCopyWithExtendedEntitiesAttribute>());

			var universalCopyWithExtendedEntitiesAttributes = (UniversalCopyWithExtendedEntitiesAttribute[])(typeof(BaseJobComInvoiceHeader).GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), false));
			AssertEquals(1, universalCopyWithExtendedEntitiesAttributes.Length);
			AssertEquals("StartUniversalCopy", universalCopyWithExtendedEntitiesAttributes[0].StartCopyMethod);
			AssertEquals("FinishUniversalCopy", universalCopyWithExtendedEntitiesAttributes[0].FinishCopyMethod);
		}

		public void TestStartUniversalCopy()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			var method = invoiceHeader.GetType().GetMethod("StartUniversalCopy", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(invoiceHeader, null);
			AssertEquals("IsUniversalCopying should set to true in StartUniversalCopy", true, invoiceHeader.IsUniversalCopying);
		}

		public void TestFinishUniversalCopy()
		{
			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobComInvoiceHeaderSchema.Constants.JZ_Description, CopyMethod = CopyMethod.Copy });
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_Description = "desc";
			var invoiceHeader2 = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader2 = (BaseJobComInvoiceHeader)new BusinessObjectCopyManager().Copy(invoiceHeader, new CopyTemplateTree { InnerNode = entityNode }).Object;
			AssertEquals("IsUniversalCopying should set to false after copy", false, invoiceHeader2.IsUniversalCopying);
		}

		public void TestCreationOfInvoiceHeaderDoesNotCreateInvoiceLinesCollection()
		{
			// If a declaration is null ensure creation of an invoice header does not generate an InvoiceLineDependentCollection 
			var testInvoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertNull("Pre-condition", testInvoice.JobDeclaration);
			Assert(@"Make sure JobComInvoiceLines was not generated on Constructor/SetDefaultValue of JobComInvoiceHeader when JobDeclaration is null. See WI00349749
				On Universal Copy, when creating and call SetDefaultValue of JobComInvoiceHeader, JobDeclaration is null. JZ_JE will set by DataRow copying. So the JobComInvoiceLines will be created from invoice, not declaration.",
				!testInvoice.IsInvoiceLinesLoaded());
		}

		public virtual void TestChargeTypeList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public virtual void TestLocalCurrencyCodeCoreOverride()
		{
			if (ExpectedBusinessObjectType != typeof(BaseJobComInvoiceHeader) && ExpectedBusinessObjectType != ObjectFactory.GetType<Integration.Customs.EU.IJobComInvoiceHeader>()
				 && ExpectedBusinessObjectType != ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceHeader>())
			{
				AssertEquals(GetLocalCurrencyCode(), NewInvoiceHeader.LocalCurrencyCode);
			}
			else
			{
				Assert(true);
			}
		}

		public virtual string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Anguilla;

		protected BaseJobComInvoiceHeader NewInvoiceHeader
		{
			get
			{
				var declaration = GetNewDeclaration();
				var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				return groupHeader.JobComInvoiceHeaders.AddNew();
			}
		}

		public virtual void TestCFRCalculationWithFreightAdjustedFlag()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			BaseJobComInvoiceHeader invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10200m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10200m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestReOrderInvoiceSequenceNumbers()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice3 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice4 = declaration.Invoices.AddNew();

			AssertEquals("PreCondition", (short)1, invoice1.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)2, invoice2.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)3, invoice3.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)4, invoice4.JZ_InvoiceDisplaySequence);

			invoice2.JZ_InvoiceDisplaySequence = 3;
			AssertEquals((short)1, invoice1.JZ_InvoiceDisplaySequence);
			AssertEquals((short)3, invoice2.JZ_InvoiceDisplaySequence);
			AssertEquals((short)2, invoice3.JZ_InvoiceDisplaySequence);
			AssertEquals((short)4, invoice4.JZ_InvoiceDisplaySequence);

			invoice3.Delete();
			AssertEquals((short)1, invoice1.JZ_InvoiceDisplaySequence);
			AssertEquals((short)2, invoice2.JZ_InvoiceDisplaySequence);
			AssertEquals((short)3, invoice4.JZ_InvoiceDisplaySequence);

			invoice2.JZ_JE = ZGuid.Empty;//detached

			AssertEquals((short)1, invoice1.JZ_InvoiceDisplaySequence);
			AssertEquals((short)2, invoice4.JZ_InvoiceDisplaySequence);

			AssertEquals((short)1, invoice2.JZ_InvoiceDisplaySequence);
		}

		public virtual void TestIWeightApportioneeRoundingIssue()
		{
			var foreignCurrency = RefCurrency.New(Factory);
			foreignCurrency.RX_Code = "XYZ";
			foreignCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), RatesAreReciprocal ? 1.231678m : 0.8119m);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice1.JZ_InvoiceAmount = 8000m;

			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice2.JZ_InvoiceAmount = 2000m;

			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, true);
			invoice1.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 0.25m : 4m;

			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4M : 0.25m;
			AssertEquals("Weight", 2000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 8000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, false);
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, false);
			declaration.ApportionInvoiceWeight(null);
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4M : 0.25m;
			AssertEquals("Weight", 5000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 5000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);
		}

		public virtual void TestInvoiceLineChargeAffectsBalanceCalculation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			declaration.ResumeApportionment();

			AssertEquals(0m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertNotEquals("Not balanced yet", 0m, invoice.JZ_Calc_Balance);

			var lineCharge = invoiceLine2.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			lineCharge.J7_IsDutiable = false;
			lineCharge.J7_IsGSTApplicable = false;
			lineCharge.J7_ChargeDescription = GetDiscountChargeDescriptionForTest();
			declaration.ResumeApportionment();
			AssertEquals("line level Discount", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Now balanced", 0m, invoice.JZ_Calc_Balance);

			var invoiceCharge = invoice.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			invoiceCharge.J7_IsDutiable = false;
			invoiceCharge.J7_IsGSTApplicable = false;
			invoiceCharge.J7_ChargeDescription = GetDiscountChargeDescriptionForTest();
			declaration.ResumeApportionment();
			AssertEquals("Invoice Level Discount. Line level Discount is disregarded", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Still balanced", 0m, invoice.JZ_Calc_Balance);
		}

		protected virtual string GetDiscountChargeCodeForTest() => CustomsChargeTypeList.Codes.Discount;

		protected virtual string GetDiscountChargeDescriptionForTest() => ZString.Empty;

		public virtual void TestNotSupportedExceptionOnSupplierNameSetter()
		{
			AssertExceptionThrown<NotSupportedException>(() => invoiceHeader.SupplierName = "FERD");
		}

		public void TestLightValidationForDifferentMessageTypesAndTransportModes()
		{
			new LightValidationForDifferentMessageTypesAndTransportModesTester(
				GetNewDeclaration,
				tester => TestLightValidation(tester.InvoiceHeader),
				declaration => AfterInitialise(declaration)
			).Test();
		}

		protected virtual void AfterInitialise(BaseJobDeclaration declaration)
		{
		}

		public void TestRebuildBillsInvoicesWhenCU_RelatedBillIsSet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			Bill houseBill2 = declaration.Bills.AddNew();

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			AssertEquals(1, houseBill.Invoices.Count);
			AssertEquals(0, houseBill2.Invoices.Count);
			AssertEquals(true, houseBill.Invoices.Contains(invoice));

			invoice.JZ_CU_RelatedHouseBill = houseBill2.PK;
			AssertEquals(0, houseBill.Invoices.Count);
			AssertEquals(1, houseBill2.Invoices.Count);
			AssertEquals(true, houseBill2.Invoices.Contains(invoice));
		}

		public void TestTNIHumanReadableName()
		{
			AssertEquals("Overseas Freight and Insurance", invoiceHeader.JZ_Calc_TNIInfo.HumanReadableName);
		}

		public void TestAllGroupInvoices()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup1 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceGroupHeader subGroup2 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice = subGroup2.JobComInvoiceHeaders.AddNew();
			AssertEquals("All Group Invoices should contain TopGroup", true, invoice.AllGroupInvoices.Contains(topGroup));
			AssertEquals("All Group Invoices should not contain SubGroup1", false, invoice.AllGroupInvoices.Contains(subGroup1));
			AssertEquals("All Group Invoices should contain SubGroup2", true, invoice.AllGroupInvoices.Contains(subGroup2));
		}

		public void TestImporter_Effective()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			AssertEquals("Should Default to null", null, invoiceHeader.Importer_Effective);
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("Drop Back to Declaration if none against InvoiceHeader", org1, invoiceHeader.Importer_Effective);
			invoiceHeader.JZ_OH_Buyer = org2.PK;
			AssertEquals("Use value from InvoiceHeader if it's available", org2, invoiceHeader.Importer_Effective);
		}

		public void TestSupplier_Effective()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			AssertEquals("Should Default to null", null, invoiceHeader.Supplier_Effective);
			declaration.JE_OH_Supplier = org1.PK;
			AssertEquals("Drop Back to Declaration if none against InvoiceHeader", org1, invoiceHeader.Supplier_Effective);
			invoiceHeader.JZ_OH_Supplier = org2.PK;
			AssertEquals("Use value from InvoiceHeader if it's available", org2, invoiceHeader.Supplier_Effective);
		}

		public void TestJZ_OH_Buyer_Effective()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			AssertEquals("Should Default to ZGuid.Empty", ZGuid.Empty, invoiceHeader.JZ_OH_Buyer_Effective);
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("Drop Back to Declaration if none against InvoiceHeader", org1.PK, invoiceHeader.JZ_OH_Buyer_Effective);
			invoiceHeader.JZ_OH_Buyer = org2.PK;
			AssertEquals("Use value from InvoiceHeader if it's available", org2.PK, invoiceHeader.JZ_OH_Buyer_Effective);
		}

		public void TestJZ_OH_Supplier_Effective()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			AssertEquals("Should Default to ZGuid.Empty", ZGuid.Empty, invoiceHeader.JZ_OH_Supplier_Effective);
			declaration.JE_OH_Supplier = org1.PK;
			AssertEquals("Drop Back to Declaration if none against InvoiceHeader", org1.PK, invoiceHeader.JZ_OH_Supplier_Effective);
			invoiceHeader.JZ_OH_Supplier = org2.PK;
			AssertEquals("Use value from InvoiceHeader if it's available", org2.PK, invoiceHeader.JZ_OH_Supplier_Effective);
		}

		public virtual void TestSupplierDefaultingIncoTermAndCurrency()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			supplier.MiscServ.OM_RX_NKEXDefCurrency = Core.Constants.CurrencyCodes.Australia;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Inco Term should default from supplier entered", "CIF", invHeader.JZ_IncoTerm);
			AssertEquals("Invoice Currency should default from supplier entered", "AUD", invHeader.JZ_RX_NKInvoice_Currency);
		}

		public void TestSupplierDefaultingResponsibleParty()
		{
			var supplier = OrgHeader.New(Factory);
			var contact = supplier.Contacts.AddNew();
			contact.OC_ContactName = "JOHN SMITH";
			contact.Allocations.AddNew().PC_Type = ContactAllocationType.USPGA;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_OH_Supplier = supplier.PK;
			AssertNull(invHeader.InvoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs(InvoiceHeaderRefsTypeList.Codes.RP));

			contact.Allocations.AddNew().PC_Type = ContactAllocationType.CIV;
			invHeader.JZ_OH_SupplierInfo.ClearValue();
			invHeader.JZ_OH_Supplier = supplier.PK;
			AssertNotNull(invHeader.InvoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs(InvoiceHeaderRefsTypeList.Codes.RP));
			AssertEquals("JOHN SMITH", invHeader.InvoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs(InvoiceHeaderRefsTypeList.Codes.RP).J2_ReferenceNumber);
		}

		[ExpectNoExceptions]
		public void TestDeletingInvoiceBeforeInvoiceLinesInstantiatedDoesNotCauseException()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = testDec.FilteredInvoiceLines.AddNew();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var testDecInAnotherFactory = anotherFactory.Load<BaseJobDeclaration>(testDec.PK);
			var invLoaded = anotherFactory.Load<BaseJobComInvoiceHeader>(invoice.PK);
			testDecInAnotherFactory.Invoices.Delete(invLoaded);
		}

		public void TestHasChangesStaysFalseWithDetachedInvoiceHeader()
		{
			BusinessObjectFactory savingFactory = new BusinessObjectFactory();
			BaseJobComInvoiceHeader savingInvoice = GetPopulatedDetachedCommercialInvoice(savingFactory);
			savingFactory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			var invoice = factory.Load<BaseJobComInvoiceHeader>(savingInvoice.PK);
			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoice);

			invoice.RunPreSaveValidation();

			Assert("Invoice should not have changes", !invoice.HasChanges);
		}

		public void TestOnlyCorrectObjectsGetSavedWithDetachedInvoiceHeader()
		{
			var factory = new BusinessObjectFactory();
			factory.AllowMultipleBusinessObjectsAroundOneRow = false;

			var invoice = GetPopulatedDetachedCommercialInvoice(factory);
			var invoiceLineAddInfoChildType = ((IAddInfoChildSupporter)invoice.JobComInvoiceLines[0]).AddInfoChild?.GetType();

			foreach (var bO in ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects)
			{
				if (bO.IsSavedByFactory)
				{
					Assert("Only invoice headers and lines should be saved. However a " + bO.GetType().FullName + " is going to be saved too.", ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO));
					if (bO is BaseJobComInvoiceHeader)
					{
						AssertEquals("Only main invoice header should be saved", invoice, bO);
					}

					if (bO is BaseJobComInvoiceLine)
					{
						AssertEquals("Only lines attached to main invoice header should be saved", invoice.PK, ((BaseJobComInvoiceLine)bO).JI_JZ);
					}
				}
			}
		}

		protected virtual bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO)
		{
			return bO is BaseJobComInvoiceHeader ||
				bO is BaseJobComInvoiceLine ||
				bO is BaseJobComInvHeaderCharge ||
				bO is CusAddInfo ||
				bO is OrgCompanyData ||
				bO is StmNote ||
				(invoiceLineAddInfoChildType != null && invoiceLineAddInfoChildType == bO.GetType()) ||
				(((IAddInfoChildSupporter)invoice).AddInfoChild is BusinessObject addInfoChild && object.ReferenceEquals(addInfoChild, bO)) ||
				bO is OrgCountryData /* gets created by org if not used in this country before */;
		}

		public void TestMarkApportionmentDirtyOnInvoiceCurrExRateChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice, false);
			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRate = 0.3243m;
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);

			invoice.JZ_InvoiceCurrExRate = 0.3242m;
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);

			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice, true);
			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRate = 0.3243m;
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRate = 0.3242m;
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice, false);
			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRate = 0.3243m;
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);
		}

		public virtual void TestMarkApportionmentDirtyOnInvoiceCurrExRateTypeChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);

			invoice.JZ_InvoiceCurrExRateType = "BSF";
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);

			invoice.JZ_InvoiceCurrExRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRateType = "BSF";
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);
		}

		public virtual void TestMarkApportionmentDirtyOnIsJZ_InvoiceCurrExRateUserEnterableChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice, false);

			declaration.ResumeApportionment();
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice, true);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ResumeApportionment();
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice, false);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyOnInvoiceAmountChanged()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			declaration.ResumeApportionment();
			invoice.JZ_InvoiceAmount = 10000;
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyOnValuationDateChanged()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			declaration.ResumeApportionment();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 1);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);
		}

		public virtual void TestMarkApportionmentDirtyOnInvoiceCurrencyChanged()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			declaration.ResumeApportionment();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyOnIncotermChanged()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			declaration.ResumeApportionment();
			invoice.JZ_IncoTerm = "CIF";
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyOnGroupInvoiceChanged()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			subGroupHeader.JZ_InvoiceNumber = "SubGroup";
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_Calc_GroupInvoice = topGroupHeader.JZ_InvoiceNumber;
			declaration.ResumeApportionment();
			invoice.JZ_Calc_GroupInvoice = subGroupHeader.JZ_InvoiceNumber;
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyOnInvoiceChargeChanged()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			declaration.ResumeApportionment();
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);
		}

		public void TestTemplateCopy()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			using (invoice.GetValidationSuspender())
			{
				invoice.JZ_AddInfo = "AddInfoZZZ"; // contains MessageType
				invoice.JZ_InvoiceNumber = "ABC";
				invoice.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				invoice.Charges.AddNew();
				invoice.JobComInvoiceLines.AddNew();
				invoice.JobComInvoiceLines.AddNew();
				Factory.Save(); // TemplateCopy only copies persistent values; AddInfo which hasn't been serialised will not be copy

				ITemplateCopyable template = invoice;
				BaseJobComInvoiceHeader copy = (BaseJobComInvoiceHeader)template.TemplateCopy();

				AssertEquals("Copied correctly", invoice.JZ_AddInfo, copy.JZ_AddInfo);
				AssertEquals("Copied correctly", invoice.JZ_OH_Supplier, copy.JZ_OH_Supplier);
				AssertEquals("Copied correctly", invoice.JobComInvoiceLines.Count, copy.JobComInvoiceLines.Count);
				AssertEquals("Copied correctly", invoice.Charges.Count, copy.Charges.Count);
				AssertEquals("Defaulted correctly", "*TBA*", copy.JZ_InvoiceNumber);
			}
		}

		public void TestTemplateCopyWithAttachedInvoice()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			using (invoice.GetValidationSuspender())
			{
				invoice.JZ_AddInfo = "AddInfoZZZ"; // contains MessageType
				invoice.JZ_InvoiceNumber = "ABC";
				invoice.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				invoice.Charges.AddNew();
				invoice.JobComInvoiceLines.AddNew();
				invoice.JobComInvoiceLines.AddNew();
				Factory.Save(); // TemplateCopy only copies persistent values; AddInfo which hasn't been serialised will not be copy

				ITemplateCopyable template = invoice;
				BaseJobComInvoiceHeader copy = (BaseJobComInvoiceHeader)template.TemplateCopy();

				AssertEquals("Copied correctly", invoice.JZ_AddInfo, copy.JZ_AddInfo);
				AssertEquals("Copied correctly", invoice.JZ_OH_Supplier, copy.JZ_OH_Supplier);
				AssertEquals("Copied correctly", invoice.JobComInvoiceLines.Count, copy.JobComInvoiceLines.Count);
				AssertEquals("Copied correctly", invoice.Charges.Count, copy.Charges.Count);
				AssertEquals("Defaulted correctly", "*TBA*", copy.JZ_InvoiceNumber);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteInvoiceBeforeInvoiceLinesAreReferenced()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var subGroup = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			var invoice = testDec.Invoices.AddNew();
			var line = testDec.FilteredInvoiceLines.AddNew();

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var decLoaded = anotherFactory.Load<BaseJobDeclaration>(testDec.PK);
			var invoiceLoaded = decLoaded.Invoices[0];
			decLoaded.Invoices.Delete(invoiceLoaded);
		}

		public virtual void TestIsInvoiceLinesLoaded()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_JE = declaration.PK;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();

			var retrievingFactory = new BusinessObjectFactory();
			var retrievedInvoice = retrievingFactory.Load<BaseJobComInvoiceHeader>(invoiceHeader.PK);
			AssertEquals("InvoiceLines not loaded yet", false, retrievedInvoice.IsInvoiceLinesLoaded());
			object x = retrievedInvoice.JobComInvoiceLines;
			AssertEquals("InvoiceLines loaded", true, retrievedInvoice.IsInvoiceLinesLoaded());
		}

		public virtual void TestDefaultINCOFromOrgLink()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			OrgHeader consignee = OrgHeader.New(Factory);
			OrgHeader consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";

			OrgSupplierBuyerLink link = consignee.SupplierLinks.AddNew(consignor);
			link.OL_RN_NKImporterCountry = declaration.CountryCode;
			link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "123";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_RL_NKFinalDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = consignor.PK;

			AssertEquals("Defaulted Inco Term", "123", invoiceHeader.JZ_IncoTerm);
		}

		public virtual void TestDefaultINCOFromSupplier()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			OrgHeader consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			invoiceHeader.JZ_OH_Supplier = consignor.PK;

			AssertEquals("Defaulted Inco Term", "321", invoiceHeader.JZ_IncoTerm);

			invoiceHeader.JZ_IncoTerm = "234";
			invoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Overridden Inco Term", "234", invoiceHeader.JZ_IncoTerm);
		}

		public virtual void TestDefaultINCOFromImporter()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_RL_NKFinalDestination = "BFXXX";
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.MiscServ.OM_IMDefaultINCOTerm = "135";

			OrgHeader consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			var link = consignee.SupplierLinks.AddNew(consignor);
			link.OL_RN_NKImporterCountry = "BF";

			declaration.JE_OH_Importer = consignee.PK;
			invoiceHeader.JZ_OH_Supplier = consignor.PK;

			AssertEquals("Defaulted Inco Term", "135", invoiceHeader.JZ_IncoTerm);
		}

		public void TestEffectiveGrossWeightReturnsActualGrossWeight()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_Weight = 123m;
			invoiceHeader.JZ_WeightUQ = "T";
			AssertEquals(new ZWeight(123, "T"), invoiceHeader.EffectiveGrossWeight);
		}

		public void TestEffectiveGrossWeightReturnsEffectiveLineWeightTotal()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = "KG";
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Weight = 900;
			invoiceLine.JI_WeightUQ = "KG";

			AssertEquals(new ZWeight(900, "KG"), invoiceHeader1.EffectiveGrossWeight);
		}

		public void TestInvoiceAmount()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_InvoiceAmount = 123m;
			AssertEquals(123m, invoiceHeader.InvoiceAmount.Amount);
			AssertEquals(declaration.LocalCurrencyCode, invoiceHeader.InvoiceAmount.Currency.Code);
		}

		public virtual void TestSettingJZ_JEByFakeDeclarationShouldNotChangeData()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			supplier.OH_FullName = "DUMMY COMP";
			supplier.MainAddress.OA_Address1 = "Address 1";

			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_AutoWeightApportion = true;
			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_Weight = 20m;
			AssertEquals(ZGuid.Empty, invoice.JZ_OH_Supplier);
			Factory.Save();
			AssertEquals(ZGuid.Empty, invoice.JZ_OH_Supplier);
			AssertEquals(20m, invoice.JZ_Weight);
		}
		public void TestSupplierName()
		{
			BaseJobDeclaration jobDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invHead = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			Assert(invHead.JZ_OH_Supplier.IsEmpty);
			AssertEquals("", invHead.SupplierName);
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "TESTSUP";
			supplier.OH_FullName = "SUPPLIER NAME";
			invHead.JZ_OH_Supplier = supplier.PK;
			AssertEquals("SUPPLIER NAME", invHead.SupplierName);
			invHead.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("", invHead.SupplierName);
		}

		public void TestSupplierNameInfo()
		{
			BaseJobDeclaration jobDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invHead = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("SupplierName", invHead.SupplierNameInfo.Name);
			AssertEquals("Should default to ReadOnly", true, invHead.SupplierNameInfo.ReadOnly);
		}

		public void TestSettingAddressFromSupplier()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.OH_Code = "TSTORG01";
			org2.OH_Code = "TSTORG02";

			var addr1 = org1.MainAddress;
			var addr2 = org2.MainAddress;

			var invoice = Factory.New<BaseJobComInvoiceHeader>();

			invoice.JZ_OA_SupplierAddress = addr1.PK;
			invoice.JZ_OH_Supplier = org1.PK;
			AssertEquals(addr1.PK, invoice.JZ_OA_SupplierAddress);
			AssertEquals(org1.PK, invoice.JZ_OH_Supplier);
			AssertSupplierAddressOrgMatchesSupplierOrg();

			invoice.JZ_OH_Supplier = org2.PK;
			// some countries (US) populate address from organization
			Assert(invoice.JZ_OA_SupplierAddress.IsEmpty || invoice.JZ_OA_SupplierAddress == addr2.PK);
			AssertEquals(org2.PK, invoice.JZ_OH_Supplier);
			AssertSupplierAddressOrgMatchesSupplierOrg();

			invoice.JZ_OA_SupplierAddress = addr2.PK;
			invoice.JZ_OH_Supplier = org2.PK;
			AssertEquals(addr2.PK, invoice.JZ_OA_SupplierAddress);
			AssertEquals(org2.PK, invoice.JZ_OH_Supplier);
			AssertSupplierAddressOrgMatchesSupplierOrg();

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
			AssertEquals(ZGuid.Empty, invoice.JZ_OH_Supplier);
			AssertSupplierAddressOrgMatchesSupplierOrg();

			void AssertSupplierAddressOrgMatchesSupplierOrg()
			{
				// We need to enforce invariant that is checked on server side in TG_JobComInvoiceHeader_SupplierAddress 
				Assert("If supplier address is specified, supplier address org must match supplier org", invoice.JZ_OA_SupplierAddress.IsEmpty || invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK == invoice.JZ_OH_Supplier);
			}
		}

		public void TestSettingSupplierFromAddressWhenDeleted()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTORG01";
			org2.OH_Code = "TSTORG02";

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_OA_SupplierAddress = org1.MainAddress.PK;
			invoice.JZ_OH_Supplier = org1.PK;

			org1.Delete();
			invoice.JZ_OH_Supplier = org2.PK;
			Assert("some countries (US) populate address from organization", invoice.JZ_OA_SupplierAddress.IsEmpty || invoice.JZ_OA_SupplierAddress == org2.MainAddress.PK);
		}

		public void TestSingleLineKeepsLineInSync()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Precondition", 0m, invoiceLine.JI_LinePrice);
			invoiceHeader.JZ_InvoiceAmount = 100m;
			AssertEquals("Value should not change", 0m, invoiceLine.JI_LinePrice);
			invoiceHeader.IsSingleLine = true;
			invoiceHeader.JZ_InvoiceAmount = 101m;
			AssertEquals("Value should change", 101m, invoiceLine.JI_LinePrice);
			invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 102m;
			AssertEquals("Value should not change", 101m, invoiceLine.JI_LinePrice);
		}

		public void TestInvoiceLines()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			AssertEquals(3, invoiceHeader.InvoiceLines.Count);
		}

		public void TestNoOfPacksPackType()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "CNT";
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("CNT", invoiceHeader.NoOfPacksPackType);
			AssertEquals("Should be able to access through the Info.Value", "CNT", invoiceHeader[BaseJobComInvoiceHeader.Schema.NoOfPacksPackType]);
		}

		public void TestNoteTypes()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("Note Types", 13, invoice.NoteTypes.Count);
		}

		public virtual void TestIWorkflowProviderImpl()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			OrgHeader org3 = OrgHeader.New(Factory);
			OrgHeader org4 = OrgHeader.New(Factory);
			var declaration = GetNewDeclaration();
			var invoice1 = declaration.Invoices.AddNew();
			var workflowProvider1 = invoice1 as IWorkflowProvider;
			AssertNotNull("JobComInvoiceHeader should be IWorkflowProvider", workflowProvider1);
			AssertNull("GetWorkflowInformationProvider", workflowProvider1.GetWorkflowInformationProvider());
			AssertNotNull("WorkflowItems", workflowProvider1.WorkflowItems);
			AssertEquals("WorkflowItems should be empty", 0, workflowProvider1.WorkflowItems.Count);
			AssertEquals("WorkflowType", WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode, workflowProvider1.WorkflowType);

			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_OH_Supplier = org2.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var ranker1 = workflowProvider1.GetTemplateSelectionCriteria();
			AssertEquals(typeof(ColumnValueRanker), ranker1.GetType());
			AssertCollectionContains("Clients should contains Importer", org1.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionNotContains("Clients should not contains Supplier", org2.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains IMP", JobMessageTypeList.Codes.Import, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ranker1 = workflowProvider1.GetTemplateSelectionCriteria();
			AssertCollectionNotContains("Clients should not contains Importer", org1.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Clients should contains Supplier", org2.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains EXP", JobMessageTypeList.Codes.Export, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_SubType1));

			invoice1.JZ_OH_Buyer = org3.PK;
			invoice1.JZ_OH_Supplier = org4.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ranker1 = workflowProvider1.GetTemplateSelectionCriteria();
			AssertEquals(typeof(ColumnValueRanker), ranker1.GetType());
			AssertCollectionContains("Clients should contains Importer", org3.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionNotContains("Clients should not contains Supplier", org4.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains IMP", JobMessageTypeList.Codes.Import, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ranker1 = workflowProvider1.GetTemplateSelectionCriteria();
			AssertCollectionNotContains("Clients should not contains Importer", org3.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Clients should contains Supplier", org4.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains EXP", JobMessageTypeList.Codes.Export, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_SubType1));

			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_OH_Buyer = org1.PK;
			invoice2.JZ_OH_Supplier = org2.PK;
			invoice2.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice2.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;
			var workflowProvider2 = invoice2 as IWorkflowProvider;
			var ranker2 = workflowProvider2.GetTemplateSelectionCriteria();
			AssertEquals(typeof(ColumnValueRanker), ranker2.GetType());
			AssertCollectionContains("Clients should contains Importer", org1.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionNotContains("Clients should not contains Supplier", org2.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains IMP", JobMessageTypeList.Codes.Import, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
			invoice2.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice2.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Export;
			ranker2 = workflowProvider2.GetTemplateSelectionCriteria();
			AssertCollectionNotContains("Clients should not contains Importer", org1.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Clients should contains Supplier", org2.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains EXP", JobMessageTypeList.Codes.Export, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
		}

		public void TestIsAdvanceShippingNotice()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			Assert("IsAdvanceShippingNotice", invoice.IsAdvanceShippingNotice);
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsAdvanceShippingNotice", !invoice.IsAdvanceShippingNotice);
		}

		public void TestAdvanceShippingNotice()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.OP_StockKeepingUnit = "U!";
			part.RelatedOrganisations.AddSupplier(consignor);
			part.RelatedOrganisations.AddOwner(consignee);

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertEquals("JZ_MessageType should not be set from Buyer and Suppiler", JobMessageTypeList.MoreCodes.AdvanceShippingNotice, invoice.JZ_MessageType);

			new FakeDeclarationCreatorForInvoice(invoice);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1;
			Assert("invoiceLine.PartSyncManager.Enabled should be false for ASN invoice", !invoiceLine.PartSyncManager.Enabled);
			invoiceLine.JI_PartNo = "PART1";
			AssertEquals("JI_OP should be null for ASN invoice", ZGuid.Empty, invoiceLine.JI_OP);
			AssertEquals("JI_Weight has NOT been defaulted", 0m, invoiceLine.JI_Weight);
			AssertNotEquals("JI_WeightUQ has NOT been defaulted", "HG", invoiceLine.JI_WeightUQ);

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JI_OP should be set for IMP invoice", part.PK, invoiceLine.JI_OP);
			AssertEquals("JI_Weight has been defaulted", 100m, invoiceLine.JI_Weight);
			AssertEquals("JI_WeightUQ has been defaulted", "HG", invoiceLine.JI_WeightUQ);

			Factory.Save();
			AssertEquals("JZ_MessageType", "IMP", invoice.JZ_MessageType);
			AssertEquals("JZ_StandAloneInvoiceDirection", "IMP", invoice.JZ_StandAloneInvoiceDirection);

			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			new FakeDeclarationCreatorForInvoice(invoice);
			invoiceLine.JI_Weight = 0m;
			invoiceLine.JI_WeightUQ = ZString.Empty;
			Factory.Save();
			AssertEquals("JI_OP should be null for ASN invoice", ZGuid.Empty, invoiceLine.JI_OP);
			AssertEquals("JZ_MessageType", "ASN", invoice.JZ_MessageType);
			AssertEquals("JZ_StandAloneInvoiceDirection", "ASN", invoice.JZ_StandAloneInvoiceDirection);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			declaration.Invoices.Add(invoice);
			AssertEquals("Product data defaulted to the invoice line", part.PK, invoiceLine.JI_OP);
			AssertEquals("JI_Weight", 100m, invoiceLine.JI_Weight);
			AssertEquals("JI_WeightUQ", "HG", invoiceLine.JI_WeightUQ);

			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			AssertEquals("JZ_StandAloneInvoiceDirection", "", invoice.JZ_StandAloneInvoiceDirection);
			AssertEquals("JZ_MessageType", "ASN", invoice.JZ_MessageType);
			AssertEquals("JI_OP should be null for ASN invoice", ZGuid.Empty, invoiceLine.JI_OP);
			Factory.Save();
			AssertEquals("JZ_StandAloneInvoiceDirection", "ASN", invoice.JZ_StandAloneInvoiceDirection);
		}

		public void TestRefreshInvoiceListOnJZ_InvoiceNumberChange()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("SortedInvoiceList Count", 0, declaration.SortedInvoiceList.Count);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			AssertEquals("SortedInvoiceList Count", 1, declaration.SortedInvoiceList.Count);
			AssertEquals("Invoice number", "1", declaration.SortedInvoiceList[0].Code);
		}

		public void TestRefreshInvoiceListOnJZ_InvoiceDisplaySequenceChange()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			AssertEquals("SortedInvoiceList Count", 2, declaration.SortedInvoiceList.Count);
			AssertEquals("Invoice number", "1", declaration.SortedInvoiceList[0].Code);
			AssertEquals("Invoice number", "2", declaration.SortedInvoiceList[1].Code);
			invoice1.JZ_InvoiceDisplaySequence = 10;
			AssertEquals("SortedInvoiceList Count", 2, declaration.SortedInvoiceList.Count);
			AssertEquals("Invoice number", "2", declaration.SortedInvoiceList[0].Code);
			AssertEquals("Invoice number", "1", declaration.SortedInvoiceList[1].Code);
		}

		public void TestRefreshInvoiceListOnDeleteInvoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			AssertEquals("SortedInvoiceList Count", 2, declaration.SortedInvoiceList.Count);
			AssertEquals("Invoice number", "1", declaration.SortedInvoiceList[0].Code);
			AssertEquals("Invoice number", "2", declaration.SortedInvoiceList[1].Code);
			invoice1.Delete();
			AssertEquals("SortedInvoiceList Count", 1, declaration.SortedInvoiceList.Count);
			AssertEquals("Invoice number", "2", declaration.SortedInvoiceList[0].Code);
		}

		public void TestManufacturerAddressBindingList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var descriptor = invoice.JZ_OA_ManufacturerAddressInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertNotNull(listAttribute);
			AssertEquals("JZ_OA_ManufacturerAddress_ZAddress.OrgAddress_List", listAttribute.ListDataSourceMember);
		}

		public void TestIUnitConverterDataProviderType()
		{
			var header = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("Pack Conversion Type should be Commercial Invoice", RPTypeList.Codes.CommercialInvoice, ((IUnitConverterDataProvider)header).Type);
		}

		public void TestConsigneeAddressBindingList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var descriptor = invoice.JZ_OA_ConsigneeAddressInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertNotNull(listAttribute);
			AssertEquals("JZ_OA_ConsigneeAddress_ZAddress.OrgAddress_List", listAttribute.ListDataSourceMember);
		}

		public void TestIsABondedWarehousingInvoiceWithDifferentSupplier()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "O234";
			supplier.MainAddress.OA_Address1 = "1";
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "O236";
			supplier2.MainAddress.OA_Address1 = "1";

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "O235";
			warehouse.MainAddress.OA_Address1 = "1";
			var declarationMock = Factory.NewMoq<TJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Protected().Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);
			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(false);
			declarationMock.Setup(m => m.IsExport).Returns(true);
			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration) { CallBase = true };
			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>()).Returns(true);
			declarationMock.Protected().Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier2.PK;

			CombineAssertions(() =>
			{
				AssertEquals("invoice line", invoice.IsABondedWarehousingInvoiceWithDifferentSupplier, false);
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				SetupCountrySpecificDataForBondedWarehousingInvoiceWithDifferentSupplierTest(declaration, invoice, invoiceLine, supplier2);
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				AssertEquals("different supplier", invoice.IsABondedWarehousingInvoiceWithDifferentSupplier, true);
				invoice.JZ_OH_Supplier = supplier.PK;
				AssertEquals("same supplier", invoice.IsABondedWarehousingInvoiceWithDifferentSupplier, false);
				invoice.JZ_OH_Supplier = ZGuid.Empty;
				AssertEquals("empty supplier", invoice.IsABondedWarehousingInvoiceWithDifferentSupplier, false);
			});
		}

		public void TestIsABondedWarehousingInvoiceWithDifferentSupplier_Entry()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "O234";
			supplier.MainAddress.OA_Address1 = "1";
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "O236";
			supplier2.MainAddress.OA_Address1 = "1";

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "O235";
			warehouse.MainAddress.OA_Address1 = "1";
			var declarationMock = Factory.NewMoq<TJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Protected().Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);
			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			declarationMock.Setup(m => m.IsExport).Returns(true);
			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>()).Returns(true);
			declarationMock.Protected().Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SetSupportsBondedWarehousingForTesting(true);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			FieldInfo info = typeof(CusEntryHeader).GetField("isBondedWarehousingFieldValidationRequiredCached", BindingFlags.NonPublic | BindingFlags.Instance);
			info.SetValue(entry, new CachedProperty<bool>(Factory, () => true));

			var mergedLine = entry.MergedLines.AddNew();
			var invoiceLine = mergedLine.InvoiceLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.JZ_OH_Supplier = supplier2.PK;

			CombineAssertions(() =>
			{
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				SetupCountrySpecificDataForBondedWarehousingInvoiceWithDifferentSupplierTest(declaration, invoice, invoiceLine1, supplier2);
				invoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);

				AssertEquals("different supplier", invoice.IsABondedWarehousingInvoiceWithDifferentSupplier, true);
				invoice.JZ_OH_Supplier = supplier.PK;
				AssertEquals("same supplier", invoice.IsABondedWarehousingInvoiceWithDifferentSupplier, false);
				invoice.JZ_OH_Supplier = ZGuid.Empty;
				AssertEquals("empty supplier", invoice.IsABondedWarehousingInvoiceWithDifferentSupplier, false);
			});
		}

		protected virtual void SetupCountrySpecificDataForBondedWarehousingInvoiceWithDifferentSupplierTest(BaseJobDeclaration jobDeclaration, BaseJobComInvoiceHeader invoiceHeader, BaseJobComInvoiceLine invoiceLine, OrgHeader supplier)
		{
		}

		public void TestDefaultCurrencyToLocalCurrency()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			foreach (var messageType in MessageTypesForDefaultCurrencyToLocalCurrency(declaration))
			{
				AssertDefaultCurrencyToLocalCurrency(declaration, messageType);
			}
		}

		public void TestJZ_OrderNumber_Caption()
		{
			AssertEquals("Order Number", DataBoundResourceStrings.GetDataForProperty(invoiceHeader.JZ_OrderNumberInfo).Caption);
		}

		protected virtual IEnumerable<string> MessageTypesForDefaultCurrencyToLocalCurrency(BaseJobDeclaration declaration) => declaration.Lookups.MessageTypeList.GetAllCodes();

		void AssertDefaultCurrencyToLocalCurrency(BaseJobDeclaration declaration, string messageType)
		{
			CombineAssertions($"Default Invoice Currency for {messageType} job", () =>
			{
				declaration.JE_MessageType = messageType;
				declaration.Invoices.RemoveAll();
				CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var invoice = declaration.Invoices.AddNew();
				AssertEquals("Registry not activated, currency should not default", "", invoice.JZ_RX_NKInvoice_Currency);

				invoice.Delete();
				CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				invoice = declaration.Invoices.AddNew();
				AssertEquals("Registry activated, currency should default to the local currency code", declaration.LocalCurrencyCode, invoice.JZ_RX_NKInvoice_Currency);
			});
		}

		#region TestSupportedAddressTypes + TestGetDocAddress

		public void TestSupportedAddressTypes()
		{
			var addressTypes = ((IDocAddresses)invoiceHeader).SupportedAddressTypes;

			AssertEquals("New address types might have been added, ensure they are tested.", ExpectedDocAddressTypes.Count, addressTypes.Count);
			foreach (DocAddressType addressType in ExpectedDocAddressTypes.Values)
			{
				AssertCollectionContains(addressType, addressTypes);
			}
		}

		protected virtual Hashtable ExpectedDocAddressTypes
		{
			get
			{
				if (fExpectedDocAddressTypes == null)
				{
					fExpectedDocAddressTypes = new Hashtable();
				}
				return fExpectedDocAddressTypes;
			}
		}
		Hashtable fExpectedDocAddressTypes;

		#endregion

		#region Implementation
		protected virtual void SetIsJZ_InvoiceCurrExRateUserEnterable(BaseJobComInvoiceHeader invoice, bool value)
		{
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = value;
		}

		protected BaseJobDeclaration declaration;
		protected BaseJobComInvoiceGroupHeader groupHeader;
		protected BaseJobComInvoiceHeader invoiceHeader;

		protected override BusinessObject GetNewBusinessObject()
		{
			SetUp();
			return invoiceHeader;
		}

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			var result = Factory.New<BaseJobDeclaration>();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetNewDeclaration();
			groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.MarkAsNeedingValidation();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(factory);
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.Charges.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoice;
		}

		protected void AssertMandatoryCharges(int chargeCount, JobComInvChargeCollection<BaseInvoiceCharge> collection, params string[] mandatoryCharges)
		{
			AssertEquals(invoiceHeader.JZ_IncoTerm + " should have " + chargeCount + " charges", chargeCount, collection.Count);
			foreach (string charge in mandatoryCharges)
			{
				Assert(charge + " exists", collection.HasChargeWithThisKey(invoiceHeader.IncoTermAndChargeFactory.GetCharge(charge).ChargeCodeChargeKey));
			}
		}

		protected BaseJobComInvoiceHeader GetPopulatedDetachedCommercialInvoice(BusinessObjectFactory factory)
		{
			BaseJobComInvoiceHeader invoice = factory.New<BaseJobComInvoiceHeader>();

			FakeDeclarationCreatorForInvoice declarationFaker = new FakeDeclarationCreatorForInvoice(invoice);

			ZQuery topFilter = new ZQuery();
			topFilter.OrderBy = OrgHeaderSchema.PK.Name + " ASC";

			ZQuery bottomFilter = new ZQuery();
			bottomFilter.OrderBy = OrgHeaderSchema.PK.Name + " DESC";

			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			invoice.JZ_OH_Buyer = factory.LoadTop1<OrgHeader>(topFilter).PK;
			invoice.JZ_OH_Supplier = factory.LoadTop1<OrgHeader>(bottomFilter).PK;
			invoice.RunPreSaveValidation();

			OrgHeader buyer = factory.Load<OrgHeader>(invoice.JZ_OH_Buyer);
			OrgHeader supplier = factory.Load<OrgHeader>(invoice.JZ_OH_Supplier);
			AssertEquals("Org header '" + buyer.OH_Code + "' should not be saved", false, buyer.IsSavedByFactory);
			AssertEquals("Org header '" + supplier.OH_Code + "' should not be saved", false, supplier.IsSavedByFactory);

			return invoice;
		}

		protected virtual bool RatesAreReciprocal
		{
			get { return false; }
		}
		#endregion
	}
}
