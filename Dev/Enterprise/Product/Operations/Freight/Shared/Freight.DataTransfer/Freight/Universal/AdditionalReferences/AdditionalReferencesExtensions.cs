using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public static class AdditionalReferencesExtensions
	{
		public static void AddAdditionalReferences(this CusEntryNumAdditionalReferenceCollection additionalReferences, List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			foreach (CusEntryNumber additionalReference in additionalReferences)
			{
				var entryType = additionalReference.CE_EntryType;
				var referenceNumber = additionalReference.CE_EntryNum;
				if (!referenceNumber.IsEmpty
					&& !contextValues.Any(item => item.Key.Type == entryType && item.Value.Equals(referenceNumber)))
				{
					contextValues.Add(new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription(entryType, additionalReference.AdditionalReferenceNumberTypeDescription), referenceNumber));
				}
			}
		}

		public static void PopulateAdditionalReferences(this AdditionalReferencesParent referencesParent, IXmlEventValueObject xmlEvent)
		{
			if (xmlEvent != null && xmlEvent.DataContext != null && xmlEvent.DataContext.CodesMappedToTarget)
			{
				var additionalReferenceTypes = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var additionalReferences = new List<KeyValuePair<ZString, ZString>>();

				var uxmlEvent = xmlEvent as UniversalEvent;

				var contextCollection = uxmlEvent != null
					? uxmlEvent.ContextCollection
					: null;

				if (contextCollection == null)
				{
					return;
				}

				foreach (var contextItem in contextCollection)
				{
					var contextItemType = contextItem.Type;
					if (contextItemType != null)
					{
						var additionalReferenceNumber = contextItem.Value.GetValueOrDefault();
						var additionalReferenceType = contextItemType.Type.GetValueOrDefault();
						if (!additionalReferenceNumber.IsEmpty && additionalReferenceTypes.ContainsCode(additionalReferenceType))
						{
							additionalReferences.Add(new KeyValuePair<ZString, ZString>(additionalReferenceType, additionalReferenceNumber));
						}
					}
				}

				referencesParent.AdditionalReferences = additionalReferences;
			}
		}

		public static void PopulateAdditionalReferences(this AdditionalReferencesParent referencesParent, UniversalShipment shipmentDataObject, bool onlyWhenCodesMappedToTarget = true)
		{
			var dataContext = shipmentDataObject.DataContext;
			if (dataContext != null && (!onlyWhenCodesMappedToTarget || dataContext.CodesMappedToTarget))
			{
				var additionalReferences = new List<KeyValuePair<ZString, ZString>>();

				if (shipmentDataObject.AdditionalReferenceCollection != null)
				{
					foreach (var additionalReferenceDataObject in shipmentDataObject.AdditionalReferenceCollection)
					{
						var additionalReferenceType = additionalReferenceDataObject.Type.GetCodeAsUpperCase();
						var additionalReferenceNumber = additionalReferenceDataObject.ReferenceNumber.GetValueOrDefault();
						if (!additionalReferenceType.IsEmpty && !additionalReferenceNumber.IsEmpty)
						{
							additionalReferences.Add(new KeyValuePair<ZString, ZString>(additionalReferenceType, additionalReferenceNumber));
						}
					}
				}

				referencesParent.AdditionalReferences = additionalReferences;
			}
		}

		public static ZString GetHIREntryNumber(this AdditionalReferencesParent referencesParent)
			=> referencesParent?.GetReferenceEntryNumber(CustomsReferenceNumberType.eHubInterchangeReference.HIR) ?? ZString.Empty;

		public static ZString GetReferenceEntryNumber(this AdditionalReferencesParent referencesParent, ZString entryType)
			=> referencesParent?.AdditionalReferences?.Where(r => r.Key == entryType).FirstOrDefault().Value ?? ZString.Empty;
	}
}
