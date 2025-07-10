using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	internal class DtbConsignmentRunSheetInstructionEventParentFinder : EventParentFinder
	{
		public DtbConsignmentRunSheetInstructionEventParentFinder(DtbConsignmentRunSheetInstructionDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var result = new List<BusinessObject>();
			var matchingFields = new TransportMatchingFields(eventDataObject, factory);

			var transportReference = matchingFields.TransportReference;
			var eventCode = eventDataObject.EventType;
			if (!string.IsNullOrEmpty(transportReference) && eventCode.HasValue && (eventCode.Value == AutoEvents.FreightLoadedCode || eventCode.Value == AutoEvents.FreightUnloadedCode))
			{
				var isDelivery = eventCode.Value == AutoEvents.FreightUnloadedCode;

				var runSheet = FindMatchingRunSheetFromRunSheetNumber(transportReference);
				var instruction = runSheet?.RunSheetInstructions.OrderBy(i => i.K1_Sequence).FirstOrDefault(i => i.IsOwnDepot && (isDelivery ? i.IsDeliveringConsignments : i.IsPickingUpConsignments) && matchingFields.IsMatchingAddress(i.Address));

				if (instruction != null)
				{
					result.Add(instruction);
				}
			}
			return result.ToArray();
		}

		DtbConsignmentRunSheet FindMatchingRunSheetFromRunSheetNumber(ZString runSheetNumber)
		{
			var runSheetQuery = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));
			runSheetQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_RunSheetNumber, runSheetNumber);

			return factory.LoadTop1<DtbConsignmentRunSheet>(runSheetQuery);
		}
	}
}
