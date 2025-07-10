using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	static class ActivityTestHelper
	{
		internal static ITopLevelDataObjectWriter GetWriter(BusinessObject businessObject, bool shouldIncludeRelatedItems = true, RecipientRoleDetail[] recipientRoles = null)
		{
			var contextManager = (IActivityDataContextManager)businessObject.GetUniversalDataContextManager();
			return contextManager.GetActivityDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoles, businessObject)), shouldIncludeRelatedItems);
		}

		internal static Activity WriteActivity(BusinessObject businessObject, ITopLevelDataObjectWriter activityWriter = null)
		{
			var writer = activityWriter ?? GetWriter(businessObject);
			return (Activity)writer.GetDataObject(businessObject);
		}

		internal static Activity GetActivityByJobNumber(IEnumerable<Activity> activities, ZString jobNumber)
		{
			return activities.SingleOrDefault(x => (x.DataContext.DataSourceCollection?.Single().Key ?? x.DataContext.DataTargetCollection?.Single().Key).Value == jobNumber);
		}

		internal static void SwapDataSourcesForDataTargets(Activity activity)
		{
			foreach (var source in activity.DataContext.DataSourceCollection)
			{
				var type = (DataContextType)Enum.Parse(typeof(DataContextType), source.Type ?? ZString.Empty);
				activity.DataContext.AddDataTarget(type, source.Key);
			}

			((DataContext)activity.DataContext).DataSourceCollection = null;
		}
	}
}
