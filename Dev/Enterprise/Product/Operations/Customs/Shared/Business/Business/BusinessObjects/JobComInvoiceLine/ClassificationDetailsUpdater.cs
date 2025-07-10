using System;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Update classification details like tariff number when part no or JI_CC is set
	/// </summary>
	public class ClassificationDetailsUpdater
	{
		public ClassificationDetailsUpdater(BaseJobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		protected readonly BaseJobComInvoiceLine invoiceLine;

		#region Are Classification Details Being Updated?

		public bool AreClassificationDetailsBeingUpdated
		{
			get { return classificationDetailsUpdateIndex > 0; }
		}
		int classificationDetailsUpdateIndex;

		IDisposable MarkThatClassificationsAreBeingUpdated()
		{
			return new ClassificationDetailsUpdaterNotifier(this);
		}

		class ClassificationDetailsUpdaterNotifier : IDisposable
		{
			public ClassificationDetailsUpdaterNotifier(ClassificationDetailsUpdater updater)
			{
				this.updater = updater;
				updater.classificationDetailsUpdateIndex++;
			}

			readonly ClassificationDetailsUpdater updater;

			public void Dispose()
			{
				updater.classificationDetailsUpdateIndex--;
			}
		}

		#endregion

		public void UpdateWhenJI_PartNoIsSet()
		{
			if (!AreClassificationDetailsBeingUpdated)
			{
				using (MarkThatClassificationsAreBeingUpdated())
				{
					var shouldSetDescription = invoiceLine.ShouldSetDescriptionWhenPartNoChanges;

					OrgSupplierPart product = invoiceLine.Part;

					if (product == null || product.IsDeleted)
					{
						invoiceLine.JI_OP = ZGuid.Empty;
					}
					else
					{
						invoiceLine.JI_OP = product.PK;
						invoiceLine.SetInvoiceUQWhenPartNoChanged(invoiceLine.PartStockTakeUnit);
						invoiceLine.CopyUNDGsIfSupported(product.UNDGs);
						invoiceLine.CopyCommodityFromProduct(product);
						invoiceLine.CopyCustomFieldsFromProduct(product);

						if (invoiceLine.PartSyncManager == null)
						{
							throw new InvalidOperationException("invoiceLine.PartSyncManager is null");
						}

						var pivot = invoiceLine.Pivot;
						if (pivot != null)
						{
							if (pivot.Classification != null)
							{
								invoiceLine.JI_CC = pivot.Classification.PK;
								invoiceLine.JI_Tariff = pivot.Classification.CC_TariffNum;
							}
							else
							{
								invoiceLine.JI_CC = ZGuid.Empty;
							}
							invoiceLine.SetTariffEtcDataFromProductsPivot(pivot); // Pivot trumps classification - set its data last
						}
						else
						{
							invoiceLine.JI_CC = ZGuid.Empty;
						}

						product.PartUnits.Load();
						invoiceLine.CopyHazMatCodeFromUNDGs(product);
					}

					invoiceLine.PartClassificationTariffDescriptionSyncroniser.RefreshPartGeneratedLineDescription();
					if (shouldSetDescription)
					{
						invoiceLine.PartClassificationTariffDescriptionSyncroniser.SetDescription();
					}

					invoiceLine.CalculateWeightAndVolume();
					invoiceLine.CalculateCustomsFactorAndQty();
					invoiceLine.Validation.ValidateJI_PartNo();
					invoiceLine.Validation.ValidateJI_CustomsQuantity();
				}
			}
		}

		public void UpdateWhenJI_CCIsSet()
		{
			if (!AreClassificationDetailsBeingUpdated)
			{
				using (MarkThatClassificationsAreBeingUpdated())
				{
					BaseCusClassification classification = invoiceLine.Classification;
					if (classification != null)
					{
						UpdateWhenJI_CCIsSetCore(classification);
					}
					invoiceLine.CalculateCustomsFactorAndQty();
				}
			}
		}

		protected virtual void UpdateWhenJI_CCIsSetCore(BaseCusClassification classification)
		{
			invoiceLine.JI_Tariff = classification.CC_TariffNum;
		}

		public void UpdateWhenJI_TariffIsSet()
		{
			if (!AreClassificationDetailsBeingUpdated)
			{
				using (MarkThatClassificationsAreBeingUpdated())
				{
					invoiceLine.JI_CC = ZGuid.Empty;
					invoiceLine.CalculateCustomsFactorAndQty();
				}
			}
		}
	}
}
