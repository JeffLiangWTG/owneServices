using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEDrawbackSummaryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildACEDrawbackSummaryMessage()
		{
			var mock = GetMock(false);
			var drawbackSummary = mock.Object;

			var mockSign = new Mock<IACEDrawbackAcknowledgeAndSign>();
			mockSign.Setup(m => m.US_AcknowledgeAndSign).Returns(true);

			var builder = new ACEDrawbackSummaryMessageBuilder(drawbackSummary, mockSign.Object);
			var message = builder.GenerateMessage();
			ZString expectedResult =
@"B  9900   DE                                20             <<MSGNO PLACEHOLDER>>
10AXJ5  70032899 1001B00000001   010998YYY3XXXXXED1235139867AB414845123 XXXXXX  
318B 012000000010012512358                                                      
40D  XJ5  70032800    1X9999999999  040810160811161111101                       
419801250030IAN TEST ACE DRAWBACK                                               
420000000001056789KG 000000000035125700000000005612010000000000956531           
43364001000000000109900000124                                                   
4336500200000000001560000031201                                                 
50D9999999991  98012501000000000010000025KG 081016HKHKG                         
51AAAAAAAAAAAAA                                     999999999222222             
5211111                                                                         
60E98010100010000000052012360KG 081016YYABCDEFG                       USDABCD   
61BCDFEEEEE                                         81236412310                 
623901XW.D.C U.S.                    D                                          
63PAAAAAAA                                 123425123   1816502146082016         
6470032800            08101600001200120001011125980101000198010100029801010003US
8936400000010001  36500000010002  36900000010003  39800000010004                
8939900000010005                                                                
9000000100098 00000010025 00000800054                                           
Y  9900   DE                                20";
			AssertMultilineASCIIEquals("ACE Drawback Message Text for Non-TFTEA claim", expectedResult, message.EM_FormattedMessageText);

			mock = GetMock(true);
			drawbackSummary = mock.Object;

			mockSign = new Mock<IACEDrawbackAcknowledgeAndSign>();
			mockSign.Setup(m => m.US_AcknowledgeAndSign).Returns(true);

			builder = new ACEDrawbackSummaryMessageBuilder(drawbackSummary, mockSign.Object);
			message = builder.GenerateMessage();
			expectedResult =
@"B  9900   DE                                20             <<MSGNO PLACEHOLDER>>
10AXJ5  70032899 1001B00000001   010998YYY3XXXXXED1235139867AB414845123 XXXXXX  
318B 012000000010012512358                                                      
40D  XJ5  70032800    1X9999999999  040810160811161111101                       
419801250030IAN TEST ACE DRAWBACK                                               
420000000001056789KG 000000000035125700000000005612010000000000956531           
43364001000000000109900000124                                                   
4336500200000000001560000031201                                                 
50D9999999991  98012501000000000010000025KG 081016HKHKG                         
51AAAAAAAAAAAAA                                     999999999222222             
5211111                                                                         
623901XW.D.C U.S.                    D                                          
63PAAAAAAA                                 123425123   1816502146082016         
6470032800            08101600001200120001011125980101000198010100029801010003US
70E98010100010000000052012360KG 081016YYABCDEFG                       USDABCDX  
71BCDFEEEEE                                         81236412310                 
7233333                                                                         
7344444                                                                         
8936400000010001  36500000010002  36900000010003  39800000010004                
8939900000010005                                                                
9000000100098 00000010025 00000800054                                           
Y  9900   DE                                20";
			AssertMultilineASCIIEquals("ACE Drawback Message Text for TFTEA claim", expectedResult, message.EM_FormattedMessageText);
		}

		Mock<IACEDrawbackSummary> GetMock(bool tfteaRequired)
		{
			var mock = new Mock<IACEDrawbackSummary>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.IsTFTEARequired).Returns(tfteaRequired);
			//B-Record
			mock.Setup(m => m.ProcessingOfficeCode).Returns("20");
			mock.Setup(m => m.ProcessingPort).Returns("9900");
			//10-Record
			mock.Setup(m => m.ActionRequestCode).Returns("A");
			mock.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mock.Setup(m => m.EntryNumber).Returns("70032899");
			mock.Setup(m => m.ClaimPort).Returns("1001");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("B00000001");
			mock.Setup(m => m.ClaimType).Returns("01");
			mock.Setup(m => m.BondWaiverIndicator).Returns("0");
			mock.Setup(m => m.BondWaiverReasonCode).Returns("998");
			mock.Setup(m => m.AcceleratedClaimIndicator).Returns("Y");
			mock.Setup(m => m.OneTimeWaiverIndicator).Returns("Y");
			mock.Setup(m => m.OneTimeWaiverIndicator).Returns("Y");
			mock.Setup(m => m.WavierOfPriorNoticeIndicator).Returns("Y");
			mock.Setup(m => m.CommercialInterchangeability).Returns("3");
			mock.Setup(m => m.ElectronicPetroleumCertification).Returns("X");
			mock.Setup(m => m.ElectronicManufacturingPetroleumCertification).Returns("X");
			mock.Setup(m => m.OilSpillTaxCertification).Returns("X");
			mock.Setup(m => m.NAFTADrawbackClaimIndicator).Returns("X");
			mock.Setup(m => m.USMCADrawbackClaimIndicator).Returns("X");
			mock.Setup(m => m.ImporterOfRecordNumber).Returns("ED1235139867");
			mock.Setup(m => m.NotifyParty4811Number).Returns("AB414845123");
			mock.Setup(m => m.SubstitutedUnusedWineCertification).Returns("X");
			mock.Setup(m => m.BillOfMaterialsFormulaCertification).Returns("X");
			mock.Setup(m => m.CertificationForValuationOfDestroyedMerchandise).Returns("X");
			mock.Setup(m => m.RetailSalesSubstitution).Returns("X");
			mock.Setup(m => m.SuperfundTaxCertification).Returns("X");

			//31-Record
			var bondInfoList = new List<IACEDrawbackBondInfo>();
			var mockSingleBond = new Mock<IACEDrawbackBondInfo>();
			mockSingleBond.Setup(m => m.BondType).Returns("8");
			mockSingleBond.Setup(m => m.BondDesignationTypeCode).Returns("B");
			mockSingleBond.Setup(m => m.SuretyCode).Returns("012");
			mockSingleBond.Setup(m => m.BondAmount).Returns(100m);
			mockSingleBond.Setup(m => m.ProducerAccountNumber).Returns("12512358");
			bondInfoList.Add(mockSingleBond.Object);
			mock.Setup(m => m.BondDetails).Returns(bondInfoList);

			//40-Record
			var importEntrySummaryList = new List<IACEDrawbackImportClaim>();
			var mockImportEntrySummary = new Mock<IACEDrawbackImportClaim>();
			mockImportEntrySummary.Setup(m => m.ActionIndicator).Returns("D");
			mockImportEntrySummary.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mockImportEntrySummary.Setup(m => m.EntryNumber).Returns("70032800");
			mockImportEntrySummary.Setup(m => m.CBPESLine).Returns("1");
			mockImportEntrySummary.Setup(m => m.CertificateofDeliveryIndicator).Returns("X");
			mockImportEntrySummary.Setup(m => m.ManufacturerRulingNumber).Returns("9999999999");
			mockImportEntrySummary.Setup(m => m.BasisOfClaim).Returns("04");
			mockImportEntrySummary.Setup(m => m.ManufDateReceived).Returns(new ZDateTime(2016, 08, 10));
			mockImportEntrySummary.Setup(m => m.ManufDateUsed).Returns(new ZDateTime(2016, 08, 11));
			mockImportEntrySummary.Setup(m => m.TrackingIdentificationNumber).Returns("11111");
			mockImportEntrySummary.Setup(m => m.DrawbackAccountingMethodCode).Returns("01");
			//41-Record
			var importClassificationList = new List<IACEDrawbackImportClassification>();
			var mockImportClassification0 = new Mock<IACEDrawbackImportClassification>();
			mockImportClassification0.Setup(m => m.HTSNumber).Returns("9801250030");
			mockImportClassification0.Setup(m => m.DescriptionText).Returns("IAN TEST ACE DRAWBACK");
			//42-Record
			var mockExportQuantityAndUQ0 = new Mock<IACEDrawbackExportQuantityAndUnit>();
			mockExportQuantityAndUQ0.Setup(m => m.Quantity).Returns(105.6789m);
			mockExportQuantityAndUQ0.Setup(m => m.UnitOfMeasure).Returns("KG");
			mockExportQuantityAndUQ0.Setup(m => m.AllowableQuantity).Returns(35.1257m);
			mockExportQuantityAndUQ0.Setup(m => m.GoodsValuePerUnit).Returns(56.1201m);
			mockExportQuantityAndUQ0.Setup(m => m.SubstitutedValuePerUnit).Returns(95.6531m);
			mockImportClassification0.Setup(m => m.ExportQuantityAndUnit).Returns(mockExportQuantityAndUQ0.Object);
			importClassificationList.Add(mockImportClassification0.Object);
			mockImportEntrySummary.Setup(m => m.ImportClassifications).Returns(importClassificationList);

			//43-Record
			var revenueAmountList = new List<IACEDrawbackRevenueClaimed>();
			var mockRevenue0Amount = new Mock<IACEDrawbackRevenueClaimed>();
			mockRevenue0Amount.Setup(m => m.AccountingClassCode).Returns(DrawbackOtherFeeTypesList.Codes.DrawbackDuty);
			mockRevenue0Amount.Setup(m => m.ClaimAmount).Returns(1000m);
			mockRevenue0Amount.Setup(m => m.CalculatedAmount).Returns(10.99m);
			mockRevenue0Amount.Setup(m => m.AdjustedClaimAmount).Returns(1.24m);
			mockRevenue0Amount.Setup(m => m.QualifierIndicator).Returns(ZString.Empty);
			revenueAmountList.Add(mockRevenue0Amount.Object);
			var mockRevenue1Amount = new Mock<IACEDrawbackRevenueClaimed>();
			mockRevenue1Amount.Setup(m => m.AccountingClassCode).Returns(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes);
			mockRevenue1Amount.Setup(m => m.ClaimAmount).Returns(2000m);
			mockRevenue1Amount.Setup(m => m.CalculatedAmount).Returns(1.56m);
			mockRevenue1Amount.Setup(m => m.AdjustedClaimAmount).Returns(3.12m);
			mockRevenue1Amount.Setup(m => m.QualifierIndicator).Returns("01");
			revenueAmountList.Add(mockRevenue1Amount.Object);
			mockImportEntrySummary.Setup(m => m.RevenueAmounts).Returns(revenueAmountList);
			importEntrySummaryList.Add(mockImportEntrySummary.Object);
			mock.Setup(m => m.ImportsEntrySummaryDetails).Returns(importEntrySummaryList);

			//50-Record
			var manufactureList = new List<IACEDrawbackManufactureClaim>();
			var mockManufacture = new Mock<IACEDrawbackManufactureClaim>();
			mockManufacture.Setup(m => m.ActionIndicator).Returns("D");
			mockManufacture.Setup(m => m.ImportManufactureRulingNumber).Returns("9999999991");
			mockManufacture.Setup(m => m.HTSNumber).Returns("9801250100");
			mockManufacture.Setup(m => m.Quantity).Returns(1000.0025m);
			mockManufacture.Setup(m => m.UnitOfMeasure).Returns("KG");
			mockManufacture.Setup(m => m.ProductionDate).Returns(new ZDateTime(2016, 08, 10));
			mockManufacture.Setup(m => m.FactoryLocation).Returns("HKHKG");
			//51-Record
			mockManufacture.Setup(m => m.DescriptionText).Returns("AAAAAAAAAAAAA");
			mockManufacture.Setup(m => m.ManufactureRulingNumber).Returns("9999999992");
			mockManufacture.Setup(m => m.ImportTrackingID).Returns("11111");
			mockManufacture.Setup(m => m.ManufacturedTrackingID).Returns("22222");
			manufactureList.Add(mockManufacture.Object);
			mock.Setup(m => m.ManufacturedArticles).Returns(manufactureList);

			//60-Record
			var exportClaimList = new List<IACEDrawbackExportClaim>();
			var mockExportClaim = new Mock<IACEDrawbackExportClaim>();
			mockExportClaim.Setup(m => m.ExportDestroyIndicator).Returns("E");
			mockExportClaim.Setup(m => m.HTSNumber).Returns("9801010001");
			mockExportClaim.Setup(m => m.Quantity).Returns(5201.2360m);
			mockExportClaim.Setup(m => m.UnitOfMeasure).Returns("KG");
			mockExportClaim.Setup(m => m.ExportDate).Returns(new ZDateTime(2016, 08, 10));
			mockExportClaim.Setup(m => m.NoticeOfIntentIndicator).Returns("Y");
			mockExportClaim.Setup(m => m.WaiverToDrawbackIndicator).Returns("Y");
			mockExportClaim.Setup(m => m.NameOfExporter).Returns("ABCDEFG");
			mockExportClaim.Setup(m => m.CountryOfUltimateDestination).Returns("US");
			mockExportClaim.Setup(m => m.BOLIndicator).Returns("D");
			mockExportClaim.Setup(m => m.BOLCarrierCode).Returns("ABCD");
			//61-Record
			mockExportClaim.Setup(m => m.DescriptionText).Returns("BCDFEEEEE");
			mockExportClaim.Setup(m => m.UniqueIdentifierNumber).Returns("81236412310");
			exportClaimList.Add(mockExportClaim.Object);
			mock.Setup(m => m.ExportArticles).Returns(exportClaimList);

			//62-Record
			mock.Setup(m => m.IntendedPort).Returns("3901");
			mock.Setup(m => m.ExaminationWitnessIndicator).Returns("X");
			mock.Setup(m => m.LocationOfDestruction).Returns("W.D.C U.S.");
			mock.Setup(m => m.ResultsOfExamination).Returns("D");

			//63-Record
			var noticeOfIntentList = new List<IACEDrawbackNoticeOfIntent>();
			var mockNoticeOfIntent = new Mock<IACEDrawbackNoticeOfIntent>();
			mockNoticeOfIntent.Setup(m => m.RecordIndicator).Returns("P");
			mockNoticeOfIntent.Setup(m => m.NameOfCBPPersonnel).Returns("AAAAAAA");
			mockNoticeOfIntent.Setup(m => m.CBPPersonnelBadge).Returns("123425123");
			mockNoticeOfIntent.Setup(m => m.CBPPersonnelPhone).Returns("1816502146");
			mockNoticeOfIntent.Setup(m => m.ProcessingExaminAtionDate).Returns(new ZDateTime(2016, 08, 20));
			noticeOfIntentList.Add(mockNoticeOfIntent.Object);
			mock.Setup(m => m.NoticeOfIntentDetais).Returns(noticeOfIntentList);

			//64-Record
			var nafatTariffList = new List<IACEDrawbackNAFATTariff>();
			var mockNTFAT = new Mock<IACEDrawbackNAFATTariff>();
			mockNTFAT.Setup(m => m.EntryNumber).Returns("70032800");
			mockNTFAT.Setup(m => m.EntryDate).Returns(new ZDateTime(2016, 08, 10));
			mockNTFAT.Setup(m => m.DutyPaidToForeignGov).Returns(1200.12m);
			mockNTFAT.Setup(m => m.ExchangeRate).Returns(1.011125);
			mockNTFAT.Setup(m => m.TariffNumber1).Returns("9801010001");
			mockNTFAT.Setup(m => m.TariffNumber2).Returns("9801010002");
			mockNTFAT.Setup(m => m.TariffNumber3).Returns("9801010003");
			mockNTFAT.Setup(m => m.CountryOfExport).Returns("US");
			nafatTariffList.Add(mockNTFAT.Object);
			mock.Setup(m => m.NAFTADetails).Returns(nafatTariffList);

			//70-Record
			var tfteaClaimList = new List<IACEDrawbackTFTEAClaim>();
			var mockTFTEAClaim = new Mock<IACEDrawbackTFTEAClaim>();
			mockTFTEAClaim.Setup(m => m.ExportDestroyIndicator).Returns("E");
			mockTFTEAClaim.Setup(m => m.HTSNumber).Returns("9801010001");
			mockTFTEAClaim.Setup(m => m.Quantity).Returns(5201.2360m);
			mockTFTEAClaim.Setup(m => m.UnitOfMeasure).Returns("KG");
			mockTFTEAClaim.Setup(m => m.ExportDate).Returns(new ZDateTime(2016, 08, 10));
			mockTFTEAClaim.Setup(m => m.NoticeOfIntentIndicator).Returns("Y");
			mockTFTEAClaim.Setup(m => m.WaiverToDrawbackIndicator).Returns("Y");
			mockTFTEAClaim.Setup(m => m.NameOfExporter).Returns("ABCDEFG");
			mockTFTEAClaim.Setup(m => m.CountryOfUltimateDestination).Returns("US");
			mockTFTEAClaim.Setup(m => m.BOLIndicator).Returns("D");
			mockTFTEAClaim.Setup(m => m.BOLCarrierCode).Returns("ABCD");
			mockTFTEAClaim.Setup(m => m.ScheduleBCode).Returns("X");
			//71-Record
			mockTFTEAClaim.Setup(m => m.DescriptionText).Returns("BCDFEEEEE");
			mockTFTEAClaim.Setup(m => m.UniqueIdentifierNumber).Returns("81236412310");
			mockTFTEAClaim.Setup(m => m.ImportTrackingNumber).Returns("33333");
			mockTFTEAClaim.Setup(m => m.ManufacturedTrackingNumber).Returns("44444");
			tfteaClaimList.Add(mockTFTEAClaim.Object);
			mock.Setup(m => m.TFTEADetails).Returns(tfteaClaimList);

			//89-Record
			var revenueTotalList = new List<IACEDrawbackRevenueTotals>();
			var mockRevenue0 = new Mock<IACEDrawbackRevenueTotals>();
			mockRevenue0.Setup(m => m.AccountingClassCode).Returns(DrawbackOtherFeeTypesList.Codes.DrawbackDuty);
			mockRevenue0.Setup(m => m.TotalAmount).Returns(100.01m);
			revenueTotalList.Add(mockRevenue0.Object);
			var mockRevenue1 = new Mock<IACEDrawbackRevenueTotals>();
			mockRevenue1.Setup(m => m.AccountingClassCode).Returns(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes);
			mockRevenue1.Setup(m => m.TotalAmount).Returns(100.02m);
			revenueTotalList.Add(mockRevenue1.Object);
			var mockRevenue2 = new Mock<IACEDrawbackRevenueTotals>();
			mockRevenue2.Setup(m => m.AccountingClassCode).Returns(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty);
			mockRevenue2.Setup(m => m.TotalAmount).Returns(100.03m);
			revenueTotalList.Add(mockRevenue2.Object);
			var mockRevenue3 = new Mock<IACEDrawbackRevenueTotals>();
			mockRevenue3.Setup(m => m.AccountingClassCode).Returns(DrawbackOtherFeeTypesList.Codes.DrawbackHMF);
			mockRevenue3.Setup(m => m.TotalAmount).Returns(100.04m);
			revenueTotalList.Add(mockRevenue3.Object);
			var mockRevenue4 = new Mock<IACEDrawbackRevenueTotals>();
			mockRevenue4.Setup(m => m.AccountingClassCode).Returns(DrawbackOtherFeeTypesList.Codes.DrawbackMPF);
			mockRevenue4.Setup(m => m.TotalAmount).Returns(100.05m);
			revenueTotalList.Add(mockRevenue4.Object);
			mock.Setup(m => m.RevenueTotals).Returns(revenueTotalList);

			//90-Record
			mock.Setup(m => m.GrandTotalDuty).Returns(1000.98m);
			mock.Setup(m => m.GrandTotalUserFee).Returns(100.25m);
			mock.Setup(m => m.GrandTotalIRTax).Returns(8000.54m);

			var trackingNumberLines = new List<IACEDrawbackTrackingNumberLine>();
			mock.Setup(m => m.TrackingNumberLines).Returns(trackingNumberLines);

			return mock;
		}
	}
}
