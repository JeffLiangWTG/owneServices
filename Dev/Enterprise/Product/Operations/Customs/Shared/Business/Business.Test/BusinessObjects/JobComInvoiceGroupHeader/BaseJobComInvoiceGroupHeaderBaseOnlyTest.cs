using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvoiceGroupHeaderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestIsDefaultTopGroupHeader()
		{
			var group = Factory.New<BaseJobComInvoiceGroupHeader>();
			Assert("Should default to false.", !group.IsDefaultTopGroupHeader);

			var declaration = Factory.New<BaseJobDeclaration>();
			var defaultTopGroup = declaration.JobComInvoiceGroupHeaders[0];

			Assert("Should be true when the group is created in the SetDefaultValues.", defaultTopGroup.IsDefaultTopGroupHeader);

			var newGroup = defaultTopGroup.JobComInvoiceGroupHeaders.AddNew();
			Assert("Should be false when the group is created in other places.", !newGroup.IsDefaultTopGroupHeader);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUniversalCopy_DuplicateTopGroupInvoiceError()
		{
			//set up source entity
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JZ_GroupInvoiceFK = declaration.JobComInvoiceGroupHeaders[0].PK;
			Factory.Save();
			AssertEquals("Should only contains a default top group invoice header.", 1, declaration.AllGroupHeaders.Count);

			//set up copy template
			var filePath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\Shared\Business\Business\BusinessObjects\Testing\UniversalCopyTemplate_MyFullCopy.xml";
			CopyTemplateTree copyTemplateTree;
			using (var reader = new StreamReader(filePath))
			{
				copyTemplateTree = (CopyTemplateTree)new XmlSerializer(typeof(CopyTemplateTree)).Deserialize(reader);
			}

			//do universal copy
			var copiedDeclaration = (BaseJobDeclaration)new BusinessObjectCopyManagerForTest().Copy(declaration, copyTemplateTree).Object;
			AssertEquals("Should only contains a default top group invoice header.", 1, copiedDeclaration.AllGroupHeaders.Count);
		}

		public void TestNewForUniversalCopy()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Should only contains a default top group invoice header.", 1, declaration.AllGroupHeaders.Count);

			var group = declaration.TopGroupInvoice;
			AssertEquals("Precondtion.", BaseJobComInvoiceGroupHeader.AllInvoices, group.JZ_InvoiceNumber);

			var type = group.GetType();

			var attribute = type
				.GetCustomAttributes<UniversalCopyInstanceTypeAttribute>(true)
				.First();

			var newDeclaration = Factory.New<BaseJobDeclaration>();
			var newGroup = newDeclaration.TopGroupInvoice;
			AssertEquals("Precondtion.", BaseJobComInvoiceGroupHeader.AllInvoices, group.JZ_InvoiceNumber);

			var method = attribute.CreationMethod;
			AssertEquals("Precondition.", "NewForUniversalCopy", method);

			var obj = type.InvokeMember(method, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, group, new object[] { Factory, newDeclaration });
			AssertSame("Should only get the existing group when the invoice number is All Invoices.", newGroup, obj);
			AssertEquals("Should only contains a default top group invoice header.", 1, newDeclaration.AllGroupHeaders.Count);

			var invoice = newDeclaration.Invoices.AddNew();
			var obj2 = type.InvokeMember(method, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, group, new object[] { Factory, invoice });
			AssertSame("Should only get the existing group when the invoice number is All Invoices.", newGroup, obj2);
			AssertEquals("Should only contains a default top group invoice header.", 1, newDeclaration.AllGroupHeaders.Count);
		}

		public void TestUniversalCopy()
		{
			var mappingKeysAttributes = (UniversalCopyMappingKeysAttribute[])(typeof(BaseJobComInvoiceGroupHeader).GetCustomAttributes(typeof(UniversalCopyMappingKeysAttribute), false));
			AssertEquals(1, mappingKeysAttributes.Length);
			AssertEquals(BaseJobComInvoiceGroupHeader.Schema.JZ_JZ_GroupInvoiceFK, mappingKeysAttributes[0].RelatedKeyPropertyName);
		}

		public void TestICommonNonApportionedChargeProvider()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var topGroup = dec.JobComInvoiceGroupHeaders[0];
			ICommonNonApportionedChargeProvider<BaseGroupInvoiceCharge> provider = topGroup;
			AssertEquals(0, topGroup.Charges.Count);
			var charge1 = provider.CreateNew();
			AssertEquals(1, topGroup.Charges.Count);
			AssertEquals(charge1, topGroup.Charges[0]);
			var charge2 = provider.CreateNew();
			AssertEquals(2, topGroup.Charges.Count);
			AssertEquals(charge1, topGroup.Charges[0]);
			AssertEquals(charge2, topGroup.Charges[1]);
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge1.J7_Amount = ZDecimal.Zero;
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_Amount = ZDecimal.Zero;
			AssertEquals(charge1, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasInsurance));
			AssertEquals(charge2, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasFreight));
			charge1.J7_Amount = 10m;
			AssertNull(provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasInsurance));
			AssertEquals(charge2, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasFreight));
		}

		public void TestIChargeHolder()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice1 = topGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();

			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.JZ_InvoiceNumber = "SUBGROUP";
			BaseJobComInvoiceHeader invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals("IChargeHolder.Charges", topGroup.Charges, ((IChargeHolder)topGroup).Charges);
			AssertEquals("IChargeHolder.CurrencyConverter", topGroup.CurrencyConverter, ((IChargeHolder)topGroup).CurrencyConverter);

			ArrayList allApportionees = new ArrayList(((IChargeHolder)topGroup).AllApportionees);
			AssertEquals("IChargeHolder.AllApportionees has invoice1", true, allApportionees.Contains(invoice1));
			AssertEquals("IChargeHolder.AllApportionees has invoiceline1", true, allApportionees.Contains(invoiceLine1));
			AssertEquals("IChargeHolder.AllApportionees has invoice2", true, allApportionees.Contains(invoice2));
			AssertEquals("IChargeHolder.AllApportionees has invoiceline2", true, allApportionees.Contains(invoiceLine2));

			invoice2.JZ_Calc_GroupInvoice = topGroup.JZ_InvoiceNumber;
			allApportionees = new ArrayList(((IChargeHolder)subGroup).AllApportionees);
			AssertEquals("IChargeHolder.AllApportionees does not have invoice1", false, allApportionees.Contains(invoice1));
			AssertEquals("IChargeHolder.AllApportionees does not have invoiceline1", false, allApportionees.Contains(invoiceLine1));
			AssertEquals("IChargeHolder.AllApportionees does not have invoice2", false, allApportionees.Contains(invoice2));
			AssertEquals("IChargeHolder.AllApportionees does not have invoiceline2", false, allApportionees.Contains(invoiceLine2));

			allApportionees = new ArrayList(((IChargeHolder)topGroup).AllApportionees);
			AssertEquals("IChargeHolder.AllApportionees has invoice1", true, allApportionees.Contains(invoice1));
			AssertEquals("IChargeHolder.AllApportionees has invoiceline1", true, allApportionees.Contains(invoiceLine1));
			AssertEquals("IChargeHolder.AllApportionees has invoice2", true, allApportionees.Contains(invoice2));
			AssertEquals("IChargeHolder.AllApportionees has invoiceline2", true, allApportionees.Contains(invoiceLine2));

			invoice2.JZ_Calc_GroupInvoice = subGroup.JZ_InvoiceNumber;
			allApportionees = new ArrayList(((IChargeHolder)subGroup).AllApportionees);
			AssertEquals("IChargeHolder.AllApportionees does not have invoice1", false, allApportionees.Contains(invoice1));
			AssertEquals("IChargeHolder.AllApportionees does not have invoiceline1", false, allApportionees.Contains(invoiceLine1));
			AssertEquals("IChargeHolder.AllApportionees has invoice2", true, allApportionees.Contains(invoice2));
			AssertEquals("IChargeHolder.AllApportionees has invoiceline2", true, allApportionees.Contains(invoiceLine2));

			ArrayList chargeHolderChildren = new ArrayList(((IChargeHolder)topGroup).ImmediateChargeHolderChildren);
			AssertEquals("ImmediateChargeHolderChildren has Invoice1", true, chargeHolderChildren.Contains(invoice1));
			AssertEquals("ImmediateChargeHolderChildren has SubGroup", true, chargeHolderChildren.Contains(subGroup));
			AssertEquals("ImmediateChargeHolderChildren does not have Invoice2", false, chargeHolderChildren.Contains(invoice2));

			AssertEquals("ImmediateChargeHolderParent for TopGroup", null, ((IChargeHolder)topGroup).ImmediateChargeHolderParent);
			AssertEquals("ImmediateChargeHolderParent for SubGroup", topGroup, ((IChargeHolder)subGroup).ImmediateChargeHolderParent);
		}

		public void TestIGroupInvoiceOrInvoice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader groupToTest = topGroup.JobComInvoiceGroupHeaders.AddNew();

			groupToTest.JobComInvoiceGroupHeaders.AddNew();
			groupToTest.JobComInvoiceHeaders.AddNew();
			groupToTest.JobComInvoiceHeaders.AddNew();

			AssertEquals("GroupInvoiceOfInvoiceOrGroupInvoiceItself", groupToTest, ((IGroupInvoiceOrInvoice)groupToTest).GroupInvoiceOfInvoiceOrGroupInvoiceItself);
			AssertEquals("ParentGroupInvoice", topGroup, ((IGroupInvoiceOrInvoice)groupToTest).ParentGroupInvoice);
			AssertEquals("IsGroupInvoice", true, ((IGroupInvoiceOrInvoice)groupToTest).IsGroupInvoice);
			AssertEquals("ChildGroupInvoices", 1, ((IGroupInvoiceOrInvoice)groupToTest).ChildGroupInvoices.Length);
			AssertEquals("ChildInvoices", 2, ((IGroupInvoiceOrInvoice)groupToTest).ChildInvoices.Length);

			BaseJobComInvoiceGroupHeader destinationGroupInvoice = topGroup.JobComInvoiceGroupHeaders.AddNew();
			((IGroupInvoiceOrInvoice)groupToTest).Move(topGroup, destinationGroupInvoice);
			AssertEquals("Top Group does not have Group To Test any more", false, topGroup.JobComInvoiceGroupHeaders.Contains(groupToTest.PK));
			AssertEquals("Group To Test now belongs to DestinationGroupInvoice", true, destinationGroupInvoice.JobComInvoiceGroupHeaders.Contains(groupToTest.PK));
		}

		public void TestICurrencyConverterDataProvider()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 1);

			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2005, 1, 1), ((ICurrencyConverterDataProvider)groupHeader).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.Customs, ((ICurrencyConverterDataProvider)groupHeader).RateType);
			AssertEquals("ICurrencyConverterDataProvider.MaximumDaysToFallback", BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack, ((ICurrencyConverterDataProvider)groupHeader).MaximumDaysToFallback);
			AssertNotNull("CurrencyConverter", ((ICurrencyConverterProvider)groupHeader).CurrencyConverter);
		}

		public void TestEffectiveValuationDate()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 1);

			BaseJobComInvoiceHeader inv1 = testDec.Invoices.AddNew();
			BaseJobComInvoiceHeader inv2 = testDec.Invoices.AddNew();

			AssertEquals("Valuation date", testDec.JE_ExportDate, testDec.JobComInvoiceGroupHeaders[0].EffectiveValuationDate);

			inv1.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 2);
			AssertEquals("Valuation date for group Inv1 and Inv2 have different valuation date and therefore it should use declaration's one", testDec.JE_ExportDate, testDec.JobComInvoiceGroupHeaders[0].EffectiveValuationDate);

			inv2.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 2);
			AssertEquals("Valuation date as the valuation date for two invoices are the same", new ZDateTime(2005, 1, 2), testDec.JobComInvoiceGroupHeaders[0].EffectiveValuationDate);

			inv2.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 3);
			AssertEquals("Valuation date is export date as there are two invoices with different valuation dates", testDec.JE_ExportDate, testDec.JobComInvoiceGroupHeaders[0].EffectiveValuationDate);
		}

		public void TestICommonInvoice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.JZ_InvoiceNumber = "SubGroup";

			AssertEquals("ImmediateParentInvoice for top group", null, ((ICommonInvoice)topGroup).ImmediateCommonInvoiceParent);
			AssertEquals("ImmediateParentInvoice for sub group", topGroup, ((ICommonInvoice)subGroup).ImmediateCommonInvoiceParent);
			AssertEquals("user friendly code for top group", topGroup.JZ_Calc_EffectiveInvoiceNumber, ((ICommonInvoice)topGroup).UserFriendlyCode);
			AssertEquals("user friendly code for sub group", subGroup.JZ_Calc_EffectiveInvoiceNumber, ((ICommonInvoice)subGroup).UserFriendlyCode);

			BaseGroupInvoiceCharge topGroupCharge = topGroup.Charges.AddNew();
			BaseGroupInvoiceCharge subGroupCharge = subGroup.Charges.AddNew();

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobComInvoiceGroupHeader topGroupLoaded = factory2.Load<BaseJobComInvoiceGroupHeader>(topGroup.PK);
			BaseJobComInvoiceGroupHeader subGroupLoaded = factory2.Load<BaseJobComInvoiceGroupHeader>(subGroup.PK);

			AssertEquals("ICommonInvoice.AllCharges", true, ((ICommonInvoice)topGroupLoaded).AllCharges.Contains(topGroupCharge.PK));
			AssertEquals("ICommonInvoice.AllCharges", false, ((ICommonInvoice)topGroupLoaded).AllCharges.Contains(subGroupCharge.PK));
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				var testDec = BaseJobDeclaration.New(Factory);
				var topGroup = testDec.JobComInvoiceGroupHeaders[0];
				testDec.JE_GB = ZGuid.Empty;
				AssertEquals("Branch is null", "ER", (topGroup as ITypeDeciderContext).Country);

				topGroup.JZ_GB = nzBranch.PK;
				AssertEquals("Branch isn't null", "NZ", (topGroup as ITypeDeciderContext).Country);
			});
		}

		public void TestGetDefaultDistributeBy_Export()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Quantity))
			{
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				AssertEquals("QTY", (groupHeader as IChargeHolder).GetDefaultDistributeBy());
			}
		}

		public void TestGetDefaultDistributeBy_Import()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Quantity))
			{
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				AssertEquals("QTY", (groupHeader as IChargeHolder).GetDefaultDistributeBy());
			}
		}
	}
}
