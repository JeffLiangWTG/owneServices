using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEReconciliationMessageBuilder : IUSMessageBuilder
	{
		public ACEReconciliationMessageBuilder(string actionCode, IReconciliation reconciliationData, ZBool isCertified, bool isPaid)
		{
			this.actionCode = actionCode;
			this.blockGenerator = new ACEInputBlockControlGenerator(reconciliationData.CompanyPK.ToGuid(), reconciliationData.ProcessingDistrictPort, reconciliationData.OfficeCode);

			blockGenerator.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummary;

			blockGenerator.B.RemotePreparerDistrictPortCode = reconciliationData.PreparerDistrictPort;
			blockGenerator.B.RemotePreparerFilerCode = reconciliationData.EntryFilerCode;
			blockGenerator.B.RemotePreparerOfficeCode = reconciliationData.OfficeCode;
			blockGenerator.B.RemotelyFiledIndicator = "1";

			this.reconciliationData = reconciliationData;
			this.signed = isCertified;
			this.isPaid = isPaid;
			this.reconTypeCode = GetReconTypeCode(reconciliationData);
		}

		readonly string actionCode;
		readonly ACEInputBlockControlGenerator blockGenerator;
		readonly IReconciliation reconciliationData;
		readonly ZBool signed;
		readonly ZBool isPaid;
		readonly ZString reconTypeCode;

		#region IUSMessageBuilder Members

		public MQEDIMessage Generate()
		{
			GenerateBlocks();
			return GenerateMessage();
		}

		#endregion

		public ZString GetSerialiseMessageContents()
		{
			GenerateBlocks();
			return blockGenerator.Serialise(true);
		}

		MQEDIMessage GenerateMessage()
		{
			return blockGenerator.CreateMessage<MQEDIMessage>(reconciliationData.Factory);
		}

		ZBool IsNoChangeRecon => (new ZString[] { "NA1", "NA2", "NA3", "NA4", "NA5", "NA6", "NA7" }).Contains(reconTypeCode);
		ZBool IsAggregateRecon => (new ZString[] { "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", "CA7" }).Contains(reconTypeCode);

		void GenerateBlocks()
		{
			CreateHeaderBlocks();
			CreateEntriesBlocks();
			CreateEntryLinesBlocks();
			CreateTotalAmountBlocks();
		}

		void Add(MessageBlock messageBlock)
		{
			if (messageBlock != null)
			{
				blockGenerator.MessageBlocks.Add(messageBlock);
			}
		}

		void CreateHeaderBlocks()
		{
			Add(Generate10());
			if (actionCode != "D")
			{
				Add(Generate11());
				Add(Generate15());
			}

			Add(GenerateD1());
			Add(GenerateD2());
			Add(GenerateD3());

			Add(GenerateC1());
			Add(GenerateC2());
			Add(GenerateC3());

			foreach (var blockP1 in GenerateP1())
			{
				Add(blockP1);
			}

			if (actionCode != "D")
			{
				foreach (var blockQ1 in GenerateQ1())
				{
					Add(blockQ1);
				}
			}
		}

		void CreateEntriesBlocks()
		{
			if (actionCode != "D")
			{
				foreach (var entry in reconciliationData.ImportEntries)
				{
					Add(Generate20(entry));
					if (!IsNoChangeRecon && !IsAggregateRecon)
					{
						foreach (var block21 in Generate21(entry))
						{
							Add(block21);
						}
					}
				}
			}
		}

		void CreateEntryLinesBlocks()
		{
			if (actionCode != "D" && !IsNoChangeRecon)
			{
				var entryLineGroupSets = reconciliationData.EntryLineGroups
					.GroupBy(x => new
					{
						x.OriginalCoutryOfOrigin,
						x.OriginalSPI,
						x.OriginalHTS,
						x.AllAdditionalHTSs,
						x.HTSChangedDueToValueIndicator,
						x.IsCottonFeeMandatory,
						x.IsNAFTARecon,
						x.CalculateYear
					})
					.Select(x => new ReconEntryLineGroupSet
					{
						OriginalHTS = x.Key.OriginalHTS,
						OriginalCoutryOfOrigin = x.Key.OriginalCoutryOfOrigin,
						OriginalSPI = x.Key.OriginalSPI,
						ReconReason = x.Max(line => line.ReconReason),
						EntryLineGroups = x.ToList(),
						IsCottonFeeMandatory = x.Key.IsCottonFeeMandatory,
						IsNAFTARecon = x.Key.IsNAFTARecon
					});

				foreach (var entryLineGroupSet in entryLineGroupSets)
				{
					Add(Generate50(entryLineGroupSet));
					var additionalOriginalHTSOrderList = new List<string>();
					foreach (var block51 in Generate51(entryLineGroupSet, additionalOriginalHTSOrderList))
					{
						Add(block51);
					}
					foreach (var block52 in Generate52(entryLineGroupSet))
					{
						Add(block52);
					}
					bool only1Block = entryLineGroupSet.EntryLineGroups.Count() == 1;
					int blockCount = 0;
					var needSendSPI = ReconIssueCodeList.NeedSendSPI(reconciliationData.IssueCode);
					List<IReconEntryLineGroup> entryLineGroups;
					if (needSendSPI)
					{
						entryLineGroups = entryLineGroupSet.EntryLineGroups.Where(x => !x.ReconSPI.IsEmpty).ToList();
						entryLineGroups.AddRange(entryLineGroupSet.EntryLineGroups.Where(x => x.ReconSPI.IsEmpty).ToList());
					}
					else
					{
						entryLineGroups = entryLineGroupSet.EntryLineGroups.ToList();
					}

					foreach (var entryLineGroup in entryLineGroups)
					{
						blockCount++;
						Add(Generate53(entryLineGroup, only1Block, blockCount == 1, needSendSPI));
						foreach (var block54 in Generate54(entryLineGroup, only1Block))
						{
							Add(block54);
						}
						foreach (var block55 in Generate55(entryLineGroup.Fees))
						{
							Add(block55);
						}
					}

					if (entryLineGroupSet.IsNAFTARecon)
					{
						Add(Generate56(entryLineGroups));
						foreach (var block57 in Generate57(entryLineGroups, additionalOriginalHTSOrderList))
						{
							Add(block57);
						}
						foreach (var block58 in Generate58(entryLineGroups))
						{
							Add(block58);
						}
					}
				}
			}
		}

		void CreateTotalAmountBlocks()
		{
			if (actionCode != "D")
			{
				Add(Generate90());

				if (!IsNoChangeRecon)
				{
					var totalFees = new List<IReconciliationImportEntryFee>();
					foreach (var entry in reconciliationData.ImportEntries)
					{
						totalFees.AddRange(entry.Fees);
					}
					if (IsAggregateRecon)
					{
						if (reconciliationData.InterestPaymentAmount > 0m)
						{
							totalFees.Add(new ReconEntryFeeIReconciliationImportEntryFee()
							{
								FeeType = ReconDeclaration.Constants.InterestAccountingClassCode,
								OriginalFee = 0m,
								ReconFee = reconciliationData.InterestPaymentAmount
							});
						}
					}

					var totalFeesGroup = totalFees.GroupBy(x => x.FeeClass).Select(x => new ReconEntryFeeIReconciliationImportEntryFee
					{
						FeeType = x.Key,
						ReconFee = x.Sum(fee => fee.EstimatedReconciliationFee),
						OriginalFee = x.Sum(fee => fee.OriginalFee)
					}).ToList();
					totalFeesGroup.Sort(new ReconEntryFeeComparer());

					foreach (var block91 in Generate91(totalFeesGroup))
					{
						Add(block91);
					}
					if (!isPaid)
					{
						foreach (var block92 in Generate92(totalFeesGroup))
						{
							Add(block92);
						}
					}
				}
			}
		}

		#region IUSMessageBuilder Members

		void IUSMessageBuilder.Generate()
		{
			GenerateMessage();
		}

		#endregion

		#region Generate Blocks

		#region Block 10
		AREC10 Generate10()
		{
			var block10 = new AREC10();
			block10.FilingActionRequestCode = actionCode;
			block10.EntryFilerCode = reconciliationData.EntryFilerCode;
			block10.EntryNumber = reconciliationData.EntryNumber;
			if (actionCode != "D")
			{
				block10.ReconciliationProcessingPort = reconciliationData.ProcessingDistrictPort;
				block10.BrokerReferenceNumber = reconciliationData.BrokerReferenceNumber.Right(9);
				block10.ElectronicSignature = signed ? "X" : "";
				block10.ImporterOfRecordNumber = reconciliationData.ImporterID;
				block10.DesignatedNotifyParty4811Number = reconciliationData.DesignatedNotifyParty4811Number;
				block10.SuretyCompanyCode = reconciliationData.SuretyCode;
				block10.ReconciliationTypeCode = this.reconTypeCode;
				block10.ImportEntryLocationCode = GetImportEntryLocationCode(reconciliationData.ImportEntrySource);
				block10.PriorDisclosureIndicator = reconciliationData.PriorDisclosureIndicator ? "Y" : "";
				block10.AggregateRefundWaiverIndicator = reconciliationData.IsWaiveRefund ? "Y" : "";
			}
			return block10;
		}

		ZString GetReconTypeCode(IReconciliation reconData)
		{
			ZString changeIndicator = ZString.Empty;
			ZString issueCodeIndicator = ZString.Empty;
			if (reconData.IsNoChangeAggregate)
			{
				changeIndicator = "NA";
			}
			else if (reconData.AggregateReconciliationIndicator)
			{
				changeIndicator = "CA";
			}
			else
			{
				changeIndicator = "CE";
			}
			switch (reconData.IssueCode)
			{
				case ReconIssueCodeList.Codes.ValueRecon:
					issueCodeIndicator = "1";
					break;
				case ReconIssueCodeList.Codes.ClassRecon:
					issueCodeIndicator = "2";
					break;
				case ReconIssueCodeList.Codes._9802Recon:
					issueCodeIndicator = "3";
					break;
				case ReconIssueCodeList.Codes.ValueClassRecon:
					issueCodeIndicator = "4";
					break;
				case ReconIssueCodeList.Codes.Value9802Recon:
					issueCodeIndicator = "5";
					break;
				case ReconIssueCodeList.Codes.Class9802Recon:
					issueCodeIndicator = "6";
					break;
				case ReconIssueCodeList.Codes.ValueClass9802Recon:
					issueCodeIndicator = "7";
					break;
				case ReconIssueCodeList.Codes.FTA:
					changeIndicator = "CE";
					issueCodeIndicator = "8";
					break;
			}
			return changeIndicator + issueCodeIndicator;
		}

		ZString GetImportEntryLocationCode(ZInt importEntrySource)
		{
			ZString result = ZString.Empty;
			switch (importEntrySource)
			{
				case 1:
					result = "US";
					break;
				case 2:
					result = "PR";
					break;
				case 3:
					result = "VI";
					break;
			}
			return result;
		}
		#endregion

		#region Block 11

		AREC11 Generate11()
		{
			var block11 = new AREC11();
			block11.ContactName = reconciliationData.ContactName.Left(20);
			block11.ContactPhoneNumber = reconciliationData.ContactPhone.Left(15);
			block11.ContactEmailAddress = reconciliationData.ContactEmail.Left(43);
			return block11;
		}

		#endregion

		#region Block 15
		AREC15 Generate15()
		{
			AREC15 block15 = null;
			if (reconciliationData.QualifyingGoodFreeTradeDec || reconciliationData.SummaryDocProvidedStatement || reconciliationData.NAFTA303ClaimStatement || reconciliationData.ProtestOrPetitionFiledStatement || reconciliationData.IssueCode == ReconIssueCodeList.Codes.FTA)
			{
				block15 = new AREC15();
				block15.QualifyingGoodFreeTradeDeclaration = reconciliationData.QualifyingGoodFreeTradeDec ? "Y" : "";
				block15.SummaryDocumentationProvidedStatement = reconciliationData.SummaryDocProvidedStatement ? "Y" : "N";
				block15.NAFTAArticle303ClaimStatement = reconciliationData.NAFTA303ClaimStatement ? "Y" : "N";
				block15.ProtestOrPetitionFiledStatement = reconciliationData.ProtestOrPetitionFiledStatement ? "Y" : "N";
			}
			return block15;
		}
		#endregion

		#region Block D1
		ARECD1 GenerateD1()
		{
			ARECD1 blockD1 = null;
			if (reconciliationData.DocumentRecipientAddress != null && !reconciliationData.DocumentRecipientAddress.IsEmpty)
			{
				blockD1 = new ARECD1();
				blockD1.DocumentRecipientIdentifier = reconciliationData.DocumentRecipientID;
				blockD1.DocumentRecipientName = reconciliationData.DocumentRecipientAddress.CompanyName.Left(35);
				blockD1.DocumentationProvidedDate = reconciliationData.DocumentProvidedDate;
			}
			return blockD1;
		}
		#endregion

		#region Block D2
		ARECD2 GenerateD2()
		{
			ARECD2 blockD2 = null;
			if (reconciliationData.DocumentRecipientAddress != null && !reconciliationData.DocumentRecipientAddress.IsEmpty)
			{
				blockD2 = new ARECD2();
				blockD2.AddressInformation1 = reconciliationData.DocumentRecipientAddress.Address1.Left(35);
				blockD2.AddressInformation2 = reconciliationData.DocumentRecipientAddress.Address2.Left(35);
			}
			return blockD2;
		}
		#endregion

		#region Block D3
		ARECD3 GenerateD3()
		{
			ARECD3 blockD3 = null;
			if (reconciliationData.DocumentRecipientAddress != null && !reconciliationData.DocumentRecipientAddress.IsEmpty)
			{
				blockD3 = new ARECD3();
				blockD3.CityName = reconciliationData.DocumentRecipientAddress.City;
				blockD3.SubEntityStateCode = reconciliationData.DocumentRecipientAddress.StateCode;
				blockD3.PostalCode = reconciliationData.DocumentRecipientAddress.Postcode;
				blockD3.CountryCode = reconciliationData.DocumentRecipientAddress.Country.Code;
			}
			return blockD3;
		}
		#endregion

		#region Block C1
		ARECC1 GenerateC1()
		{
			ARECC1 blockC1 = null;
			if (reconciliationData.ClaimentAddress != null && !reconciliationData.ClaimentAddress.IsEmpty)
			{
				blockC1 = new ARECC1();
				blockC1.ClaimantIdentifier = reconciliationData.ClaimentID;
				blockC1.ClaimantName = reconciliationData.ClaimentAddress.CompanyName.Left(35);
				blockC1.ClaimDate = reconciliationData.ClaimDate;
				blockC1.ClaimIdentifier = reconciliationData.ClaimIdentifier;
			}
			return blockC1;
		}
		#endregion

		#region Block C2
		ARECC2 GenerateC2()
		{
			ARECC2 blockC2 = null;
			if (reconciliationData.ClaimentAddress != null && !reconciliationData.ClaimentAddress.IsEmpty)
			{
				blockC2 = new ARECC2();
				blockC2.AddressInformation1 = reconciliationData.ClaimentAddress.Address1.Left(35);
				blockC2.AddressInformation2 = reconciliationData.ClaimentAddress.Address2.Left(35);
			}
			return blockC2;
		}
		#endregion

		#region Block C3
		ARECC3 GenerateC3()
		{
			ARECC3 blockC3 = null;
			if (reconciliationData.ClaimentAddress != null && !reconciliationData.ClaimentAddress.IsEmpty)
			{
				blockC3 = new ARECC3();
				blockC3.CityName = reconciliationData.ClaimentAddress.City;
				blockC3.SubEntityStateCode = reconciliationData.ClaimentAddress.StateCode;
				blockC3.PostalCode = reconciliationData.ClaimentAddress.Postcode;
				blockC3.CountryCode = reconciliationData.ClaimentAddress.Country.Code;
			}
			return blockC3;
		}
		#endregion

		#region Block P1
		IEnumerable<ARECP1> GenerateP1()
		{
			var protestIDs = reconciliationData.ImportEntries.Cast<IReconciliationImportEntry>().Where(x => !x.ProtestID.IsEmpty).Select(x => x.ProtestID).Distinct();
			foreach (var protestID in protestIDs)
			{
				yield return new ARECP1() { ProtestPetitionIdentifier = protestID };
			}
		}
		#endregion

		#region Block Q1
		IEnumerable<ARECQ1> GenerateQ1()
		{
			var pendingActionIDs = reconciliationData.ImportEntries.Cast<IReconciliationImportEntry>().Where(x => !x.PendingActionID.IsEmpty)
				.Select(x => new
				{
					Type = x.PendingActionIDType,
					ID = x.PendingActionID
				}).Distinct();
			foreach (var pendActionID in pendingActionIDs)
			{
				yield return new ARECQ1() { PendingActionIdentifierTypeCode = pendActionID.Type, PendingActionIdentifier = pendActionID.ID };
			}
		}
		#endregion

		#region Block 20
		AREC20 Generate20(IReconciliationImportEntry entryData)
		{
			var block20 = new AREC20();
			block20.AssociatedEntryFilerCode = entryData.EntryFilerCode;
			block20.AssociatedEntryNumber = entryData.EntryNumber;
			return block20;
		}
		#endregion

		#region Block 21
		IEnumerable<AREC21> Generate21(IReconciliationImportEntry entryData)
		{
			List<AREC21> block21s = new List<AREC21>();
			AREC21 block = new AREC21();
			int recordNumber = 5;
			var count = 0;
			foreach (var fee in entryData.Fees)
			{
				count++;
				ZString reconFee = ConvertAccountingFeeToString(fee.EstimatedReconciliationFee, 11, 2);

				if (count % recordNumber == 1)
				{
					block = new AREC21();
					block21s.Add(block);
					block.AccountingClassCodeEntrySummary1 = fee.FeeClass;
					block.EstimatedReconciledRevenueAmountEntrySummary1 = reconFee;
				}
				else if (count % recordNumber == 2)
				{
					block.AccountingClassCodeEntrySummary2 = fee.FeeClass;
					block.EstimatedReconciledRevenueAmountEntrySummary2 = reconFee;
				}
				else if (count % recordNumber == 3)
				{
					block.AccountingClassCodeEntrySummary3 = fee.FeeClass;
					block.EstimatedReconciledRevenueAmountEntrySummary3 = reconFee;
				}
				else if (count % recordNumber == 4)
				{
					block.AccountingClassCodeEntrySummary4 = fee.FeeClass;
					block.EstimatedReconciledRevenueAmountEntrySummary4 = reconFee;
				}
				else if (count % recordNumber == 0)
				{
					block.AccountingClassCodeEntrySummary5 = fee.FeeClass;
					block.EstimatedReconciledRevenueAmountEntrySummary5 = reconFee;
				}
			}
			return block21s;
		}
		#endregion

		#region Block 50
		AREC50 Generate50(ReconEntryLineGroupSet entryLineGroup)
		{
			return new AREC50
			{
				PrimaryHTSNumber = entryLineGroup.OriginalHTS,
				CountryOfOriginCode = entryLineGroup.OriginalCoutryOfOrigin,
				TradeAgreementSpecialProgramClaimCode = entryLineGroup.OriginalSPI,
				ReconciliationReasonText = entryLineGroup.ReconReason
			};
		}
		#endregion

		#region Block 51
		IEnumerable<AREC51> Generate51(ReconEntryLineGroupSet entryLineGroupSet, List<string> additionalOriginalHTSOrderList)
		{
			List<AREC51> block51s = new List<AREC51>();
			var block = new AREC51();
			int recordNumber = 4;
			var count = 0;
			var secondaryLines = entryLineGroupSet.EntryLineGroups.FirstOrDefault().SecondaryLines;
			foreach (var secondaryLine in secondaryLines)
			{
				count++;
				var additionalOriginalHTS = secondaryLine.OriginalHTS;
				additionalOriginalHTSOrderList.Add(additionalOriginalHTS);
				if (count % recordNumber == 1)
				{
					block = new AREC51();
					block51s.Add(block);
					block.AdditionalIdentifyingHTSNumber1 = additionalOriginalHTS;
				}
				else if (count % recordNumber == 2)
				{
					block.AdditionalIdentifyingHTSNumber2 = additionalOriginalHTS;
				}
				else if (count % recordNumber == 3)
				{
					block.AdditionalIdentifyingHTSNumber3 = additionalOriginalHTS;
				}
				else if (count % recordNumber == 0)
				{
					block.AdditionalIdentifyingHTSNumber4 = additionalOriginalHTS;
				}
			}
			return block51s;
		}
		#endregion

		#region Block 52
		IEnumerable<AREC52> Generate52(ReconEntryLineGroupSet entryLineGroupSet)
		{
			List<AREC52> block52s = new List<AREC52>();
			var block = new AREC52();
			int recordNumber = 4;
			var count = 0;
			var origEntryLines = entryLineGroupSet.EntryLineGroups.SelectMany(x => x.OriginalEntryLines);
			IEnumerable<IReconOriginalEntryLine> origEntryLinesInMessage;
			if (reconciliationData.IssueCode == ReconIssueCodeList.Codes.FTA)
			{
				origEntryLinesInMessage = origEntryLines.GroupBy(x => new { x.EntryFilerCode, x.EntryNumber, x.EntryLineNumber }).Select(x => x.First());
			}
			else
			{
				origEntryLinesInMessage = origEntryLines;
			}

			foreach (var entryLine in origEntryLinesInMessage)
			{
				count++;
				if (count % recordNumber == 1)
				{
					block = new AREC52();
					block52s.Add(block);
					block.UnderlyingEntrySummaryLineItemIdentifier1FilerCode = entryLine.EntryFilerCode;
					block.UnderlyingEntrySummaryLineItemIdentifier1EntrySummaryNumber = entryLine.EntryNumber;
					block.UnderlyingEntrySummaryLineItemIdentifier1LineNumber = entryLine.EntryLineNumber.PadLeft(3, '0');
				}
				else if (count % recordNumber == 2)
				{
					block.UnderlyingEntrySummaryLineItemIdentifier2FilerCode = entryLine.EntryFilerCode;
					block.UnderlyingEntrySummaryLineItemIdentifier2EntrySummaryNumber = entryLine.EntryNumber;
					block.UnderlyingEntrySummaryLineItemIdentifier2LineNumber = entryLine.EntryLineNumber.PadLeft(3, '0');
				}
				else if (count % recordNumber == 3)
				{
					block.UnderlyingEntrySummaryLineItemIdentifier3FilerCode = entryLine.EntryFilerCode;
					block.UnderlyingEntrySummaryLineItemIdentifier3EntrySummaryNumber = entryLine.EntryNumber;
					block.UnderlyingEntrySummaryLineItemIdentifier3LineNumber = entryLine.EntryLineNumber.PadLeft(3, '0');
				}
				else if (count % recordNumber == 0)
				{
					block.UnderlyingEntrySummaryLineItemIdentifier4FilerCode = entryLine.EntryFilerCode;
					block.UnderlyingEntrySummaryLineItemIdentifier4EntrySummaryNumber = entryLine.EntryNumber;
					block.UnderlyingEntrySummaryLineItemIdentifier4LineNumber = entryLine.EntryLineNumber.PadLeft(3, '0');
				}
			}
			return block52s;
		}

		#endregion

		#region Block 53
		AREC53 Generate53(IReconEntryLineGroup entryLineGroup, bool only1Block, bool isFirstBlock, bool needSendSPI)
		{
			var block53 = new AREC53();
			if (ReconIssueCodeList.NeedSendTariff(reconciliationData.IssueCode, entryLineGroup.HTSChangedDueToValueIndicator))
			{
				block53.ReconciledPrimaryHTSNumber = entryLineGroup.ReconHTS;
			}
			if (ReconIssueCodeList.ExpectCustomsValueChanged(reconciliationData.IssueCode) || !only1Block)
			{
				block53.ReconciledPrimaryMerchandiseValue = ConvertAccountingFeeToString(entryLineGroup.ReconCustomsValue, 10, 0);
			}
			block53.ReconciledPrimaryDuty = ConvertAccountingFeeToString(entryLineGroup.ReconDuty, 11, 2);
			if (needSendSPI)
			{
				block53.ReconciledTradeAgreementSpecialProgramClaimCode = entryLineGroup.ReconSPI;
			}
			if (ReconIssueCodeList.NeedSendHTSChangedDueToValue(reconciliationData.IssueCode) && isFirstBlock)
			{
				block53.HTSChangedDueToValueIndicator = entryLineGroup.HTSChangedDueToValueIndicator ? "Y" : "";
			}
			return block53;
		}
		#endregion

		#region Block 54
		IEnumerable<AREC54> Generate54(IReconEntryLineGroup entryLineGroup, bool only1Block)
		{
			List<AREC54> block54s = new List<AREC54>();
			var block = new AREC54();
			int recordNumber = 2;
			var count = 0;
			foreach (var secondarylLine in entryLineGroup.SecondaryLines)
			{
				count++;
				if (count % recordNumber == 1)
				{
					block = new AREC54();
					block54s.Add(block);
					if (ReconIssueCodeList.NeedSendTariff(reconciliationData.IssueCode, entryLineGroup.HTSChangedDueToValueIndicator))
					{
						block.AdditionalReconciledHTSNumber1 = secondarylLine.ReconHTS;
					}
					if (ReconIssueCodeList.ExpectCustomsValueChanged(reconciliationData.IssueCode) || !only1Block)
					{
						block.AdditionalReconciledMerchandiseValue1 = ConvertAccountingFeeToString(secondarylLine.ReconCustomsValue, 10, 0);
					}
					block.AdditionalReconciledDuty1 = ConvertAccountingFeeToString(secondarylLine.ReconDuty, 11, 2);
				}
				else if (count % recordNumber == 0)
				{
					if (ReconIssueCodeList.NeedSendTariff(reconciliationData.IssueCode, entryLineGroup.HTSChangedDueToValueIndicator))
					{
						block.ReconciledHTSNumber2 = secondarylLine.ReconHTS;
					}
					if (ReconIssueCodeList.ExpectCustomsValueChanged(reconciliationData.IssueCode) || !only1Block)
					{
						block.AdditionalReconciledMerchandiseValue2 = ConvertAccountingFeeToString(secondarylLine.ReconCustomsValue, 10, 0);
					}
					block.AdditionalReconciledDuty2 = ConvertAccountingFeeToString(secondarylLine.ReconDuty, 11, 2);
				}
			}
			return block54s;
		}
		#endregion

		#region Block 55
		IEnumerable<AREC55> Generate55(IEnumerable<IReconciliationImportEntryFee> blockTotalFees)
		{
			List<AREC55> block55s = new List<AREC55>();
			var block = new AREC55();
			int recordNumber = 5;
			var count = 0;
			foreach (var fee in blockTotalFees)
			{
				count++;
				ZString reconFee = ConvertAccountingFeeToString(fee.EstimatedReconciliationFee, 11, 2);
				if (count % recordNumber == 1)
				{
					block = new AREC55();
					block55s.Add(block);
					block.AccountingClassCodeLine1 = fee.FeeClass;
					block.ReconciledLineRevenueAmountLine1 = reconFee;
				}
				else if (count % recordNumber == 2)
				{
					block.AccountingClassCodeLine2 = fee.FeeClass;
					block.ReconciledLineRevenueAmountLine2 = reconFee;
				}
				else if (count % recordNumber == 3)
				{
					block.AccountingClassCodeLine3 = fee.FeeClass;
					block.ReconciledLineRevenueAmountLine3 = reconFee;
				}
				else if (count % recordNumber == 4)
				{
					block.AccountingClassCodeLine4 = fee.FeeClass;
					block.ReconciledLineRevenueAmountLine4 = reconFee;
				}
				else if (count % recordNumber == 0)
				{
					block.AccountingClassCodeLine5 = fee.FeeClass;
					block.ReconciledLineRevenueAmountLine5 = reconFee;
				}
			}
			return block55s;
		}
		#endregion

		#region Block 56
		AREC56 Generate56(List<IReconEntryLineGroup> entryLineGroups)
		{
			return new AREC56
			{
				OriginalPrimaryMerchandiseValue = ConvertAccountingFeeToString(entryLineGroups.Sum(x => x.OriginalCustomsValue), 10, 0),
				OriginalPrimaryHTSDuty = ConvertAccountingFeeToString(entryLineGroups.Sum(x => x.OriginalDuty), 11, 2)
			};
		}
		#endregion

		#region Block 57
		IEnumerable<AREC57> Generate57(List<IReconEntryLineGroup> entryLineGroups, List<string> additionalOriginalHTSOrderList)
		{
			List<AREC57> block57s = new List<AREC57>();
			var block = new AREC57();
			int recordNumber = 2;
			var count = 0;
			var secondaryLines = entryLineGroups.SelectMany(x => x.SecondaryLines);
			foreach (var additionalOriginalHTS in additionalOriginalHTSOrderList)
			{
				count++;
				if (count % recordNumber == 1)
				{
					block = new AREC57();
					block57s.Add(block);
					block.AdditionalOriginalMerchandiseValue1 = ConvertAccountingFeeToString(secondaryLines.Where(x => x.OriginalHTS == additionalOriginalHTS).Sum(x => x.OriginalCustomsValue), 10, 0);
					block.AdditionalOriginalHTSDuty1 = ConvertAccountingFeeToString(secondaryLines.Where(x => x.OriginalHTS == additionalOriginalHTS).Sum(x => x.OriginalDuty), 11, 2);
				}
				else if (count % recordNumber == 0)
				{
					block.AdditionalOriginalMerchandiseValue2 = ConvertAccountingFeeToString(secondaryLines.Where(x => x.OriginalHTS == additionalOriginalHTS).Sum(x => x.OriginalCustomsValue), 10, 0);
					block.AdditionalOriginalHTSDuty2 = ConvertAccountingFeeToString(secondaryLines.Where(x => x.OriginalHTS == additionalOriginalHTS).Sum(x => x.OriginalDuty), 11, 2);
				}
			}
			return block57s;
		}
		#endregion

		#region Block 58
		IEnumerable<AREC58> Generate58(List<IReconEntryLineGroup> entryLineGroups)
		{
			List<AREC58> block58s = new List<AREC58>();
			var block = new AREC58();
			int recordNumber = 5;
			var count = 0;
			var blockTotalFeesGroup = entryLineGroups.SelectMany(x => x.Fees).GroupBy(x => x.FeeClass).Select(x => new ReconEntryFeeIReconciliationImportEntryFee
			{
				FeeType = x.Key,
				OriginalFee = x.Sum(fee => fee.OriginalFee)
			}).ToList();
			blockTotalFeesGroup.Sort(new ReconEntryFeeComparer());

			foreach (var fees in blockTotalFeesGroup)
			{
				count++;
				var feeClass = fees.FeeType;
				var originalFee = ConvertAccountingFeeToString(fees.OriginalFee, 11, 2);
				if (count % recordNumber == 1)
				{
					block = new AREC58();
					block58s.Add(block);
					block.AccountingClassCodeLine1 = feeClass;
					block.OriginalLineRevenueAmount1 = originalFee;
				}
				else if (count % recordNumber == 2)
				{
					block.AccountingClassCodeLine2 = feeClass;
					block.OriginalLineRevenueAmount2 = originalFee;
				}
				else if (count % recordNumber == 3)
				{
					block.AccountingClassCodeLine3 = feeClass;
					block.OriginalLineRevenueAmount3 = originalFee;
				}
				else if (count % recordNumber == 4)
				{
					block.AccountingClassCodeLine4 = feeClass;
					block.OriginalLineRevenueAmount4 = originalFee;
				}
				else if (count % recordNumber == 0)
				{
					block.AccountingClassCodeLine5 = feeClass;
					block.OriginalLineRevenueAmount5 = originalFee;
				}
			}
			return block58s;
		}
		#endregion

		#region Block 90
		AREC90 Generate90()
		{
			var block90 = new AREC90();

			if (!isPaid)
			{
				block90.PaymentTypeCode = reconciliationData.PaymentTypeIndicator;
				block90.PreliminaryStatementPrintDate = reconciliationData.PreliminaryStatementPrintDate;
				block90.StatementClientBranchIdentifier = reconciliationData.ClientBranchDesignation;
			}
			return block90;
		}
		#endregion

		#region Block 91
		IEnumerable<AREC91> Generate91(IEnumerable<IReconciliationImportEntryFee> totalFees)
		{
			List<AREC91> block91s = new List<AREC91>();
			var block = new AREC91();
			int recordNumber = 5;
			var count = 0;
			foreach (var fee in totalFees)
			{
				count++;
				ZString reconFee = ConvertAccountingFeeToString(fee.EstimatedReconciliationFee, 11, 2);
				if (count % recordNumber == 1)
				{
					block = new AREC91();
					block91s.Add(block);
					block.AccountingClassCode1 = fee.FeeClass;
					block.ReconciledRevenueAmountTotal1 = reconFee;
				}
				else if (count % recordNumber == 2)
				{
					block.AccountingClassCode2 = fee.FeeClass;
					block.ReconciledRevenueAmountTotal2 = reconFee;
				}
				else if (count % recordNumber == 3)
				{
					block.AccountingClassCode3 = fee.FeeClass;
					block.ReconciledRevenueAmountTotal3 = reconFee;
				}
				else if (count % recordNumber == 4)
				{
					block.AccountingClassCode4 = fee.FeeClass;
					block.ReconciledRevenueAmountTotal4 = reconFee;
				}
				else if (count % recordNumber == 0)
				{
					block.AccountingClassCode5 = fee.FeeClass;
					block.ReconciledRevenueAmountTotal5 = reconFee;
				}
			}
			return block91s;
		}
		#endregion

		#region Block 92
		IEnumerable<AREC92> Generate92(IEnumerable<IReconciliationImportEntryFee> totalFees)
		{
			List<AREC92> block92s = new List<AREC92>();
			var block = new AREC92();
			int recordNumber = 5;
			var count = 0;
			foreach (var fee in totalFees)
			{
				count++;
				var feeToBePaid = fee.EstimatedReconciliationFee - fee.OriginalFee;
				if (feeToBePaid <= 0m)
				{
					feeToBePaid = 0.00m;
				}

				ZString feeToBePaidConverted = ConvertAccountingFeeToString(feeToBePaid, 11, 2);

				if (count % recordNumber == 1)
				{
					block = new AREC92();
					block92s.Add(block);
					block.AccountingClassCode1 = fee.FeeClass;
					block.PayableRevenueAmountTotal1 = feeToBePaidConverted;
				}
				else if (count % recordNumber == 2)
				{
					block.AccountingClassCode2 = fee.FeeClass;
					block.PayableRevenueAmountTotal2 = feeToBePaidConverted;
				}
				else if (count % recordNumber == 3)
				{
					block.AccountingClassCode3 = fee.FeeClass;
					block.PayableRevenueAmountTotal3 = feeToBePaidConverted;
				}
				else if (count % recordNumber == 4)
				{
					block.AccountingClassCode4 = fee.FeeClass;
					block.PayableRevenueAmountTotal4 = feeToBePaidConverted;
				}
				else if (count % recordNumber == 0)
				{
					block.AccountingClassCode5 = fee.FeeClass;
					block.PayableRevenueAmountTotal5 = feeToBePaidConverted;
				}
			}
			return block92s;
		}
		#endregion
		#endregion
		string ConvertAccountingFeeToString(ZDecimal value, int feeLength, int impliedDecimalPlaces)
		{
			string result;
			ZDecimal messageValue = value * DecimalPowers[impliedDecimalPlaces];
			result = messageValue.ToString(0);
			if (messageValue != messageValue.Truncate() ||
				!messageValue.IsWithinSqlPrecisionAndScale(feeLength, 0) ||
				result.Length > feeLength ||
				messageValue < ZDecimal.Zero)
			{
				result = new string('*', feeLength);
			}
			else if (result.Length < feeLength)
			{
				result = result.PadLeft(feeLength, '0');
			}
			return result;
		}
		readonly static ImmutableArray<int> DecimalPowers = ImmutableArray.Create(1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000);
	}

	public class ReconEntryLineGroupSet
	{
		public ZString OriginalHTS;
		public ZString OriginalCoutryOfOrigin;
		public ZString OriginalSPI;
		public ZString ReconReason;
		public ZBool IsCottonFeeMandatory;
		public ZBool IsNAFTARecon;
		public IEnumerable<IReconEntryLineGroup> EntryLineGroups;
	}
}
