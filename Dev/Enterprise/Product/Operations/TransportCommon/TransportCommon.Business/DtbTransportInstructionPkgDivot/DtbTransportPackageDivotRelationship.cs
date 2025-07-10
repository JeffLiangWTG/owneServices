using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	// Transport > Instructions > PackageDivots
	// Hook onto Instructions.CountChanged
	// Foreach Instruction Hook onto each Instruction.PackageDivots.CountChanged

	class DtbTransportPackageDivotRelationship<T> : ICollectionRelationship
		where T : DtbTransportInstructionPkgDivot
	{
		public DtbTransportPackageDivotRelationship(DtbTransport transport)
		{
			Transport = transport;
			HookInstructionEvents();
		}

		#region Events

		void HookInstructionEvents()
		{
			// if transport is deleted we should unhook the instructions collection
			// but we shouldn't need to as Transport.Instructions.CountChanged will never be called after the booking is deleted
			Transport.Instructions.CountChanged += new EventHandler(Instructions_CountChanged);
		}

		void Instructions_CountChanged(object sender, EventArgs e)
		{
			foreach (var instruction in Instructions_Listening.ToArray())
			{
				if (!Transport.Instructions.Contains(instruction))
				{
					UnHook(instruction);
				}
			}

			foreach (DtbTransportInstruction instruction in Transport.Instructions)
			{
				if (!Instructions_Listening.Contains(instruction))
				{
					Hook(instruction);
				}
			}

			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		void Hook(DtbTransportInstruction instruction)
		{
			Argument.NotNull(instruction, "instruction");

			Instructions_Listening.Add(instruction);
			instruction.PackageDivots.CountChanged += new EventHandler(PackageDivots_CountChanged);
		}

		void UnHook(DtbTransportInstruction instruction)
		{
			Argument.NotNull(instruction, "instruction");

			Instructions_Listening.Remove(instruction);
			instruction.PackageDivots.CountChanged -= new EventHandler(PackageDivots_CountChanged);
		}

		void PackageDivots_CountChanged(object sender, EventArgs e)
		{
			if (!IsRebuilding)
			{
				OnRefreshed();
			}
		}

		List<DtbTransportInstruction> Instructions_Listening
		{
			get { return instructions_Listening ?? (instructions_Listening = new List<DtbTransportInstruction>()); }
		}

		List<DtbTransportInstruction> instructions_Listening;

		#endregion

		#region ICollectionRelationship Members

		#region Master

		BusinessObject ICollectionRelationship.Master
		{
			get { return Transport; }
		}

		readonly DtbTransport Transport;

		#endregion

		#region RelationshipFilter

		ZQuery ICollectionRelationship.RelationshipFilter
		{
			get { return BuildRelationshipFilter(false); }
		}

		ZQuery BuildRelationshipFilter(bool ignoreActiveFilter)
		{
			ZQuery result = null;

			try
			{
				IsRebuilding = true;

				var instructionPKs = Array.ConvertAll(Transport.Instructions.ToArray<DtbTransportInstruction>(), i => i.PK);
				result = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instructionPKs);
				result.AddToFilter(AdditionalFilter);
				result.IgnoreActiveFilter = ignoreActiveFilter;
				result.ModificationsEnabled = false;
			}
			finally
			{
				IsRebuilding = false;
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
			var result = (DtbTransportPackageDivotRelationship<T>)MemberwiseClone();

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
			return factory.Load<T>(new ZQuery(filter, BuildRelationshipFilter(false)));
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
