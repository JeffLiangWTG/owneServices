//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobEquipmentValidation
//
//    This class should be used for overriding validation in AutoJobEquipmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobEquipmentValidation : AutoJobEquipmentValidation
	{
		public JobEquipmentValidation(AutoJobEquipment parent) : base(parent)
		{
		}

		protected new JobEquipment Parent
		{
			get { return (JobEquipment)base.Parent; }
		}

		protected override void CheckJEQ_Code()
		{
			base.CheckJEQ_Code();
			MandatoryValidation.CheckEntered(Parent.JEQ_CodeInfo);
			if (!Parent.JEQ_Code.IsEmpty)
			{
				var query = new ZQuery(JobEquipmentSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(JobEquipmentSchema.JEQ_Code, Parent.JEQ_Code);
				var anotherJobEquipment = Parent.Factory.LoadTop1<JobEquipment>(query);
				if (anotherJobEquipment != null)
				{
					Parent.JEQ_CodeInfo.AddError(DuplicateJobEquipmentCode);
				}
			}
		}

		public static string DuplicateJobEquipmentCode
		{
			get { return Res.GetString("9e7e2fc9-6999-4565-87c6-76acbaee7165", "The code entered already exists on another equipment combination record."); }
		}

		protected override void CheckJEQ_Description()
		{
			base.CheckJEQ_Description();
			MandatoryValidation.CheckEntered(Parent.JEQ_DescriptionInfo);
		}
	}
}
