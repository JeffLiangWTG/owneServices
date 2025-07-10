using CargoWise.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class EntryInstructionProvider
	{
		#region Constructors

		public EntryInstructionProvider(BaseJobDeclaration parentDeclaration, ICusEntryInstructionComparer<CusEntryInstruction> comparer)
		{
			Argument.NotNull(parentDeclaration, nameof(parentDeclaration));
			ParentDeclaration = parentDeclaration;
			EntryInstructionComparer = comparer ?? new CusEntryInstructionComparer();
		}

		public EntryInstructionProvider(BaseJobDeclaration parentDeclaration)
			: this(parentDeclaration, null)
		{
		}

		EntryInstructionProvider()
		{
		}

		#endregion

		#region Properties

		public bool IsNoEntryInstruction => ParentDeclaration == null;

		public ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions
		{
			get
			{
				if (customsEntryInstructions == null && !IsNoEntryInstruction)
				{
					customsEntryInstructions = GetNewCusEntryInstructionCollectionCore();
					customsEntryInstructions.Load();
					ParentDeclaration.RegisterEditableChildObject(customsEntryInstructions);
				}
				return customsEntryInstructions;
			}
		}
		ICusEntryInstructionCollection<CusEntryInstruction> customsEntryInstructions;

		protected virtual ICusEntryInstructionCollection<CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);

		public virtual CodeDescriptionPairList SortedEntryInstructionList
		{
			get
			{
				if (sortedEntryInstructionList == null)
				{
					sortedEntryInstructionList = new CodeDescriptionPairList();
					if (!IsNoEntryInstruction)
					{
						sortedEntryInstructionList.AddRange(CustomsEntryInstructions);
						sortedEntryInstructionList.Sort();
					}
				}
				return sortedEntryInstructionList;
			}
		}
		CodeDescriptionPairList sortedEntryInstructionList;

		public ICusEntryInstructionComparer<CusEntryInstruction> EntryInstructionComparer { get; }

		#endregion

		protected BaseJobDeclaration ParentDeclaration { get; }

		#region Static

		public static EntryInstructionProvider NoEntryInstructionProvider
		{
			get { return new EntryInstructionProvider(); }
		}

		#endregion

		#region Implementation

		public void DeleteAll()
		{
			if (!IsNoEntryInstruction && CustomsEntryInstructions != null)
			{
				CustomsEntryInstructions.DeleteAll();
			}
		}

		public virtual void RefreshSortedEntryInstructionList()
		{
			sortedEntryInstructionList = null;
		}

		#endregion
	}
}
