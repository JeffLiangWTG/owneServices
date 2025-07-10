using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public class CusContainerOnEntryInstruction : AutoCusContainerOnEntryInstruction
	{
		public CusContainerOnEntryInstruction(CusEntryInstruction instruction)
				: base(instruction.Factory)
		{
			Instruction = Argument.NotNull(instruction, nameof(instruction));
		}
		protected readonly CusEntryInstruction Instruction;

		#region related objects

		BaseCusContainer fContainer;
		public BaseCusContainer Container
		{
			get => fContainer != null && !fContainer.IsDeleted ? fContainer : null;
			set => fContainer = value;
		}

		public CusContainerEntryInstructionPivot Pivot
		{
			get { return Container != null ? Instruction.ContainersPivot.GetRelatedPivot(Container) : null; }
		}

		#endregion

		#region Properties

		[ResourceStringData("Enterprise.Customs.Business.NonPersistentCusContainerEntryInstructionPivot|ContainerNumber", Caption = "Container Number")]
		public override ZString ContainerNumber => Container?.CO_ContainerNumber ?? ZString.Empty;

		[BusinessObjectTestExclude()]
		[ResourceStringData("Enterprise.Customs.Business.NonPersistentCusContainerEntryInstructionPivot|IsForEntry", Caption = "Is For Entry?")]
		public override ZBool IsForEntry
		{
			get { return Pivot != null; }
			set
			{
				if (IsForEntry != value)
				{
					Instruction.ToggleLinkageWithContainer(Container, value);
					base.IsForEntry = value;
				}
			}
		}

		#endregion

		#region override

		protected override void AddToFactoryCache()
		{
			//DO NOT Allow memory to be held up by factory
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
		}

		protected sealed override void OnFactorySaving()
		{
		}

		public sealed override void OnSaved(bool saveSucceeded)
		{
		}

		public sealed override void OnSaving()
		{
		}

		#endregion
	}
}
