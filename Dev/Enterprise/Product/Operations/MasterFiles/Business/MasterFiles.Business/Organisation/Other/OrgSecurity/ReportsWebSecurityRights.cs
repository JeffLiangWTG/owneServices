using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business
{
	public class ReportsWebSecurityRights : WebSecurityRightsProvider
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This is the factory pattern")]
		protected ReportsWebSecurityRights(BusinessObjectFactory factory)
		{
#if NETFRAMEWORK
			var distinctCodes = GetWebReports(factory)
				.Select(report => GetSecurityRightCodeAndDescriptionForReport(report))
				.WhereNotNull()
				.DistinctBy(pair => pair.Code);
#else
			var distinctCodes = IEnumerableExtensions
				.DistinctBy(GetWebReports(factory)
				.Select(report => GetSecurityRightCodeAndDescriptionForReport(report))
				.WhereNotNull(), pair => pair.Code);
#endif

			foreach (var pair in distinctCodes)
			{
				Add(new WebSecurityRight(pair.Code, (NoResString)pair.Description, WebSecurityApplication.EdiWebTracker, false));
			}
		}

		public static CodeDescriptionPair GetSecurityRightCodeAndDescriptionForReport(StmMenuItem report)
		{
			CodeDescriptionPair result = null;
			if (report.SU_MenuType == Core.Constants.StmMenuItemTypes.WebReports)
			{
				var code = report.PK.ToString();
				var description = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", report.SU_BusinessContext, report.SU_MenuNameMultilingual);
				result = new CodeDescriptionPair(code, description);
			}

			return result;
		}

		public static WebSecurityRight GetSecurityRightForReport(StmMenuItem report)
		{
			var codeAndDescription = GetSecurityRightCodeAndDescriptionForReport(report);
			return codeAndDescription != null ? new WebSecurityRight(codeAndDescription.Code, (NoResString)codeAndDescription.Description, WebSecurityApplication.EdiWebTracker, false) : null;
		}

		protected virtual StmMenuItemCollection GetWebReports(BusinessObjectFactory factory)
		{
			var reports = new StmMenuItemCollection(factory);
			var reportQuery = new ZQuery(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.WebReports);
			reportQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, Core.Constants.BusinessContextPrefixes.Reports);
			reports.Load(reportQuery);
			return reports;
		}

		public static ReportsWebSecurityRights New(BusinessObjectFactory factory)
		{
			return OverridableNewDelegate.Value?.Invoke(factory) ?? new ReportsWebSecurityRights(factory);
		}

		protected delegate ReportsWebSecurityRights NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
	}
}
