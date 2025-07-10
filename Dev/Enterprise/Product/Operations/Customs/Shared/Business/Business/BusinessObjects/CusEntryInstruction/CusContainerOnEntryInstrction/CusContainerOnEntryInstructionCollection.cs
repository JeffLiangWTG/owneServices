using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface ICusContainerOnEntryInstructionCollection<out T> : IBusinessObjectCollection<T>
		where T : CusContainerOnEntryInstruction
	{
		new T this[int index] { get; }
		T[] AllLinkedContainers { get; }
	}

	public class CusContainerOnEntryInstructionCollection<T> : NonPersistentBusinessObjectCollection<T>, ICusContainerOnEntryInstructionCollection<T>
		where T : CusContainerOnEntryInstruction
	{
		public CusContainerOnEntryInstructionCollection(CusEntryInstruction instruction)
				: base(instruction.Factory)
		{
			Instruction = Argument.NotNull(instruction, nameof(instruction));
			if (instruction?.JobDeclaration is BaseJobDeclaration declaration && declaration.SupportContainerEntryInstructionPivot)
			{
				Load();
				if (declaration.CusContainers != null)
				{
					declaration.CusContainers.CountChanged += OnContainerChanged;
				}
			}
		}
		protected readonly CusEntryInstruction Instruction;

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		public void OnContainerChanged(object sender, EventArgs e)
		{
			if (!Instruction.IsDeleted)
			{
				Load();
			}
		}

		public override void Load()
		{
			RemoveAndDeleteAll();
			if (Instruction.JobDeclaration is BaseJobDeclaration declaration && declaration.CusContainers != null)
			{
				foreach (var container in declaration.CusContainers)
				{
					AddNew(container);
				}
			}
		}

		public T AddNew(BaseCusContainer container)
		{
			var result = AddNew();
			result.Container = container;
			return result;
		}

		#region Implementation

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public T[] AllLinkedContainers => this.Cast<T>().Where(x => x.IsForEntry).ToArray();

		protected override BusinessObject CreateNonPersistentBusinessObject() => (T)Activator.CreateInstance(typeof(T), Instruction);
		#endregion
	}
}
