using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDynamicWorkOrderLineDataObjectReader : WhsPickableDocketLineDataObjectReader<WhsDynamicWorkOrder, WhsDynamicWorkOrderLine>
	{
		internal WhsDynamicWorkOrderLineDataObjectReader(
			OrderLine orderLineDataObject,
			IXmlImportLogger logger,
			UniversalObjectFactory factory,
			WhsDynamicWorkOrder parent,
			IEnumerable<WhsDynamicWorkOrderLine> matchedLines,
			WhsDynamicWorkOrderLine parentLine = null)
			: base(orderLineDataObject, logger, factory, parent, matchedLines)
		{
			ParentLine = parentLine;
		}

		WhsDynamicWorkOrderLine ParentLine { get; }

		protected override void PopulateBusinessObjectCore(WhsDynamicWorkOrderLine orderLine)
		{
			base.PopulateBusinessObjectCore(orderLine);

			if (IsDisassemblyImport
				&& dataObject?.CustomsData.IsSecondaryInwardsProcessedItem != null
				&& dataObject.CustomsData.IsSecondaryInwardsProcessedItem.Value)
			{
				throw new DataObjectReadFailureException(Res.GetString("cefa79a6-ceaf-4226-b829-a7bbd80e57ce", "'Is Secondary Inward Processed Item' lines cannot be imported for Revert Assembly Dynamic Work Orders."));
			}

			var orderLineCollection = dataObject.OrderLineCollection;
			if (orderLineCollection != null && orderLineCollection.Count > 0)
			{
				if (IsComponentLineImport)
				{
					throw new DataObjectReadFailureException(Res.GetString("de062008-f628-480a-a6fe-246be10eaef9", "Component lines cannot have child component lines."));
				}
				else if (IsDisassemblyImport)
				{
					throw new DataObjectReadFailureException(Res.GetString("c49ef087-0de2-402f-975f-428a8c24477f", "Component lines cannot be imported for Revert Assembly Dynamic Work Orders."));
				}
				else
				{
					var dynamicWorkOrderComponentLineCollectionReader = new WhsDynamicWorkOrderComponentLineCollectionReader(orderLine, orderLineCollection.ToArray(), this);
					dynamicWorkOrderComponentLineCollectionReader.ReadIntoCollection();
				}
			}
		}

		protected override void PopulateCustomsDataCore(WhsDynamicWorkOrderLine line)
		{
			if (!IsComponentLineImport)
			{
				new WhsBondedWarehouseAttributeDataObjectReader(logger).ReadMainAndSecondaryInwardsProcessedItemIntoBusinessObject(dataObject.CustomsData, line.CustomsData);
			}
		}

		protected override void SetBondedEntryKey(WhsDynamicWorkOrderLine line)
		{
			if (IsComponentLineImport)
			{
				var bondedEntryKey = dataObject.CustomsData?.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine();
				SetValue(line, WhsDocketLineSchema.WE_BondedEntryKey, bondedEntryKey);
			}
		}

		bool IsComponentLineImport => ParentLine != null;

		bool IsDisassemblyImport => !Parent.IsAssembly;

		protected override bool ImportUnitPriceFields => false;

		protected override WhsDocketLineCollection GetChildDocketLineCollection() => ParentLine?.ChildComponentLinesCollection ?? Parent.Lines;

		protected override string NotAllowedToChangeRestrictedFieldsMessage => Res.GetString("b55cf2fa-98ef-47bb-8b7f-d4192a0daf21", "Cannot change product on an allocated Dynamic Work Order.");

		protected override string DocketLineType => WhsDynamicWorkOrderDataObjectReader.DynamicWorkOrderDocketType;

		protected override bool CanCreateNewProducts => false;

		protected override CustomsDataSourceHelper<WhsDynamicWorkOrder> GetNewCustomsDataSourceHelper(TopLevelDataObject topLevelDataObject, IDataContextDataObject topLevelDataContext) => null;

		protected override string GetBusinessObjectHumanReadableName(WhsDynamicWorkOrderLine businessObject)
		{
			var businessObjectName = base.GetBusinessObjectHumanReadableName(businessObject);
			if (IsComponentLineImport)
			{
				businessObjectName = (NoResString)"Component " + businessObjectName;
			}

			return businessObjectName;
		}

		#region WhsDynamicWorkOrderComponentLineCollectionReader

		class WhsDynamicWorkOrderComponentLineCollectionReader : DataObjectCollectionReader<OrderLine, WhsDynamicWorkOrderLine>
		{
			internal WhsDynamicWorkOrderComponentLineCollectionReader(WhsDynamicWorkOrderLine parentDynamicWorkOrderLine, OrderLine[] orderLineDataObjects, WhsDynamicWorkOrderLineDataObjectReader reader)
				: base(orderLineDataObjects)
			{
				ParentDynamicWorkOrderLine = Argument.NotNull(parentDynamicWorkOrderLine, nameof(parentDynamicWorkOrderLine));
				Reader = Argument.NotNull(reader, nameof(reader));
			}

			readonly WhsDynamicWorkOrderLine ParentDynamicWorkOrderLine;
			readonly WhsDynamicWorkOrderLineDataObjectReader Reader;

			protected override WhsDynamicWorkOrderLine[] BusinessObjects => ParentDynamicWorkOrderLine.ChildComponentLinesCollection.Cast<WhsDynamicWorkOrderLine>().ToArray();

			protected override void AddToCollection(WhsDynamicWorkOrderLine businessObject)
			{
				ParentDynamicWorkOrderLine.ChildComponentLinesCollection.Add(businessObject);
			}

			protected override void RemoveFromCollection(WhsDynamicWorkOrderLine businessObject)
			{
				ParentDynamicWorkOrderLine.ChildComponentLinesCollection.Delete(businessObject);
			}

			protected override WhsDynamicWorkOrderLine FindMatchingBusinessObject(OrderLine dataObject)
			{
				return null;
			}

			protected override WhsDynamicWorkOrderLine ReadIntoBusinessObject(OrderLine dataObject, WhsDynamicWorkOrderLine businessObject)
			{
				var bizoRead = new WhsDynamicWorkOrderLineDataObjectReader(dataObject, Reader.logger, Reader.factory, ParentDynamicWorkOrderLine.DynamicWorkOrder, MatchedLines, parentLine: ParentDynamicWorkOrderLine).ReadIntoBusinessObject();
				if (bizoRead != null)
				{
					MatchedLines.Add(bizoRead);
				}
				return bizoRead;
			}

			HashSet<WhsDynamicWorkOrderLine> MatchedLines => matchedLines ?? (matchedLines = new HashSet<WhsDynamicWorkOrderLine>());
			HashSet<WhsDynamicWorkOrderLine> matchedLines;
		}

		#endregion
	}
}
