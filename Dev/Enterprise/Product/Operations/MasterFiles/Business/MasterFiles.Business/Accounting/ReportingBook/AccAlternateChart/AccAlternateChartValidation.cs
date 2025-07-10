using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartValidation : AutoAccAlternateChartValidation
	{
		public AccAlternateChartValidation(AutoAccAlternateChart parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckAccountFormat();
		}

		void CheckAccountFormat()
		{
			Parent.ClearRowNotifications();
			if (((AccAlternateChart)Parent).AlternateChartFormats.Count == 0)
			{
				Parent.AddRowError(Res.GetString("A72B103B-F6D7-4ED8-AF16-5096487725A0", "The Alternate Chart of Accounts should have at least one Tier."));
			}
		}

		protected override void CheckAAC_Code()
		{
			var codeInfo = Parent.AAC_CodeInfo;
			MandatoryValidation.CheckEntered(codeInfo);
			if (!codeInfo.HasErrors())
			{
				var newExpression = (NoResString)@"^[a-zA-Z0-9]{1,10}$";
				var r = new Regex(newExpression);

				if (!r.IsMatch(Parent.AAC_Code))
				{
					codeInfo.AddError(Res.GetString("7CE584BD-940B-439D-ACF0-782FE28D3F7F", "Invalid Chart Code. A valid code cannot include special characters."));
				}
			}
			if (!codeInfo.HasErrors())
			{
				var query = new ZQuery(AccAlternateChartSchema.AAC_Code, Parent.AAC_Code);
				query.AddToFilter(AccAlternateChartSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var chart = Parent.Factory.LoadTop1<AccAlternateChart>(query);

				if (chart != null)
				{
					if (chart.AAC_IsGlobal)
					{
						codeInfo.AddError(Res.GetString("D516DBF7-414B-4356-A761-3B12F1A1C240", "This Chart Code is already used by a Global Chart. Please enter another code."));
					}
					else
					{
						codeInfo.AddError(Res.GetString("E9844060-D10B-4F7F-9D1F-1F818ABE9CE2", "This Chart Code is already used by a non-Global Chart in '{0}' system company. Please enter another code.", chart.Company.GC_Code));
					}
				}
			}
		}

		protected override void CheckAAC_IsFixedLength()
		{
			if (Parent.AAC_IsFixedLengthInfo.HasChanges && Parent.Factory.Exists(typeof(AccAlternateGLAccount), new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Parent.PK)))
			{
				Parent.AAC_IsFixedLengthInfo.AddError(Res.GetString("E0E936C8-3650-4714-9C3B-EB5A9CD7C2DA", "Alternate GL Account is created for this chart, you cannot change Fixed Length."));
			}
		}

		protected override void CheckAAC_Description()
		{
			MandatoryValidation.CheckEntered(Parent.AAC_DescriptionInfo);
		}

		protected override void CheckAAC_BalanceSheetStyle()
		{
			MandatoryValidation.CheckEntered(Parent.AAC_BalanceSheetStyleInfo);
			if (!Parent.AAC_BalanceSheetStyleInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.AAC_BalanceSheetStyleInfo, Parent.Lookups.BaseBalanceSheetStyleList);
			}
		}

		protected override void CheckAAC_ReportOrder()
		{
			MandatoryValidation.CheckEntered(Parent.AAC_ReportOrderInfo);
			if (!Parent.AAC_ReportOrderInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("76FA5629-9369-455C-AFB6-C89284560430", "Please specify a valid value. It must be 'PTB' or 'BTP'"), Parent.AAC_ReportOrderInfo);
			}
		}
	}
}

