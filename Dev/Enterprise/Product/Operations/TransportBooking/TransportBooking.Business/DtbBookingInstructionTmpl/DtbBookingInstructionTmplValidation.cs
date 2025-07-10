using System;
using System.Linq;
using CargoWise.EntityFramework;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInstructionTmplValidation : Common.DtbBookingInstructionTmplValidation
	{
		public DtbBookingInstructionTmplValidation(DtbBookingInstructionTmpl parent)
			: base(parent)
		{
		}

		protected override void CheckK2_OrgType()
		{
			base.CheckK2_OrgType();
			MandatoryValidation.CheckEntered(Parent.K2_OrgTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.K2_OrgTypeInfo);
		}

		protected override void CheckK2_InstructionType()
		{
			base.CheckK2_InstructionType();
			MandatoryValidation.CheckEntered(Parent.K2_InstructionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.K2_InstructionTypeInfo);
		}

		protected override void CheckK2_Sequence()
		{
			base.CheckK2_Sequence();
			if (InstructionTemplate.Template != null)
			{
				CheckSequence(InstructionTemplate.Template.Instructions);
			}
		}

		void CheckSequence(DtbBookingInstructionTmplCollection instructions)
		{
			if (instructions.Count > 0)
			{
				var instructionsArray = instructions.ToArray();
				Array.Sort(instructionsArray, (t1, t2) => t1.K2_Sequence.CompareTo(t2.K2_Sequence));

				if (Parent == instructionsArray[0] && Parent.K2_Sequence != 1)
				{
					Parent.K2_SequenceInfo.AddError(Res.GetString("e17d9e77-7d19-4aba-8352-07703f407b74", "The first Instruction should have a sequence number of 1."));
				}
				else
				{
					for (int index = 0; index < instructionsArray.Length; index++)
					{
						var currentInstruction = instructionsArray[index];
						if (Parent == currentInstruction && Parent.K2_Sequence != index + 1)
						{
							Parent.K2_SequenceInfo.AddError(Res.GetString("2b1f93d0-4ab2-4a78-955a-99cc6852eb13", "Sequence must be sequential integers, continuously increasing by one."));
							break;
						}
					}
				}
			}
		}

		protected override void CheckK2_DropMode()
		{
			base.CheckK2_DropMode();
			ListValidation.ErrorIfInvalidCode(Parent.K2_DropModeInfo);
		}

		protected override void CheckK2_IsLooseRateable()
		{
			base.CheckK2_IsLooseRateable();

			if (Parent.K2_IsLooseRateable && InstructionTemplate.Template != null)
			{
				var numberOfLooseRateable = InstructionTemplate.Template.Instructions.Count(i => i.K2_IsLooseRateable);
				if (numberOfLooseRateable == 1)
				{
					Parent.K2_IsLooseRateableInfo.AddError(Res.GetString("cd6f7421-2ce8-4c82-9d5e-2462b3233df9", "A Rating Instruction Pair can only consist of 2 Instructions."));
				}
			}
		}

		protected override void CheckK2_IsContainerRateable()
		{
			base.CheckK2_IsContainerRateable();

			if (Parent.K2_IsContainerRateable && InstructionTemplate.Template != null)
			{
				var numberOfContainerRateable = InstructionTemplate.Template.Instructions.Count(i => i.K2_IsContainerRateable);
				if (numberOfContainerRateable == 1)
				{
					Parent.K2_IsContainerRateableInfo.AddError(Res.GetString("cd6f7421-2ce8-4c82-9d5e-2462b3233df9", "A Rating Instruction Pair can only consist of 2 Instructions."));
				}
			}
		}

		protected override void CheckK2_PackageType()
		{
			base.CheckK2_PackageType();
			ListValidation.ErrorIfInvalidCode(Parent.K2_PackageTypeInfo);
		}

		public DtbBookingInstructionTmpl InstructionTemplate
		{
			get { return (DtbBookingInstructionTmpl)Parent; }
		}
	}
}
