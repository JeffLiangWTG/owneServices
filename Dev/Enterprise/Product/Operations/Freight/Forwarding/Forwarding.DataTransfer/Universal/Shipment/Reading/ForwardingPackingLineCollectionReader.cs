using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPackingLineCollectionReader : DataObjectCollectionReader<PackingLine, ForwardingPackLine>
	{
		public ForwardingPackingLineCollectionReader(ForwardingPackingLineCollectionReadingContext readingContext)
			: base(readingContext.PackingLineDataObjectCollection)
		{
			logger = Argument.NotNull(readingContext.Logger, "logger");
			factory = Argument.NotNull(readingContext.Factory, "factory");
			parentShipment = Argument.NotNull(readingContext.ShipmentBO, "parentShipment");
			linkManager = Argument.NotNull(readingContext.ContainerLinkManager, "linkManager");
			orderLineLinkManager = readingContext.OrderLineLinkManager;
			packLineDOToPackLineBOMap = readingContext.PackLineBOToPackingLineDOMap;
			disableMatchOfExistingPackLine = readingContext.DisableMatchOfExistingPackLine;
		}

		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly ForwardingShipment parentShipment;
		readonly IContainerLinkManager<ForwardingConsol> linkManager;
		readonly IOrderLineLinkManager orderLineLinkManager;
		readonly Dictionary<PackingLine, ForwardingPackLine> packLineDOToPackLineBOMap;
		readonly bool disableMatchOfExistingPackLine;

		protected override ForwardingPackLine[] BusinessObjects
		{
			get
			{
				return businessObjects ?? (businessObjects = parentShipment.OuterPackLines.ToArray<ForwardingPackLine>());
			}
		}
		ForwardingPackLine[] businessObjects;

		protected override void AddToCollection(ForwardingPackLine businessObject)
		{
			parentShipment.OuterPackLines.Add(businessObject);
		}

		protected override void RemoveFromCollection(ForwardingPackLine businessObject)
		{
			parentShipment.OuterPackLines.RemoveAndDelete(businessObject);
		}

		protected override ForwardingPackLine FindMatchingBusinessObject(PackingLine dataObject)
		{
			if (disableMatchOfExistingPackLine)
			{
				return null;
			}
			var matchingPacklineFromPackLineID = BusinessObjects
				.FirstOrDefault(packline => packline.JL_PackLineId != ZString.Empty &&
					packline.JL_PackLineId == (dataObject?.PackingLineID ?? ZString.Empty));

			if (matchingPacklineFromPackLineID != null)
			{
				return matchingPacklineFromPackLineID;
			}

			if (DataObjects.Length == 1
				&& BusinessObjects.Length == 1
				&& ContentType == CollectionContent.Complete
				&& AreUNDGsMatched(BusinessObjects[0], dataObject))
			{
				return BusinessObjects[0];
			}

			if (PackingLineDOToForwardingPackLineMap.ContainsKey(dataObject))
			{
				var matchedForwardingPackingLine = PackingLineDOToForwardingPackLineMap[dataObject];

				if (PackingLineDOToForwardingPackLineMap.Count(x => x.Value == matchedForwardingPackingLine) == 1
					&& AreUNDGsMatched(matchedForwardingPackingLine, dataObject))
				{
					return matchedForwardingPackingLine;
				}
			}

			return null;
		}

		protected override ForwardingPackLine ReadIntoBusinessObject(PackingLine dataObject, ForwardingPackLine businessObject)
		{
			var reader = GetPackingLineDataObjectReader(dataObject, logger, factory, parentShipment, linkManager, orderLineLinkManager, businessObject);
			var result = reader.ReadIntoBusinessObject();

			if (packLineDOToPackLineBOMap != null)
			{
				packLineDOToPackLineBOMap.Add(dataObject, result);
			}

			return result;
		}

		protected virtual ForwardingPackingLineDataObjectReader GetPackingLineDataObjectReader(PackingLine packingLineDataObject
			, IXmlImportLogger logger
			, UniversalObjectFactory factory
			, ForwardingShipment parentShipment
			, IContainerLinkManager<ForwardingConsol> containerLinkManager
			, IOrderLineLinkManager orderLineLinkManager
			, ForwardingPackLine packingLineBO)
		{
			return new ForwardingPackingLineDataObjectReader(
				packingLineDataObject,
				logger,
				factory,
				parentShipment,
				linkManager,
				orderLineLinkManager,
				data => packingLineBO);
		}

		Dictionary<PackingLine, ForwardingPackLine> PackingLineDOToForwardingPackLineMap
		{
			get
			{
				if (packingLineDOToForwardingPackLineMap == null)
				{
					packingLineDOToForwardingPackLineMap = new Dictionary<PackingLine, ForwardingPackLine>();

					foreach (var packingLine in DataObjects)
					{
						var matchedForwardingPackingLine = FindBestMatch(packingLine);

						if (matchedForwardingPackingLine != null)
						{
							packingLineDOToForwardingPackLineMap.Add(packingLine, matchedForwardingPackingLine);
						}
					}
				}

				return packingLineDOToForwardingPackLineMap;
			}
		}
		Dictionary<PackingLine, ForwardingPackLine> packingLineDOToForwardingPackLineMap;

		ForwardingPackLine FindBestMatch(PackingLine dataObject)
		{
			var matchedByPackageTypeAndCount = BusinessObjects.Where(x => IsMatchingByPackageTypeAndCount(dataObject, x)).ToArray();
			var matchedByReferences = matchedByPackageTypeAndCount
				.Where(x => IsMatchingValueFromDataObject(x.JL_RefNumber, x.JL_ExportRefNumber, dataObject.ReferenceNumber, dataObject.ExportReferenceNumber))
				.Take(2)
				.ToArray();

			if (matchedByReferences.Length > 1)
			{
				return null;
			}
			else if (matchedByReferences.Length == 1)
			{
				return matchedByReferences[0];
			}

			return GetMatchingFromDescriptionAndMarks(dataObject, matchedByPackageTypeAndCount);
		}

		ForwardingPackLine GetMatchingFromDescriptionAndMarks(PackingLine dataObject, ForwardingPackLine[] matchedByPackageTypeAndCount)
		{
			ForwardingPackLine[] packLineBOsForMatchingGoodsDescriptionAndMarksNos = null;
			if (!dataObject.ReferenceNumber.HasValue && !dataObject.ExportReferenceNumber.HasValue)
			{
				packLineBOsForMatchingGoodsDescriptionAndMarksNos = matchedByPackageTypeAndCount;
			}
			else
			{
				var matchedWithBothEmptyReferences = matchedByPackageTypeAndCount.Where(x => x.JL_RefNumber.IsEmpty && x.JL_ExportRefNumber.IsEmpty).ToArray();
				var fallbackMatchesForEmptyReferences = matchedByPackageTypeAndCount.Where(x => IsMatchingForNullAndEmptyValueFromDataObject(x.JL_RefNumber, x.JL_ExportRefNumber, dataObject.ReferenceNumber, dataObject.ExportReferenceNumber)).ToArray();

				if (fallbackMatchesForEmptyReferences.Length == 1)
				{
					packLineBOsForMatchingGoodsDescriptionAndMarksNos = matchedWithBothEmptyReferences.Union(fallbackMatchesForEmptyReferences).ToArray();
				}
				else
				{
					packLineBOsForMatchingGoodsDescriptionAndMarksNos = matchedWithBothEmptyReferences;
				}
			}

			var matchedByGoodsDescriptionAndMarksNos = packLineBOsForMatchingGoodsDescriptionAndMarksNos
				.Where(x => IsMatchingValueFromDataObject(x.JL_Description, x.JL_MarksAndNumbers, dataObject.GetCleanSingleLineGoodsDescription(), dataObject.MarksAndNos))
				.Take(2)
				.ToArray();

			return matchedByGoodsDescriptionAndMarksNos.Length == 1
				? matchedByGoodsDescriptionAndMarksNos[0]
				: null;
		}

		bool AreUNDGsMatched(ForwardingPackLine packingLineBO, PackingLine packingLineDO)
		{
			if (packingLineDO.UNDGCollection == null
				|| packingLineDO.UNDGCollection.Count == 0)
			{
				return true;
			}
			else if (packingLineBO.UNDGs.Count == 1
				&& packingLineDO.UNDGCollection.Count == 1)
			{
				return CommonUniversalFreightHelper.IsUNDGDataObjectMatchedToUNDGBusinessObject(packingLineDO.UNDGCollection[0], packingLineBO.UNDGs[0]);
			}

			return false;
		}

		bool IsMatchingByPackageTypeAndCount(PackingLine packLineDO, ForwardingPackLine packLineBO)
		{
			return packLineDO.PackQty.HasValue
				&& packLineDO.PackType != null
				&& packLineDO.PackType.Code.HasValue
				&& !packLineDO.PackQty.Value.IsEmpty
				&& packLineBO.JL_PackageCount == packLineDO.PackQty.Value
				&& packLineBO.JL_F3_NKPackType == packLineDO.PackType.GetCodeAsUpperCase();
		}

		bool IsMatchingForNullAndEmptyValueFromDataObject(ZString boValue1, ZString boValue2, ZString? doValue1ToMatch, ZString? doValue2ToMatch)
		{
			var isValue1NullOrMatchedToEmpty = !doValue1ToMatch.HasValue || (boValue1.IsEmpty && boValue1 == doValue1ToMatch.Value);
			var isValue2NullOrMatchedToEmpty = !doValue2ToMatch.HasValue || (boValue2.IsEmpty && boValue2 == doValue2ToMatch.Value);

			return isValue1NullOrMatchedToEmpty
				&& isValue2NullOrMatchedToEmpty
				&& !(boValue1.IsEmpty && boValue2.IsEmpty)
				&& (doValue1ToMatch.HasValue || doValue2ToMatch.HasValue);
		}

		bool IsMatchingValueFromDataObject(ZString boValue1, ZString boValue2, ZString? doValue1ToMatch, ZString? doValue2ToMatch)
		{
			var isValue1Matched = doValue1ToMatch.HasValue && boValue1 == doValue1ToMatch.Value;
			var isValue2Matched = doValue2ToMatch.HasValue && boValue2 == doValue2ToMatch.Value;

			var isOnlyValue1Matched = !doValue2ToMatch.HasValue && !boValue1.IsEmpty && isValue1Matched;
			var isOnlyValue2Matched = !doValue1ToMatch.HasValue && !boValue2.IsEmpty && isValue2Matched;
			var isBothValuesMatched = isValue1Matched && isValue2Matched && !(boValue1.IsEmpty && boValue2.IsEmpty);

			return isBothValuesMatched || isOnlyValue1Matched || isOnlyValue2Matched;
		}
	}
}
