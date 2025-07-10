using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ReconciliationMessageBuilder : IUSMessageBuilder
	{
		public ReconciliationMessageBuilder(string actionCode, IReconciliation reconciliationData)
		{
			this.actionCode = actionCode;
			this.block = new ABIInputBlockControlGenerator(reconciliationData.EntryFilerCode, reconciliationData.ProcessingDistrictPort, reconciliationData.OfficeCode);

			if (reconciliationData.PreparerDistrictPort != reconciliationData.ProcessingDistrictPort)
			{
				block.B.PreparerDistrictPort = reconciliationData.PreparerDistrictPort;
				block.B.PreparerFilerCode = reconciliationData.EntryFilerCode;
				block.B.PreparerOfficeCode = reconciliationData.OfficeCode;
				block.B.PreparerIndicator = "2";//2 for recon
			}

			this.reconciliationData = reconciliationData;
		}

		readonly string actionCode;
		readonly ABIInputBlockControlGenerator block;
		readonly IReconciliation reconciliationData;

		#region IUSMessageBuilder Members

		public MQEDIMessage Generate()
		{
			block.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling;
			GenerateBlocks();
			return GenerateMessage();
		}

		#endregion

		MQEDIMessage GenerateMessage()
		{
			return block.CreateMessage<MQEDIMessage>(reconciliationData.Factory);
		}

		void GenerateBlocks()
		{
			Add(RECR10Populator.Populate(actionCode, reconciliationData));

			if (actionCode != "D")
			{
				Add(RECR15Populator.Populate(reconciliationData));
				Add(RECR16Populator.Populate(reconciliationData));
				Add(RECR17Populator.Populate(reconciliationData));

				var r90 = new RECR90();
				var r91 = new RECR91();
				var r89s = new RECR89Generator();
				int importEntryCount = 1;

				foreach (IReconciliationImportEntry importEntry in reconciliationData.ImportEntries)
				{
					var r20 = RECR20Populator.Populate(importEntry, importEntryCount++, reconciliationData.AggregateReconciliationIndicator);
					r90.ImportTrailerCounter++;

					if (!reconciliationData.IsNoChangeAggregate)
					{
						r90.TotalOriginalDuty += importEntry.OriginalDuty;
						r90.TotalEstimateReconciliationDuty += reconciliationData.IsWaiveRefund && importEntry.EstimatedReconciliationDuty < importEntry.OriginalDuty ? importEntry.OriginalDuty : importEntry.EstimatedReconciliationDuty;
						r90.TotalOriginalTax += importEntry.OriginalTax;
						r90.TotalEstimateReconciliationTax += reconciliationData.IsWaiveRefund && importEntry.EstimatedReconciliationTax < importEntry.OriginalTax ? importEntry.OriginalTax : importEntry.EstimatedReconciliationTax;
					}

					if (!reconciliationData.AggregateReconciliationIndicator)
					{
						r91.TotalEstimateReconciliationInterest += importEntry.EstimatedReconciliationInterest;
					}

					Add(r20);

					var importEntryFees = importEntry.Fees;

					if (!reconciliationData.AggregateReconciliationIndicator)
					{
						var r21s = new RECR21Generator().Generate(importEntryFees);
						foreach (RECR21 r21 in r21s)
						{
							r20.FeeTrailerCounter++;
							Add(r21);
						}
					}

					if (!reconciliationData.IsNoChangeAggregate)
					{
						r89s.AddFees(importEntryFees, reconciliationData.IsWaiveRefund);
					}
				}

				if (reconciliationData.AggregateReconciliationIndicator)
				{
					r89s.AddRefundedFees(reconciliationData.AggregateRefundedFees);

					r91.TotalEstimateReconciliationInterest = reconciliationData.InterestPaymentAmount;
				}

				foreach (RECR89 r89 in r89s.Generate())
				{
					r90.TotalOriginalFees += r89.TotalOriginalFee + r89.TotalOriginalFee1 + r89.TotalOriginalFee2;
					r90.TotalEstimateReconciliationFees += r89.TotalEstimateReconciliationFee + r89.TotalEstimateReconciliationFee1 + r89.TotalReconciliationFee2;
					Add(r89);
				}

				Add(r90);
				Add(r91);
			}
		}

		void Add(MessageBlock messageBlock)
		{
			if (messageBlock != null)
			{
				block.MessageBlocks.Add(messageBlock);
			}
		}

		#region IUSMessageBuilder Members

		void IUSMessageBuilder.Generate()
		{
			GenerateMessage();
		}

		#endregion
	}
}
