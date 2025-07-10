using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsNumberViewStmNumsWrapperValidation : CustomsNumberViewStmNumsWrapperValidation
	{
		public SGCustomsNumberViewStmNumsWrapperValidation(SGCustomsNumberViewStmNumsWrapper parent)
			: base(parent)
		{
		}

		protected override void ValidateAllCore()
		{
			Parent.ClearRowNotifications();
			ValidateForRowNotifications();
		}

		void ValidateForRowNotifications()
		{
			if (Parent.SN_Type == NumberRangeTypeList.Codes.SingaporeMessageNumber)
			{
				var company = Parent.StmNums?.Provider?.Parent as GlbCompany;
				var startNumber = Parent.SN_MinimumValue;
				var endNumber = Parent.SN_MaximumValue;
				var overlappedStmNums = GetAllMatchingName().FirstOrDefault(x => (x.SN_MinimumValue <= startNumber && startNumber <= x.SN_MaximumValue) || (startNumber <= x.SN_MinimumValue && x.SN_MinimumValue <= endNumber));

				if (overlappedStmNums != null)
				{
					Parent.AddRowError(Res.GetString("8e33c6e5-fb2d-4eec-9349-11cf358d2644", "The SMN Number Range is overlapping with Company ({0}) which shares the same Customs Registration Number.", overlappedStmNums.SN_OwnerForDisplay));
				}

				if (company != null && Parent.SN_FountainName != company.GC_CustomsRegistrationNo)
				{
					Parent.AddRowWarning(Res.GetString("dff6c5b3-ced7-4ba9-8b58-9f3699f9a04b", "This range would be ignored as the Customs Registration Number doesn't match the company's Customs Registration Number."));
				}
			}
		}

		CustomsNumberViewStmNums[] GetAllMatchingName()
		{
			CustomsNumberViewStmNums[] result = null;
			if (!Parent.StmNums.SN_Name.IsEmpty)
			{
				var query = new ZQuery(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, Parent.StmNums.SN_NameWithoutSequence + CustomsNumberViewStmNums.Schema.SequenceSeparator);
				if (Parent.StmNums.SN_ID.IsEmpty)
				{
					query.AddToFilter(ViewStmNumsSchema.PK, SQLComparisonOperator.NotEqual, Parent.StmNums.PK);
				}
				else
				{
					query.AddToFilter(ViewStmNumsSchema.SN_ID, SQLComparisonOperator.NotEqual, Parent.StmNums.SN_ID);
				}
				result = CustomsNumberViewStmNumsHelper.LoadStmNums(Parent.StmNums.Factory, query);
			}
			return result ?? (System.Array.Empty<CustomsNumberViewStmNums>());
		}
	}
}
