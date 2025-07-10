//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessFieldChangeRuleValidation
//
//    This class should be used for overriding validation in AutoProcessFieldChangeRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Workflow.Business
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class ProcessFieldChangeRuleValidation : AutoProcessFieldChangeRuleValidation
	{
		public ProcessFieldChangeRuleValidation(AutoProcessFieldChangeRule parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			using (CheckFields())
			{
				base.ValidateAll();
			}
		}

		protected override void CheckPFR_ProcessType()
		{
			base.CheckPFR_ProcessType();
			MandatoryValidation.CheckEntered(Parent.PFR_ProcessTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PFR_ProcessTypeInfo);
		}

		protected override void CheckPFR_GroupName()
		{
			base.CheckPFR_GroupName();
			MandatoryValidation.CheckEntered(Parent.PFR_GroupNameInfo);
			CheckGroupNameIsUnique();
			CheckHasAtLeastOneField();
		}
		bool checkFields;

		IDisposable CheckFields()
		{
			checkFields = true;
			return new DisposableAction(() => checkFields = false);
		}

		void CheckHasAtLeastOneField()
		{
			if (checkFields && ((ProcessFieldChangeRule)Parent).Fields.Count == 0)
			{
				Parent.PFR_GroupNameInfo.AddError(Res.GetString("1377E1C4-6608-4C6B-8B2C-C61C7289105A", "This field change event must contain at least 1 field."));
			}
		}

		protected override void CheckPFR_SE_NKEvent()
		{
			base.CheckPFR_SE_NKEvent();
			CheckTypeEventReferenceIsUnique();
			MandatoryValidation.CheckEntered(Parent.PFR_SE_NKEventInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PFR_SE_NKEventInfo);
		}

		protected override void CheckPFR_Reference()
		{
			base.CheckPFR_Reference();
			ValidatePFR_SE_NKEvent();
		}

		void CheckGroupNameIsUnique()
		{
			var factory = Parent.Factory;
			var duplicatesQuery = new ZQuery(ProcessFieldChangeRuleSchema.PFR_GroupName, Parent.PFR_GroupName);
			var results = factory.Load<ProcessFieldChangeRule>(duplicatesQuery).Length;

			if (results > 1)
			{
				Parent.PFR_GroupNameInfo.AddError(Res.GetString("5B6BEBCA-A44D-4D75-8EA6-4E3D6B605AA3", "{0} is already used for a field change event name.", Parent.PFR_GroupName));
			}
		}

		void CheckTypeEventReferenceIsUnique()
		{
			var duplicatesQuery = new ZQuery(ProcessFieldChangeRuleSchema.PFR_SE_NKEvent, Parent.PFR_SE_NKEvent);
			duplicatesQuery.AddToFilter(ProcessFieldChangeRuleSchema.PFR_ProcessType, Parent.PFR_ProcessType);
			duplicatesQuery.AddToFilter(ProcessFieldChangeRuleSchema.PFR_Reference, Parent.PFR_Reference);
			duplicatesQuery.AddToFilter(ProcessFieldChangeRuleSchema.PFR_IsActive, true);

			var results = Parent.Factory.Load<ProcessFieldChangeRule>(duplicatesQuery).Length;
			if (results > 1)
			{
				Parent.PFR_SE_NKEventInfo.AddError(Res.GetString("62BCD1A5-4139-428C-A424-08D2B2C11BE0", "Workflow Type, Event Code and Reference must be unique on Field Change Rule. The duplicate values are ({0},{1},{2})", Parent.PFR_ProcessType, Parent.PFR_SE_NKEvent, Parent.PFR_Reference));
			}
		}
	}
}

