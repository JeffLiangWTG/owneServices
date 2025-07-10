using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ViewStmNumsValidation : AutoViewStmNumsValidation
	{
		public ViewStmNumsValidation(AutoViewStmNums parent)
			: base(parent)
		{
		}

		protected new ViewStmNums Parent
		{
			get { return (ViewStmNums)base.Parent; }
		}

		public void ValidateSN_ValueForDisplay()
		{
			ValidateCalculatedProperty(Parent.SN_ValueForDisplayInfo);
		}

		protected virtual void CheckSN_ValueForDisplay()
		{
			if (Parent.IsInDatabase
				&& !Parent.HasChanges
				&& Parent.SN_ValueForDisplay == -1)
			{
				Parent.SN_ValueForDisplayInfo.AddWarning(Res.GetString("e1412d2f-b51b-4270-beae-61d279a0b733", "All numbers used"));
			}
		}

		protected override void CheckSN_Owner()
		{
			base.CheckSN_Owner();
			ValidateDuplicate();
		}

		void ValidateDuplicate()
		{
			var query = new ZQuery(ViewStmNumsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, Parent.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, Parent.SN_Owner);
			if (Parent.Factory.Load<ViewStmNums>(query).Length > 0)
			{
				Parent.SN_OwnerInfo.AddError(DuplicateRecordAlreadyExists);
			}
		}

		internal static string DuplicateRecordAlreadyExists => Res.GetString("{29D5F192-E51A-4ADE-918A-E961797B5098}", "There is already another record with the same Name and Owner.");

		protected override void CheckSN_Name()
		{
			base.CheckSN_Name();
			ValidateSN_Owner();
		}

		protected override void CheckSN_MinimumValue()
		{
			base.CheckSN_MinimumValue();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.SN_MinimumValueInfo, ViewStmNums.Schema.MinimumValue);
			CompareValidation.CheckLessThanOrEqualTo(Parent.SN_MinimumValueInfo, Parent.DefaultTypeRangeMax);
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.SN_MinimumValueInfo, Parent.SN_MaximumValueInfo);
		}

		protected override void CheckSN_MaximumValue()
		{
			base.CheckSN_MaximumValue();

			var maximumValue = Parent.SN_MaximumValue;
			if (maximumValue < 0)
			{
				var message = ResString.GetMultilingualString("fa5c3ee4-8b47-4fd8-a0af-e2f0c6440d61",
					"Range End must be greater than zero, please change the Range Start or Count.");

				Parent.SN_MaximumValueInfo.AddError(message);
			}

			if (maximumValue > Parent.DefaultTypeRangeMax)
			{
				var message = ResString.GetMultilingualString("b08e1792-4635-42de-8260-90de0ad35787",
					"Range End must be less than or equal to {0}, please change the Range Start or Count.",
					Parent.DefaultTypeRangeMax);

				Parent.SN_MaximumValueInfo.AddError(message);
			}

			if (maximumValue < Parent.SN_MinimumValue)
			{
				var message = ResString.GetMultilingualString("ee5af438-cef1-4009-afaa-2a20ec250b42",
					"Range End must be greater than or equal to the Range Start, please change the Range Start or Count.");

				Parent.SN_MaximumValueInfo.AddError(message);
			}
			ValidateSN_ValueForDisplay();
		}

		protected override void CheckSN_IDIsNotEmpty()
		{
		}

		#region SN_Type

		protected override void CheckSN_Type()
		{
			base.CheckSN_Type();
			MandatoryValidation.CheckEntered(Parent.SN_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SN_TypeInfo);
			ValidateSN_Prefix();
		}

		#endregion

		#region SN_Prefix

		protected override void CheckSN_Prefix()
		{
			base.CheckSN_Prefix();
			var owner = Parent.Owner;
			if (owner != null)
			{
				var query = new ZQuery(ViewStmNumsSchema.SN_Owner, owner.PK);
				query.AddToFilter(ViewStmNumsSchema.SN_Prefix, Parent.SN_Prefix);
				query.AddToFilter(ViewStmNumsSchema.SN_Type, Parent.SN_Type);
				var allStmNums = Parent.Factory.Load<ViewStmNums>(query);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.SN_PrefixInfo, allStmNums.Where(num => num.PK != Parent.PK), Res.GetString("1a1f514b-86d2-42fc-9054-37f02c4aad48", "The Range Type and Prefix has been duplicated and must be unique."));
			}
		}

		#endregion

		#region SN_Count
		public void ValidateSN_Count()
		{
			ValidateCalculatedProperty(Parent.SN_CountInfo);
		}

		protected virtual void CheckSN_Count()
		{
			CompareValidation.CheckNumberGreaterThanZero(Parent.SN_CountInfo);

			if (Parent.SN_MinimumValue > ViewStmNums.Schema.MinimumValue && Parent.SN_MinimumValue <= Parent.DefaultTypeRangeMax)
			{
				long maximumAllowedCount = (long)(Parent.DefaultTypeRangeMax - Parent.SN_MinimumValue + 1);
				CompareValidation.CheckLessThanOrEqualTo(Parent.SN_CountInfo, maximumAllowedCount);
			}
			ValidateSN_MinimumValue();
			ValidateSN_ValueForDisplay();
		}
		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSN_Type();
			ValidateSN_Prefix();
			ValidateSN_ValueForDisplay();
			ValidateSN_Count();
		}

		#endregion
	}
}
