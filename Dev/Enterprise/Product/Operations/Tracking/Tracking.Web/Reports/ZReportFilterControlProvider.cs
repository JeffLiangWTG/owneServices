using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
#pragma warning disable CW1147 //Baseline WI00653447
	public static class ZReportFilterControlProvider
	{
		public static Control[] CreateFilterControls(FilterField filterField)
		{
			if (IsSupportedOnWeb(filterField))
			{
				if (filterField is TextField)
				{
					return new[] { new ZTextBox { BindTo = (NoResString)"Value" } };
				}
				if (filterField is NumberField)
				{
					return new[] { new ZNumericTextBox { BindTo = "ZValue" } };
				}
				if (filterField is DateField)
				{
					return new[] { new ZDateEdit { BindTo = (NoResString)"Value" } };
				}

				if (filterField is DateTimeOffsetField)
				{
					return new[] { new ZDateEdit { BindTo = (NoResString)"Value", DateTimeFormat = ZDateTimePickerFormat.IncludingTimeZone } };
				}
				if (filterField is DateTimeOffsetRangeField)
				{
					return new Control[] {
						new Label { Text = Res.GetString("267A03DC-D530-4EE0-96EF-5D8D3A89440A","From:") + @"&nbsp;" },
						new ZDateEdit { BindTo = "ValueLow", DateTimeFormat = ZDateTimePickerFormat.IncludingTimeZone },
						new Label { Text = @"&nbsp;" + Res.GetString("1236B2C5-6F96-4AAD-B487-F80FC7CA7105","To:") + @"&nbsp;" },
						new ZDateEdit { BindTo = "ValueHigh", DateTimeFormat = ZDateTimePickerFormat.IncludingTimeZone }
					};
				}
				if (filterField is DateRangeField)
				{
					return new Control[] {
						new Label { Text = Res.GetString("cdad7d88-e462-4b88-ac7a-f782b4f4ecbf","From:") + @"&nbsp;" },
						new ZDateEdit { BindTo = "ValueLow" },
						new Label { Text = @"&nbsp;" + Res.GetString("4fe5a5b3-7187-45e6-8dbb-f66cd23063d4","To:") + @"&nbsp;" },
						new ZDateEdit { BindTo = "ValueHigh" }
					};
				}
				if (filterField is MultipleChoice || filterField is CodeListMultipleChoice)
				{
					return new[] { new ZFilterStripDropDownList { BindTo = "ZValue", BindToList = (NoResString)"List", ShowEmptyItem = true, IsDescriptionsList = true } };
				}
				if (filterField is CodeLookupField && HasWebModule((CodeLookupField)filterField))
				{
					return new[]
							 {
								 new ZFindBox
								 {
									 BindTo = "ZValue",
									 BindToList = "BindToList",
									 ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(((CodeLookupField)filterField).ModuleID)
								 }
							 };
				}
				if (filterField is LookupField && HasWebModule((LookupField)filterField))
				{
					return new[]
							 {
								 new ZGuidFindBox
								 {
									 BindTo = "ZValue",
									 BindToList = "BindToList",
									 ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(((LookupField)filterField).ModuleID)
								 }
							 };
				}
				if (filterField is OptionGroup)
				{
					return new[]
										{
												new ZCheckBoxList
												{
														BindTo = "BindableBooleanItems",
												}
										};
				}
			}

			return Array.Empty<Control>();
		}

		static bool HasWebModule(LookupFilterFieldBase filterField)
		{
			return new WebReportFilterHelper().HasWebModule(filterField);
		}

		public static bool IsSupportedOnWeb(FilterField filterField)
		{
			return new WebReportFilterHelper().IsSupportedOnWeb(filterField);
		}

		public static bool ShouldBeHiddenOnWeb(Report report, FilterField filter)
		{
			return report != null && filter == report.LinkedLookupField;
		}

		public static void SetWebUserRestrictingValues(Report report, OrgContactWebUser currentUser)
		{
			if (report.LinkedLookupField != null)
			{
				report.LinkedLookupField.ZValue = currentUser.CurrentOrg;
			}
			else
			{
				if (!report.ForceWebPublish)
				{
					throw new NotSupportedException(string.Format("Report {0} that has no link to the client is not supported on Web", report.Name));
				}
			}
		}
	}
#pragma warning restore CW1147
}
