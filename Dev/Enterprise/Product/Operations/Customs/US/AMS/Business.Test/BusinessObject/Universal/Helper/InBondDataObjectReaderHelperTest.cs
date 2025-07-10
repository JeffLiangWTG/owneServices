using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	sealed class InBondDataObjectReaderHelperTest : DataTransfer.Universal.Testing.InBondDataObjectReaderHelperTest<CusInBondHeader>
	{
		public void TestHandlingOfUnProcessedMoveHeaders()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.MovementHeaders.DeleteAll();
			var moveHeader1_1 = header1.MovementHeaders.AddNew();
			moveHeader1_1.BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
			var moveHeader1_2 = header1.MovementHeaders.AddNew();
			moveHeader1_2.BM_SubApplicationCode = SubApplicationCodeList.Codes.PermitToTransfer;
			var moveHeader1_3 = header1.MovementHeaders.AddNew();
			moveHeader1_3.BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
			var header2 = Factory.New<CusInBondHeader>();
			header2.MovementHeaders.DeleteAll();
			var moveHeader2_1 = header2.MovementHeaders.AddNew();
			moveHeader2_1.BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
			var moveHeader2_2 = header2.MovementHeaders.AddNew();
			moveHeader2_2.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			var header3 = Factory.New<CusInBondHeader>();
			header3.MovementHeaders.DeleteAll();
			var moveHeader3_1 = header3.MovementHeaders.AddNew();
			moveHeader3_1.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			var helper = new InBondDataObjectReaderHelper(Factory);
			helper.MarkUnprocessedExistingMovementsFor(header1);
			helper.MarkUnprocessedExistingMovementsFor(header2);
			helper.MarkProcessed(moveHeader1_2);
			helper.MarkProcessed(moveHeader2_1);
			helper.MarkProcessed(moveHeader3_1);
			foreach (var moveHeader in new[] { moveHeader1_1, moveHeader1_2, moveHeader1_3, moveHeader2_1, moveHeader2_2, moveHeader3_1 })
			{
				AssertEquals("IsDeleted", false, moveHeader.IsDeleted);
			}

			var logger = new TestErrorLogger();
			helper.DeleteUnprocessedMovementsFor(header3, logger);
			foreach (var moveHeader in new[] { moveHeader1_1, moveHeader1_2, moveHeader1_3, moveHeader2_1, moveHeader2_2, moveHeader3_1 })
			{
				AssertEquals("IsDeleted", false, moveHeader.IsDeleted);
			}

			AssertEquals("logger.Logs", string.Empty, logger.Logs);
			helper.DeleteUnprocessedMovementsFor(header1, logger);
			AssertEquals("moveHeader1_1.IsDeleted", true, moveHeader1_1.IsDeleted);
			AssertEquals("moveHeader1_3.IsDeleted", true, moveHeader1_3.IsDeleted);
			foreach (var moveHeader in new[] { moveHeader1_2, moveHeader2_1, moveHeader2_2, moveHeader3_1 })
			{
				AssertEquals("IsDeleted", false, moveHeader.IsDeleted);
			}

			AssertEquals("logger.Logs", @"Information - Deleted AMS Movement Header from UniversalShipment.
Information - Deleted In-Bond Movement Header  from UniversalShipment.", logger.Logs);
			logger.ClearLogs();
			helper.DeleteUnprocessedMovementsFor(header2, logger);
			AssertEquals("moveHeader2_2.IsDeleted", true, moveHeader2_2.IsDeleted);
			foreach (var moveHeader in new[] { moveHeader1_2, moveHeader2_1, moveHeader3_1 })
			{
				AssertEquals("IsDeleted", false, moveHeader.IsDeleted);
			}

			AssertEquals("logger.Logs", "Information - Deleted In-Bond Movement Header  from UniversalShipment.", logger.Logs);
		}

		public void TestHandlingOfUnProcessedAMSBills()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			var oceanBill = header.OceanBill;
			oceanBill.B0_MasterBillNumber = "OCU111111";
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "HSB1111111";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "HSB2222222";
			Factory.SaveForTesting();

			var helper = new InBondDataObjectReaderHelper(Factory);
			helper.MarkUnprocessedExistingBillsFor(header);
			helper.MarkProcessed(bill1);
			foreach (var bill in new[] { oceanBill, bill1, bill2 })
			{
				AssertEquals("bill.IsDeleted", false, bill.IsDeleted);
			}

			var logger = new TestErrorLogger();
			helper.DeleteUnprocessedBillsFor(header, logger);
			AssertEquals("OceanBill.IsDeleted", true, oceanBill.IsDeleted);
			AssertEquals("bill1.IsDeleted", false, bill1.IsDeleted);
			AssertEquals("bill2.IsDeleted", true, bill2.IsDeleted);
		}

		public void TestThrowReadFailureExceptionWhenNonSupportedElementsFound()
		{
			var dataContextDataObject = DataContextFactory.New();
			dataContextDataObject.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContextDataObject.AddDataTarget(DataContextType.USAMS, null);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContextDataObject,
				Branch = Branch.New(GlbBranch.CurrentBranch),
			};

			var helper = new InBondDataObjectReaderHelper(Factory);
			AssertNoExceptionThrown("No exception is thrown because AllowUpdateOfCustomsDeclarationAfterCommencement flag is false in shipment", () =>
			{
				helper.ThrowReadFailureExceptionWhenNonSupportedElementsFound(shipment, elementsNotSupportedList =>
				{
					elementsNotSupportedList.Add("AABBCC");
				});
			});

			AssertExceptionThrown<DataObjectReadFailureException>("Exception is thrown when AllowUpdateOfCustomsDeclarationAfterCommencement is true", () =>
			{
				shipment.AllowUpdateOfCustomsDeclarationAfterCommencement = true;
				helper.ThrowReadFailureExceptionWhenNonSupportedElementsFound(shipment, elementsNotSupportedList =>
				{
					elementsNotSupportedList.Add("AABBCC");
				});
			});
		}

		protected override DataTransfer.Universal.InBondDataObjectReaderHelper CreateHelper(UniversalObjectFactory factory)
		{
			return new InBondDataObjectReaderHelper(factory);
		}
	}
}
