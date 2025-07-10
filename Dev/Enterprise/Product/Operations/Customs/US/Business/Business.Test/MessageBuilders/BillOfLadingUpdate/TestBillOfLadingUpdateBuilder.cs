using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

//Used by test objects testing 7501 document printing
namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class TestBillOfLadingUpdateBuilder
	{
		internal TestBillOfLadingUpdateBuilder(JobDeclaration declaration, bool requestBillOfLadingResult = true)
		{
			Declaration = declaration;
			uS_RequestBillOfLadingResult = requestBillOfLadingResult;
		}
		readonly bool uS_RequestBillOfLadingResult;

		internal MQEDIMessage PopulateMessage()
		{
			string processingPort = Declaration.IsRemoteLocationFiling ? Declaration.US_PreparerDistrictPort : Declaration.ProcessingDistrictPort;

			var block = new ABIInputBlockControlGenerator(Declaration.EntryFilerCode, processingPort, Declaration.ProcessingOfficeCode);

			block.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.BillofLadingUpdate;

			block.AddMessageBlock(MakeBOLL1());

			foreach (Bill bill in Declaration.LowestBills)
			{
				bool shouldSendOnlyBillDetails = bill.ITAndSplitDetails.Count == 0 ||
									(bill.ITAndSplitDetails.GetListOfUniqueVITNumbers().Count == 0 &&
									!USCustomsDataRegistry.Instance.SendAllITNumbersInBOLMessage.Value);

				if (shouldSendOnlyBillDetails)
				{
					block.AddMessageBlock(MakeBOLL3(bill));
				}
				else
				{
					foreach (IBillDetails billDetails in bill.ITAndSplitDetails)
					{
						bool shouldSendITNumber = billDetails.ITNumber.StartsWith("V") ||
												USCustomsDataRegistry.Instance.SendAllITNumbersInBOLMessage.Value;
						if (shouldSendITNumber)
						{
							block.AddMessageBlock(MakeBOLL3(billDetails));
						}
					}
				}
			}

			MQEDIMessage message = block.CreateMessage<MQEDIMessage>(Declaration.Factory);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			message.EM_GB = Declaration.JE_GB;
			Declaration.Messages.Add(message);

			IMessageAttacheeInDeclaration parent = Declaration;
			new BillOfLadingUpdateMessageStatusCalculator(parent).CalculateStatus(message, ABIResponseStatus.Undefined);

			return message;
		}

		MessageBlock MakeBOLL3(IBillDetails billDetails)
		{
			BOLL3 boll3 = new BOLL3();

			boll3.InBondNumber = billDetails.ITNumber;
			boll3.IssuerCodeOfMasterBillNumber = billDetails.IssuerCodeOfMasterBillNumber;
			boll3.MasterBillNumber = billDetails.MasterBillNumber;
			boll3.IssuerCodeOfHouseBillNumber = billDetails.IssuerCodeOfHouseBillNumber;
			boll3.HouseBillNumber = billDetails.HouseBillNumber;
			boll3.SubHouseBillNumber = billDetails.SubHouseBillNumber;
			//future use
			boll3.IssuerCodeOfSubHouseBillNumber = ZString.Empty;//billDetails.IssuerCodeOfSubHouseBillNumber;
			boll3.ManifestQuantity = billDetails.PackageQuantity;
			boll3.Unit = billDetails.PackageType;

			return boll3;
		}

		MessageBlock MakeBOLL1()
		{
			BOLL1 boll1 = new BOLL1();

			boll1.DistrictPortOfEntry = Declaration.US_SchDEntry;
			boll1.EntryFilerCode = Declaration.EntryFilerCode;
			boll1.EntryNumber = Declaration.ImportEntryNumber;
			boll1.BillOfLadingProcessingResults = uS_RequestBillOfLadingResult ? "Y" : "N";
			boll1.CarrierCode = Declaration.CarrierCodeForEntrySummary;
			boll1.VoyageFlightNumber = Declaration.VoyageFlightNumber.Left(5);
			//on advice from our CBP rep
			//From: RUBENSTEIN, PHYLLIS [mailto:phyllis.rubenstein@dhs.gov] 
			//Sent: Wednesday, August 26, 2009 3:22 PM
			//To: Christina Ruszczak
			//Subject: RE: BEA-92007240
			//Once we have a date of arrival in our system (from the carrier or from CBP arriving something), we do not allow you to send in an 
			//estimated date of arrival on the bill-of-lading update.  I advise my clients to never send in the estimated date of arrival since it 
			//never gets them anywhere.  Some clients try to use the estimated date on the LN job to kick off the cargo selectivity if they used the 
			//wrong date on the EI/HI/HN transmission.  That does not work.  If, for example, they transmitted an estimated date of 9/26, the LN 
			//could be sent with the new date of 8/26 - but if the carrier has already arrived the bill, this same error will occur.  If the carrier 
			//has not arrived the bill in the port, we will accept the LN transmission, but we still won't do the cargo selectivity until sometime 
			//near the first date that was sent.  Re-transmitting the EI/HI/HN will be the only way to fix the date.
			//So, leaving the date of arrival blank on the L1 record is the best bet.
			//Phyllis
			if (Declaration.IsAir) // Exception as per CS00106333
			{
				boll1.EstimatedDateOfArrival = Declaration.US_EntryDate.Date;
			}
			boll1.LocationOfGoods = Declaration.US_US_NKLocationOfGoods;

			return boll1;
		}

		JobDeclaration Declaration { get; }
	}
}
