using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Workflow.Business
{
	[DescriptionProperty(ZArchitecture.Schema.UniversalValidationRuleSetSchema.Constants.VRS_Name)]
	[CodeProperty(ZArchitecture.Schema.UniversalValidationRuleSetSchema.Constants.VRS_Code)]
	public class UniversalValidationRuleSet : AutoUniversalValidationRuleSet
	{
		public UniversalValidationRuleSet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(UniversalValidationRuleSetLookups.DataContextList))]
		public override ZString VRS_DataContext { get => base.VRS_DataContext; set => base.VRS_DataContext = value; }

		[ReadOnlyMember(nameof(VRS_Criteria_ReadOnly))]
		public override ZString VRS_Criteria { get => base.VRS_Criteria; set => base.VRS_Criteria = value; }

		bool VRS_Criteria_ReadOnly => VRS_AlwaysApply;

		[ChildEditable]
		public UniversalValidationRuleCollection Rules
		{
			get
			{
				if (rules == null)
				{
					rules = new UniversalValidationRuleCollection(this);
					RegisterEditableChildObject(rules);
				}
				return rules;
			}
		}
		UniversalValidationRuleCollection rules;

		#region Save / Delete

		public override void OnSaving()
		{
			SetJobNumberIfRequired();
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				VRS_Code = "";
			}
		}

		public override void Delete()
		{
			Rules.DeleteAll();

			base.Delete();
		}

		public void SetJobNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(VRS_CodeInfo, JobNumberFountain);
			}
		}

		protected virtual INumberFountainProxy JobNumberFountain
		{
			get { return Env.NumberFountains.UniversalValidationRuleSetNumber; }
		}

		#endregion

		#region Implementation

		protected override ZString HumanReadableNameCore => VRS_Name;

		#endregion
	}
}
