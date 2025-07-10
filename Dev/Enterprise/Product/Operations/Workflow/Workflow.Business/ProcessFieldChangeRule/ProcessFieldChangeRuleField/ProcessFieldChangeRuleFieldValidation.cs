//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessFieldChangeRuleFieldValidation
//
//    This class should be used for overriding validation in AutoProcessFieldChangeRuleFieldValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Workflow.Business
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.Workflow.Integration;
	using Enterprise.ZArchitecture.Schema;

	public class ProcessFieldChangeRuleFieldValidation : AutoProcessFieldChangeRuleFieldValidation
	{
		public ProcessFieldChangeRuleFieldValidation(AutoProcessFieldChangeRuleField parent) : base(parent)
		{
		}

		protected override void CheckPFL_FieldName()
		{
			base.CheckPFL_FieldName();
			MandatoryValidation.CheckEntered(Parent.PFL_FieldNameInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PFL_FieldNameInfo);
			CheckFieldIsNotAlreadyInGroup();
		}

		void CheckFieldIsNotAlreadyInGroup()
		{
			var rule = ((ProcessFieldChangeRuleField)Parent).Parent;
			if (rule != null)
			{
				var field = Parent.PFL_FieldName;
				var query = new ZQuery(ProcessFieldChangeRuleFieldSchema.PFL_FieldName, field);
				var count = new List<IProcessFieldChangeRuleField>(rule.Fields.Find(query)).Count;
				if (count > 1)
				{
					Parent.PFL_FieldNameInfo.AddError(Res.GetString("95CF854F-2936-43BA-B1BC-EE11ECAB75CB", "{0} is already included", field));
				}
			}
		}
	}
}


