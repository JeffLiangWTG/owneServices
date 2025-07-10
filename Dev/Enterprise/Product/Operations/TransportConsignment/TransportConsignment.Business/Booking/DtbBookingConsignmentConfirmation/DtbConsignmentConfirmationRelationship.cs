using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	// Consignment -> Instructions -> Confirmations
	// Hook onto Consignment.Instructions.CountChanged
	// Foreach Instruction Hook onto Instruction.Confirmations.CountChanged

	public sealed class DtbConsignmentConfirmationRelationship : ICollectionRelationship
	{
		public DtbConsignmentConfirmationRelationship(DtbBookingConsignment consignment)
		{
			Consignment = consignment;
			HookInstructions();
		}

		#region Events

		void HookInstructions()
		{
			// if Consignment is deleted we should unhook the instructions collection
			// but we shouldn't need to as Consignment.Instructions.CountChanged will never be called after the Consignment is deleted
			Consignment.Instructions.CountChanged += new EventHandler(Instructions_CountChanged);
			HookUnhookEachInstruction();
		}

		void Instructions_CountChanged(object sender, EventArgs e)
		{
			HookUnhookEachInstruction();

			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		void HookUnhookEachInstruction()
		{
			foreach (var instruction in Instructions_Listening.ToArray())
			{
				if (!Consignment.Instructions.Contains(instruction))
				{
					UnHook(instruction);
				}
			}

			foreach (var instruction in Consignment.Instructions)
			{
				if (!Instructions_Listening.Contains(instruction))
				{
					Hook(instruction);
				}
			}
		}

		void Hook(DtbConsignmentInstruction instruction)
		{
			Argument.NotNull(instruction, "packageDivot");

			Instructions_Listening.Add(instruction);
			instruction.Confirmations.CountChanged += new EventHandler(Confirmations_CountChanged);
		}

		void UnHook(DtbConsignmentInstruction instruction)
		{
			Argument.NotNull(instruction, "packageDivot");

			Instructions_Listening.Remove(instruction);
			instruction.Confirmations.CountChanged -= new EventHandler(Confirmations_CountChanged);
		}

		void Confirmations_CountChanged(object sender, EventArgs e)
		{
			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		List<DtbConsignmentInstruction> Instructions_Listening
		{
			get { return instructions_Listening ?? (instructions_Listening = new List<DtbConsignmentInstruction>()); }
		}

		List<DtbConsignmentInstruction> instructions_Listening;

		#endregion

		#region ICollectionRelationship Members

		#region Master

		BusinessObject ICollectionRelationship.Master
		{
			get { return Consignment; }
		}

		readonly DtbBookingConsignment Consignment;

		#endregion

		#region RelationshipFilter

		ZQuery ICollectionRelationship.RelationshipFilter
		{
			get { return BuildRelationshipFilter(false); }
		}

		ZQuery BuildRelationshipFilter(bool ignoreActiveFilter)
		{
			ZQuery result = null;

			using (new DisposableAction(
				() => IsRebuilding = true,
				() => IsRebuilding = false))
			{
				var confirmations = new List<DtbConsignmentConfirmation>();
				confirmations.AddRange(Consignment.Instructions.SelectMany(i => i.Confirmations));

				result = new ZQuery(DtbBookingConfirmationSchema.PK, confirmations.Where(c => c.IsPickUp || c.IsDelivery).Select(c => c.PK).ToArray());

				result.AddToFilter(AdditionalFilter);
				result.IgnoreActiveFilter = ignoreActiveFilter;
				result.ModificationsEnabled = false;
			}

			return result;
		}

		bool IsRebuilding;

		bool ICollectionRelationship.MatchesRelationshipFilter(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			Argument.NotNull(businessObject, "businessObject");
			return businessObject.MatchesFilter(BuildRelationshipFilter(ignoreActiveFilter));
		}

		#endregion

		#region AdditionalFilter

		ICollectionRelationship ICollectionRelationship.AddFilter(ZQuery additionalFilter)
		{
			var result = (DtbConsignmentConfirmationRelationship)MemberwiseClone();

			if (additionalFilter != null)
			{
				result.AdditionalFilter = new ZQuery(AdditionalFilter, additionalFilter);
				result.AdditionalFilter.ModificationsEnabled = false;
			}

			return result;
		}

		ZQuery AdditionalFilter;

		#endregion

		#region Refreshed

		void OnRefreshed()
		{
			if (Refreshed != null)
			{
				Refreshed(this, EventArgs.Empty);
			}
		}

		event EventHandler ICollectionRelationship.RelationshipFilterChanged
		{
			add { Refreshed += value; }
			remove { Refreshed -= value; }
		}

		event EventHandler Refreshed;

		#endregion

		#region LoadBusinessObjects

		BusinessObject[] ICollectionRelationship.LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter)
		{
			return factory.Load<DtbConsignmentConfirmation>(new ZQuery(filter, BuildRelationshipFilter(false)));
		}

		#endregion

		#region HasChangesIncludingRelationship

		bool ICollectionRelationship.HasChangesIncludingRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			return businessObject.HasChanges;
		}

		#endregion

		#region ClearHasChangesIncludingRelationship

		void ICollectionRelationship.ClearHasChangesIncludingRelationship(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			IBusinessObjectState businessObjectState = businessObject;
			businessObjectState.ClearHasChangesIncludingChildren();
		}

		#endregion

		#region SupportsAddToRelationship

		bool ICollectionRelationship.SupportsAddToRelationship()
		{
			return false;
		}

		#endregion

		#region Add To / Remove From Relationship

		void ICollectionRelationship.AddToRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Can't add using this relationship");
		}

		void ICollectionRelationship.RemoveFromRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Can't remove using this relationship");
		}

		#endregion

		#region Clear

		void ICollectionRelationship.Clear()
		{
			throw new InvalidOperationException("Can't clear using this relationship");
		}

		#endregion

		#endregion
	}
}
