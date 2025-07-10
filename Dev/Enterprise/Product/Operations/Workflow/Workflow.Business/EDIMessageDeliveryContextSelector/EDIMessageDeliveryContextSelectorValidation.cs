//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessageDeliveryContextSelectorValidation
//
//    This class should be used for overriding validation in AutoEDIMessageDeliveryContextSelectorValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Workflow.Business
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class EDIMessageDeliveryContextSelectorValidation : AutoEDIMessageDeliveryContextSelectorValidation
	{
		public EDIMessageDeliveryContextSelectorValidation(AutoEDIMessageDeliveryContextSelector parent) : base(parent)
		{
		}

		protected override void CheckECS_Code()
		{
			base.CheckECS_Code();
			var info = Parent.ECS_CodeInfo;
			MandatoryValidation.CheckEntered(info);

			if (info.Value.ToString().Length != info.MaxLength)
			{
				info.AddError(Res.GetString("BD1AFE81-FA5E-4A10-B0D7-E67030DD27F3", "{0} must be {1} characters in length.", info.HumanReadableName, info.MaxLength));
			}
			else if (info.Value.ToString().Any(Char.IsWhiteSpace))
			{
				info.AddError(Res.GetString("C31C43BD-48DE-4BFE-9F34-18C3FD503FAC", "{0} cannot contain whitespace.", info.HumanReadableName));
			}

			var duplicates = Parent.Factory.Load<EDIMessageDeliveryContextSelector>(new ZQuery(EDIMessageDeliveryContextSelectorSchema.ECS_Code, Parent.ECS_Code)).Length;
			if (duplicates > 1)
			{
				AddUniquenessError(info);
			}
		}

		protected override void CheckECS_Description()
		{
			base.CheckECS_Description();
			MandatoryValidation.CheckEntered(Parent.ECS_DescriptionInfo);

			var duplicates = Parent.Factory.Load<EDIMessageDeliveryContextSelector>(new ZQuery(EDIMessageDeliveryContextSelectorSchema.ECS_Description, Parent.ECS_Description)).Length;
			if (duplicates > 1)
			{
				AddUniquenessError(Parent.ECS_DescriptionInfo);
			}
		}

		protected override void CheckECS_ProcessType()
		{
			base.CheckECS_ProcessType();
			MandatoryValidation.CheckEntered(Parent.ECS_ProcessTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ECS_ProcessTypeInfo);
		}

		void AddUniquenessError(ZPropertyInfo info)
		{
			info.AddError(Res.GetString("3DD827E7-97EF-42AF-82B8-0AEFECB9237E", "{0} must be unique. Duplicate: {1}.", info.HumanReadableName, info.Value));
		}
	}
}
