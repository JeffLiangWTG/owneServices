using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class SharedCusPermitHeaderValidation : CusPermitHeaderValidation
	{
		public SharedCusPermitHeaderValidation(SharedCusPermitHeader parent)
			: base(parent)
		{
		}

		public new SharedCusPermitHeader Parent => (SharedCusPermitHeader)base.Parent;

		#region Override Checks

		protected override void CheckCPH_Number()
		{
			MandatoryValidation.CheckEntered(Parent.CPH_NumberInfo);
		}

		protected override void CheckCPH_QtyValIndicator()
		{
			var parent = Parent;
			if (parent.Lookups.PermitQtyValIndicators.Count > 0)
			{
				var targetInfo = parent.CPH_QtyValIndicatorInfo;
				var countrySpecificInstruction = parent.GetCountrySpecificInstruction();
				if (countrySpecificInstruction?.IsQtyValIndicatorMandatory ?? false)
				{
					MandatoryValidation.CheckEntered(targetInfo);
				}
				ListValidation.ErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckCPH_EndDate()
		{
			var startDate = Parent.CPH_StartDate;
			var endDate = Parent.CPH_EndDate;
			if (endDate.IsValid)
			{
				if (startDate.IsValid && endDate < startDate)
				{
					Parent.CPH_EndDateInfo.AddError(EndDateBeforeStartDate);
				}
				else if (IsInPreSaveValidation) //Showing this error makes an extra DB Hit which is unnecessary until saving
				{
					var permitNumber = Parent.CPH_Number;
					var permitHolderPK = Parent.CPH_OH_PermitHolder;
					if (!permitNumber.IsEmpty && permitHolderPK.IsValid)
					{
						var overlappingPermit = Parent.Factory.LoadTop1<SharedCusPermitHeader>(GetOverlappingPermitQuery(endDate));
						if (overlappingPermit != null)
						{
							Parent.CPH_EndDateInfo.AddError(EndDateInRangeOfOtherPermit(overlappingPermit));
						}
					}
				}
			}
		}

		protected override void CheckCPH_EndDateIsValidZDateRange()
		{
		}

		public static string EndDateBeforeStartDate
		{
			get { return Res.GetString("0E3B65EC-AC1F-46AC-AFC6-E09E734B25C0", "End Date shouldn't be before the Start Date."); }
		}

		public static string EndDateInRangeOfOtherPermit(SharedCusPermitHeader permit)
		{
			return Res.GetString("4797DDA1-07A2-4E70-8CFE-7A7DEE795DE5", "End Date overlaps with {0}. {1} with the same permit number and holder may not overlap.", GetPermitDescriptionWithRange(permit), permit.ShortName);
		}

		public static string GetPermitDescriptionWithRange(SharedCusPermitHeader permit)
		{
			return Res.GetString("FA563B1D-792B-4070-973D-0DC809EF556B", "{0}: {1} to {2}", permit.HumanReadableName, permit.CPH_StartDate.ToString(), permit.CPH_EndDate.ToString());
		}

		protected override void CheckCPH_StartDate()
		{
			if (IsInPreSaveValidation) //Showing these errors make extra DB Hits which is unnecessary until saving
			{
				var startDate = Parent.CPH_StartDate;
				if (startDate.IsValid)
				{
					var permitNumber = Parent.CPH_Number;
					if (!permitNumber.IsEmpty && Parent.CPH_OH_PermitHolder.IsValid)
					{
						var overlappingPermit = Parent.Factory.LoadTop1<SharedCusPermitHeader>(GetOverlappingPermitQuery(startDate));
						if (overlappingPermit != null)
						{
							Parent.CPH_StartDateInfo.AddError(StartDateInRangeOfOtherPermit(overlappingPermit));
						}
						else if (Parent.CPH_EndDate.IsValid)
						{
							var containedPermit = Parent.Factory.LoadTop1<SharedCusPermitHeader>(GetContainedPermitQuery());
							if (containedPermit != null)
							{
								Parent.CPH_StartDateInfo.AddError(OtherPermitInDateRange(containedPermit));
							}
						}
					}
				}
			}
		}

		ZQuery GetOverlappingPermitQuery(ZDate date)
		{
			var result = new ZQuery(CusPermitHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			result.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Parent.CPH_RN_NKCountryCode);
			result.AddToFilter(CusPermitHeaderSchema.CPH_Number, Parent.CPH_Number);
			result.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, Parent.CPH_OH_PermitHolder);
			result.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
			result.AddToFilter(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
			return result;
		}

		ZQuery GetContainedPermitQuery()
		{
			var result = new ZQuery(CusPermitHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			result.AddToFilter(CusPermitHeaderSchema.CPH_Number, Parent.CPH_Number);
			result.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, Parent.CPH_OH_PermitHolder);
			result.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.CPH_StartDate);
			result.AddToFilter(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.LessThanOrEqualTo, Parent.CPH_EndDate);
			return result;
		}

		public static string StartDateInRangeOfOtherPermit(SharedCusPermitHeader permit)
		{
			return Res.GetString("299DA754-CEDB-4F32-8F1B-BA6074B6578F", "Start Date overlaps with {0}. {1} with the same number and holder may not overlap.", GetPermitDescriptionWithRange(permit), permit.ShortName);
		}

		public static string OtherPermitInDateRange(SharedCusPermitHeader permit)
		{
			return Res.GetString("D94310AB-E014-4200-A084-C21463D41617", "The date range of this {1} overlaps with another {0}. {1} with the same permit number and holder may not overlap.", GetPermitDescriptionWithRange(permit), permit.ShortName);
		}

		protected override void CheckCPH_Type()
		{
			var targetInfo = Parent.CPH_TypeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckCPH_SubType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CPH_SubTypeInfo);
		}

		protected override void CheckCPH_OA_AppliesTo()
		{
			var appliesTo = Parent.CPH_OA_AppliesTo;
			var permitType = Parent.CPH_Type;

			if (!permitType.IsEmpty)
			{
				var appliesToIndicator = Parent.GetCountrySpecificInstruction()?.GetAppliesToIndicator(permitType, Parent.CPH_SubType) ?? AppliesToIndicator.ForceEmpty;
				if (appliesToIndicator == AppliesToIndicator.Mandatory && appliesTo.IsEmpty)
				{
					Parent.CPH_OA_AppliesToInfo.AddError(AppliesToRequired(Parent.CPH_Type));
				}
				else if (appliesToIndicator == AppliesToIndicator.ForceEmpty && !appliesTo.IsEmpty)
				{
					Parent.CPH_OA_AppliesToInfo.AddWarning(AppliesToNotApplicable(Parent.CPH_Type));
				}
			}
		}

		internal static string AppliesToRequired(ZString permitType) => Res.GetString("AAEEC75F-4E12-4590-B800-5D97FA1076BD", "Applies To is required for permit type {0}", permitType);
		internal static string AppliesToNotApplicable(ZString permitType) => Res.GetString("79EC8072-F8ED-4A90-88D1-6326EFF54FF2", "Applies To is not applicable to permit type {0}", permitType);

		protected override void CheckCPH_UnitOfMeasure()
		{
			ZString uom = Parent.CPH_UnitOfMeasure;
			if (uom.IsEmpty)
			{
				if (Parent.IsQTY)
				{
					MandatoryValidation.CheckEntered(Parent.CPH_UnitOfMeasureInfo);
				}
			}
			else if (uom.Length < 2)
			{
				Parent.CPH_UnitOfMeasureInfo.AddError(UnitOfMeasureMinimumLength);
			}
		}

		internal static string UnitOfMeasureMinimumLength
		{
			get { return Res.GetString("C6119E0C-D197-460C-B722-FA29793CE337", "Unit of Measure must be at least 2 characters."); }
		}

		#endregion

		#region New Checks

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTransactionCategory();
		}

		public void ValidateTransactionCategory()
		{
			ValidateCalculatedProperty(Parent.TransactionCategoryInfo);
		}

		protected virtual void CheckTransactionCategory()
		{
		}

		#endregion

		#region Implementation

		bool IsInPreSaveValidation => ((IBusinessObjectInternals)Parent).IsInPreSaveValidation;

		#endregion
	}
}
