using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class NX5105GoodsShipment_GovernmentAgencyGoodsItemAbstractTest<TGovernmentAgencyGoodsItem, TManufacturerWrapper> : TestCaseWithFactory
		where TGovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
		where TManufacturerWrapper : PartyDetailsWrapper
	{
		#region Sequence Numeric
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_SequenceNumeric()
		{
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CEI = EntryInstruction.PK;
			invoiceLine1.JI_CL = EntryLine.PK;
			invoiceLine1.JI_LineNo = 1;
			EntryLine.CL_LineNumber = 1;
			var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "GoodItem.SequenceNumeric should be ");
		}

		#endregion
		#region Additional Documents
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_AdditionalDocuments()
		{
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CEI = EntryInstruction.PK;
			invoiceLine1.JI_CL = EntryLine.PK;
			var cusSupporting1 = invoiceLine1.PermitCusSupportingCollection.AddNew();
			cusSupporting1.CSI_ReferenceNumber = "ref1";
			cusSupporting1.CSI_LineNo = 1;
			var cusSupporting2 = invoiceLine1.PermitCusSupportingCollection.AddNew();
			cusSupporting2.CSI_ReferenceNumber = "ref2";
			cusSupporting2.CSI_LineNo = 2;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CEI = EntryInstruction.PK;
			invoiceLine2.JI_CL = EntryLine.PK;
			var cusSupporting3 = invoiceLine2.PermitCusSupportingCollection.AddNew();
			cusSupporting3.CSI_ReferenceNumber = "ref3";
			cusSupporting3.CSI_LineNo = 3;
			var cusSupporting4 = invoiceLine2.PermitCusSupportingCollection.AddNew();
			cusSupporting4.CSI_ReferenceNumber = "ref3";
			cusSupporting4.CSI_LineNo = 3;
			var cusSupporting5 = invoiceLine2.PermitCusSupportingCollection.AddNew();
			cusSupporting5.CSI_ReferenceNumber = "ref5";
			cusSupporting5.CSI_LineNo = 5;
			var cusSupporting6 = invoiceLine2.PermitCusSupportingCollection.AddNew();
			cusSupporting6.CSI_ReferenceNumber = "ref6";
			cusSupporting6.CSI_LineNo = ZShort.Zero;
			var cusSupporting7 = invoiceLine2.PermitCusSupportingCollection.AddNew();
			cusSupporting7.CSI_ReferenceNumber = "ref7";
			cusSupporting7.CSI_LineNo = 7;
			var cusSupporting8 = invoiceLine2.PermitCusSupportingCollection.AddNew();
			cusSupporting8.CSI_ReferenceNumber = "ref8";
			cusSupporting8.CSI_LineNo = 8;
			var emptyRef = invoiceLine2.PermitCusSupportingCollection.AddNew();
			emptyRef.CSI_ReferenceNumber = "";
			emptyRef.CSI_LineNo = 0;
			CombineAssertions(() =>
			{
				var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
				var firstDocument = goodItem.AdditionalDocuments.ElementAt(0);
				var secondDocument = goodItem.AdditionalDocuments.ElementAt(1);
				var thirdDocument = goodItem.AdditionalDocuments.ElementAt(2);
				var fourthDocument = goodItem.AdditionalDocuments.ElementAt(3);
				var fifthDocument = goodItem.AdditionalDocuments.ElementAt(4);
				NUnit.Framework.Assert.That(goodItem.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(5), "GoodItem.AdditionalDocuments.Count() should be ");
				NUnit.Framework.Assert.That(firstDocument.ID, NUnit.Framework.Is.EqualTo("ref1").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].ID should be ");
				NUnit.Framework.Assert.That(firstDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[0].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(secondDocument.ID, NUnit.Framework.Is.EqualTo("ref2").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].ID should be ");
				NUnit.Framework.Assert.That(secondDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(2).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[1].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(thirdDocument.ID, NUnit.Framework.Is.EqualTo("ref3").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].ID should be ");
				NUnit.Framework.Assert.That(thirdDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[2].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(fourthDocument.ID, NUnit.Framework.Is.EqualTo("ref5").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].ID should be ");
				NUnit.Framework.Assert.That(fourthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[3].SequenceNumeric should be ");
				NUnit.Framework.Assert.That(fifthDocument.ID, NUnit.Framework.Is.EqualTo("ref6").Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].ID should be ");
				NUnit.Framework.Assert.That(fifthDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "GoodItem.AdditionalDocuments[4].SequenceNumeric should be ");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_AdditionalDocumentsWithExemptionOfControllingAgenciesCusSupportings()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CEI = EntryInstruction.PK;
			invoiceLine.JI_CL = EntryLine.PK;
			var cusSupporting1 = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting1.CSI_ReferenceNumber = "ref1";
			cusSupporting1.CSI_LineNo = 1;
			var cusSupporting2 = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting2.CSI_ReferenceNumber = "ref2";
			cusSupporting2.CSI_LineNo = 2;
			var exemptionOfControllingAgenciesCusSupporting1 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting1.CSI_ReferenceNumber = "ref3";
			var exemptionOfControllingAgenciesCusSupporting2 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting2.CSI_ReferenceNumber = "ref4";
			var exemptionOfControllingAgenciesCusSupporting3 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting3.CSI_ReferenceNumber = "ref5";
			var exemptionOfControllingAgenciesCusSupporting4 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting4.CSI_ReferenceNumber = "ref6";
			CombineAssertions(() =>
			{
				var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
				var firstDocument = goodItem.AdditionalDocuments.ElementAt(0);
				var secondDocument = goodItem.AdditionalDocuments.ElementAt(1);
				var thirdDocument = goodItem.AdditionalDocuments.ElementAt(2);
				var fourthDocument = goodItem.AdditionalDocuments.ElementAt(3);
				var fifthDocument = goodItem.AdditionalDocuments.ElementAt(4);
				NUnit.Framework.Assert.That(goodItem.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(5), "GoodItem.AdditionalDocuments.Count() should be ");
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

		#endregion
		#region Commodity
		[ExpectNoExceptions]
		public virtual void TestGovernmentAgencyGoodsItem_Commodity()
		{
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.Commodity, NUnit.Framework.Is.TypeOf(typeof(NX5105GovernmentAgencyGoodsItem_Commodity)));
		}

		#endregion
		#region Goods Measure
		[ExpectNoExceptions]
		public virtual void TestGovernmentAgencyGoodsItem_GoodsMeasure()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryHeader.CH_CEI_Instruction = EntryInstruction.PK;
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.GoodsMeasure, NUnit.Framework.Is.TypeOf(typeof(NX5105GovernmentAgencyGoodsItem_GoodsMeasure)));
		}

		#endregion
		#region Origin
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_Origin()
		{
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			invoiceLine.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.Origin, NUnit.Framework.Is.TypeOf(typeof(NX5105GovernmentAgencyGoodsItem_Origin)));
			invoiceLine.JI_CountryOfOrigin = "";
			goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.Origin.CountryCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "GoodItem.Origin.CountryCode should be ");
			invoiceLine.JI_CountryOfOrigin = "IT";
			goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.Origin.CountryCode, NUnit.Framework.Is.EqualTo("IT").Using(CustomComparers.TypeComparison), "GoodItem.Origin.CountryCode should be ");
			invoiceLine.CertificateOfOriginNumber = "";
			goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.Origin.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(IAdditionalDocument)));
			invoiceLine.CertificateOfOriginNumber = "123";
			invoiceLine.CertificateOfOriginNumberItemNumber = 1234;
			NUnit.Framework.Assert.That(goodItem.Origin.AdditionalDocument.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "GoodItem.Origin.AdditionalDocument.ID should be ");
			NUnit.Framework.Assert.That(goodItem.Origin.AdditionalDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(1234).Using(CustomComparers.TypeComparison), "GoodItem.Origin.AdditionalDocument.SequenceNumeric should be ");
		}

		#endregion
		#region Previous Document
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_PreviousDocument()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_CEI_Instruction = EntryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, entryLine.RandomLine);
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.JI_PreviousEntryLineNumber = 3;
			NUnit.Framework.Assert.That(goodItem.PreviousDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			invoiceLine.JI_PreviousEntryNumber = "AA";
			invoiceLine.JI_PreviousEntryLineNumber = ZShort.Zero;
			NUnit.Framework.Assert.That(goodItem.PreviousDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			invoiceLine.JI_PreviousEntryNumber = "AA123456";
			invoiceLine.JI_PreviousEntryLineNumber = 12;
			NUnit.Framework.Assert.That(goodItem.PreviousDocument.ID, NUnit.Framework.Is.EqualTo("AA123456").Using(CustomComparers.TypeComparison), "GoodItem.PreviousDocument.ID should be ");
			NUnit.Framework.Assert.That(goodItem.PreviousDocument.LineNumeric, NUnit.Framework.Is.EqualTo(12).Using(CustomComparers.TypeComparison), "GoodItem.PreviousDocument.LineNumeric should be ");
		}

		#endregion
		#region Goods Statistical Measure
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_GoodsStatisticalMeasure()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CEI = EntryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, entryLine.RandomLine);
			invoiceLine1.JI_CustomsSecondQuantity = 0m;
			invoiceLine1.JI_CustomsSecondUnitQty = ZString.Empty;
			NUnit.Framework.Assert.That(goodItem.GoodsStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(IGoodsStatisticalMeasure)));
			invoiceLine1.JI_CustomsSecondUnitQty = "KGM";
			invoiceLine1.JI_CustomsSecondQuantity = 100m;
			NUnit.Framework.Assert.That(goodItem.GoodsStatisticalMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "GoodItem.GoodsStatisticalMeasure.TariffQuantity should be ");
			NUnit.Framework.Assert.That(goodItem.GoodsStatisticalMeasure.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("KGM").Using(CustomComparers.TypeComparison), "GoodItem.GoodsStatisticalMeasure.StatisticalUnitCode should be ");
		}

		#endregion
		#region Pre Bonded Document
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_PreBondedDocument()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_CEI_Instruction = EntryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, entryLine.RandomLine);
			invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
			invoiceLine.PreviousBondedEntryLineNumber = 3;
			NUnit.Framework.Assert.That(goodItem.PreBondedDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			invoiceLine.PreviousBondedEntryNumber = "AA";
			invoiceLine.PreviousBondedEntryLineNumber = ZShort.Zero;
			NUnit.Framework.Assert.That(goodItem.PreBondedDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			invoiceLine.PreviousBondedEntryNumber = "AA123456";
			invoiceLine.PreviousBondedEntryLineNumber = 12;
			NUnit.Framework.Assert.That(goodItem.PreBondedDocument.ID, NUnit.Framework.Is.EqualTo("AA123456").Using(CustomComparers.TypeComparison), "GoodItem.PreBondedDocument.ID should be ");
			NUnit.Framework.Assert.That(goodItem.PreBondedDocument.LineNumeric, NUnit.Framework.Is.EqualTo(12).Using(CustomComparers.TypeComparison), "GoodItem.PreBondedDocument.LineNumeric should be ");
		}

		#endregion
		[ExpectNoExceptions]
		public virtual void TestExtraInfoForClassification()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_CEI_Instruction = EntryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = EntryInstruction.PK;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(entryLine, entryLine.RandomLine) as NX5105GoodsShipment_GovernmentAgencyGoodsItem;
			invoiceLine1.JI_ExtraInfoForClassification = "A";
			invoiceLine2.JI_ExtraInfoForClassification = "B";
			NUnit.Framework.Assert.That(goodItem.ExtraInfoForClassification, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			invoiceLine1.JI_ExtraInfoForClassification = "";
			NUnit.Framework.Assert.That(goodItem.ExtraInfoForClassification, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				GetGovernmentAgencyGoodsItem(null, null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				InvoiceLine.JI_CEI = EntryInstruction.PK;
				GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			});
		}

		[ExpectNoExceptions]
		public virtual void TestGoodsLicensingStatisticalMeasure()
		{
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			InvoiceLine.JI_CustomsThirdQuantity = 999.9m;
			InvoiceLine.JI_CustomsThirdUnitQty = "KGM";
			NUnit.Framework.Assert.That(goodItem.GoodsLicensingStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(IGoodsLicensingStatisticalMeasure)));
			InvoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
			InvoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			NUnit.Framework.Assert.That(goodItem.Manufacturer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public virtual void TestCheckNotApplicableProperties()
		{
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			var item = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			TestTWCreator.AddInvoiceLineReservedFields(InvoiceLine as JobComInvoiceLine);
			NUnit.Framework.Assert.That(item.AdditionalInformations, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));
			SharedHelperTest.AssertAdditionalInformations(item.AdditionalInformations);
			NUnit.Framework.Assert.That(item.ApprovalDocument, NUnit.Framework.Is.EqualTo(default(ILPCODetail)));
			NUnit.Framework.Assert.That(item.CommoditySpecification, NUnit.Framework.Is.EqualTo(default(ICommoditySpecification)));
			NUnit.Framework.Assert.That(item.MedicalInstrument, NUnit.Framework.Is.EqualTo(default(ILPCODetail)));
			NUnit.Framework.Assert.That(item.ShippingIdentifications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IShippingIdentification>)));
			NUnit.Framework.Assert.That(item.ControlInspectionStartDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(item.ExaminationPlace, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TransportEquipments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ITransportEquipment>)));
			NUnit.Framework.Assert.That(item.AdditionalDeclaration, NUnit.Framework.Is.EqualTo(default(IAdditionalDeclaration)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			var item = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			TestTWCreator.AddInvoiceLineReservedFields(InvoiceLine as JobComInvoiceLine);
			NUnit.Framework.Assert.That(item.AdditionalInformations, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));
			SharedHelperTest.AssertAdditionalInformations(item.AdditionalInformations);
		}

		[ExpectNoExceptions]
		public void TestManufacturer()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_Address1 = "Address1";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			address.OA_CompanyNameOverride = "test name";
			address.OA_Language = Core.SharedConstants.Languages.English;
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "0123456789", Core.Constants.CountryCodes.Taiwan);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "9876543210", Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.New<JobDeclaration>();
			jobdeclaration.JE_MessageType = "IMP";
			var invoiceLine = (JobComInvoiceLine)jobdeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = jobdeclaration.CusEntryInstruction;
			invoiceLine.JI_CL = EntryLine.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var goodItemManufacturer = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine).Manufacturer;
			NUnit.Framework.Assert.That(goodItemManufacturer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));

			var manuFacturerAddress = invoiceLine.ManufacturerDocAddress;
			manuFacturerAddress.E2_OA_Address = address.PK;
			goodItemManufacturer = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine).Manufacturer;
			NUnit.Framework.Assert.That(goodItemManufacturer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			goodItemManufacturer = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine).Manufacturer;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodItemManufacturer, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
				NUnit.Framework.Assert.That(goodItemManufacturer.GetType(), NUnit.Framework.Is.EqualTo(typeof(TManufacturerWrapper)));
			});
		}

		[ExpectNoExceptions]
		public void TestPackaging()
		{
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			invoiceLine.JI_CEI = EntryInstruction.PK;
			var item = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);

			CombineAssertions(() =>
			{
				invoiceLine.JI_PackagingQTY = 123;
				invoiceLine.JI_PackagingUQ = "PKG";
				var packaging = item.Packaging;
				NUnit.Framework.Assert.That(packaging.QuantityQuantity, NUnit.Framework.Is.EqualTo(123M).Using(CustomComparers.TypeComparison), "QuantityQuantity");
				NUnit.Framework.Assert.That(packaging.TypeCode, NUnit.Framework.Is.EqualTo("PKG").Using(CustomComparers.TypeComparison), "TypeCode");

				invoiceLine.JI_PackagingQTY = ZDecimal.Zero;
				NUnit.Framework.Assert.That(item.Packaging, NUnit.Framework.Is.EqualTo(default(IPackaging)), "QTY == 0, not output Packaging - should be [null]");
			});
		}

		CusEntryLine entryLine;
		protected CusEntryLine EntryLine => entryLine ??= Factory.New<CusEntryLine>();

		CusEntryInstruction entryInstruction;
		protected CusEntryInstruction EntryInstruction => entryInstruction ??= Factory.New<CusEntryInstruction>();

		BaseJobComInvoiceLine invoiceLine;
		protected BaseJobComInvoiceLine InvoiceLine => invoiceLine ??= EntryLine.InvoiceLines.AddNew();

		protected abstract IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine);
	}
}
