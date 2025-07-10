using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Tools.TextStandardizer.JobCategorizer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.GUI
{
	public static class JobCategoryHelper
	{
		internal static void FindSuggestedJobCategories(OrgContact contact, bool shouldOverwrite = true)
		{
			contact.SuggestedJobCategories.Clear();

			if (IsUserDefinedCategoryOrEmpty(contact.OC_JobCategory))
			{
				var jobCategories = GetJobCategories(contact.OC_Title);
				if (jobCategories.Any())
				{
					contact.SuggestedJobCategories = jobCategories;
					if (shouldOverwrite)
					{
						OverwriteJobCategory(contact, jobCategories);
					}
				}
			}
		}

		internal static void OverwriteJobCategory(OrgContact contact, List<(string, string)> suggestedJobCategories = null)
		{
			var isEmployeeUndefinedOrEmpty = string.IsNullOrEmpty(contact.OC_JobCategory) || contact.OC_JobCategory == OrgContactJobCategories.Codes.EMU;
			suggestedJobCategories = suggestedJobCategories ?? contact.SuggestedJobCategories;
			if (suggestedJobCategories.Count == 1 && isEmployeeUndefinedOrEmpty)
			{
				contact.OC_JobCategory = suggestedJobCategories.First().Item1;
			}
		}

		internal static bool IsUserDefinedCategoryOrEmpty(string code)
		{
			return string.IsNullOrEmpty(code) || JobCategoriesMap.Any(jc => jc.Value.Item1 == code);
		}

		internal static void RegisterPropertyChangedEvent(OrgContact contact, Action<bool, bool> action)
		{
			if (contact != null)
			{
				contact.TriggerFindSuggestedJobCategories += (sender, e) =>
				{
					var findEventArgs = e as FindSuggestedJobCategoriesEventArgs;
					action.Invoke(findEventArgs.ShouldOverwrite, findEventArgs.UseCache);
				};
			}
		}

		#region JobCategorizerRunner

		internal static List<(string, string)> GetJobCategories(string title)
		{
			var result = new List<(string, string)>();
			var categories = Runner.FindCategories(title);
			foreach (var obj in categories)
			{
				if (JobCategoriesMap.TryGetValue((obj.Level, obj.Area), out var category))
				{
					result.Add(category);
				}
			}

			return result;
		}

		static JobCategorizerRunner Runner => runner ?? (runner = new JobCategorizerRunner(true));

		[ThreadSafe]
		static JobCategorizerRunner runner;

		internal static Dictionary<(Level, Area), (string, string)> JobCategoriesMap => jobCategoriesMap ??
			(jobCategoriesMap = new Dictionary<(Level, Area), (string, string)>()
				{
					{ (Level.Leadership, Area.Undefined), (OrgContactJobCategories.Codes.LEA,OrgContactJobCategories.Descriptions.LEA) },
					{ (Level.SeniorManagement, Area.Administration), (OrgContactJobCategories.Codes.SMA, OrgContactJobCategories.Descriptions.SMA) },
					{ (Level.SeniorManagement, Area.Finance), (OrgContactJobCategories.Codes.SMF, OrgContactJobCategories.Descriptions.SMF) },
					{ (Level.SeniorManagement, Area.Operations), (OrgContactJobCategories.Codes.SMO, OrgContactJobCategories.Descriptions.SMO) },
					{ (Level.SeniorManagement, Area.SalesAndMarketing), (OrgContactJobCategories.Codes.SMS, OrgContactJobCategories.Descriptions.SMS) },
					{ (Level.SeniorManagement, Area.Undefined), (OrgContactJobCategories.Codes.SMU, OrgContactJobCategories.Descriptions.SMU) },
					{ (Level.Management, Area.Administration), (OrgContactJobCategories.Codes.MAA, OrgContactJobCategories.Descriptions.MAA) },
					{ (Level.Management, Area.Finance), (OrgContactJobCategories.Codes.MAF, OrgContactJobCategories.Descriptions.MAF) },
					{ (Level.Management, Area.Operations), (OrgContactJobCategories.Codes.MAO, OrgContactJobCategories.Descriptions.MAO) },
					{ (Level.Management, Area.SalesAndMarketing), (OrgContactJobCategories.Codes.MAS, OrgContactJobCategories.Descriptions.MAS) },
					{ (Level.Management, Area.Undefined), (OrgContactJobCategories.Codes.MAU, OrgContactJobCategories.Descriptions.MAU) },
					{ (Level.Employee, Area.Administration), (OrgContactJobCategories.Codes.EMA, OrgContactJobCategories.Descriptions.EMA) },
					{ (Level.Employee, Area.Finance), (OrgContactJobCategories.Codes.EMF, OrgContactJobCategories.Descriptions.EMF) },
					{ (Level.Employee, Area.Operations), (OrgContactJobCategories.Codes.EMO, OrgContactJobCategories.Descriptions.EMO) },
					{ (Level.Employee, Area.SalesAndMarketing), (OrgContactJobCategories.Codes.EMS, OrgContactJobCategories.Descriptions.EMS) },
					{ (Level.Employee, Area.Undefined), (OrgContactJobCategories.Codes.EMU, OrgContactJobCategories.Descriptions.EMU) }
				}
			);
		[ThreadSafe]
		static Dictionary<(Level, Area), (string, string)> jobCategoriesMap;

		#endregion
	}
}
