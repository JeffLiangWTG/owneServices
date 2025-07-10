using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using BusinessCodeLists = Enterprise.Warehouse.Transactions.CodeLists;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsAdjustmentDataObjectReader : WhsDocketDataObjectReader<WhsAdjustment, WhsAdjustmentLine>
	{
		internal WhsAdjustmentDataObjectReader(UniversalShipment whsAdjustmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(whsAdjustmentDataObject, logger, factory)
		{
		}

		#region Matching Job

		protected override string DocketType
		{
			get { return Res.GetString("c7076096-3149-4ef6-bca8-b46236e719d1", "Adjustment"); }
		}

		protected override string DocketTypeCode
		{
			get { return BusinessCodeLists.DocketType.Codes.Adjustment; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseAdjustment; }
		}

		protected override void AddAdditionalFilter(UniversalShipment dataObject, ZQuery query)
		{
			// Don't add additional filter.
		}

		protected override WhsImportStrategy GetNewImportStrategy()
		{
			return new ImportFromAdjustmentStartegy(this);
		}

		protected override bool IsImportJobCostingAllowed(WhsAdjustment targetBO) => false;

		#endregion

		#region Matching References (fallbacks)

		protected override IMatchingBusinessEntityFinder<WhsAdjustment> GetCombinedReferenceMatcher()
		{
			return null; // No combined reference match on Warehouse Adjustment. Not enough references to make it realistic.
		}

		#endregion

		#region Create / Update Job

		protected override void PopulateBusinessObjectCore(WhsAdjustment adjustment)
		{
			base.PopulateBusinessObjectCore(adjustment);
			PopulateExternalReferenceAndSplit(adjustment);
		}

		void PopulateExternalReferenceAndSplit(WhsAdjustment adjustment)
		{
			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				if (IsNewBO && orderDataObject.OrderNumber.HasValue)
				{
					adjustment.IsUniqueExternalReferenceCreatedOnSave = false;
				}
				SetValueIfNotReadOnly(adjustment, WhsDocketSchema.WD_ExternalReference, orderDataObject.OrderNumber);
			}
		}

		protected override DataObjectList<AdditionalReference> GetAdditionalReferencesFromImport() => null; // Disable additional references

		protected override bool ShouldDeleteUnmatchedDocketLines(WhsAdjustment adjustment, IEnumerable<OrderLine> lines)
		{
			return dataObject.Order?.OrderLineCollection?.Content != CollectionContent.Partial;
		}

		protected override DataObjectReader<OrderLine, WhsAdjustmentLine> GetNewLineReader(WhsAdjustment adjustment, OrderLine orderLineDataObject, IEnumerable<WhsAdjustmentLine> matchedLines)
		{
			return new WhsAdjustmentLineDataObjectReader(orderLineDataObject, logger, factory, adjustment);
		}

		#endregion

		#region ImportFromAdjustmentStartegy class

		public class ImportFromAdjustmentStartegy : WhsImportStrategy
		{
			public ImportFromAdjustmentStartegy(WhsAdjustmentDataObjectReader reader)
				: base(reader)
			{
			}

			protected new WhsAdjustmentDataObjectReader Reader
			{
				get { return (WhsAdjustmentDataObjectReader)base.Reader; }
			}

			protected override void AfterPopulateCore(WhsAdjustment docket)
			{
				base.AfterPopulateCore(docket);

				var adjustmentLines = docket.Lines.Cast<WhsAdjustmentLine>();
				(new CommittingLinesAdjustmentHelper(docket)).UncommitExcessInventoryAndCommitRequiredInventoryWithValidationSuspended(adjustmentLines);

				if (adjustmentLines.Any(l => l.WE_TransactionQuantity < 0 && Math.Abs(l.WE_TransactionQuantity) != l.CommittedQuantity))
				{
					var validationErrors = new ZStringBuilder();
					validationErrors.AppendIfNotEmpty(Res.GetString("8B84BD2E-3BF9-4708-8F9A-6146980888C2",
						"Adjustment could not be imported into the Warehouse because there is not enough stock for some of the adjustment out line(s) to commit."));

					Reader.ThrowImportFailureExceptionIfNotEmpty(validationErrors);
				}

				// to be uncommented and perhaps moved to base when we decide to FIX import properly
				//using (((IBusinessObjectInternals)docket).ResumeValidationForAllDescendantsTemporarily())
				//{
				//	docket.RunPreSaveValidation();
				//}
				//if (docket.HasErrors)
				//{
				//	var errorsFromNotify = ((NotificationBuffer)docket.NotificationSubscriber).AsString.TrimEnd();
				//	var errorsFromDocket = string.Join("\r\n", docket.NotificationsIncludingChildren.GetErrors().Select(e => e.Message));

				//	var validationErrors = new ZStringBuilder();
				//	validationErrors.AppendIfNotEmpty(errorsFromNotify);
				//	validationErrors.AppendIfNotEmpty(errorsFromDocket);

				//	var message = new ZStringBuilder(Res.GetString("8B84BD2E-3BF9-4708-8F9A-6146980888C2",
				//		"{0} could not be imported into the Warehouse because of the following error(s):\r\n{1}", Reader.DocketType, validationErrors.ToStringWithNewLineBetweenAppends()));

				//	Reader.ThrowImportFailureExceptionIfNotEmpty(message);
				//}
			}
		}

		#endregion
	}
}
