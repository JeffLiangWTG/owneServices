using System;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccReportingBookValidation : AutoAccReportingBookValidation
	{
		public AccReportingBookValidation(AutoAccReportingBook parent) : base(parent)
		{
			Parent = parent as AccReportingBook;
		}

		new AccReportingBook Parent { get; }

		#region ValidateAll

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateUniqueReportingBook();
			ValidateARB_IsGlobal();
		}

		void ValidateUniqueReportingBook()
		{
			if (!Parent.HasErrors)
			{
				var query = new ZDBOnlyQuery(typeof(AccReportingBook));
				query.AddToFilter(AccReportingBookSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(AccReportingBookSchema.ARB_GC_CompanyOfPeriod, Parent.ARB_GC_CompanyOfPeriod == ZGuid.Empty ? DBNull.Value : Parent.ARB_GC_CompanyOfPeriod);
				query.AddToFilter(AccReportingBookSchema.ARB_AAC_AlternateChart, Parent.ARB_AAC_AlternateChart);
				query.AddToFilter(AccReportingBookSchema.ARB_RX_NKCurrency, Parent.ARB_RX_NKCurrency);
				query.AddToFilter(AccReportingBookSchema.ARB_IncludePresentationJournals, Parent.ARB_IncludePresentationJournals);
				query.AddToFilter(AccReportingBookSchema.ARB_IncludeChildPresentation, Parent.ARB_IncludeChildPresentation);

				var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateChart), AccAlternateChartSchema.PK, AccReportingBookSchema.ARB_AAC_AlternateChart);
				subQuery.AddToFilter(AccAlternateChartSchema.AAC_GC_Company, Parent.AlternateChartCompany);
				subQuery.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, DBNull.Value);
				query.AddSubQuery(subQuery, JoinCondition.And);

				var reportingBook = Parent.Factory.LoadTop1<AccReportingBook>(query);
				if (reportingBook != null)
				{
					var errorMsgBuilder = new ZStringBuilder();
					if (reportingBook.AlternateChartCompany != Guid.Empty)
					{
						errorMsgBuilder.AppendLine(Res.GetString("B9995C71-453E-49D5-8BBD-FE2723F36C13", "Another reporting book with the same configuration settings already existed in {0}",
							Parent.Factory.Load<GlbCompany>(reportingBook.AlternateChartCompany)?.GC_Code ?? ZString.Empty));
					}
					else
					{
						errorMsgBuilder.AppendLine(Res.GetString("181839E7-5417-4754-85D2-984C33478549", "A Global Reporting Book with the same configuration settings already existed."));
					}
					errorMsgBuilder.Append("	").AppendLine(Res.GetString("CFF9F5D0-7D3F-4B3D-9C42-3D17BDE8E356", @"Code = {0}
	Description = {1}
	Alternate Chart = {2}",
	reportingBook.ARB_Code, reportingBook.ARB_Description, reportingBook.AlternateChart.AAC_Code));
					if (!reportingBook.ARB_RX_NKCurrency.IsEmpty)
					{
						errorMsgBuilder.Append("	").AppendLine(Res.GetString("F33B4EC8-5A2E-4AAE-A190-17EAB2F492E7", "Currency = {0}", reportingBook.ARB_RX_NKCurrency));
					}
					errorMsgBuilder.Append("	").AppendLine(Res.GetString("B2AB366D-253C-4178-9F16-F7BECE49915E", "Presentation Journals = {0} Code", reportingBook.ARB_IncludePresentationJournals));
					errorMsgBuilder.Append("	").AppendLine(Res.GetString("A2D1F4E5-0C8B-4A3C-8F7D-6E9B5A2E0F7A", "Include Child Presentation = {0}", reportingBook.ARB_IncludeChildPresentation));
					if (reportingBook.AlternateChartCompany == Guid.Empty)
					{
						errorMsgBuilder.Append("	").AppendLine(Res.GetString("6C268E03-C5D4-4116-A047-CC8306097555", "Reporting Period = {0}",
							Parent.Factory.Load<GlbCompany>(reportingBook.ARB_GC_CompanyOfPeriod)?.GC_Code ?? ZString.Empty));
					}

					Parent.AddRowError(errorMsgBuilder.ToString());
				}
			}
		}

		#endregion

		#region ARB_Code

		protected override void CheckARB_Code()
		{
			var codeInfo = Parent.ARB_CodeInfo;
			MandatoryValidation.CheckEntered(codeInfo);
			if (!codeInfo.HasErrors())
			{
				var newExpression = (NoResString)@"^[a-zA-Z0-9]{1,10}$";
				var regex = new Regex(newExpression);
				if (!regex.IsMatch(Parent.ARB_Code))
				{
					codeInfo.AddError(Res.GetString("C27763D2-96B3-4604-8DA7-C20610CCBA65", "Invalid reporting book code. A valid code cannot include special characters."));
				}
				CheckUniqueCode();
			}
		}

		void CheckUniqueCode()
		{
			if (!Parent.ARB_CodeInfo.HasErrors())
			{
				var filter = new ZQuery(AccReportingBookSchema.ARB_Code, Parent.ARB_Code);
				filter.AddToFilter(AccReportingBookSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var factory = new BusinessObjectFactory();
				var reportingBook = factory.LoadTop1<AccReportingBook>(filter);
				if (reportingBook != null)
				{
					if (reportingBook.AlternateChartCompany != Guid.Empty)
					{
						Parent.ARB_CodeInfo.AddError(Res.GetString("F67A39F7-3D58-444F-BC44-42DA0A0FA0BD",
							"The code is already used by another Reporting Book in {0} system company.", factory.Load<GlbCompany>(reportingBook.AlternateChartCompany)?.GC_Code ?? ZString.Empty));
					}
					else
					{
						Parent.ARB_CodeInfo.AddError(Res.GetString("7e3babda-502a-4abf-8ef7-e2fdae746b23",
							"The code is already used by a Global Reporting Book."));
					}
				}
			}
		}

		#endregion

		#region ARB_Description

		protected override void CheckARB_Description()
		{
			MandatoryValidation.CheckEntered(Parent.ARB_DescriptionInfo);
		}

		#endregion

		#region ARB_GC_CompanyOfPeriod

		protected override void CheckARB_GC_CompanyOfPeriod()
		{
			if (!Parent.ARB_GC_CompanyOfPeriod.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.ARB_GC_CompanyOfPeriodInfo);
			}
		}

		#endregion

		#region ARB_AAC_AlternateChart

		protected override void CheckARB_AAC_AlternateChart()
		{
			if (!Parent.ARB_AAC_AlternateChartInfo.HasErrors() && Parent.AlternateChart != null)
			{
				if (Parent.ARB_IsGlobal && !Parent.AlternateChart.AAC_IsGlobal)
				{
					Parent.ARB_AAC_AlternateChartInfo.AddError(Res.GetString("8DD7C8FE-7F82-4A2F-91D1-587AC9EF8986",
						"The Reporting Book is Global, please select a Global Alternate Chart."));
				}
				else if (!Parent.ARB_IsGlobal && Parent.AlternateChart.AAC_IsGlobal)
				{
					Parent.ARB_AAC_AlternateChartInfo.AddError(Res.GetString("0320F8EB-0A1A-47C3-809A-4799C928F77D",
						"The Reporting Book is Non-global, please select a Non-global Alternate Chart."));
				}
			}

			if (!Parent.ARB_AAC_AlternateChartInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.ARB_AAC_AlternateChartInfo);
			}
		}

		#endregion

		#region ARB_IncludePresentationJournals

		protected override void CheckARB_IncludePresentationJournals()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ARB_IncludePresentationJournalsInfo);
		}

		#endregion

		#region ARB_RX_NKCurrency

		protected override void CheckARB_RX_NKCurrency()
		{
			if (Parent.ARB_RX_NKCurrency.IsEmpty)
			{
				return;
			}

			ListValidation.ErrorIfInvalidCode(Parent.ARB_RX_NKCurrencyInfo);
			var alternateChart = Parent.AlternateChart;
			if (Parent.ARB_RX_NKCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && alternateChart?.AccAlternateChartCurrencyTranslations?.Count == 0)
			{
				Parent.ARB_RX_NKCurrencyInfo.AddError(Res.GetString("D1D3D3A4-3D3D-4D3D-8D3D-3D3D3D3D3D3D",
					"No currency translation rules have been configured for Alternate Chart \"{0} - {1}\", the rules must be configured before running the reports.",
					alternateChart.AAC_Code, alternateChart.AAC_Description));
			}
		}

		#endregion

		#region ARB_IsGlobal

		public void ValidateARB_IsGlobal()
		{
			ValidateCalculatedProperty(Parent.ARB_IsGlobalInfo);
		}

		protected virtual void CheckARB_IsGlobal()
		{
			if (Parent.IsInDatabase
				&& Parent.ARB_IsGlobal
				&& IsReportingBookReferencedByComplianceReport())
			{
				Parent.ARB_IsGlobalInfo.AddError(Res.GetString("121EC8C2-4EA9-41AE-8BD5-8D947E75ADCA", "Reporting Book has been used in at least one Compliance Report, 'Is Global' cannot be ticked."));
			}
		}

		bool IsReportingBookReferencedByComplianceReport()
		{
			var query = new ZQuery(AccComplianceReportSchema.ACR_ARB_ReportingBook, Parent.PK);
			return Parent.Factory.ExistsInDatabase(AccComplianceReportSchema.Constants.TableName, query);
		}

		#endregion
	}
}
