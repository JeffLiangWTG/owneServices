using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class CommodityAbstractTests<TCommodity> : TestCaseWithFactory
		where TCommodity : ICommodity
	{
		[ExpectNoExceptions]
		public virtual void TestAdditionalDocuments()
		{
			var cusSupporting1 = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting1.CSI_ReferenceNumber = "ref1";
			cusSupporting1.CSI_LineNo = 1;
			var cusSupporting2 = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting2.CSI_ReferenceNumber = "ref2";
			cusSupporting2.CSI_LineNo = 2;
			var cusSupportingDuplicate = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupportingDuplicate.CSI_ReferenceNumber = "ref2";
			cusSupportingDuplicate.CSI_LineNo = 2;
			var exemptionOfControllingAgenciesCusSupporting1 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting1.CSI_ReferenceNumber = "ref3";
			var exemptionOfControllingAgenciesCusSupporting2 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting2.CSI_ReferenceNumber = "ref4";
			var exemptionOfControllingAgenciesCusSupporting3 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting3.CSI_ReferenceNumber = "ref5";
			var exemptionOfControllingAgenciesCusSupporting4 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting4.CSI_ReferenceNumber = "ref6";
			var exemptionOfControllingAgenciesCusSupportingDuplicate = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupportingDuplicate.CSI_ReferenceNumber = "ref3";
			CombineAssertions(() =>
			{
				var additionalDocuments = Commodity.AdditionalDocuments;
				var firstDocument = additionalDocuments.ElementAt(0);
				var secondDocument = additionalDocuments.ElementAt(1);
				var thirdDocument = additionalDocuments.ElementAt(2);
				var fourthDocument = additionalDocuments.ElementAt(3);
				var fifthDocument = additionalDocuments.ElementAt(4);
				NUnit.Framework.Assert.That(additionalDocuments.Count(), NUnit.Framework.Is.EqualTo(5), "GoodItem.AdditionalDocuments.Count() should be ");
				NUnit.Framework.Assert.That(firstDocument.ID, NUnit.Framework.Is.EqualTo("ref1").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].ID should be ");
				NUnit.Framework.Assert.That(firstDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(secondDocument.ID, NUnit.Framework.Is.EqualTo("ref2").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].ID should be ");
				NUnit.Framework.Assert.That(secondDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(2).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(thirdDocument.ID, NUnit.Framework.Is.EqualTo("ref3").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].ID should be ");
				NUnit.Framework.Assert.That(thirdDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(fourthDocument.ID, NUnit.Framework.Is.EqualTo("ref4").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].ID should be ");
				NUnit.Framework.Assert.That(fourthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(fifthDocument.ID, NUnit.Framework.Is.EqualTo("ref5").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].ID should be ");
				NUnit.Framework.Assert.That(fifthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].SequenceNumeric should be ");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocumentsOnlyPermits()
		{
			for (ZInt i = 1; i <= 8; i++)
			{
				var permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
				permitCusSupporting.CSI_ReferenceNumber = "ref" + i;
				permitCusSupporting.CSI_LineNo = ZShort.Parse(i.ToString());
			}

			var additionalDocuments = Commodity.AdditionalDocuments;
			var firstDocument = additionalDocuments.ElementAt(0);
			var secondDocument = additionalDocuments.ElementAt(1);
			var thirdDocument = additionalDocuments.ElementAt(2);
			var fourthDocument = additionalDocuments.ElementAt(3);
			var fifthDocument = additionalDocuments.ElementAt(4);
			NUnit.Framework.Assert.That(additionalDocuments.Count(), NUnit.Framework.Is.EqualTo(5), "GoodItem.AdditionalDocuments.Count() should be ");
			NUnit.Framework.Assert.That(firstDocument.ID, NUnit.Framework.Is.EqualTo("ref1").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].ID should be ");
			NUnit.Framework.Assert.That(firstDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(secondDocument.ID, NUnit.Framework.Is.EqualTo("ref2").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].ID should be ");
			NUnit.Framework.Assert.That(secondDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(2).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(thirdDocument.ID, NUnit.Framework.Is.EqualTo("ref3").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].ID should be ");
			NUnit.Framework.Assert.That(thirdDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(fourthDocument.ID, NUnit.Framework.Is.EqualTo("ref4").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].ID should be ");
			NUnit.Framework.Assert.That(fourthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(4).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(fifthDocument.ID, NUnit.Framework.Is.EqualTo("ref5").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].ID should be ");
			NUnit.Framework.Assert.That(fifthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].SequenceNumeric should be ");
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocumentsOnlyExemptionOfControllingAgencies()
		{
			for (int i = 1; i <= 8; i++)
			{
				var exemptionOfControllingAgenciesCusSupporting = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
				exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumber = "ref" + i;
				exemptionOfControllingAgenciesCusSupporting.CSI_LineNo = ZShort.Parse(i.ToString());
			}

			var additionalDocuments = Commodity.AdditionalDocuments;
			var firstDocument = additionalDocuments.ElementAt(0);
			var secondDocument = additionalDocuments.ElementAt(1);
			var thirdDocument = additionalDocuments.ElementAt(2);
			var fourthDocument = additionalDocuments.ElementAt(3);
			var fifthDocument = additionalDocuments.ElementAt(4);
			NUnit.Framework.Assert.That(additionalDocuments.Count(), NUnit.Framework.Is.EqualTo(5), "GoodItem.AdditionalDocuments.Count() should be ");
			NUnit.Framework.Assert.That(firstDocument.ID, NUnit.Framework.Is.EqualTo("ref1").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].ID should be ");
			NUnit.Framework.Assert.That(firstDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(secondDocument.ID, NUnit.Framework.Is.EqualTo("ref2").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].ID should be ");
			NUnit.Framework.Assert.That(secondDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(thirdDocument.ID, NUnit.Framework.Is.EqualTo("ref3").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].ID should be ");
			NUnit.Framework.Assert.That(thirdDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(fourthDocument.ID, NUnit.Framework.Is.EqualTo("ref4").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].ID should be ");
			NUnit.Framework.Assert.That(fourthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].SequenceNumeric should be ");
			NUnit.Framework.Assert.That(fifthDocument.ID, NUnit.Framework.Is.EqualTo("ref5").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].ID should be ");
			NUnit.Framework.Assert.That(fifthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].SequenceNumeric should be ");
		}

		[ExpectNoExceptions]
		public void TestCommercialCategorizationID()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(Commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo(invoiceLine.JI_Model));
				invoiceLine.JI_Model = "NO.008 ";
				NUnit.Framework.Assert.That(Commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public virtual void TestDescription()
		{
			invoiceLine.JI_Group = "  Short grouping  ";
			invoiceLine.JI_DeclarationGoodsDescription = "  TW_Dec  ";
			var entryLine = invoiceLine.CusEntryLine;
			entryLine.CL_Description = ZString.Empty;
			NUnit.Framework.Assert.That(Commodity.Description, NUnit.Framework.Is.EqualTo(@"Short grouping

  TW_Dec").Using(CustomComparers.TypeComparison));
			entryLine.CL_Description = "  CL_Des  ";
			NUnit.Framework.Assert.That(Commodity.Description, NUnit.Framework.Is.EqualTo(@"Short grouping

  CL_Des").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsGroupNameCode()
		{
			NUnit.Framework.Assert.That(Commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestBarCode()
		{
			NUnit.Framework.Assert.That(Commodity.BarCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestChineseDescription()
		{
			NUnit.Framework.Assert.That(Commodity.ChineseDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestEnglishDescription()
		{
			NUnit.Framework.Assert.That(Commodity.EnglishDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCITESImportPermitID()
		{
			NUnit.Framework.Assert.That(Commodity.CITESImportPermitID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestFTATariffCode()
		{
			NUnit.Framework.Assert.That(Commodity.FTATariffCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestSHTCImportPermitID()
		{
			NUnit.Framework.Assert.That(Commodity.SHTCImportPermitID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTariffCodeExtensionCode()
		{
			NUnit.Framework.Assert.That(Commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestClassifications()
		{
			invoiceLine.JI_HazMatCode = "1100";
			invoiceLine.UNDGs.AddNew().DI_DG = MasterFiles.Business.UNDGSubstanceLoader.LoadSubstances(Factory, "1100", "", "IMO").First().PK;
			invoiceLine.Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(Commodity.Classifications.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestCommodityRelatedPackaging()
		{
			NUnit.Framework.Assert.That(Commodity.CommodityRelatedPackaging, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ICommodityRelatedPackaging)));
		}

		[ExpectNoExceptions]
		public void TestConstituent()
		{
			NUnit.Framework.Assert.That(Commodity.Constituent.GetType(), NUnit.Framework.Is.EqualTo(typeof(Constituent)));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee()
		{
			NUnit.Framework.Assert.That(Commodity.DutyTaxFee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ICommodityDutyTaxFee)));
		}

		[ExpectNoExceptions]
		public void TestGovernmentProcedure()
		{
			NUnit.Framework.Assert.That(Commodity.GovernmentProcedure, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IGovernmentProcedure)));
		}

		[ExpectNoExceptions]
		public void TestHandlingInstructionsCodes()
		{
			NUnit.Framework.Assert.That(Commodity.HandlingInstructionsCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)));
		}

		[ExpectNoExceptions]
		public virtual void TestInvoiceLine()
		{
			NUnit.Framework.Assert.That(Commodity.InvoiceLine.GetType(), NUnit.Framework.Is.EqualTo(typeof(CommodityInvoiceLine)));
		}

		[ExpectNoExceptions]
		public void TestPreviousDocument()
		{
			NUnit.Framework.Assert.That(Commodity.PreviousDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPreviousDocument)));
		}

		[ExpectNoExceptions]
		public void TestCommodityNumbers()
		{
			invoiceLine.JI_CustomsOwnerPartNo = "ABF1";
			invoiceLine.JI_CustomsSupplierPartNo = "XX12X";
			var commodityNumbers = Commodity.CommodityNumbers;
			NUnit.Framework.Assert.That(commodityNumbers.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(commodityNumbers.Any(x => x.ID == invoiceLine.JI_CustomsOwnerPartNo && x.IdentifierTypeCode == "BP"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(commodityNumbers.Any(x => x.ID == invoiceLine.JI_CustomsSupplierPartNo && x.IdentifierTypeCode == "SA"), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestDutyOtherTaxFees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateCusRateType("TW", "DTY");
			rateType.ZZR_CustomsValueFormula = "CV";
			var comRateType = helper.CreateCusRateType("TW", "COM");
			comRateType.ZZR_CustomsValueFormula = "CV + DTA + DTS";
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "DTS", rateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "CTA", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "CTS", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "TAT", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "HWS", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "TPF", comRateType.PK);
			Factory.Save();
			var fees = EntryLine.Fees;
			var fee1 = fees.AddNew();
			fee1.CF_Rate = 0.00001M;
			fee1.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
			fee1.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			fee1.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			var fee2 = fees.AddNew();
			fee2.CF_Rate = 0.00001M;
			fee2.CF_ChargeType = "XXX";
			fee2.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			fee2.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			var dutyOtherTaxFees = Commodity.DutyOtherTaxFees;
			NUnit.Framework.Assert.That(dutyOtherTaxFees.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.TypeCode == "XXX"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IDutyOtherTaxFee)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.TypeCode == "B52" && x.MethodCode == "1" && x.TaxRateNumeric == 0.00001M), NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IDutyOtherTaxFee)));
			fee1.CF_Rate = 0M;
			dutyOtherTaxFees = Commodity.DutyOtherTaxFees;
			NUnit.Framework.Assert.That(dutyOtherTaxFees.Count(), NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeAmount()
		{
			NUnit.Framework.Assert.That(Commodity.DutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IDutyTaxFeeAmount)));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeQuantity()
		{
			NUnit.Framework.Assert.That(Commodity.DutyTaxFeeQuantity, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IDutyTaxFeeQuantity)));
		}

		[ExpectNoExceptions]
		public void TestFood()
		{
			NUnit.Framework.Assert.That(Commodity.Food, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IFood)));
		}

		[ExpectNoExceptions]
		public void TestQuarantine()
		{
			NUnit.Framework.Assert.That(Commodity.Quarantine, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IQuarantine)));
		}

		[ExpectNoExceptions]
		public void TestVehicle()
		{
			NUnit.Framework.Assert.That(Commodity.Vehicle, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IVehicle)));
		}

		[ExpectNoExceptions]
		public void TestWine()
		{
			NUnit.Framework.Assert.That(Commodity.Wine, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IWine)));
		}

		[ExpectNoExceptions]
		public void TestCargoDescription()
		{
			NUnit.Framework.Assert.That(Commodity.CargoDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestBondedNoteCode()
		{
			invoiceLine.JI_BondedGoodsCode = "01";
			NUnit.Framework.Assert.That(Commodity.BondedNoteCode, NUnit.Framework.Is.EqualTo("01").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestVehicleIDs()
		{
			var chassisJobComInvLineRef = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassisJobComInvLineRef.JG_ReferenceNumber = "T200";
			var vehicleIDs = Commodity.VehicleIDs;
			NUnit.Framework.Assert.That(vehicleIDs.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(vehicleIDs.Any(x => x == "T200"), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_BrandName = "BR212";
				NUnit.Framework.Assert.That(Commodity.Name, NUnit.Framework.Is.EqualTo("BR212").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_BrandName = ZString.Empty;
				NUnit.Framework.Assert.That(Commodity.Name, NUnit.Framework.Is.EqualTo("NO BRAND").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_BrandName = "NO.008 ";
				NUnit.Framework.Assert.That(Commodity.Name, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestClassification()
		{
			NUnit.Framework.Assert.That(Commodity.Classification, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IClassification)));
		}

		[ExpectNoExceptions]
		public virtual void TestCommodity()
		{
			NUnit.Framework.Assert.That(Commodity.GetType(), NUnit.Framework.Is.EqualTo(typeof(Commodity)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
		}

		CusEntryHeader entryHeader;
		protected CusEntryLine EntryLine => entryHeader.MergedLines.Cast<CusEntryLine>().First();
		protected abstract ICommodity Commodity { get; }
		protected JobComInvoiceLine invoiceLine;
	}
}
