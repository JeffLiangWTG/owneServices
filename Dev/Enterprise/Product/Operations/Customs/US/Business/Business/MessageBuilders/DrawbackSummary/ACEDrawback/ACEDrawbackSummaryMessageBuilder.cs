using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEDrawbackSummaryMessageBuilder : IUSMessageBuilder
	{
		public ACEDrawbackSummaryMessageBuilder(IACEDrawbackSummary drawbackData, IACEDrawbackAcknowledgeAndSign sign)
		{
			this.drawbackData = drawbackData;
			this.sign = sign;
			this.block = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), drawbackData.ProcessingPort, drawbackData.ProcessingOfficeCode);
		}
		readonly IACEDrawbackSummary drawbackData;
		readonly ACEInputBlockControlGenerator block;
		readonly IACEDrawbackAcknowledgeAndSign sign;
		ZInt trackingIDStartIndex;

		#region IUSMessageBuilder Members

		void IUSMessageBuilder.Generate()
		{
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery;
			GenerateMessageBlocks();
		}

		#endregion

		public MQEDIMessage GenerateMessage()
		{
			((IUSMessageBuilder)this).Generate();
			return block.CreateMessage<MQEDIMessage>(drawbackData.Factory);
		}

		public void Build()
		{
			var message = GenerateMessage();
			drawbackData.AddMessage(message);
			CalculateStatus(message);
		}

		public ZString GetSerialiseMessageContents()
		{
			((IUSMessageBuilder)this).Generate();
			return block.Serialise(true);
		}

		void CalculateStatus(MQEDIMessage message)
		{
			if (drawbackData.ActionRequestCode == UpdateActionCodeConverter.AddCode)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal;
			}
			else if (drawbackData.ActionRequestCode == UpdateActionCodeConverter.ReplaceCode)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement;
			}
			message.UpdateErrorFlag();
			drawbackData.MessageStatus = new DrawbackSummaryMessageStatusCalculator().Calculate(message, ABIResponseStatus.Undefined);
		}

		#region Generate Message

		void GenerateMessageBlocks()
		{
			trackingIDStartIndex = 0;
			foreach (var trackingNumberLine in drawbackData.TrackingNumberLines)
			{
				trackingIDStartIndex = trackingNumberLine.ArrangeTrackingNumbers(trackingIDStartIndex);
			}

			block.MessageBlocks.Add(Generate10Block());
			block.MessageBlocks.AddRange(Generate31Blocks());

			foreach (var importEntryDetail in drawbackData.ImportsEntrySummaryDetails)
			{
				block.MessageBlocks.AddRange(GenerateImportEntryMessageBlocks(importEntryDetail));
			}

			foreach (var manufacturerArticle in drawbackData.ManufacturedArticles)
			{
				block.MessageBlocks.AddRange(GenerateManufacturerArticleMessageBlocks(manufacturerArticle));
			}

			if (!drawbackData.IsTFTEARequired)
			{
				foreach (var exportArticle in drawbackData.ExportArticles)
				{
					block.MessageBlocks.AddRange(GenerateExportArticleMessageBlocks(exportArticle));
				}
			}

			foreach (var messageBlock in GenerateNoticeOfIntentMessageBlocks())
			{
				block.MessageBlocks.Add(messageBlock);
			}

			foreach (var messageBlock in GenerateNAFTAMessageBlocks())
			{
				block.MessageBlocks.Add(messageBlock);
			}

			if (drawbackData.IsTFTEARequired)
			{
				foreach (var tfteaDetail in drawbackData.TFTEADetails)
				{
					block.MessageBlocks.AddRange(GenerateTFTEAMessageBlocks(tfteaDetail));
				}
			}

			foreach (var messageBlock in GenerateRevenueTotalMessageBlocks())
			{
				block.MessageBlocks.Add(messageBlock);
			}

			block.MessageBlocks.Add(GenerateRevenueTotalsMessageBlock());
		}

		ADRW10 Generate10Block()
		{
			return new ADRW10()
			{
				SummaryFilingActionRequestCode = drawbackData.ActionRequestCode,
				EntryFilerCode = drawbackData.EntryFilerCode,
				EntryNumberOrDrawbackClaimNumber = drawbackData.EntryNumber,
				DrawbackFilingPort = drawbackData.ClaimPort,
				BrokerReferenceNumber = drawbackData.BrokerReferenceNumber,
				DrawbackProvision = drawbackData.ClaimType,
				BondWaiverIndicator = drawbackData.BondWaiverIndicator,
				BondWaiverReasonCode = drawbackData.BondWaiverReasonCode,
				AcceleratedPaymentRequestIndicator = drawbackData.AcceleratedClaimIndicator,
				OneTimeWaiverIndicatorOTW = drawbackData.OneTimeWaiverIndicator,
				WavierPriorNoticeWPN = drawbackData.WavierOfPriorNoticeIndicator,
				CommercialInterchangeabilityCIDOrCommercialRuling = drawbackData.CommercialInterchangeability,
				ElectronicPetroleumCertification = drawbackData.ElectronicPetroleumCertification,
				ElectronicManufacturingPetroleumCertification = drawbackData.ElectronicManufacturingPetroleumCertification,
				OilSpillTaxCertification = drawbackData.OilSpillTaxCertification,
				NAFTADrawbackClaimIndicator = drawbackData.NAFTADrawbackClaimIndicator,
				ElectronicSignature = sign.US_AcknowledgeAndSign ? "X" : string.Empty,
				ClaimantIDOrImporterRecordNumberOfTheDrawbackClaimant = drawbackData.ImporterOfRecordNumber,
				DesignatedNotifyParty4811Number = drawbackData.NotifyParty4811Number,
				SubstitutedUnusedWineCertification = drawbackData.SubstitutedUnusedWineCertification,
				BillOfMaterialsFormulaCertification = drawbackData.BillOfMaterialsFormulaCertification,
				CertificationForValuationOfDestroyedMerchandise = drawbackData.CertificationForValuationOfDestroyedMerchandise,
				USMCADrawbackClaimIndicator = drawbackData.USMCADrawbackClaimIndicator,
				RetailSalesSubstitution = drawbackData.RetailSalesSubstitution,
				SuperfundTaxCertification = drawbackData.SuperfundTaxCertification
			};
		}

		IEnumerable<ADRW31> Generate31Blocks()
		{
			foreach (var bondDetail in drawbackData.BondDetails)
			{
				yield return Generate31Block(bondDetail);
			}
		}

		ADRW31 Generate31Block(IACEDrawbackBondInfo drawbackBond)
		{
			return new ADRW31()
			{
				BondTypeCode = drawbackBond.BondType,
				BondDesignationTypeCode = drawbackBond.BondDesignationTypeCode,
				SuretyCompanyCode = drawbackBond.SuretyCode,
				SingleTransactionBondAmount = drawbackBond.BondAmount,
				SingleTransactionBondProducerAccountNumber = drawbackBond.ProducerAccountNumber,
			};
		}

		IEnumerable<MessageBlock> GenerateImportEntryMessageBlocks(IACEDrawbackImportClaim drawbackImportClaim)
		{
			yield return Generate40Block(drawbackImportClaim);

			foreach (var block in GenerateImportClassificationMessageBlocks(drawbackImportClaim))
			{
				yield return block;
			}

			foreach (var block in GenerateImportRevenueMessageBlocks(drawbackImportClaim))
			{
				yield return block;
			}
		}

		ADRW40 Generate40Block(IACEDrawbackImportClaim drawbackImportClaim)
		{
			return new ADRW40
			{
				ActionIndicator = drawbackImportClaim.ActionIndicator,
				EntryFilerCode = drawbackImportClaim.EntryFilerCode,
				EntryNumber = drawbackImportClaim.EntryNumber,
				CBPESLine = drawbackImportClaim.CBPESLine,
				DrawbackEligibleCertificateOfDeliveryCD = drawbackImportClaim.CertificateofDeliveryIndicator,
				ManufactureRulingNumber = drawbackImportClaim.ManufacturerRulingNumber,
				BasisOfClaim = drawbackImportClaim.BasisOfClaim,
				ManufDateReceived = drawbackImportClaim.ManufDateReceived.Date,
				ManufDateUsed = drawbackImportClaim.ManufDateUsed.Date,
				ImportTrackingIdentificationNumberITIN = drawbackImportClaim.TrackingIdentificationNumber,
				DrawbackAccountingMethodCode = drawbackImportClaim.DrawbackAccountingMethodCode
			};
		}

		IEnumerable<MessageBlock> GenerateImportClassificationMessageBlocks(IACEDrawbackImportClaim drawbackImportClaim)
		{
			foreach (var importClassification in drawbackImportClaim.ImportClassifications)
			{
				yield return new ADRW41
				{
					HTSNumber = importClassification.HTSNumber,
					ArticleDescriptionText = importClassification.DescriptionText
				};

				var exportQuantityAndUnit = importClassification.ExportQuantityAndUnit;
				yield return new ADRW42
				{
					Quantity = exportQuantityAndUnit.Quantity,
					UnitOfMeasureCode = exportQuantityAndUnit.UnitOfMeasure,
					AllowableQTY = exportQuantityAndUnit.AllowableQuantity,
					EnteredGoodsValuePerUnit = exportQuantityAndUnit.GoodsValuePerUnit,
					SubstitutedValuePerUnit = exportQuantityAndUnit.SubstitutedValuePerUnit
				};
			}
		}

		IEnumerable<ADRW43> GenerateImportRevenueMessageBlocks(IACEDrawbackImportClaim drawbackImportClaim)
		{
			foreach (var revenueAmount in drawbackImportClaim.RevenueAmounts)
			{
				yield return new ADRW43
				{
					AccountingClassCode = revenueAmount.AccountingClassCode,
					ClaimAmount = revenueAmount.ClaimAmount,
					CalculatedAmount = revenueAmount.CalculatedAmount,
					AdjustedClaimedAmount = revenueAmount.AdjustedClaimAmount,
					QualifierIndicator = revenueAmount.QualifierIndicator
				};
			}
		}

		IEnumerable<MessageBlock> GenerateManufacturerArticleMessageBlocks(IACEDrawbackManufactureClaim manufacturerClaim)
		{
			yield return new ADRW50
			{
				ActionIndicator = manufacturerClaim.ActionIndicator,
				ImportManufactureRulingNumber = manufacturerClaim.ImportManufactureRulingNumber,
				HTSNumber = manufacturerClaim.HTSNumber,
				Quantity = manufacturerClaim.Quantity,
				UnitOfMeasureCode = manufacturerClaim.UnitOfMeasure,
				ProductionDate = manufacturerClaim.ProductionDate.Date,
				FactoryLocation = manufacturerClaim.FactoryLocation
			};

			yield return new ADRW51
			{
				ArticleDescriptionText = manufacturerClaim.DescriptionText,
				ManufactureRulingNumber = manufacturerClaim.ManufactureRulingNumber,
				ManufacturedTrackingIdentificationNumberMTIN = manufacturerClaim.ManufacturedTrackingID
			};

			if (!manufacturerClaim.ImportTrackingID.IsEmpty)
			{
				yield return new ADRW52
				{
					ImportTrackingIdentificationNumberITIN = manufacturerClaim.ImportTrackingID
				};
			}
		}

		IEnumerable<MessageBlock> GenerateExportArticleMessageBlocks(IACEDrawbackExportClaim exportClaim)
		{
			yield return new ADRW60
			{
				ExportDestroyIndicator = exportClaim.ExportDestroyIndicator,
				HTSNumber = exportClaim.HTSNumber,
				Quantity = exportClaim.Quantity,
				UnitOfMeasureCode = exportClaim.UnitOfMeasure,
				ExportDestroyDate = exportClaim.ExportDate.Date,
				NoticeOfIntentIndicator = exportClaim.NoticeOfIntentIndicator,
				WaiverToDrawbackClaimRights = exportClaim.WaiverToDrawbackIndicator,
				NameOfExporterDestroyer = exportClaim.NameOfExporter,
				CountryOfUltimateDestination = exportClaim.CountryOfUltimateDestination,
				BOLIndicator = exportClaim.BOLIndicator,
				BOLCarrierCode = exportClaim.BOLCarrierCode
			};

			yield return new ADRW61
			{
				ArticleDescriptionText = exportClaim.DescriptionText,
				UniqueIdentifierNumber = exportClaim.UniqueIdentifierNumber
			};
		}

		IEnumerable<MessageBlock> GenerateNoticeOfIntentMessageBlocks()
		{
			if (!drawbackData.IntendedPort.IsEmpty || !drawbackData.ExaminationWitnessIndicator.IsEmpty || !drawbackData.LocationOfDestruction.IsEmpty)
			{
				yield return new ADRW62
				{
					IntendedPortOfExport = drawbackData.IntendedPort,
					ExaminationWitnessIndicator = drawbackData.ExaminationWitnessIndicator,
					LocationOfDestruction = drawbackData.LocationOfDestruction,
					ResultsOfExaminationOrWitnessOfDestruction = drawbackData.ResultsOfExamination
				};

				foreach (var noticeOfIntent in drawbackData.NoticeOfIntentDetais)
				{
					yield return new ADRW63
					{
						RecordIndicator = noticeOfIntent.RecordIndicator,
						NameOfCBPPersonnel = noticeOfIntent.NameOfCBPPersonnel,
						CBPPersonnelBadge = noticeOfIntent.CBPPersonnelBadge,
						CBPPersonnelPhone = noticeOfIntent.CBPPersonnelPhone,
						ProcessingExaminAtionDate = noticeOfIntent.ProcessingExaminAtionDate.Date
					};
				}
			}
		}

		IEnumerable<ADRW64> GenerateNAFTAMessageBlocks()
		{
			foreach (var nafta in drawbackData.NAFTADetails)
			{
				yield return new ADRW64
				{
					EntryNumber = nafta.EntryNumber,
					EntryDate = nafta.EntryDate.Date,
					DutyPaidToForeignGovtInLocalCurrency = nafta.DutyPaidToForeignGov,
					ExchangeRate = nafta.ExchangeRate,
					TariffNumber1 = nafta.TariffNumber1,
					TariffNumber2 = nafta.TariffNumber2,
					TariffNumber3 = nafta.TariffNumber3,
					CountryOfExport = nafta.CountryOfExport
				};
			}
		}

		IEnumerable<MessageBlock> GenerateTFTEAMessageBlocks(IACEDrawbackTFTEAClaim tfteaClaim)
		{
			yield return new ADRW70
			{
				ExportDestroyIndicator = tfteaClaim.ExportDestroyIndicator,
				HTSNumber = tfteaClaim.HTSNumber,
				Quantity = tfteaClaim.Quantity,
				UnitOfMeasureCode = tfteaClaim.UnitOfMeasure,
				ExportDestroyDate = tfteaClaim.ExportDate.Date,
				NoticeOfIntentIndicator = tfteaClaim.NoticeOfIntentIndicator,
				WaiverToDrawbackClaimRights = tfteaClaim.WaiverToDrawbackIndicator,
				NameOfExporterDestroyer = tfteaClaim.NameOfExporter,
				CountryOfUltimateDestination = tfteaClaim.CountryOfUltimateDestination,
				BOLIndicator = tfteaClaim.BOLIndicator,
				BOLCarrierCode = tfteaClaim.BOLCarrierCode,
				ScheduleBCode = tfteaClaim.ScheduleBCode
			};

			yield return new ADRW71
			{
				ArticleDescriptionText = tfteaClaim.DescriptionText,
				UniqueIdentifierNumber = tfteaClaim.UniqueIdentifierNumber
			};

			if (!tfteaClaim.ImportTrackingNumber.IsEmpty)
			{
				yield return new ADRW72
				{
					ImportTrackingIdentificationNumberITIN = tfteaClaim.ImportTrackingNumber
				};
			}

			if (!tfteaClaim.ManufacturedTrackingNumber.IsEmpty)
			{
				yield return new ADRW73
				{
					ManufacturedTrackingIdentificationNumberMTIN = tfteaClaim.ManufacturedTrackingNumber
				};
			}
		}

		IEnumerable<ADRW89> GenerateRevenueTotalMessageBlocks()
		{
			ADRW89 adrw89 = null;

			foreach (var revenueTotal in drawbackData.RevenueTotals)
			{
				if (adrw89 == null)
				{
					adrw89 = new ADRW89();
				}

				if (adrw89.AccountingClassCode1.IsEmpty)
				{
					adrw89.AccountingClassCode1 = revenueTotal.AccountingClassCode;
					adrw89.TotalAmount1 = revenueTotal.TotalAmount;
				}
				else if (adrw89.AccountingClassCode2.IsEmpty)
				{
					adrw89.AccountingClassCode2 = revenueTotal.AccountingClassCode;
					adrw89.TotalAmount2 = revenueTotal.TotalAmount;
				}
				else if (adrw89.AccountingClassCode3.IsEmpty)
				{
					adrw89.AccountingClassCode3 = revenueTotal.AccountingClassCode;
					adrw89.TotalAmount3 = revenueTotal.TotalAmount;
				}
				else if (adrw89.AccountingClassCode4.IsEmpty)
				{
					adrw89.AccountingClassCode4 = revenueTotal.AccountingClassCode;
					adrw89.TotalAmount4 = revenueTotal.TotalAmount;

					yield return adrw89;
					adrw89 = null;
				}
			}

			if (adrw89 != null)
			{
				yield return adrw89;
			}
		}

		ADRW90 GenerateRevenueTotalsMessageBlock()
		{
			return new ADRW90
			{
				GrandTotalDutyAmount = drawbackData.GrandTotalDuty,
				GrandTotalUserFeeAmount = drawbackData.GrandTotalUserFee,
				GrandTotalIRTaxAmount = drawbackData.GrandTotalIRTax
			};
		}

		#endregion
	}
}
