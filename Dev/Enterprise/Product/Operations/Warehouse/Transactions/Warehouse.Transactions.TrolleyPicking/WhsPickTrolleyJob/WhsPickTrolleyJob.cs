using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsPickTrolleyJob : AutoWhsPickTrolleyJob, IWhsPickTrolleyJob, ICriticalChangesVersionID
	{
		public WhsPickTrolleyJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WTJ_CriticalChangesVersionID), ConcurrencyPolicy.Strict);
		}

		#region Slots

		[ChildEditable(true)]
		public WhsPickTrolleySlotCollection Slots
		{
			get
			{
				if (slots == null)
				{
					slots = new WhsPickTrolleySlotCollection(this);
					RegisterEditableChildObject(slots);
				}
				return slots;
			}
		}

		WhsPickTrolleySlotCollection slots;

		#endregion

		#region Properties

		public override ZString WTJ_Status
		{
			get { return base.WTJ_Status; }
			set
			{
				if (WTJ_Status != value)
				{
					if (value == PickTrolleyStatus.Codes.Finalised)
					{
						WTJ_FinalisedDateUtc = ZDateTime.UtcNow;
						UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsPickTrolleyJob>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
					}
					else if (WTJ_Status == PickTrolleyStatus.Codes.Finalised)
					{
						WTJ_FinalisedDateUtc = ZDateTime.Empty;
					}

					base.WTJ_Status = value;
					Slots.ForEach(s => s.Package?.ClearActionStrategyCacheIncludingChildren());
				}
			}
		}

		public TrolleyPickingType PickingType
		{
			get
			{
				TrolleyPickingType result;

				if (Slots.Count == 0 || Slots.All(s => s.Package == null))
				{
					result = TrolleyPickingType.None;
				}
				else if (Slots.Any(s => s.Package?.GetIsTote() ?? false))
				{
					result = TrolleyPickingType.Tote;
				}
				else
				{
					result = TrolleyPickingType.Carton;
				}

				return result;
			}
		}

		ZGuid ICriticalChangesVersionID.CriticalChangesVersionID
		{
			set => WTJ_CriticalChangesVersionID = value;
			get => WTJ_CriticalChangesVersionID;
		}

		public bool IsImmutableStatus => WTJ_FinalisedDateUtc.IsValid;

		#endregion
	}
}

// Add tests to TrolleyPicking.Testing project.
