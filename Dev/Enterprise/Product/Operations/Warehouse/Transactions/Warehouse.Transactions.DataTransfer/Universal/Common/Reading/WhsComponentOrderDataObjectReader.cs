using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsComponentOrderDataObjectReader<TDocket, TDocketLine> : WhsDocketDataObjectReader<TDocket, TDocketLine>
		where TDocket : WhsComponentOrder
		where TDocketLine : WhsComponentOrderLine
	{
		protected WhsComponentOrderDataObjectReader(UniversalShipment docketDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(docketDataObject, logger, factory)
		{
		}

		protected override void PopulateBusinessObjectCore(TDocket componentOrder)
		{
			base.PopulateBusinessObjectCore(componentOrder);

			var volumeUnit = dataObject.TotalVolumeUnit.GetCodeAsUpperCase();
			if (!volumeUnit.IsEmpty)
			{
				SetValueIfNotReadOnly(componentOrder, WhsDocketSchema.WD_TotalCubicUnit, volumeUnit);
			}

			var weightUnit = dataObject.TotalWeightUnit.GetCodeAsUpperCase();
			if (!weightUnit.IsEmpty)
			{
				SetValueIfNotReadOnly(componentOrder, WhsDocketSchema.WD_TotalWeightUnit, weightUnit);
			}

			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				ImportStrategy.SetExternalReference(componentOrder);
				SetValue(componentOrder, WhsDocketSchema.WD_ExternalReferenceSplit, orderDataObject.OrderNumberSplit);

				SetValueIfNotReadOnly(componentOrder, WhsDocketSchema.WD_PickOption, componentOrder.Lookups.PickOptions, orderDataObject.PickOption);
				SetValueIfNotReadOnly(componentOrder, WhsDocketSchema.WD_TotalCubic, orderDataObject.TotalLineVolume);
				SetValueIfNotReadOnly(componentOrder, WhsDocketSchema.WD_TotalWeight, orderDataObject.TotalLineWeight);
				SetValueIfNotReadOnly(componentOrder, WhsDocketSchema.WD_TotalUnits, orderDataObject.TotalUnits);
				PopulatePickPriority(componentOrder, orderDataObject);
			}

			if (dataObject.LocalProcessing != null)
			{
				var dataObjectHelper = new WhsDataObjectReaderHelper(componentOrder.Warehouse);
				SetValueIfNotReadOnly(componentOrder, WhsDocketSchema.WD_RequiredDate, dataObjectHelper.ConvertToZDateTimeOffset(dataObject.LocalProcessing.DeliveryRequiredBy));
			}
		}

		protected override void CalculateTotalsIfNeededCore(TDocket docket)
		{
			base.CalculateTotalsIfNeededCore(docket);

			if (docket.Lines.Any(l => l.HasChanges))
			{
				CalculateVolumeAndWeightIfNeeded(docket);
				CalculateTotalUnitsIfNeeded(docket);
			}
		}

		void CalculateVolumeAndWeightIfNeeded(TDocket docket)
		{
			var volumeNeedsCalculation = dataObject.Order?.TotalLineVolume == null;
			var weightNeedsCalculation = dataObject.Order?.TotalLineWeight == null;

			if (volumeNeedsCalculation || weightNeedsCalculation)
			{
				var volume = 0m;
				var weight = 0m;
				var volumeUnit = docket.GetVolumeMeasure();
				var weightUnit = docket.GetWeightMeasure();

				foreach (var lineWithProductAndQty in GetLinesForTotalWeightAndVolumeCalculation(docket).ToProductAndQuantities())
				{
					volume += UnitOfMeasureConverter.GetQuantityFromLine(docket, volumeUnit, lineWithProductAndQty);
					weight += UnitOfMeasureConverter.GetQuantityFromLine(docket, weightUnit, lineWithProductAndQty);
				}

				if (volumeNeedsCalculation)
				{
					SetValueIfNotReadOnly(docket, WhsDocketSchema.WD_TotalCubic, volume);
				}

				if (weightNeedsCalculation)
				{
					SetValueIfNotReadOnly(docket, WhsDocketSchema.WD_TotalWeight, weight);
				}
			}
		}

		protected abstract IEnumerable<WhsDocketLine> GetLinesForTotalWeightAndVolumeCalculation(TDocket whsComponentOrder);

		protected abstract void CalculateTotalUnitsIfNeeded(TDocket docket);

		protected override bool ShouldDeleteUnmatchedDocketLines(TDocket docket, IEnumerable<OrderLine> lines)
			=> dataObject.Order?.OrderLineCollection?.Content != CollectionContent.Partial;

		protected override bool SupportsWorkflowCustomFieldImport => false;

		protected override string CannotUpdateLinesWarningMessage
			=> Res.GetString("24fe0531-2147-4dd2-b63c-90ff8400ed81", "Cannot update {0} Lines on a Finalized or In Picking {0}.", DocketType);

		protected override IMatchingBusinessEntityFinder<TDocket> GetCombinedReferenceMatcher() => null; // We only use Client + ExternalReference + SplitNo

		public class ComponentOrderImportStrategy : WhsImportStrategy
		{
			public ComponentOrderImportStrategy(WhsComponentOrderDataObjectReader<TDocket, TDocketLine> reader)
				: base(reader)
			{
			}

			protected override void AddAdditionalFilterCore(UniversalShipment dataObject, ZQuery query)
			{
				var orderNumberSplit = dataObject.Order?.OrderNumberSplit;
				if (orderNumberSplit != null)
				{
					query.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, orderNumberSplit.Value);
				}

				query.OrderBy = WhsDocketSchema.Constants.WD_SystemCreateTimeUtc + OrderByClause.Descending;
			}

			protected override void FinaliseDocketWithoutUserConfirmation(TDocket docket)
			{
				var pick = docket.Factory.New<WhsPick>();
				pick.PickOrdersFailed += OnPickOrdersFailed;
				pick.AutoPickAttempt += OnAutoPickAttempted;

				var successfullyAllocatedPick = pick.PickOrders(docket, saveFactory: false);

				pick.AutoPickAttempt -= OnAutoPickAttempted;
				pick.PickOrdersFailed -= OnPickOrdersFailed;

				if (successfullyAllocatedPick)
				{
					if (ShouldRejectShortfall(docket))
					{
						RejectIfInShortfall(docket);
					}

					OnSuccessfulAllocation(docket);

					base.FinaliseDocketWithoutUserConfirmation(docket);
				}

				void OnPickOrdersFailed(object sender, WhsPick.DocketPickabilityEventArgs e)
				{
					Reader.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(e.Message));
				}

				void OnAutoPickAttempted(object sender, WhsPick.AutoPickEventArgs e)
				{
					OnAutoPickAttemptedCore(docket, e);
				}
			}

			void OnSuccessfulAllocation(TDocket docket) => OnSuccessfulAllocationCore(docket);

			protected virtual void OnSuccessfulAllocationCore(TDocket docket)
			{
			}

			protected virtual void OnAutoPickAttemptedCore(TDocket docket, WhsPick.AutoPickEventArgs args)
			{
			}

			protected void FinaliseComponentOrder(TDocket docket)
			{
				if (CanFinaliseComponentOrder(docket))
				{
					SetRequiredDateToTodayIfEmpty(docket);
					FinaliseDocket(docket);
				}
			}

			bool ShouldRejectShortfall(TDocket docket) => ShouldRejectShortfallCore(docket);
			protected virtual bool ShouldRejectShortfallCore(TDocket docket) => true;

			bool CanFinaliseComponentOrder(TDocket docket)
				=> !docket.IsInDatabase
				&& docket.WD_IsInwardsProcessingJob
				&& docket.Warehouse.WW_IsVirtualWarehouse;

			protected override void AfterFinaliseDocket(TDocket docket)
			{
				base.AfterFinaliseDocket(docket);
				docket.Pick.FinalisePick();
				SendErrorReporterIfFailedToFinalizePick(docket);
			}

			protected override bool CanUpdateDocketLinesCore(TDocket docket) => base.CanUpdateDocketLinesCore(docket) && !docket.IsAttachedToPick;
		}
	}
}
