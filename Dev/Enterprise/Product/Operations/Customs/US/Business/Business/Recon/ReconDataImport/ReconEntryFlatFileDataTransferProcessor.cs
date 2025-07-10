namespace Enterprise.Customs.US.Business
{
	public class ReconEntryFlatFileDataTransferProcessor : ReconFlatFileDataTransferProcessor
	{
		#region Export

		public override void ExportReconDataToCollection(ReconFlattenedDataLineCollection collection, ReconDeclaration reconDeclaration)
		{
			int processed = 0;

			foreach (ReconOriginalEntryHeader reconOriginalEntryHeader in reconDeclaration.OriginalEntries)
			{
				if (!OnProgressChanged(processed * 100 / reconDeclaration.OriginalEntries.Count, string.Format("Processing ({0} of {1}) ...", processed, reconDeclaration.OriginalEntries.Count)))
				{
					break;
				}
				processed++;
				ExportOriginalDetailsToOneDataLine(collection.AddNew(), reconOriginalEntryHeader);
			}
		}

		void ExportOriginalDetailsToOneDataLine(ReconFlattenedDataLine reconDataLine, ReconOriginalEntryHeader reconOriginalEntryHeader)
		{
			ExportOriginalEntryDetails(reconDataLine, reconOriginalEntryHeader);
		}

		void ExportOriginalEntryDetails(ReconFlattenedDataLine dataLine, ReconOriginalEntryHeader reconOriginalEntryHeader)
		{
			dataLine.FTAReconFiled = reconOriginalEntryHeader.US_NAFTAReconIndicator;
			dataLine.EntryNumber = reconOriginalEntryHeader.CH_OrigEntryReference.Left(ReconFlattenedDataLine.Schema.EntryNumberMaxLength);
			dataLine.PaymentDate = reconOriginalEntryHeader.US_PaymentDate;
			dataLine.EntryDate = reconOriginalEntryHeader.US_R_ReleaseDate;
			dataLine.EntryPort = reconOriginalEntryHeader.US_SchDEntry.Left(ReconFlattenedDataLine.Schema.EntryPortMaxLength);
			dataLine.ImportationDate = reconOriginalEntryHeader.US_ImportDate;
			dataLine.OwnerReferenceNumber = reconOriginalEntryHeader.US_R_OwnerRef;
			dataLine.GoodsDescription = reconOriginalEntryHeader.US_R_GoodsDescription;
			dataLine.HasNoLineDetails = reconOriginalEntryHeader.US_R_NoLineDetails ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
			dataLine.MessageMode = reconOriginalEntryHeader.US_R_MsgMode.Left(ReconFlattenedDataLine.Schema.MessageModeMaxLength);
			dataLine.MonthlyFiling = reconOriginalEntryHeader.US_R_MonthlyFiling;
			dataLine.ChangedLinesOnly = reconOriginalEntryHeader.US_R_ChangedLinesOnly;
			dataLine.MPC = reconOriginalEntryHeader.MPC;
			dataLine.OriginalCustomsValue = reconOriginalEntryHeader.US_R_OrigCV;

			dataLine.OriginalDuty = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty);
			dataLine.ReconDuty = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty);

			dataLine.OriginalMPF = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			dataLine.ReconMPF = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);

			dataLine.OriginalHMF = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF);
			dataLine.ReconHMF = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF);

			dataLine.OriginalAVO = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado);
			dataLine.ReconAVO = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado);

			dataLine.OriginalBeef = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef);
			dataLine.ReconBeef = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef);

			dataLine.OriginalBlueberry = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry);
			dataLine.ReconBlueberry = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry);

			dataLine.OriginalCotton = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton);
			dataLine.ReconCotton = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton);

			dataLine.OriginalDairy = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee);
			dataLine.ReconDairy = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee);

			dataLine.OriginalDistilledSpirits = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits);
			dataLine.ReconDistilledSpirits = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits);

			dataLine.OriginalMailFee = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail);
			dataLine.ReconMailFee = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail);

			dataLine.OriginalFreshLimes = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes);
			dataLine.ReconFreshLimes = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes);

			dataLine.OriginalHoney = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey);
			dataLine.ReconHoney = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey);

			dataLine.OriginalMango = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango);
			dataLine.ReconMango = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango);

			dataLine.OriginalMerchandiseInformal = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal);
			dataLine.ReconMerchandiseInformal = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal);

			dataLine.OriginalMerchandiseSurcharge = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge);
			dataLine.ReconMerchandiseSurcharge = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge);

			dataLine.OriginalMushroom = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom);
			dataLine.ReconMushroom = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom);

			dataLine.OriginalOtherAgencies = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies);
			dataLine.ReconOtherAgencies = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies);

			dataLine.OriginalRaspberry = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry);
			dataLine.ReconRaspberry = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry);

			dataLine.OriginalPork = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork);
			dataLine.ReconPork = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork);

			dataLine.OriginalPotato = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato);
			dataLine.ReconPotato = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato);

			dataLine.OriginalSoftwoodLumber = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber);
			dataLine.ReconSoftwoodLumber = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber);

			dataLine.OriginalTobacco = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco);
			dataLine.ReconTobacco = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco);

			dataLine.OriginalWatermelon = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon);
			dataLine.ReconWatermelon = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon);

			dataLine.OriginalWines = reconOriginalEntryHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines);
			dataLine.ReconWines = reconOriginalEntryHeader.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines);

			var impDeclaration = reconOriginalEntryHeader.OriginalDeclaration;

			if (impDeclaration != null)
			{
				dataLine.JobNumber = impDeclaration.JE_DeclarationReference;
			}
		}

		#endregion

		#region Import

		public override void ImportReconDataFromCollection(ReconFlattenedDataLineCollection collection, ReconDeclaration reconDeclaration)
		{
			for (int r = 0; r < collection.Count; r++)
			{
				if (!OnProgressChanged(r * 100 / collection.Count, string.Format("Processing ({0} of {1}) ...", r, collection.Count)))
				{
					break;
				}

				ImportFromOneDataLine(collection[r], reconDeclaration);
			}
		}

		void ImportFromOneDataLine(ReconFlattenedDataLine dataLine, ReconDeclaration reconDeclaration)
		{
			ImportOriginalEntryDetails(dataLine, reconDeclaration);
		}

		void ImportOriginalEntryDetails(ReconFlattenedDataLine dataLine, ReconDeclaration reconDeclaration)
		{
			ReconOriginalEntryHeader originalEntry = LocateEntry(dataLine, reconDeclaration);

			if (originalEntry != null)
			{
				using (originalEntry.GetValidationSuspender())
				using (originalEntry.ReconCharges.SuspendListChanged())
				using (originalEntry.OriginalCharges.SuspendListChanged())
				{
					SetCommonEntryLevelInfo(dataLine, originalEntry);

					if (dataLine.HasNoLineDetails.IsEmpty)
					{
						originalEntry.US_R_NoLineDetails = true; //entry by entry recon should result in a true value for no line details
					}

					if (!dataLine.GoodsDescription.IsEmpty)
					{
						originalEntry.US_R_GoodsDescription = dataLine.GoodsDescription;
					}

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, dataLine.OriginalDuty.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, dataLine.ReconDuty.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, dataLine.OriginalMPF.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, dataLine.ReconMPF.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.HMF, dataLine.OriginalHMF.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.HMF, dataLine.ReconHMF.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, dataLine.OriginalAVO.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, dataLine.ReconAVO.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Beef, dataLine.OriginalBeef.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Beef, dataLine.ReconBeef.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry, dataLine.OriginalBlueberry.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry, dataLine.ReconBlueberry.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Cotton, dataLine.OriginalCotton.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Cotton, dataLine.ReconCotton.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee, dataLine.OriginalDairy.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee, dataLine.ReconDairy.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, dataLine.OriginalDistilledSpirits.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, dataLine.ReconDistilledSpirits.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail, dataLine.OriginalMailFee.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail, dataLine.ReconMailFee.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes, dataLine.OriginalFreshLimes.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes, dataLine.ReconFreshLimes.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Honey, dataLine.OriginalHoney.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Honey, dataLine.ReconHoney.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Mango, dataLine.OriginalMango.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Mango, dataLine.ReconMango.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, dataLine.OriginalMerchandiseInformal.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, dataLine.ReconMerchandiseInformal.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, dataLine.OriginalMerchandiseSurcharge.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, dataLine.ReconMerchandiseSurcharge.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom, dataLine.OriginalMushroom.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom, dataLine.ReconMushroom.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies, dataLine.OriginalOtherAgencies.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies, dataLine.ReconOtherAgencies.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, dataLine.OriginalRaspberry.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, dataLine.ReconRaspberry.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Pork, dataLine.OriginalPork.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Pork, dataLine.ReconPork.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Potato, dataLine.OriginalPotato.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Potato, dataLine.ReconPotato.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, dataLine.OriginalSoftwoodLumber.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, dataLine.ReconSoftwoodLumber.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco, dataLine.OriginalTobacco.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco, dataLine.ReconTobacco.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon, dataLine.OriginalWatermelon.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon, dataLine.ReconWatermelon.Round(2));

					originalEntry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, dataLine.OriginalWines.Round(2));
					originalEntry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, dataLine.ReconWines.Round(2));

					originalEntry.US_R_MonthlyFiling = dataLine.MonthlyFiling;
					originalEntry.US_R_ChangedLinesOnly = dataLine.ChangedLinesOnly;
					originalEntry.MPC = dataLine.MPC;
					originalEntry.US_R_OrigCV = dataLine.OriginalCustomsValue;
					originalEntry.US_NAFTAReconIndicator = dataLine.FTAReconFiled;

					originalEntry.AddAggregateFeesIfNecessary();
				}
			}
		}

		#endregion
	}
}
