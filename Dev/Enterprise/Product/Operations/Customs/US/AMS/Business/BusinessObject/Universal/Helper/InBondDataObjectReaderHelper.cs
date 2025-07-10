using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class InBondDataObjectReaderHelper : DataTransfer.Universal.InBondDataObjectReaderHelper
	{
		public InBondDataObjectReaderHelper(UniversalObjectFactory factory, string dataProviderForCodeMapping = null)
			: base(factory, dataProviderForCodeMapping)
		{
		}

		protected override IEnumerable<Customs.Business.CusInBondBill> GetAllInBondBills(Customs.Business.CusInBondHeader header)
		{
			var amsInBondHeader = (CusInBondHeader)header;
			var result = new List<Customs.Business.CusInBondBill>();
			result.AddRange(base.GetAllInBondBills(header));
			if (amsInBondHeader.OceanBill is Customs.Business.CusInBondBill oceanBill && oceanBill.IsInDatabase)
			{
				result.Add(oceanBill);
			}
			return result;
		}

		protected override IEnumerable<Customs.Business.CusInBondMoveHeader> GetAllInBondMoveHeaders(Customs.Business.CusInBondHeader header)
		{
			var result = new List<CusInBondMoveHeader>();
			var amsInBondHeader = (CusInBondHeader)header;
			result.AddRange(amsInBondHeader.MovementHeaders.OfType<CusInBondMoveHeader>());
			result.AddRange(amsInBondHeader.PTTMovements.OfType<CusInBondMoveHeader>());
			result.AddRange(amsInBondHeader.InBondMovementHeaders.OfType<CusInBondMoveHeader>());
			return result;
		}

		public void MarkUnprocessedExistingMoveDetailsFor(CusInBondMoveHeader header)
		{
			foreach (var moveDetail in GetAllInBondMoveDetails(header))
			{
				if (!ExistingMoveDetailProcessingDictionary.ContainsKey(moveDetail))
				{
					ExistingMoveDetailProcessingDictionary.Add(moveDetail, false);
				}
			}
		}

		public void MarkProcessed(CusInBondMoveDetail moveDetail)
		{
			if (ExistingMoveDetailProcessingDictionary.ContainsKey(moveDetail))
			{
				ExistingMoveDetailProcessingDictionary[moveDetail] = true;
			}
		}

		public void DeleteUnprocessedMoveDetailsFor(CusInBondMoveHeader header, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingMoveDetailProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.B9_BM == header.PK)
				{
					var moveDetail = pair.Key;
					ExistingMoveDetailProcessingDictionary.Remove(moveDetail);
					LogDelete(logger, moveDetail);
					moveDetail.Delete();
				}
			}
		}

		Dictionary<CusInBondMoveDetail, bool> ExistingMoveDetailProcessingDictionary
		{
			get { return existingMoveDetailProcessingDictionary ?? (existingMoveDetailProcessingDictionary = new Dictionary<CusInBondMoveDetail, bool>()); }
		}
		Dictionary<CusInBondMoveDetail, bool> existingMoveDetailProcessingDictionary;

		protected virtual IEnumerable<CusInBondMoveDetail> GetAllInBondMoveDetails(CusInBondMoveHeader header)
		{
			return header.MovementDetails.OfType<CusInBondMoveDetail>();
		}

		public void MarkUnprocessedExistingContainersFor(CusInBondMoveDetail header)
		{
			foreach (var moveDetail in header.Containers)
			{
				if (!ExistingContainerProcessingDictionary.ContainsKey(moveDetail))
				{
					ExistingContainerProcessingDictionary.Add(moveDetail, false);
				}
			}
		}

		public void MarkProcessed(CusInBondContainer container)
		{
			if (ExistingContainerProcessingDictionary.ContainsKey(container))
			{
				ExistingContainerProcessingDictionary[container] = true;
			}
		}

		public void DeleteUnprocessedContainersFor(CusInBondMoveDetail header, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingContainerProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.BC_ParentID == header.PK)
				{
					var moveDetail = pair.Key;
					ExistingContainerProcessingDictionary.Remove(moveDetail);
					LogDelete(logger, moveDetail);
					moveDetail.Delete();
				}
			}
		}

		Dictionary<CusInBondContainer, bool> ExistingContainerProcessingDictionary
		{
			get { return existingContainerProcessingDictionary ?? (existingContainerProcessingDictionary = new Dictionary<CusInBondContainer, bool>()); }
		}
		Dictionary<CusInBondContainer, bool> existingContainerProcessingDictionary;

		public void ThrowReadFailureExceptionWhenNonSupportedElementsFound(Shipment shipment, Action<List<string>> addNonSupportedElements)
		{
			if (shipment != null && shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.GetValueOrDefault())
			{
				var elementsNotSupported = new List<string>();
				addNonSupportedElements(elementsNotSupported);

				if (elementsNotSupported.Count > 0)
				{
					var nonSupportedElementsBuilder = new ZStringBuilder();
					nonSupportedElementsBuilder.AppendLine();
					elementsNotSupported.ForEach(e => nonSupportedElementsBuilder.AppendLine("· <" + e + ">"));

					throw new DataObjectReadFailureException($@"The following elements are not supported when 'AllowUpdateOfCustomsDeclarationAfterCommencement' is flagged as true. {nonSupportedElementsBuilder}");
				}
			}
		}
	}
}
