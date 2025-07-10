using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowEventContextValidation : ZValidation
	{
		public WorkflowEventContextValidation(WorkflowEventContext parent)
			: base(parent)
		{
			Parent = parent;
		}

		WorkflowEventContext Parent { get; set; }

		public override Type AutoValidationType
		{
			get { return typeof(WorkflowEventContext); }
		}

		public override void ValidateAll()
		{
			ValidateMasterClassifier();
			ValidateMasterType();
		}

		#region MasterClassifier

		public void ValidateMasterClassifier()
		{
			((IValidationInternals)this).Validate(Parent.MasterClassifierInfo, CheckMasterClassifier);
		}

		protected virtual void CheckMasterClassifier()
		{
			ListValidation.ErrorIfInvalidCode(Parent.MasterClassifierInfo);
		}

		#endregion

		#region MasterType

		public void ValidateMasterType()
		{
			((IValidationInternals)this).Validate(Parent.MasterTypeInfo, CheckMasterType);
		}

		protected virtual void CheckMasterType()
		{
			MandatoryValidation.CheckEntered(Parent.MasterTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MasterTypeInfo);
		}

		#endregion
	}
}
