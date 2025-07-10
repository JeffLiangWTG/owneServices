using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class AdditionalReferencesMatchingHelper<T> : CombinationKeyMatcherCore<T>
		where T : BusinessObject
	{
		public AdditionalReferencesMatchingHelper(BusinessObjectFactory factory) : base(factory) { }

		public delegate void AddToQueryDelegate(ZQuery query);

		public IEnumerable<(string KeyValue, string KeySource)> GetKeysFromAdditionalReferences(AdditionalReferencesParent referencesParent)
		{
			foreach (var pair in GetCusEntryNumberReferenceParts(referencesParent?.AdditionalReferences))
			{
				const string magicIdentifier = "CCUS"; // This is an arbitrary string so that we can visually differentiate between logs
				yield return (magicIdentifier + pair.Key + pair.Value, $"{referencesParent.GetType().Name}/AdditionalReferences");
			}
		}

		IEnumerable<KeyValuePair<ZString, ZString>> GetCusEntryNumberReferenceParts(List<KeyValuePair<ZString, ZString>> additionalReferences)
		{
			return GetEntryNumberReferencePartsCore(additionalReferences, FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value.GetCodeDescriptionPairList());
		}

		IEnumerable<KeyValuePair<ZString, ZString>> GetNonCustomsEntryNumberReferenceParts(List<KeyValuePair<ZString, ZString>> additionalReferences)
		{
			return GetEntryNumberReferencePartsCore(additionalReferences, ShipmentNonCustomsAdditionalReferenceNumberTypes);
		}

		IEnumerable<KeyValuePair<ZString, ZString>> GetEntryNumberReferencePartsCore(List<KeyValuePair<ZString, ZString>> additionalReferences, CodeDescriptionPairList additionalReferenceNumberTypes)
		{
			foreach (var additionalReference in additionalReferences ?? Enumerable.Empty<KeyValuePair<ZString, ZString>>())
			{
				var key = additionalReference.Key;
				if (key.Length <= 3 && additionalReferenceNumberTypes.ContainsCode(key))
				{
					var additionalValue = additionalReference.Value;
					if (DoesValueLookLikeAForeignKey(additionalValue))
					{
						yield return additionalReference;
					}
				}
			}
		}

		CodeDescriptionPairList ShipmentNonCustomsAdditionalReferenceNumberTypes => new ShipmentNonCustomsAdditionalReferenceCodesCodeList();

		bool DoesValueLookLikeAForeignKey(ZString additionalValue)
		{
			return additionalValue.Length >= 5; // This is a bit bad and I am sorry.
		}

		public List<ZGuid> GetParentsMatchingAdditionalReferences(AdditionalReferencesParent referencesParent, AddToQueryDelegate addToQuery)
		{
			return GetParentsMatchingAdditionalReferences(referencesParent?.AdditionalReferences, addToQuery);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public List<ZGuid> GetParentsMatchingAdditionalReferences(List<KeyValuePair<ZString, ZString>> additionalReferences, AddToQueryDelegate addToQuery)
		{
			var cusEntryNumberParts = GetCusEntryNumberReferenceParts(additionalReferences).ToArray();
			var pairs = cusEntryNumberParts.Any() ? cusEntryNumberParts : GetNonCustomsEntryNumberReferenceParts(additionalReferences).ToArray();
			if (!pairs.Any())
			{
				return null;
			}

			var additionalReferencesSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);

			additionalReferencesSubQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);

			var additionalReferencesSubSubQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));

			foreach (var additionalReference in pairs)
			{
				var additionalReferenceSubQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
				additionalReferenceSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, additionalReference.Key);
				additionalReferenceSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, additionalReference.Value);

				additionalReferencesSubSubQuery.AddToFilter(additionalReferenceSubQuery, JoinCondition.Or);
			}

			additionalReferencesSubQuery.AddToFilter(additionalReferencesSubSubQuery);

			var parentQuery = new ZDBOnlyQuery(typeof(T));
			if (addToQuery != null)
			{
				addToQuery(parentQuery);
			}

			parentQuery.AddSubQuery(additionalReferencesSubQuery, JoinCondition.And);

			return GetParentPKs(parentQuery);
		}
	}
}
