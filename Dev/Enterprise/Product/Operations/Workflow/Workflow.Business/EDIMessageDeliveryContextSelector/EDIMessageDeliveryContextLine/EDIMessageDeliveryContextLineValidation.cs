//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessageDeliveryContextLineValidation
//
//    This class should be used for overriding validation in AutoEDIMessageDeliveryContextLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Workflow.Business
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;

	public class EDIMessageDeliveryContextLineValidation : AutoEDIMessageDeliveryContextLineValidation
	{
		public EDIMessageDeliveryContextLineValidation(AutoEDIMessageDeliveryContextLine parent) : base(parent)
		{
			Parent = parent as EDIMessageDeliveryContextLine;
		}

		protected readonly new EDIMessageDeliveryContextLine Parent;

		#region ContextType

		protected override void CheckECL_ContextType()
		{
			base.CheckECL_ContextType();

			var info = Parent.ECL_ContextTypeInfo;
			MandatoryValidation.CheckEntered(info);

			var value = info.Value.ToString();
			if (value.Any(Char.IsWhiteSpace))
			{
				info.AddError(Res.GetString("C80F3641-1132-4C6E-A058-B0A815787754", "{0} cannot contain whitespace.", info.HumanReadableName));
			}

			if (Parent.Parent.Lines.Cast<EDIMessageDeliveryContextLine>().Any(l => l.ECL_ContextType == Parent.ECL_ContextType && l != Parent))
			{
				info.AddError(Res.GetString("1A6380B3-4F99-4CC6-BD8F-705AFC769D88", "{0} must be unique. Duplicate: {1}.", info.HumanReadableName, info.Value));
			}
		}

		#endregion

		#region Value

		protected override void CheckECL_Value()
		{
			base.CheckECL_Value();

			var info = Parent.ECL_ValueInfo;
			MandatoryValidation.CheckEntered(info);
		}

		#endregion
	}
}
