using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceGroupHeader))]
	public class BaseJobComInvoiceGroupHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeOfCharges()
		{
			AssertEquals("Charges' type should be expected", ExpectedTypeOfCharges, ((BaseJobComInvoiceGroupHeader)GetNewBusinessObject()).Charges.GetType());
		}

		public virtual void TestChargeTypeList()
		{
			var dec = GetNewDeclarationForTest();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.TopGroupInvoice;
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

		public void TestInvoiceNumberReadOnly()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			AssertEquals("ReadOnly of Invoice number for Top Group should be true", true, topGroup.JZ_InvoiceNumberInfo.ReadOnly);
			AssertEquals("Readonly of invoice number for sub group should be false", false, subGroup.JZ_InvoiceNumberInfo.ReadOnly);
		}

		public void TestIDeclarationProvider()
		{
			BaseJobDeclaration dec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = dec.JobComInvoiceGroupHeaders[0];
			AssertEquals(groupHeader.JobDeclaration, ((IDeclarationProvider)groupHeader).Declaration);
		}

		public void TestFetchStrategy()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			AssertEquals("GroupHeader.FetchStrategy type", typeof(FetchStrategies.JobComInvoiceGroupHeaderFetchStrategy), groupHeader.FetchStrategy.GetType());
		}

		public void TestJZ_Calc_EffectiveInvoiceNumber()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			AssertEquals("Effective invoice number for group", "All Invoices", groupHeader.JZ_Calc_EffectiveInvoiceNumber);

			AssertEquals("Effective invoice number for group", "All Invoices", groupHeader.JZ_Calc_EffectiveInvoiceNumber);
		}

		public void TestILandedCostDistributeTo()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			testDec.Invoices.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			testDec.FilteredInvoiceLines.AddNew();

			AssertEquals("ILandedCostDistributeTo.PK", groupHeader.PK, ((ILandedCostDistributeTo)groupHeader).PK);
			AssertEquals("ILandedCostDistributeTo.UniqueCode", BaseJobComInvoiceGroupHeader.GroupInvoiceConstString + groupHeader.JZ_InvoiceNumber, ((ILandedCostDistributeTo)groupHeader).UniqueCode);
			AssertEquals("ILandedCostDistributeTo.Description", BaseJobComInvoiceGroupHeader.GroupInvoiceConstString + groupHeader.JZ_InvoiceNumber, ((ILandedCostDistributeTo)groupHeader).Description);
			AssertEquals("ILandedCostDistributeTo.TableCode", JobComInvoiceHeaderSchema.Constants.Prefix, ((ILandedCostDistributeTo)groupHeader).TableCode);
			AssertEquals("ILandedCostDistributeTo.UltimateDistributee", 2, new List<IUltimateDistributee>(((ILandedCostDistributeTo)groupHeader).UltimateDistributees).Count);
		}

		public virtual void TestChargesToImportForLandedCosting()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				BaseJobComInvoiceHeader fOBInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
				fOBInvoice.JZ_IncoTerm = "FOB";
				fOBInvoice.JZ_InvoiceAmount = 10000m;
				fOBInvoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				var groupCharges = groupHeader.Charges;
				groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);
				groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, testDec.LocalCurrencyCode);
				BaseJobComInvHeaderCharge groupCommission = groupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, testDec.LocalCurrencyCode);
				testDec.ResumeApportionment();
				AssertEquals("FOB Invoice has three charges apportioned", 3, fOBInvoice.GroupCharges.Count);
				AssertEquals("First row is OFT", CustomsChargeTypeList.Codes.OverseasFreight, fOBInvoice.GroupCharges[0].J7_ChargeType);
				AssertEquals("Apportioned OFT not included in lines", false, fOBInvoice.GroupCharges[0].J7_IsIncludedInITOT);

				AssertEquals("Second row is DED", CustomsChargeTypeList.Codes.DeductionCharge, fOBInvoice.GroupCharges[1].J7_ChargeType);
				AssertEquals("Apportioned DED included in lines", false, fOBInvoice.GroupCharges[1].J7_IsIncludedInITOT);

				groupCommission.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();

				IDefaultLandedCostInput[] result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
				AssertEquals("two charges in Result", 2, result.Length);
				AssertEquals("OFT", true, result[0].ChargeDescription.Contains(OFTChargeDescription));
				AssertEquals("DED should be brought", true, result[1].ChargeDescription.Contains("Deduction (or Discount) from Entry"));
				AssertEquals("DED should be brought as a negative amount", -100m, result[1].AmountToDistribute.Amount);
				AssertEquals("OTH charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(CustomsChargeTypeList.Descriptions.Commission.ToString().ToUpper()));
			}
		}

		protected virtual ZString OFTChargeDescription => CustomsChargeTypeList.Descriptions.OverseasFreight.ToString().ToUpper();

		public void TestChargesToImportForLandedCostingWhenNoInvoicesExist()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);
			groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, testDec.LocalCurrencyCode);

			IDefaultLandedCostInput[] result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			testDec.ResumeApportionment();
			AssertEquals("No Invoices", 0, groupHeader.AllJobComInvoiceHeaders.Count);
			AssertEquals("No charge in Result", 0, result.Length);
		}

		public void TestChargesToImportForLandedCostingWhenApportionmentIsDirty()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader fOBInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fOBInvoice.JZ_IncoTerm = "FOB";
			fOBInvoice.JZ_InvoiceAmount = 10000m;
			fOBInvoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);
			AssertEquals("Apportionment is dirty", true, testDec.ApportionmentDirty);

			IDefaultLandedCostInput[] result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			AssertEquals("No charge in Result as it is dirty", 0, result.Length);
		}

		public void TestMakeNonPersistent()
		{
			BaseJobComInvoiceGroupHeader header = Factory.New<BaseJobComInvoiceGroupHeader>();
			header.HasChanges = true;
			Assert("Should be saved", header.IsSavedByFactory);
			header.MakeNonPersistent();
			Assert("Should no longer be saved", !header.IsSavedByFactory);
		}

		public void TestMarkApportionmentDirtyOnChargesChanges()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseGroupInvoiceCharge charge = groupHeader.Charges.AddNew();
			testDec.ApportionmentDirty = false;

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("Apportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyOnGroupHeaderDeleted()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			testDec.ApportionmentDirty = false;

			topGroupHeader.JobComInvoiceGroupHeaders.RemoveAndDelete(subGroupHeader);
			AssertEquals("Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		protected virtual BaseJobDeclaration GetNewDeclarationForTest() => BaseJobDeclaration.New(Factory);

		public void TestZeroValueApportion()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];

				var charge = topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 300, testDec.LocalCurrencyCode);
				var charge2 = topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300, testDec.LocalCurrencyCode);
				if (DistributeByShouldBeChangedForApportion)
				{
					charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
					charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				ChargeCodeChargeKey oFTKey = topGroup.Charges[0].ChargeKey;
				ChargeCodeChargeKey oNSKey = topGroup.Charges[1].ChargeKey;

				BaseJobComInvoiceHeader invoice = topGroup.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceAmount = 2000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				testDec.ResumeApportionment();
				AssertEquals("PreCondition: OFT apportioned", 300m, invoice.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("PreCondition: OSN apportioned", 300m, invoice.GroupCharges.GetCharge(oNSKey).Amount);

				topGroup.Charges[0].J7_Amount = 0;
				testDec.ResumeApportionment();
				AssertEquals("OFT apportioned", 0m, invoice.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("OSN apportioned", 300m, invoice.GroupCharges.GetCharge(oNSKey).Amount);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteGroupHeadder()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.JZ_InvoiceNumber = "GroupInvoice1";

			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_Calc_GroupInvoice = subGroup.JZ_InvoiceNumber;
			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_Calc_GroupInvoice = subGroup.JZ_InvoiceNumber;

			AssertEquals("PreCondition: FK set", subGroup.PK, invoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("PreCondition: FK set", subGroup.PK, invoice2.JZ_JZ_GroupInvoiceFK);
			subGroup.Delete();

			AssertEquals("Invoices Deleted", 0, testDec.Invoices.Count);
		}

		/// <summary>Initial set-up
		/// Declaration --	GroupHeader
		///						|----		Invoice1
		///						|----		Invoice2
		///						|----		GroupHeader2
		///										|---		Invoice3
		///										|---		Invoice4
		/// Changed
		/// Declaration --	GroupHeader
		///						|----		Invoice1
		///						|----		Invoice2
		///						|----		Invoice3
		///						|----		GroupHeader2
		///										|---		Invoice4
		/// 
		/// </summary>
		public void TestJZ_Calc_GroupInvoice()
		{
			BaseJobDeclaration declaration = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			groupHeader2.JZ_InvoiceNumber = "GroupInv2";

			BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

			AssertEquals("PreCondition:Group Invoice number", "All Invoices", invoice1.JZ_Calc_GroupInvoice);
			AssertEquals("PreCondition:Group Invoice number", "All Invoices", invoice2.JZ_Calc_GroupInvoice);
			AssertEquals("PreCondition:Group Invoice number", "GroupInv2", invoice3.JZ_Calc_GroupInvoice);
			AssertEquals("PreCondition:Group Invoice number", "GroupInv2", invoice4.JZ_Calc_GroupInvoice);

			invoice3.JZ_Calc_GroupInvoice = groupHeader.JZ_InvoiceNumber;
			AssertEquals("Group Invoice number", "All Invoices", invoice1.JZ_Calc_GroupInvoice);
			AssertEquals("Group Invoice number", "All Invoices", invoice2.JZ_Calc_GroupInvoice);
			AssertEquals("Group Invoice number", "All Invoices", invoice3.JZ_Calc_GroupInvoice);
			AssertEquals("Group Invoice number", "GroupInv2", invoice4.JZ_Calc_GroupInvoice);
		}

		public void TestAllJobComInvoiceHeaders()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			AssertEquals("Group Header AllJobComInvoiceGroupHeaders Count", 0, groupHeader.AllJobComInvoiceHeaders.Count);
			groupHeader.JobComInvoiceGroupHeaders.AddNew();
			groupHeader.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceHeader headerToDelete1 = groupHeader.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("Group Header AllJobComInvoiceGroupHeaders Count", 2, groupHeader.AllJobComInvoiceHeaders.Count);
			groupHeader.JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceHeader headerToDelete2 = groupHeader.JobComInvoiceHeaders.AddNew();
			AssertEquals("Group Header AllJobComInvoiceGroupHeaders Count", 4, groupHeader.AllJobComInvoiceHeaders.Count);

			groupHeader.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Delete(headerToDelete1);
			AssertEquals("Contains", false, groupHeader.JobComInvoiceHeaders.Contains(headerToDelete1));
			AssertEquals("All JobComInvoiceHeaders shouldnt contain this invoice", false, groupHeader.AllJobComInvoiceHeaders.Contains(headerToDelete1));
			AssertEquals("Group Header AllJobComInvoiceGroupHeaders Count", 3, groupHeader.AllJobComInvoiceHeaders.Count);

			headerToDelete2.JZ_JE = ZGuid.Empty;
			AssertEquals("Contains", false, groupHeader.JobComInvoiceHeaders.Contains(headerToDelete2));
			AssertEquals("All JobComInvoiceHeaders shouldnt contain this invoice", false, groupHeader.AllJobComInvoiceHeaders.Contains(headerToDelete2));
			AssertEquals("Group Header AllJobComInvoiceGroupHeaders Count", 2, groupHeader.AllJobComInvoiceHeaders.Count);

			BaseJobComInvoiceGroupHeader subGroup = groupHeader.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			subGroup.JobComInvoiceHeaders.AddNew();
			subGroup.JobComInvoiceHeaders.AddNew();
			subGroup.JobComInvoiceHeaders.AddNew();
			AssertEquals("Group Header AllJobComInvoiceHeaders Count", 5, testDec.Invoices.Count);
			AssertEquals("SubGroup AllJobComInvoiceHeader", 3, subGroup.AllJobComInvoiceHeaders.Count);
			AssertEquals("Group Header AllJobComInvoiceHeaders Count", 5, groupHeader.AllJobComInvoiceHeaders.Count);
			groupHeader.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.DeleteAll();
			AssertEquals("Group Header AllJobComInvoiceHeaders Count", 2, testDec.Invoices.Count);
		}

		public void TestAllJobComInvoiceHeadersReadOnly()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			testDec.SetReadOnlyIncludingChildren(true);
			AssertEquals("Collection should be read only when declaration is read only", true, testDec.Invoices.ReadOnly);

			testDec = BaseJobDeclaration.New(Factory);
			testDec.SetReadOnlyIncludingChildren(false);
			AssertEquals("Collection should not be read only when declaration is not read only", false, testDec.Invoices.ReadOnly);
		}

		public void TestValidateGroupChargesWhenInvoicesAreAdded()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			var charge = groupHeader.Charges.AddNew();
			charge.J7_ChargeType = charge.Lookups.ChargeTypeList[0].Code;
			charge.J7_Amount = 1000;
			charge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			AssertEquals("No Error until One Invoice is Added", false, groupHeader.Charges[0].J7_ChargeTypeInfo.HasMessageErrors());
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			groupHeader.Charges[0].RunPreSaveValidation();
			AssertEquals("Should be an error now that it's apportioned", true, groupHeader.Charges[0].J7_ChargeTypeInfo.HasMessageErrors());
			invoice.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			AssertEquals("GroupHeader now has an invoice", 1, groupHeader.AllJobComInvoiceHeaders.Count);

			groupHeader.Charges[0].RunPreSaveValidation();
			AssertEquals("this group charge is to be apportioned", false, groupHeader.Charges[0].J7_ChargeTypeInfo.HasMessageErrors());
		}

		/// <summary>
		/// GroupHeader1 -----  Invoice1
		///					|	Invoice2
		///					|	GroupInvoice2
		///							|-----------Invoice3
		/// </summary>
		public void TestApportion()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

				BaseJobComInvoiceGroupHeader groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvHeaderCharge oFT = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, testDec.LocalCurrencyCode);

				if (DistributeByShouldBeChangedForApportion)
				{
					oFT.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				ChargeCodeChargeKey oFTKey = oFT.ChargeKey;

				BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvoiceHeader invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
				invoice1.JZ_InvoiceAmount = 1000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				BaseJobComInvoiceHeader invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
				invoice2.JZ_InvoiceAmount = 1000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
				invoice3.JZ_InvoiceAmount = 1000m;
				invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				testDec.ResumeApportionment();
				AssertEquals("Apportioned OFT", 333.3m, decimal.Round(invoice1.GroupCharges.GetCharge(oFTKey).Amount, 1));
				AssertEquals("Apportioned OFT", 333.3m, decimal.Round(invoice2.GroupCharges.GetCharge(oFTKey).Amount, 1));
				AssertEquals("Apportioned OFT", 333.3m, decimal.Round(invoice3.GroupCharges.GetCharge(oFTKey).Amount, 1));

				invoice3.JZ_InvoiceAmount = 3000m;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned OFT", 200m, invoice1.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned OFT", 200m, invoice2.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned OFT", 600m, invoice3.GroupCharges.GetCharge(oFTKey).Amount);
			}
		}

		/// <summary>
		/// GroupHeader1 ------		Invoice1
		///					|--		Invoice2
		///					|--		GroupHeader2 --- Invoice3
		///										 |-- Invoice4
		/// </summary>
		public void TestApportionWhenSubGroupHasItsOwnCharge()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

				BaseJobComInvoiceGroupHeader groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

				BaseJobComInvoiceHeader invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

				BaseJobComInvHeaderCharge charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, testDec.LocalCurrencyCode);
				var charge2 = groupHeader2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 600, testDec.LocalCurrencyCode);

				if (DistributeByShouldBeChangedForApportion)
				{
					charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
					charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				ChargeCodeChargeKey oFTKey = charge.ChargeKey;

				invoice1.JZ_InvoiceAmount = 1000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2.JZ_InvoiceAmount = 1000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice3.JZ_InvoiceAmount = 1000m;
				invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice4.JZ_InvoiceAmount = 1000m;
				invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned Freight for invoice1", 200m, invoice1.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice2", 200m, invoice2.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice3", 300m, invoice3.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice4", 300m, invoice4.GroupCharges.GetCharge(oFTKey).Amount);
			}
		}

		public void TestApportionWhenSubGroupDoesntHaveCharge()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

				BaseJobComInvoiceGroupHeader groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

				BaseJobComInvoiceHeader invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

				BaseJobComInvHeaderCharge charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, testDec.LocalCurrencyCode);
				ChargeCodeChargeKey oFTKey = charge.ChargeKey;

				if (DistributeByShouldBeChangedForApportion)
				{
					charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				invoice1.JZ_InvoiceAmount = 1000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2.JZ_InvoiceAmount = 1000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice3.JZ_InvoiceAmount = 1000m;
				invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice4.JZ_InvoiceAmount = 1000m;
				invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned Freight for invoice1", 250m, invoice1.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice2", 250m, invoice2.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice3", 250m, invoice3.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice4", 250m, invoice4.GroupCharges.GetCharge(oFTKey).Amount);
			}
		}

		public void TestApportionWhenSubGroupComesToHaveCharge()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

				BaseJobComInvoiceGroupHeader groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

				BaseJobComInvoiceHeader invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

				BaseJobComInvHeaderCharge charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, testDec.LocalCurrencyCode);
				ChargeCodeChargeKey oFTKey = charge.ChargeKey;

				if (DistributeByShouldBeChangedForApportion)
				{
					charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				invoice1.JZ_InvoiceAmount = 1000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2.JZ_InvoiceAmount = 1000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice3.JZ_InvoiceAmount = 1000m;
				invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice4.JZ_InvoiceAmount = 1000m;
				invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				var charge2 = groupHeader2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 600, testDec.LocalCurrencyCode);

				if (DistributeByShouldBeChangedForApportion)
				{
					charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}
				testDec.ResumeApportionment();
				AssertEquals("Apportioned Freight for invoice1", 200m, invoice1.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice2", 200m, invoice2.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice3", 300m, invoice3.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice4", 300m, invoice4.GroupCharges.GetCharge(oFTKey).Amount);
			}
		}

		public void TestApportionWhenSubGroupChargeGetsCleared()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				testDec.AutoCreateChargesBasedOnIncoTerm = false;
				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

				BaseJobComInvoiceGroupHeader groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

				BaseJobComInvoiceHeader invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
				BaseJobComInvoiceHeader invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

				var charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, testDec.LocalCurrencyCode);
				var charge2 = groupHeader2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 600, testDec.LocalCurrencyCode);
				ChargeCodeChargeKey oFTKey = groupHeader1.Charges[0].ChargeKey;

				if (DistributeByShouldBeChangedForApportion)
				{
					charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
					charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				invoice1.JZ_InvoiceAmount = 1000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2.JZ_InvoiceAmount = 1000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice3.JZ_InvoiceAmount = 1000m;
				invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice4.JZ_InvoiceAmount = 1000m;
				invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				testDec.ResumeApportionment();

				AssertEquals("PreCondition:Apportioned Freight for invoice1", 200m, invoice1.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("PreCondition:Apportioned Freight for invoice2", 200m, invoice2.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("PreCondition:Apportioned Freight for invoice3", 300m, invoice3.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("PreCondition:Apportioned Freight for invoice4", 300m, invoice4.GroupCharges.GetCharge(oFTKey).Amount);

				groupHeader2.Charges[0].Delete();
				testDec.ResumeApportionment();
				AssertEquals("Apportioned Freight for invoice1", 250m, invoice1.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice2", 250m, invoice2.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice3", 250m, invoice3.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Freight for invoice4", 250m, invoice4.GroupCharges.GetCharge(oFTKey).Amount);
			}
		}

		/// <summary>															GroupCharge		Apportioned
		/// GroupInvoice1	----------------------------------------------------	2000
		///		|------		Invoice1																250
		///		|------		Invoice2																250
		///		|------		GroupInvoice2	------------------------------------		0									
		///						|---		Invoice3												250
		///						|---		Invoice4												250
		///						|---		GroupInvoice3-----------------------	  1000	
		///										|---			Invoice5							500
		///										|---			Invoice6							500
		///									
		/// </summary>
		public void TestApportionWithInvoicesWithoutSubGroupCharges()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = GetNewDeclarationForTest();
				BaseJobComInvoiceGroupHeader groupHeader1 = testDec.JobComInvoiceGroupHeaders[0];
				//groupHeader1.JZ_InvoiceNumber = "GroupInvoice1";
				BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();
				groupHeader2.JZ_InvoiceNumber = "GroupInvoice2";
				BaseJobComInvoiceGroupHeader groupHeader3 = groupHeader2.JobComInvoiceGroupHeaders.AddNew();
				groupHeader3.JZ_InvoiceNumber = "GroupInvoice3";
				BaseJobComInvoiceHeader invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
				invoice1.JZ_InvoiceNumber = "INV1";
				BaseJobComInvoiceHeader invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
				invoice2.JZ_InvoiceNumber = "INV2";
				BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
				invoice3.JZ_InvoiceNumber = "INV3";
				BaseJobComInvoiceHeader invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();
				invoice4.JZ_InvoiceNumber = "INV4";
				BaseJobComInvoiceHeader invoice5 = groupHeader3.JobComInvoiceHeaders.AddNew();
				invoice5.JZ_InvoiceNumber = "INV5";
				BaseJobComInvoiceHeader invoice6 = groupHeader3.JobComInvoiceHeaders.AddNew();
				invoice6.JZ_InvoiceNumber = "INV6";

				invoice1.JZ_InvoiceAmount = 1000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2.JZ_InvoiceAmount = 1000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice3.JZ_InvoiceAmount = 1000m;
				invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice4.JZ_InvoiceAmount = 1000m;
				invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice5.JZ_InvoiceAmount = 1000m;
				invoice5.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice6.JZ_InvoiceAmount = 1000m;
				invoice6.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				BaseJobComInvHeaderCharge charge1 = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 2000m, testDec.LocalCurrencyCode);
				BaseJobComInvHeaderCharge charge2 = groupHeader3.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);
				ChargeCodeChargeKey oFTKey = charge1.ChargeKey;

				if (DistributeByShouldBeChangedForApportion)
				{
					charge1.J7_DistributeBy = DistributedByForApportionDefaultValue;
					charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				testDec.ResumeApportionment();
				AssertEquals("Apportioned Charges", 250m, invoice1.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Charges", 250m, invoice2.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Charges", 250m, invoice3.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Charges", 250m, invoice4.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Charges", 500m, invoice5.GroupCharges.GetCharge(oFTKey).Amount);
				AssertEquals("Apportioned Charges", 500m, invoice6.GroupCharges.GetCharge(oFTKey).Amount);
			}
		}

		public void TestIsInvoicePartOfThisGroup()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup1 = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceGroupHeader subGroup2 = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice = subGroup1.JobComInvoiceHeaders.AddNew();

			AssertEquals("Invoice belongs to SubGroup1", true, subGroup1.IsThisInvoicePartOfThisGroup(invoice));
			AssertEquals("Invoice does not belong to SubGroup2", false, subGroup2.IsThisInvoicePartOfThisGroup(invoice));
			AssertEquals("Invoice belongs to TopGroup", true, topGroupHeader.IsThisInvoicePartOfThisGroup(invoice));
		}

		public void TestICommonInvoiceInvoiceLines()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceGroupHeader subGroup1 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice1 = subGroup1.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();

			BaseJobComInvoiceGroupHeader subGroup2 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup2.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals("ICommonInvoice.InvoiceLines SubGroup1", true, ((ICommonInvoice)subGroup1).InvoiceLines.Contains(line1));
			AssertEquals("ICommonInvoice.InvoiceLines SubGroup2", false, ((ICommonInvoice)subGroup1).InvoiceLines.Contains(line2));

			AssertEquals("ICommonInvoice.InvoiceLines SubGroup1", false, ((ICommonInvoice)subGroup2).InvoiceLines.Contains(line1));
			AssertEquals("ICommonInvoice.InvoiceLines SubGroup2", true, ((ICommonInvoice)subGroup2).InvoiceLines.Contains(line2));

			AssertEquals("ICommonInvoice.InvoiceLines SubGroup1", true, ((ICommonInvoice)topGroup).InvoiceLines.Contains(line1));
			AssertEquals("ICommonInvoice.InvoiceLines SubGroup2", true, ((ICommonInvoice)topGroup).InvoiceLines.Contains(line2));
		}

		public void TestICommonInvoice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();

			AssertEquals("ICommonInvoice.ImmediateCommonInvoiceParent of SubGroup", topGroup, ((ICommonInvoice)subGroup).ImmediateCommonInvoiceParent);
			AssertEquals("ICommonInvoice.ImmediateCommonInvoiceParent of TopGroup", null, ((ICommonInvoice)topGroup).ImmediateCommonInvoiceParent);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			if (GetType() == typeof(BaseJobComInvoiceGroupHeaderTest))
			{
				Assert($"Covered by {nameof(FetchStrategies.Testing.JobComInvoiceGroupHeaderFetchStrategyTest)}.", true);
				return;
			}

			base.TestCalcPropertiesWithDbHitsUseFetchHints();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader newObject = declaration.JobComInvoiceGroupHeaders[0];
			return newObject;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(factory);
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.JobComInvoiceHeaders.AddNew();
			var charge = groupHeader.Charges.AddNew();

			if (DistributeByShouldBeChangedForApportion)
			{
				charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}

			return groupHeader;
		}

		protected virtual Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<BaseGroupInvoiceCharge>);

		protected virtual ZString DistributedByForApportionDefaultValue => ZString.Empty;
		protected virtual ZBool DistributeByShouldBeChangedForApportion => false;

		#endregion
	}
}
