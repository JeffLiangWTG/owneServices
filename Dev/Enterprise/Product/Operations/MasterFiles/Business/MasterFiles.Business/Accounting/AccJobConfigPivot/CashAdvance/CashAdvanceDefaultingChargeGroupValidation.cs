using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CashAdvanceDefaultingChargeGroupValidation : AccJobConfigPivotValidation
	{
		public CashAdvanceDefaultingChargeGroupValidation(CashAdvanceDefaultingChargeGroup parent) : base(parent)
		{
		}

		protected override void CheckJCT_Code()
		{
			base.CheckJCT_Code();
			MandatoryValidation.CheckEntered(Parent.JCT_CodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JCT_CodeInfo);
			CheckItIsNotDuplicate();
		}

		void CheckItIsNotDuplicate()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			var parentCollection = (CashAdvanceDefaultingChargeGroupCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is CashAdvanceDefaultingChargeGroupCollection);

			if (parentCollection == null)
			{
				return;
			}

			if (parentCollection.Cast<CashAdvanceDefaultingChargeGroup>().Any(c => c != Parent && ((CashAdvanceDefaultingChargeGroup)Parent).IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		internal static string IsDuplicateErrorString => Res.GetString("04fc7f0b-99dc-4392-8b1b-967e9479b215", "Another record already exists for the same Charge Group.");
	}
}
