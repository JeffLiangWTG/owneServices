using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class NotesChecker
	{
		protected NotesChecker(EnterpriseBusinessObject parent)
		{
			this.parent = parent;
		}

		readonly EnterpriseBusinessObject parent;

		#region Presence of Instruction Notes

		public ZBool HasSpecialInstructions => HasInstructionNotesOfType(PredefinedNoteTypes.Instance.SpecialInstructions.Description);

		public ZBool HasGoodsHandlingInstructions => HasInstructionNotesOfType(PredefinedNoteTypes.Instance.HandlingInstructions.Description);

		ZBool HasInstructionNotesOfType(string instructionDescription)
		{
			return parent.Notes.FindByDescription(instructionDescription, true).Any();
		}

		#endregion

		#region Fetch Hints

		public void AddNotesIncludingRelatedFetchHints()
		{
			foreach (var businessObject in parent.BusinessObjectsWithRelatedNotes)
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, businessObject.PK);
			}

			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, parent.PK);
		}

		public abstract void AddBusinessObjectsWithRelatedNotesFetchHints();

		#endregion

		#region Factory

		protected BusinessObjectFactory Factory => factory ?? (factory = parent.Factory);
		BusinessObjectFactory factory;

		#endregion
	}
}
