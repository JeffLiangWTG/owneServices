using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEEntrySummaryMessageBuilder : EntryHeaderMessageBuilder<ACEInputBlockControlGenerator>
	{
		public ACEEntrySummaryMessageBuilder(IACECusEntryHeader entryHeader, ISEAdditionalData signed, UpdateActionCode action)
			: this(entryHeader, signed, ZString.Empty, null, action)
		{
		}

		public ACEEntrySummaryMessageBuilder(IACECusEntryHeader entryHeader, ISEAdditionalData signed, ZString pscExplanationText, IPSCReasonCodeProvider pscReasonCodeProvider, UpdateActionCode action)
			: base(entryHeader, action)
		{
			this.actionCode = action;
			this.signed = signed;
			this.pscExplanationText = pscExplanationText;
			this.pscReasonCodeProvider = pscReasonCodeProvider;
		}

		readonly ISEAdditionalData signed;
		readonly UpdateActionCode actionCode;
		readonly ZString pscExplanationText;
		readonly IPSCReasonCodeProvider pscReasonCodeProvider;

		new IACECusEntryHeader entryHeader
		{
			get { return (IACECusEntryHeader)base.entryHeader; }
		}

		protected override void UpdateMessageBlocks(ACEInputBlockControlGenerator message)
		{
			message.B.SetupBlockBDetails(entryHeader);
			message.MessageBlocks.AddRange(new ACEEntrySummaryBlockBuilder(actionCode, entryHeader, pscExplanationText, pscReasonCodeProvider, signed, true).Build());
		}

		#region Implementation

		protected override string ApplicationIdentifier
		{
			get { return ACEApplicationIdentifierCodeList.Codes.EntrySummary; }
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			switch (action)
			{
				case UpdateActionCode.Delete:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
					break;
				case UpdateActionCode.Replace:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
					break;
				default:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
					break;
			}
		}

		protected override ACEInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			return new ACEInputBlockControlGenerator(entryHeader);
		}

		protected override List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			List<UpdateActionCode> result = base.GetSupportedUpdateActionCodeList();

			result.Add(UpdateActionCode.Add);
			result.Add(UpdateActionCode.Replace);
			result.Add(UpdateActionCode.Delete);

			return result;
		}

		#endregion
	}

	internal class ACEEntrySummaryBlockBuilder
	{
		public ACEEntrySummaryBlockBuilder(UpdateActionCode actionCode, IACECusEntryHeader entryHeader, ZString pscExplanationText, IPSCReasonCodeProvider pscReasonCodeProvider, ISEAdditionalData signed, bool enablePGATracking = false)
		{
			this.actionCode = actionCode;
			this.entryHeader = entryHeader;
			this.pscExplanationText = pscExplanationText;
			this.pscReasonCodeProvider = pscReasonCodeProvider;
			this.signed = signed;
			this.zeroAmountFees = new List<ZString>();
			this.aceCargoReleaseBlockBuilder = new SimplifiedEntryBlockBuilder((IACECargoReleaseHeader)entryHeader, null);
			this.enablePGATracking = enablePGATracking;
		}
		readonly UpdateActionCode actionCode;
		readonly IACECusEntryHeader entryHeader;
		readonly ZString pscExplanationText;
		readonly IPSCReasonCodeProvider pscReasonCodeProvider;
		readonly ISEAdditionalData signed;
		readonly List<ZString> zeroAmountFees;
		readonly SimplifiedEntryBlockBuilder aceCargoReleaseBlockBuilder;
		readonly bool enablePGATracking;

		bool IsSE13Required
		{
			get { return (actionCode == UpdateActionCode.Add || actionCode == UpdateActionCode.Replace) && SignedAndCertifiedForCargoRelease; }
		}

		bool SignedAndCertifiedForCargoRelease
		{
			get { return signed.US_CertifyCargoRelease && entryHeader.IsACECargoReleaseCertification; }
		}

		internal IEnumerable<MessageBlock> Build()
		{
			if (actionCode == UpdateActionCode.Delete)
			{
				var lastMsgBlock = entryHeader.LastENSAcceptedMessageBlock;
				if (lastMsgBlock != null)
				{
					EntryHeaderBlock.Add(GenerateENS10FromAcceptedMessage(lastMsgBlock));
				}
				else
				{
					EntryHeaderBlock.Add(MakeENS10());
				}
			}
			else
			{
				EntryHeaderBlock.Add(MakeENS10());
				EntryHeaderBlock.Add(MakeENS11());
				EntryHeaderBlock.Add(MakeENS20());

				if (IsSE13Required && signed != null)
				{
					EntryHeaderBlock.Add(MakeSE13());
				}

				if (!entryHeader.VoyageNumber.IsEmpty)
				{
					EntryHeaderBlock.Add(MakeENS21());
				}

				EntryHeaderBlock.AddRange(BuildCargoManifestDetails());

				if (!entryHeader.WarehouseEntryNumber.IsEmpty)
				{
					EntryHeaderBlock.Add(MakeENS30());
				}

				EntryHeaderBlock.AddRange(BuildBondDetails());

				if (!entryHeader.IsPSC)
				{
					EntryHeaderBlock.AddRange(BuildConsolidatedEntryDetails());
				}

				if (!entryHeader.MissingDocumentCodes.IsEmpty)
				{
					EntryHeaderBlock.Add(MakeENS33());
				}

				EntryHeaderBlock.AddRange(BuildEntryFees());

				if (entryHeader.IsPSC)
				{
					var ens35 = MakeENS35();

					if (ens35 != null)
					{
						EntryHeaderBlock.Add(ens35);
					}

					EntryHeaderBlock.AddRange(MakeENS36s());
				}

				if (SignedAndCertifiedForCargoRelease)
				{
					EntryHeaderBlock.AddRange(GenerateSE20Blocks());

					//Sold To Party (BY) is Mandatory, we have validation on Invoice Line level. We do not submit BY here because we submit it in block 47 ('S - Sold To Party')
					//If the 47-Record in the AE, Article Party Code 'C' (Delivered To Party), and/or 'S' (Sold To Party) is present in the ACE Entry Summary transaction,
					//these parties will be automatically converted by the certify from summary to ACE Cargo Release code for the commercial entities Ship To Party and Buyer respectively.
					// if we will submit it here, Entry Summary message will be accepted, but ACE Cargo Release will be rejected with error "Duplicate Party reported", since each entity code may be reported one time only.
					var entities = ((IACECargoReleaseHeader)entryHeader).Entities.Where(entity =>
						entity.EntityCode == EntityCodeList.Codes.SellingParty ||
						entity.EntityCode == EntityCodeList.Codes.ShipToParty ||
						entity.EntityCode == EntityCodeList.Codes.Exporter ||
						entity.EntityCode == EntityCodeList.Codes.Shipper ||
						entity.EntityCode == EntityCodeList.Codes.Distributor ||
						entity.EntityCode == EntityCodeList.Codes.Packager);

					EntryHeaderBlock.AddRange(aceCargoReleaseBlockBuilder.GenerateHeaderEntityBlocks(entities));
				}

				var shouldSendUltimateConsignee = ShouldSendUltimateConsigneeOnLineLevel();

				bool aD_CVDExist = false;
				foreach (IACECusEntryLine entryline in entryHeader.EntryLines)
				{
					aD_CVDExist |= !entryline.CountervailingCaseNumber.IsEmpty || !entryline.AntidumpingCaseNumber.IsEmpty;
					EntryHeaderBlock.AddRange(BuildBlocksForLine(entryline, shouldSendUltimateConsignee));
				}

				EntryHeaderBlock.AddRange(BuildTotalBlocks(aD_CVDExist));
			}

			return EntryHeaderBlock;
		}

		AENS10 GenerateENS10FromAcceptedMessage(AENS10 lastAcceptedMessage, UpdateActionCode action = UpdateActionCode.Delete)
		{
			var block10 = new AENS10();

			if (lastAcceptedMessage != null)
			{
				block10.SummaryFilingActionRequestCode = UpdateActionCodeConverter.ConvertToString(action);
				block10.EntryFilerCode = lastAcceptedMessage.EntryFilerCode;
				block10.EntryNumber = lastAcceptedMessage.EntryNumber;

				block10.DistrictPortOfEntry = lastAcceptedMessage.DistrictPortOfEntry;
				block10.EntryTypeCode = lastAcceptedMessage.EntryTypeCode;
				block10.KnownImporterIndicator = lastAcceptedMessage.KnownImporterIndicator;
				block10.AcceleratedLiquidationRequestIndicator = lastAcceptedMessage.AcceleratedLiquidationRequestIndicator;
				block10.PostSummaryCorrectionIndicator = lastAcceptedMessage.PostSummaryCorrectionIndicator;
			}
			return block10;
		}

		bool ShouldSendUltimateConsigneeOnLineLevel()
		{
			var ultimateConsigneeNumber = entryHeader.UltimateConsigneeNumber;
			return entryHeader.EntryLines.OfType<ISimplifiedEntryLine>().Any(line => line.Entities.Any(x => x.EntityCode == EntityCodeList.Codes.Consignee && x.EntityIdentifier != ultimateConsigneeNumber));
		}

		#region Header Message Block Building

		MessageBlock MakeENS10()
		{
			var result = new AENS10();

			result.SummaryFilingActionRequestCode = actionCode.ConvertToString();
			result.EntryFilerCode = entryHeader.EntryFilerCode;
			result.EntryNumber = MQEDIMessage.USEntryNumberPlaceHolder;
			result.DistrictPortOfEntry = entryHeader.DistrictPortOfEntry;
			result.EntryTypeCode = entryHeader.EntryType;
			result.ElectronicSignature = signed.US_AcknowledgeAndSign ? "X" : "";
			result.TIBDeclarationIndicator = signed.CertifyTIB ? "Y" : "";

			if (actionCode != UpdateActionCode.Delete)
			{
				result.BrokerReferenceNumber = entryHeader.BrokerReferenceNumber;

				result.ModeOfTransportationMOTCode = entryHeader.ModeOfTransportationCode;
				result.BondWaiverIndicator = entryHeader.BondWaivedOrNoBond ? "0" : "";
				result.PGAExpeditedReleaseIndicator = signed.US_CertifyCargoRelease ? ZString.Empty : entryHeader.PGAExpeditedReleaseIndicator;

				if (!entryHeader.IsPSC)
				{
					result.CargoReleaseCertificationRequestIndicator = signed.US_CertifyCargoRelease ? (entryHeader.IsACECargoReleaseCertification ? "A" : "Y") : "";
					result.ConsolidatedSummaryIndicator = entryHeader.Consolidated ? "Y" : "";
					result.LiveEntryIndicator = entryHeader.LiveEntry ? "Y" : "";
					result.TradeAgreementReconciliationIndicator = entryHeader.NAFTAReconciliation ? "Y" : "";
					result.ReconciliationIssueCode = entryHeader.OtherReconciliationIndicator;
				}

				result.ElectronicInvoiceIndicator = entryHeader.IsElectronicInvoicing ? "Y" : "";

				ZString indicator = entryHeader.ConsolidatedInformalIndicator;
				if (indicator == ConsolidatedInformalList.Codes.Personal || indicator == ConsolidatedInformalList.Codes.Samples)
				{
					result.ShipmentUsageTypeCode = indicator;
				}

				indicator = entryHeader.DeferredTaxIndicator;
				if (indicator == TaxDeferIndicatorList.Codes.DeferredTax || indicator == TaxDeferIndicatorList.Codes.DeferredTaxWithEFT)
				{
					result.DeferredTaxPaymentCode = indicator;
				}

				if (!entryHeader.IsPaid && !entryHeader.IsPSC)
				{
					result.PaymentTypeCode = entryHeader.PaymentTypeIndicator;
					result.PreliminaryStatementPrintDate = entryHeader.PreliminaryStatementPrintDate;
					result.PeriodicStatementMonth = entryHeader.PeriodicStatementMonth;
					result.StatementClientBranchIdentifier = entryHeader.ClientBranchDesignation;
				}
				result.BondWaiverReasonCode = entryHeader.BondWaiverReasonCode;
			}

			if (entryHeader.IsPSC)
			{
				result.PostSummaryCorrectionIndicator = "Y";
				result.AcceleratedLiquidationRequestIndicator = entryHeader.AcceleratedLiqReqIndicator ? "Y" : "";
			}
			result.KnownImporterIndicator = entryHeader.KnownImporterIndicator ? "Y" : "";
			return result;
		}

		MessageBlock MakeENS11()
		{
			AENS11 result = new AENS11();

			result.ImporterOfRecordNumber = entryHeader.ImporterOfRecordNumber;
			result.ConsigneeNumber = entryHeader.UltimateConsigneeNumber;
			result.DesignatedNotifyParty4811Number = entryHeader.NotifyPartyNumber;
			result.EstimatedEntryDate = entryHeader.EstimatedEntryDate;
			result.DateOfImportation = entryHeader.DateOfImportation;
			result.USStateOfDestinationCode = entryHeader.StateOfDestination;

			result.NewForeignTradeZoneIdentifier = entryHeader.ImportFTZNumber;

			return result;
		}

		MessageBlock MakeSE13()
		{
			var se13 = new ASESE13();
			se13.ContactName = signed.ContactName;
			se13.ContactPhone = signed.ContactPhone;
			se13.DISIndicator = signed.DISIndicator ? "1" : string.Empty;
			return se13;
		}

		MessageBlock MakeENS20()
		{
			AENS20 result = new AENS20();

			result.CarrierCode = entryHeader.CarrierCode;
			result.DistrictPortOfUnlading = entryHeader.DistrictPortOfUnlading;
			result.EstimatedDateOfArrival = entryHeader.EstimatedDateOfArrival;

			if (!entryHeader.IsPSC)
			{
				result.LocationOfGoodsCode = entryHeader.LocationOfGoods;
			}

			result.ConveyanceName = entryHeader.ImportingVesselName.Left(20);
			result.VesselCode = ZString.Empty;// If sent, it would result in an error if certifying for cargo release & sent with name. And the error message(07I) says the vessel code is no longer used
			result.DesignatedExamPortCode = entryHeader.DesignatedExamPort;
			result.InBondInTransitDate = entryHeader.ITDate;

			return result;
		}

		MessageBlock MakeENS21()
		{
			AENS21 result = new AENS21();

			result.TripIdentifier = entryHeader.VoyageNumber;

			return result;
		}

		MessageBlock MakeENS30()
		{
			AENS30 result = new AENS30();

			result.AssociatedWarehouseEntryDistrictPortCode = entryHeader.DistrictPortCodeOfWarehouseEntry;
			result.AssociatedWarehouseEntryNumber = entryHeader.WarehouseEntryNumber;
			result.AssociatedWarehouseEntryFilerCode = entryHeader.EntryFilerCodeOfWarehouseEntry;
			result.FinalWarehouseWithdrawalIndicator = entryHeader.FinalWarehouseIndicator ? "Y" : "";

			return result;
		}

		MessageBlock MakeENS33()
		{
			AENS33 result = new AENS33();

			result.MissingDocumentCode1 = entryHeader.MissingDocumentCodes.Left(2);
			result.MissingDocumentCode2 = entryHeader.MissingDocumentCodes.SubstringSafe(2, 2);

			return result;
		}

		MessageBlock MakeENS35()
		{
			AENS35 result = null;

			if (pscReasonCodeProvider != null)
			{
				var reasons = pscReasonCodeProvider.PSCHeaderReasonCodes.ToArray();

				if (reasons.Length > 0)
				{
					result = new AENS35();

					int index = 0;

					foreach (var reason in reasons)
					{
						switch (index)
						{
							case 0:
								result.PostSummaryCorrectionHeaderReasonCode1 = reason;
								break;
							case 1:
								result.PostSummaryCorrectionHeaderReasonCode2 = reason;
								break;
							case 2:
								result.PostSummaryCorrectionHeaderReasonCode3 = reason;
								break;
							case 3:
								result.PostSummaryCorrectionHeaderReasonCode4 = reason;
								break;
							case 4:
								result.PostSummaryCorrectionHeaderReasonCode5 = reason;
								break;
						}

						index++;
					}
				}
			}

			return result;
		}

		IEnumerable<MessageBlock> MakeENS36s()
		{
			int index = 1;

			foreach (ZString text in pscExplanationText.Split(75))
			{
				var result = new AENS36();
				result.PSCFilingExplanationText = text;
				yield return result;

				index++;

				if (index > 99)
				{
					break;
				}
			}
		}

		#endregion

		#region Cargo Manifest Details

		static class CargoManifestBillType
		{
			public const string InBond = "I";
			public const string MasterBill = "M";
			public const string HouseBill = "H";
			public const string SubhouseBill = "S";
			public const string ExpressTracking = "T";
		}

		IEnumerable<MessageBlock> BuildCargoManifestDetails()
		{
			foreach (IBillDetails billDetails in entryHeader.LowestBillDetails)
			{
				if (EntryTypeList.IsCargoManifestGroupingAllowed(entryHeader.EntryType, entryHeader.ModeOfTransportationCode))
				{
					foreach (var block in GenerateBillBlocks(billDetails))
					{
						yield return block;
					}
				}
				else if (EntryTypeList.IsInbondCargoManifestGroupAllowed(entryHeader.EntryType))
				{
					foreach (var block in GenerateInbondNumberBlocks(billDetails))
					{
						yield return block;
					}
				}
			}
		}

		IEnumerable<MessageBlock> GenerateBillBlocks(IBillDetails billDetails)
		{
			yield return MakeENS22(billDetails);

			// It should maintain the order of reporting, I, M, H, S
			if (!billDetails.ITNumber.IsEmpty)
			{
				yield return MakeENS23(CargoManifestBillType.InBond, ZString.Empty, billDetails.ITNumber);
			}

			if (!billDetails.MasterBillNumber.IsEmpty)
			{
				yield return MakeENS23(!billDetails.IsExpressTracking ? CargoManifestBillType.MasterBill : CargoManifestBillType.ExpressTracking, billDetails.IssuerCodeOfMasterBillNumber, billDetails.MasterBillNumber);
			}

			if (!billDetails.HouseBillNumber.IsEmpty)
			{
				yield return MakeENS23(CargoManifestBillType.HouseBill, billDetails.IssuerCodeOfHouseBillNumber, billDetails.HouseBillNumber);
			}

			if (!billDetails.SubHouseBillNumber.IsEmpty)
			{
				// issuer code for Subhouse bill is never allowed
				yield return MakeENS23(CargoManifestBillType.SubhouseBill, ZString.Empty, billDetails.SubHouseBillNumber);
			}

			if (SignedAndCertifiedForCargoRelease)
			{
				foreach (MessageBlock block in BlockBuilderHelper.GenerateSplitOrNonAMSDetailsAndContainers(billDetails))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GenerateInbondNumberBlocks(IBillDetails billDetails)
		{
			if (!billDetails.ITNumber.IsEmpty)
			{
				yield return MakeENS22(billDetails);
				yield return MakeENS23(CargoManifestBillType.InBond, ZString.Empty, billDetails.ITNumber);
			}
		}

		MessageBlock MakeENS22(IBillDetails billDetails)
		{
			var result = new AENS22();

			result.ManifestedQuantity = billDetails.PackageQuantity;
			result.ManifestedQuantityUnitOfMeasureCode = billDetails.PackageType;

			return result;
		}

		MessageBlock MakeENS23(ZString billType, ZString issuerCode, ZString billNumber)
		{
			AENS23 result = new AENS23();

			result.ManifestComponentTypeCode = billType;
			result.ManifestComponentIssuerCode = issuerCode;
			result.ManifestComponentIdentifier = billNumber;

			return result;
		}
		#endregion

		#region Bond Details

		IEnumerable<MessageBlock> BuildBondDetails()
		{
			var bondType = entryHeader.BondType;
			if (!bondType.IsEmpty && bondType != BondTypeList.Codes.NoBondRequired)
			{
				yield return MakeENS31(entryHeader.BondType, entryHeader.DesignationCode, entryHeader.ContinuousBondSuperseded, entryHeader.SuretyCode, entryHeader.BondAmount, entryHeader.BondProducerAccountNumber);
			}

			var aDDCVDBondType = entryHeader.ADDCVDBondType;
			if (!aDDCVDBondType.IsEmpty && aDDCVDBondType == BondTypeList.Codes.SingleTransactionBond)
			{
				yield return MakeENS31(entryHeader.ADDCVDBondType, "A", false, entryHeader.ADDCVDSuretyCode, entryHeader.ADDCVDSingleTransactionBondAmount, entryHeader.ADDCVDSingleTransactionBondAccNo);
			}
		}

		MessageBlock MakeENS31(ZString bondType, ZString designation, ZBool supersedingBondIndicator, ZString suretyCode, ZDecimal singleBondAmount, ZString bondProducerAccNo)
		{
			AENS31 result = new AENS31();

			result.BondTypeCode = bondType;
			result.BondDesignationTypeCode = designation;
			result.ContinuousBondIndicator = supersedingBondIndicator ? "Y" : "";
			result.SuretyCompanyCode = suretyCode;
			result.SingleTransactionBondAmount = Math.Ceiling(singleBondAmount);
			result.SingleTransactionBondProducerAccountNumber = bondProducerAccNo;

			return result;
		}

		#endregion

		#region Release Detail

		IEnumerable<MessageBlock> BuildConsolidatedEntryDetails()
		{
			var consolidatedReleaseEntryNumbers = entryHeader.ConsolidatedReleaseEntryNumbers.ToArray();
			if (consolidatedReleaseEntryNumbers.Length > 0)
			{
				AENS32 ens32 = null;

				int index = 1;

				foreach (var entryNumber in consolidatedReleaseEntryNumbers)
				{
					if (ens32 == null)
					{
						ens32 = new AENS32();
					}

					UpdateENS32(index, ens32, entryNumber);

					index++;

					if (index == 7)
					{
						index = 1;
						yield return ens32;
						ens32 = null;
					}
				}

				if (ens32 != null)
				{
					yield return ens32;
				}
			}
		}

		void UpdateENS32(int index, AENS32 ens32, ZString entryNumber)
		{
			switch (index)
			{
				case 1:
					ens32.ReleaseEntryFilerCode1 = entryNumber.Left(3);
					ens32.ReleaseEntryNumber1 = entryNumber.SubstringSafe(3);
					break;

				case 2:
					ens32.ReleaseEntryFilerCode2 = entryNumber.Left(3);
					ens32.ReleaseEntryNumber2 = entryNumber.SubstringSafe(3);
					break;

				case 3:
					ens32.ReleaseEntryFilerCode3 = entryNumber.Left(3);
					ens32.ReleaseEntryNumber3 = entryNumber.SubstringSafe(3);
					break;

				case 4:
					ens32.ReleaseEntryFilerCode4 = entryNumber.Left(3);
					ens32.ReleaseEntryNumber4 = entryNumber.SubstringSafe(3);
					break;

				case 5:
					ens32.ReleaseEntryFilerCode5 = entryNumber.Left(3);
					ens32.ReleaseEntryNumber5 = entryNumber.SubstringSafe(3);
					break;

				case 6:
					ens32.ReleaseEntryFilerCode6 = entryNumber.Left(3);
					ens32.ReleaseEntryNumber6 = entryNumber.SubstringSafe(3);
					break;
			}
		}

		#endregion

		#region Entry Fees

		IEnumerable<MessageBlock> BuildEntryFees()
		{
			AENS34 result = null;

			var informalFee = entryHeader.InformalFee;
			if (informalFee > 0)
			{
				result = new AENS34();

				PopulateENS34(result, Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, informalFee);
			}

			var dutiableMailFee = entryHeader.DutiableMailFee;
			if (dutiableMailFee > 0)
			{
				if (result == null)
				{
					result = new AENS34();
				}

				PopulateENS34(result, Core.Constants.USCustoms.FeeCodes.DutiableMail, dutiableMailFee);
			}

			if (result != null && !result.AccountingClassCode1.IsEmpty && !result.AccountingClassCode2.IsEmpty)
			{
				yield return result;
				result = null;
			}

			var manualSurcharge = entryHeader.ManualSurcharge;
			if (manualSurcharge > 0)
			{
				if (result == null)
				{
					result = new AENS34();
				}

				PopulateENS34(result, Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, manualSurcharge);

				yield return result;
				result = null;
			}

			if (result != null)
			{
				yield return result;
			}
		}

		void PopulateENS34(AENS34 aens34, ZString feeType, ZDecimal amount)
		{
			if (aens34.AccountingClassCode1.IsEmpty)
			{
				aens34.AccountingClassCode1 = feeType;
				aens34.HeaderFeeAmount1 = amount;
			}
			else
			{
				aens34.AccountingClassCode2 = feeType;
				aens34.HeaderFeeAmount2 = amount;
			}
		}

		#endregion

		#region SE20 blocks

		IEnumerable<MessageBlock> GenerateSE20Blocks()
		{
			if (entryHeader.IsExpressConsignment)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.ExpressConsignmentShipment, "Y");
			}

			var splitShipmentReleaseCode = entryHeader.SplitShipmentReleaseCode;
			if (!splitShipmentReleaseCode.IsEmpty)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.SplitShipmentReleaseElectionCode, splitShipmentReleaseCode);
			}

			var iACECargoReleaseHeader = entryHeader as IACECargoReleaseHeader;
			var electedExamSite = iACECargoReleaseHeader.ElectedExamSite;
			if (!electedExamSite.IsEmpty)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.ElectedExamSite, electedExamSite);
			}

			var disReferenceNo = signed != null ? signed.DISIDRefNo : ZString.Empty;
			if (IsSE13Required && !disReferenceNo.IsEmpty)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.DISReferenceNumber, disReferenceNo);
			}

			if (entryHeader.IsNonAMS)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.NonAMSBillOfLading, "Y");
			}

			if (!iACECargoReleaseHeader.RailReferenceNumber.IsEmpty)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.RailReferenceNumber, iACECargoReleaseHeader.RailReferenceNumber);
			}

			var entryType = entryHeader.EntryType;
			if (entryType == EntryTypeList.Codes.Warehouse || entryType == EntryTypeList.Codes.ReWarehouse)
			{
				var currentLocation = ((IACECargoReleaseHeader)entryHeader).CurrentFirmsCodeForWarehousingEntry;
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.CurrentGoodsLocation, currentLocation);
			}

			if (!iACECargoReleaseHeader.GeneralOrderNumber.IsEmpty)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.GeneralOrderNumber, iACECargoReleaseHeader.GeneralOrderNumber);
			}

			if (entryHeader.IsPerishable)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.Perishable, "Y");
			}

			if (entryHeader.IsSelfCertification)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.SelfCertification, "Y");
			}
		}

		#endregion

		#region BuildBlocksForLine

		IEnumerable<MessageBlock> BuildBlocksForLine(IACECusEntryLine entryLine, bool shouldSendUltimateConsignee)
		{
			yield return MakeENS40(entryLine);

			if (entryHeader.EntryType == EntryTypeList.Codes.ConsumptionFTZ)
			{
				yield return MakeENS41(entryLine);
			}

			if (!entryLine.PreImportationReviewProgramRulingsNumber.IsEmpty)
			{
				yield return MakeENS43(entryLine);
			}

			foreach (MessageBlock block in BuildCommercialDescriptionBlocks(entryLine))
			{
				yield return block;
			}

			foreach (MessageBlock block in BuildArticlePartyGroupingBlocks(entryLine))
			{
				yield return block;
			}

			if (SignedAndCertifiedForCargoRelease)
			{
				//Sold To Party (BY) is Mandatory, we have validation on Invoice Line level. We do not submit BY here because we submit it in block 47 ('S - Sold To Party')
				// if we will submit it here, Entry Summary message will be accepted, but ACE Cargo Release will be rejected with error "Duplicate Party reported"
				var entities = ((ISimplifiedEntryLine)entryLine).Entities.Where(entity =>
						entity.EntityCode == EntityCodeList.Codes.SellingParty ||
						entity.EntityCode == EntityCodeList.Codes.ManufacturerSupplier ||
						shouldSendUltimateConsignee && entity.EntityCode == EntityCodeList.Codes.Consignee ||
						entity.EntityCode == EntityCodeList.Codes.ShipToParty ||
						entity.EntityCode == EntityCodeList.Codes.Exporter ||
						entity.EntityCode == EntityCodeList.Codes.Shipper ||
						entity.EntityCode == EntityCodeList.Codes.Distributor ||
						entity.EntityCode == EntityCodeList.Codes.Packager);

				foreach (MessageBlock block in aceCargoReleaseBlockBuilder.GenerateLineLevelEntityBlocks(entities))
				{
					yield return block;
				}
			}

			entryLine.ClearPGALineNumbers();

			foreach (var block in GetENS50Blocks(entryLine))
			{
				yield return block;
			}

			if (entryLine is CusEntryLine xvvEntry && xvvEntry.IsVParentLine)
			{
				foreach (var vChildEntry in xvvEntry.ChildVLines.Cast<IACECusEntryLine>())
				{
					foreach (var block in GetENS50Blocks(vChildEntry))
					{
						yield return block;
					}
				}
			}

			foreach (MessageBlock block in BuildVisaAndLicenceDetails(entryLine))
			{
				yield return block;
			}

			foreach (MessageBlock block in BuildADDCVDCaseGroupingBlocks(entryLine))
			{
				yield return block;
			}

			foreach (MessageBlock block in BuildImportersAdditionalDeclarationGroupingBlocks(entryLine))
			{
				yield return block;
			}

			foreach (MessageBlock block in BuildIRTaxAndFeesGroupingBlocks(entryLine))
			{
				yield return block;
			}

			if (SignedAndCertifiedForCargoRelease)
			{
				foreach (MessageBlock block in BlockBuilderHelper.BuildSanctionsAdditionalInfoGroupingBlocks(entryLine))
				{
					yield return block;
				}
			}

			if (entryHeader.IsPSC)
			{
				var ens63 = BuildPSCLineReason(entryLine);

				if (ens63 != null)
				{
					yield return ens63;
				}
			}

			foreach (MessageBlock block in BuildCensusWarningGroupingBlocks(entryLine))
			{
				yield return block;
			}
		}

		IEnumerable<MessageBlock> GetENS50Blocks(IACECusEntryLine entryLine)
		{
			var adjustCustomsValue = ZDecimal.Zero;
			var alternativeTariffToSendCustomsValue = ZString.Empty;
			if (!entryLine.SecondaryTariffLines.Any(x => x.IsCombineLine) && entryLine.IsSupLine)
			{
				var childLine = entryLine.SecondaryTariffLines.FirstOrDefault(line => !Chapter98Helper.Is99Tariff(line.Tariff));
				if (childLine != null && childLine.ValueInUSD.IsEmpty)
				{
					adjustCustomsValue = entryLine.CL_CustomsValue;
					alternativeTariffToSendCustomsValue = childLine.Tariff;
				}
			}

			// 50 + OGA blocks
			foreach (MessageBlock block in BuildTariffGroupingBlocks(entryLine, alternativeTariffToSendCustomsValue))
			{
				yield return block;
			}

			foreach (ISecondaryTariffLine secondaryTariffLine in entryLine.SecondaryTariffLines)
			{
				foreach (MessageBlock block in BuildTariffGroupingBlocks(secondaryTariffLine, adjustCustomsValue, alternativeTariffToSendCustomsValue))
				{
					if (!adjustCustomsValue.IsEmpty && secondaryTariffLine.Tariff == alternativeTariffToSendCustomsValue)
					{
						adjustCustomsValue = ZDecimal.Zero;
					}

					yield return block;
				}
			}
		}

		#endregion

		#region Line Blocks

		MessageBlock MakeENS40(IACECusEntryLine entryLine)
		{
			AENS40 result = new AENS40();

			result.LineItemIdentifier = entryLine.CL_LineNumberFormatted;
			result.ArticleSetIndicator = entryLine.ArticleSetIndicator;
			result.CountryOfOriginCode = entryLine.CountryOfOrigin;
			result.CountryOfExportCode = entryLine.CountryOfExport;
			result.DateOfExportation = entryLine.DateOfExportation;
			result.DateOfExportationforTextiles = entryLine.DateOfExportationFromCountryOfOrigin;
			result.TradeAgreementSpecialProgramClaimCode = !entryLine.SpecialProgramsIndicatorCountry.IsEmpty ? entryLine.SpecialProgramsIndicatorCountry : entryLine.SpecialProgramsIndicatorPrimary;
			result.ChargesAmount = entryLine.Charges;

			if (entryHeader.ModeOfTransportationCode == TransportModeCodes.Codes.VesselContainer || entryHeader.ModeOfTransportationCode == TransportModeCodes.Codes.VesselNonContainer || entryHeader.ModeOfTransportationCode == TransportModeCodes.Codes.BorderWaterBorne)
			{
				result.ForeignPortOfLadingCode = entryLine.PortOfLading;
			}

			result.GrossShippingWeight = entryLine.GrossWeightInKilograms;
			result.CategoryCodeforTextiles = entryLine.TextileCategoryNumber;
			result.ProductClaimCode = entryLine.SpecialProgramsIndicatorSecondary;
			result.RelatedPartyIndicator = entryLine.RelatedPartyIndicator ? "Y" : "N";
			result.NAFTANetCostIndicator = entryLine.NAFTANetCostIndicator ? "Y" : "";
			result.FeeExemptionCode = entryLine.FeeExemptionCode;
			result.ADCVDNonReimbursementStatement = !entryLine.ADDCVDNonReimbursementStatement.IsEmpty ? "Y" : "";

			return result;
		}

		MessageBlock MakeENS41(IACECusEntryLine entryLine)
		{
			AENS41 result = new AENS41();
			result.FTZMerchandiseStatusCode = entryLine.ZoneStatus;
			result.PrivilegedFTZMerchandiseFilingDate = entryLine.PrivilegedStatusFilingDate;
			result.FTZLineItemQuantity = entryLine.FTZLineItemQuantity;
			return result;
		}

		MessageBlock MakeENS43(IACECusEntryLine entryLine)
		{
			AENS43 result = new AENS43();

			result.RulingTypeCode = entryLine.PreImportationReviewProgramRulingsType;
			result.RulingNumber = entryLine.PreImportationReviewProgramRulingsNumber;

			return result;
		}

		#endregion

		#region Commercial Descriptions

		IEnumerable<MessageBlock> BuildCommercialDescriptionBlocks(IACECusEntryLine entryLine)
		{
			if (entryHeader != null)
			{
				int countOf44 = 0;
				foreach (ZString commercialDescriptionLong in entryLine.CommercialDescriptions)
				{
					foreach (ZString commercialDescription in commercialDescriptionLong.Split(70))
					{
						if (countOf44 > 99)
						{
							break;
						}

						countOf44++;
						AENS44 ens44 = new AENS44();
						ens44.CommercialDescriptionText = commercialDescription;
						yield return ens44;
					}

					if (countOf44 > 99)
					{
						break;
					}
				}
			}
		}

		#endregion

		#region Article Party Grouping

		IEnumerable<MessageBlock> BuildArticlePartyGroupingBlocks(IACECusEntryLine entryLine)
		{
			if (!entryLine.ManufacturerSupplierCode.IsEmpty)
			{
				yield return MakeENS47("M", entryLine.ManufacturerSupplierCode);
			}

			if (!entryLine.DeliveredToPartyCode.IsEmpty && !SignedAndCertifiedForCargoRelease)
			{
				yield return MakeENS47("C", entryLine.DeliveredToPartyCode);
			}

			if (!entryLine.SoldToPartyID.IsEmpty)
			{
				yield return MakeENS47("S", entryLine.SoldToPartyID);
			}

			if (!entryLine.ForeignExporterMID.IsEmpty)
			{
				yield return MakeENS47("E", entryLine.ForeignExporterMID);
			}
		}

		MessageBlock MakeENS47(ZString partyType, ZString partyIDNumber)
		{
			AENS47 result = new AENS47();

			result.ArticlePartyTypeCode = partyType;
			result.ArticlePartyIdentifier = partyIDNumber;

			return result;
		}

		#endregion

		#region Tariff Grouping

		IEnumerable<MessageBlock> BuildTariffGroupingBlocks(IACECusEntryLine entryLine, ZString alternativeTariffToSendCustomsValue)
		{
			yield return MakeENS50(entryLine.Tariff, entryLine.DutyAmount, !alternativeTariffToSendCustomsValue.IsEmpty ? ZDecimal.Zero : entryLine.CL_CustomsValue, entryLine.SupCustomsValue, entryLine.Quantity1, entryLine.UnitOfMeasure1, entryLine.Quantity2, entryLine.UnitOfMeasure2, entryLine.Quantity3, entryLine.UnitOfMeasure3);

			if (signed != null && SignedAndCertifiedForCargoRelease && entryLine.IsDisclaimSanction)
			{
				yield return BlockBuilderHelper.GenerateSE60_01("Y");
			}

			if (signed != null && SignedAndCertifiedForCargoRelease && entryHeader.EntryType == EntryTypeList.Codes.ConsumptionFTZ && !entryLine.ZoneStatus.IsEmpty && !entryLine.FTZCurrentTariff.IsEmpty)
			{
				yield return BlockBuilderHelper.GenerateSE61Block(entryLine.FTZCurrentTariff);
			}

			foreach (MessageBlock block in BuildOGABlocks(entryLine))
			{
				yield return block;
			}

			if (entryHeader.IsACECargoReleaseCertification)
			{
				foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(entryLine, signed, enablePGATracking))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> BuildTariffGroupingBlocks(ISecondaryTariffLine entryLine, ZDecimal adjustValue, ZString alternativeTariffToSendCustomsValue)
		{
			yield return MakeENS50(entryLine.Tariff, entryLine.Duty, adjustValue.IsEmpty || alternativeTariffToSendCustomsValue != entryLine.Tariff ? entryLine.ValueInUSD : adjustValue, entryLine.SupCustomsValue, entryLine.Quantity1, entryLine.UQ1, entryLine.Quantity2, entryLine.UQ2, entryLine.Quantity3, entryLine.UQ3);

			if (signed != null && SignedAndCertifiedForCargoRelease && entryLine.IsDisclaimSanction)
			{
				yield return BlockBuilderHelper.GenerateSE60_01("Y");
			}

			foreach (MessageBlock block in BuildOGABlocks(entryLine))
			{
				yield return block;
			}

			if (entryHeader.IsACECargoReleaseCertification)
			{
				foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(entryLine, signed, enablePGATracking))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> BuildOGABlocks(IGovernmentAgenciesCommon line)
		{
			if (!entryHeader.IsACECargoReleaseCertification)
			{
				var disclaimingBlock = ACEOGABlocksCreator.CreateDisclaimingBlocksForACSCertification(line, signed.US_CertifyCargoRelease);
				if (disclaimingBlock != null)
				{
					yield return disclaimingBlock;
				}

				foreach (MessageBlock block in ACEOGABlocksCreator.CreateBlocks(line, signed.US_CertifyCargoRelease))
				{
					yield return block;
				}
			}
		}

		MessageBlock MakeENS50(ZString tariff, ZDecimal duty, ZDecimal goodsValue, ZDecimal supCustomsValue, ZDecimal qty1, ZString uq1, ZDecimal qty2, ZString uq2, ZDecimal qty3, ZString uq3)
		{
			AENS50 result = new AENS50();

			result.HTSNumber = tariff;
			result.DutyAmount = duty;
			result.ValueOfGoodsAmount = supCustomsValue > 0m ? supCustomsValue : goodsValue;
			result.Quantity1 = qty1.ToStringForABI(uq1, 12, 2);
			result.UnitOfMeasureCode1 = uq1;
			result.Quantity2 = qty2.ToStringForABI(uq2, 12, 2);
			result.UnitOfMeasureCode2 = uq2;
			result.Quantity3 = qty3.ToStringForABI(uq3, 12, 2);
			result.UnitOfMeasureCode3 = uq3;

			return result;
		}

		ACEOGABlocksCreator ACEOGABlocksCreator
		{
			get { return aceOGABlocksCreator ?? (aceOGABlocksCreator = new ACEOGABlocksCreator()); }
		}
		ACEOGABlocksCreator aceOGABlocksCreator;

		#endregion

		#region Visa and Licence Details

		IEnumerable<MessageBlock> BuildVisaAndLicenceDetails(IACECusEntryLine entryLine)
		{
			if (!entryLine.VisaNumber.IsEmpty)
			{
				yield return MakeENS51(entryLine);
			}

			foreach (KeyValuePair<ZString, ZString> pair in entryLine.LicenceTypeAndNumbers)
			{
				yield return MakeENS52(pair.Key, pair.Value);
			}
		}

		MessageBlock MakeENS51(IACECusEntryLine entryLine)
		{
			AENS51 result = new AENS51();
			result.StandardVisaNumber = entryLine.VisaNumber;
			return result;
		}

		MessageBlock MakeENS52(ZString type, ZString number)
		{
			AENS52 result = new AENS52();
			result.LicenseCertificatePermitTypeCode = type;
			result.LicenseNumberCertificateNumberPermitNumber = number;
			return result;
		}

		#endregion

		#region BuildADDCVDCaseGroupingBlocks

		IEnumerable<MessageBlock> BuildADDCVDCaseGroupingBlocks(IACECusEntryLine entryLine)
		{
			if (!entryLine.AntidumpingCaseNumber.IsEmpty)
			{
				var depositRate = entryLine.ADDCaseRateTypeQualifier == DepositRateIndicatorList.Codes.AdValorem ? entryLine.ADDDepositRate * 100 : (decimal)entryLine.ADDDepositRate;

				yield return MakeENS53(entryLine.AntidumpingCaseNumber,
					entryLine.IsADDBonded,
					depositRate,
					entryLine.ADDCaseRateTypeQualifier,
					entryLine.ADDSpecificDepositValue,
					entryLine.ADDQuantity,
					entryLine.AntidumpingDuty,
					entryLine.ADDDecID);
			}

			if (!entryLine.CountervailingCaseNumber.IsEmpty)
			{
				var depositRate = entryLine.CVDCaseRateTypeQualifier == DepositRateIndicatorList.Codes.AdValorem ? entryLine.CVDDepositRate * 100 : (decimal)entryLine.CVDDepositRate;

				yield return MakeENS53(entryLine.CountervailingCaseNumber,
					entryLine.IsCVDBonded,
					depositRate,
					entryLine.CVDCaseRateTypeQualifier,
					entryLine.CVDSpecificDepositValue,
					entryLine.CVDQuantity,
					entryLine.CountervailingDuty,
					string.Empty);
			}
		}

		MessageBlock MakeENS53(ZString caseNumber, ZBool isBonded, ZDecimal depositRate, ZString rateType, ZDecimal goodsValue, ZDecimal qty, ZDecimal dutyAmount, ZString nonReimburseDecID)
		{
			AENS53 result = new AENS53();

			result.CaseNumber = caseNumber;
			result.BondCashClaimCode = isBonded ? "B" : "C";
			result.CaseDepositRate = depositRate;
			result.CaseRateTypeQualifierCode = rateType;
			result.ADCVDValueOfGoodsAmount = rateType == DepositRateIndicatorList.Codes.AdValorem ? goodsValue : ZDecimal.Zero;
			result.ADCVDQuantity = rateType == DepositRateIndicatorList.Codes.Specific ? qty : ZDecimal.Zero;
			result.ADCVDDutyAmount = dutyAmount;
			result.ADDNonReimbursementDeclarationIdentifier = nonReimburseDecID;

			return result;
		}

		#endregion

		#region BuildImportersAdditionalDeclarationGroupingBlocks

		IEnumerable<MessageBlock> BuildImportersAdditionalDeclarationGroupingBlocks(IACECusEntryLine entryLine)
		{
			foreach (KeyValuePair<ZString, ZString> additionalDeclarationDetail in entryLine.AdditionalDeclarationDetails)
			{
				var result = new AENS54();
				result.ImportersAdditionalDeclarationTypeCode = additionalDeclarationDetail.Key;
				result.ImportersAdditionalDeclarationInformation = additionalDeclarationDetail.Value;
				yield return result;
			}
		}

		#endregion

		#region BuildIRTaxAndFeesGroupingBlocks

		IEnumerable<MessageBlock> BuildIRTaxAndFeesGroupingBlocks(IACECusEntryLine entryLine)
		{
			if (entryLine.ExciseTax > 0 || entryLine.IRTaxMandatory)
			{
				AENS60 ens60 = new AENS60();
				ens60.AccountingClassCode = entryLine.IRTaxCode;
				ens60.IRTaxAmount = entryLine.ExciseTax;
				yield return ens60;
			}

			var fees = new CusEntryLineFeesGenerator((CusEntryLine)entryLine).Fees;

			foreach (IFee fee in fees.Where(x => ShouldReportIn61(x)))
			{
				yield return GetChargeBlock(fee, new AENS61());
			}

			foreach (IFee fee in fees.Where(x => !ShouldReportIn61(x)))
			{
				yield return GetChargeBlock(fee, new AENS62());
			}
		}

		static bool ShouldReportIn61(IFee fee)
		{
			return fee.Code == Core.Constants.USCustoms.FeeCodes.Coffee;
		}

		MessageBlock GetChargeBlock(IFee fee, IChargeBlock chargeBlock)
		{
			AddToZeroAmountFeesIfNecessary(fee);//We think this should happen for coffee fee as well to have the coffee fee in 89 

			chargeBlock.AccountingClassCode = fee.Code;
			chargeBlock.UserFeeAmount = fee.Amount;
			return (MessageBlock)chargeBlock;
		}

		void AddToZeroAmountFeesIfNecessary(IFee fee)
		{
			if (fee.Amount == 0 && !zeroAmountFees.Contains(fee.Code))
			{
				zeroAmountFees.Add(fee.Code);
			}
		}

		#endregion

		#region BuildPSCLineReason

		MessageBlock BuildPSCLineReason(IACECusEntryLine entryLine)
		{
			AENS63 result = null;

			if (pscReasonCodeProvider != null)
			{
				var reasons = pscReasonCodeProvider.GetPSCLineReasonCodes(entryLine.CL_LineNumberFormatted).ToArray();

				if (reasons.Length > 0)
				{
					int index = 0;
					result = new AENS63();

					foreach (var reason in reasons)
					{
						switch (index)
						{
							case 0:
								result.PostSummaryCorrectionLineReasonCode1 = reason;
								break;
							case 1:
								result.PostSummaryCorrectionLineReasonCode2 = reason;
								break;
							case 2:
								result.PostSummaryCorrectionLineReasonCode3 = reason;
								break;
							case 3:
								result.PostSummaryCorrectionLineReasonCode4 = reason;
								break;
							case 4:
								result.PostSummaryCorrectionLineReasonCode5 = reason;
								break;
						}

						index++;
					}
				}
			}

			return result;
		}

		#endregion

		#region BuildCensusWarningGroupingBlocks

		IEnumerable<MessageBlock> BuildCensusWarningGroupingBlocks(IACECusEntryLine entryLine)
		{
			var censusWarningOverrideCodes = entryLine.CensusWarningOverrideCodes.ToArray();
			if (censusWarningOverrideCodes.Length > 0)
			{
				AENSCW02 result = new AENSCW02();

				int index = 0;

				foreach (var cwo in censusWarningOverrideCodes)
				{
					index++;

					switch (index)
					{
						case 1:
							result.CensusWarningConditionCode1 = cwo.ConditionCode;
							result.CensusWarningConditionOverrideCode1 = cwo.OverrideCode;
							break;
						case 2:
							result.CensusWarningConditionCode2 = cwo.ConditionCode;
							result.CensusWarningConditionOverrideCode2 = cwo.OverrideCode;
							break;
						case 3:
							result.CensusWarningConditionCode3 = cwo.ConditionCode;
							result.CensusWarningConditionOverrideCode3 = cwo.OverrideCode;
							break;
						case 4:
							result.CensusWarningConditionCode4 = cwo.ConditionCode;
							result.CensusWarningConditionOverrideCode4 = cwo.OverrideCode;
							break;
						case 5:
							result.CensusWarningConditionCode5 = cwo.ConditionCode;
							result.CensusWarningConditionOverrideCode5 = cwo.OverrideCode;
							break;
						case 6:
							result.CensusWarningConditionCode6 = cwo.ConditionCode;
							result.CensusWarningConditionOverrideCode6 = cwo.OverrideCode;
							break;
						case 7:
							result.CensusWarningConditionCode7 = cwo.ConditionCode;
							result.CensusWarningConditionOverrideCode7 = cwo.OverrideCode;
							break;
					}
				}

				yield return result;
			}
		}

		#endregion

		#region BuildTotalBlocks

		IEnumerable<MessageBlock> BuildTotalBlocks(bool aD_CVDExist)
		{
			if (aD_CVDExist)
			{
				AENS88 ens88 = new AENS88();
				ens88.TotalBondedADDutyAmount = entryHeader.BondedADDDuty;
				ens88.TotalBondedCVDutyAmount = entryHeader.BondedCVDDuty;
				ens88.TotalCashDepositADDutyAmount = entryHeader.PayableADDDuty;
				ens88.TotalCashDepositCVDutyAmount = entryHeader.PayableCVDDuty;
				yield return ens88;
			}

			List<IFee> totalFees = new List<IFee>(entryHeader.Fees);

			foreach (ZString feeCode in zeroAmountFees)
			{
				if (!totalFees.Exists(x => x.Code == feeCode))
				{
					IFee fee = new Fee();
					fee.Code = feeCode;

					totalFees.Add(fee);
				}
			}

			foreach (MessageBlock block in new ACEENS89Creator().MakeENS89(totalFees))
			{
				yield return block;
			}

			AENS90 ens90 = new AENS90();
			ens90.GrandTotalADDutyAmount = entryHeader.TotalAntidumpingDuty;
			ens90.GrandTotalCVDutyAmount = entryHeader.TotalCountervailingDuty;
			ens90.GrandTotalDutyAmount = entryHeader.TotalEstimatedDuty;
			ens90.GrandTotalIRTaxAmount = entryHeader.TotalEstimatedTax;
			ens90.GrandTotalUserFeeAmount = entryHeader.GrandTotalFee;
			ens90.GrandTotalOtherRevenueAmount = entryHeader.GrandTotalOtherRevenueAmount;
			yield return ens90;
		}

		#endregion

		protected List<MessageBlock> EntryHeaderBlock
		{
			get { return entryHeaderBlock ?? (entryHeaderBlock = new List<MessageBlock>()); }
		}
		List<MessageBlock> entryHeaderBlock;
	}
}
