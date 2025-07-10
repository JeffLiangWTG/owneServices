using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	static class WhsDocketEventParentFinderExtensions
	{
		public static void AddDocketReferences(this List<KeyValuePair<TypeWithDescription, IZType>> contextValues, WhsDocket docket)
		{
			foreach (WhsDocketReference reference in docket.References)
			{
				var referenceType = reference.WX_RefType;
				var referenceNumber = reference.WX_Reference;

				if (!referenceNumber.IsEmpty)
				{
					var description = reference.Lookups.ReferenceTypes.GetDescriptionFromCode(referenceType);
					contextValues.Add(new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription(referenceType, description), referenceNumber));
				}
			}
		}

		public static void AddOrderNumber(this List<KeyValuePair<TypeWithDescription, IZType>> contextValues, WhsDocket docket)
			=> contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, docket.WD_ExternalReference);

		public static void AddOrderNumberSplit(this List<KeyValuePair<TypeWithDescription, IZType>> contextValues, WhsDocket docket)
			=> contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumberSplit, docket.WD_ExternalReferenceSplit);
	}
}
