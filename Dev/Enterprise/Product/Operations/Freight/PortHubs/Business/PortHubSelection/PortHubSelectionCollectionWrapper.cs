using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubSelectionCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PortHubSelectionCollectionWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Related Objects

		[ChildEditable(true)]
		public PortHubSelectionCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new PortHubSelectionCollection(Factory);
					collection.Load();

					RegisterEditableChildObject(collection);
				}

				return collection;
			}
		}
		PortHubSelectionCollection collection;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDuplicateSelections();
		}

		void ValidateDuplicateSelections()
		{
			var error = Res.GetString("3749fcf0-fd5b-4db5-ab05-958b28d34d8e", "There exists another Selection matching these Conditions and at least one matching Zone.");

			foreach (var selection in Collection)
			{
				selection.RemoveRowError(error);
			}

			foreach (var group in Collection.Cast<PortHubSelection>().GroupBy(x => x, new PortHubComparerForValidation()))
			{
				var zones = new Dictionary<ZGuid, PortHubSelection>();

				foreach (var selection in group)
				{
					var zonePKs = selection.PortHubZonePivots.Cast<PortHubZonePivot>().Select((p) => p.TX_TZ_Zone).Distinct();
					foreach (var zonePK in zonePKs)
					{
						if (zones.ContainsKey(zonePK))
						{
							var originalSelection = zones[zonePK];
							if (!originalSelection.HasRowErrors)
							{
								originalSelection.AddRowError(error);
							}
							if (!selection.HasRowErrors)
							{
								selection.AddRowError(error);
							}
						}
						else
						{
							zones.Add(zonePK, selection);
						}
					}
				}
			}
		}

		class PortHubComparerForValidation : IEqualityComparer<PortHubSelection>
		{
			public bool Equals(PortHubSelection selection1, PortHubSelection selection2)
			{
				var selection1Properties = GetPropertiesForDuplicateValidation(selection1);
				var selection2Properties = GetPropertiesForDuplicateValidation(selection2);
				return selection1Properties.Zip(selection2Properties, (property1, property2) => property1.Equals(property2)).All(x => x);
			}

			public int GetHashCode(PortHubSelection selection)
			{
				return GetPropertiesForDuplicateValidation(selection).Aggregate(0, (hashCode, property) => hashCode ^ property.GetHashCode());
			}

			IEnumerable<IZType> GetPropertiesForDuplicateValidation(PortHubSelection selection)
			{
				yield return selection.TY_Direction;
				yield return selection.TY_UndgClass;
				yield return selection.TY_RS_NKServiceLevel;
				yield return selection.TY_F3_NKPackType;
				yield return selection.TY_OA_DispatchDepotAddress;
				yield return selection.TY_PackMode;
			}
		}

		#endregion
	}
}
