using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public static class WorkflowCustomFieldsGridInitializer
	{
		public static IDisposable AddWorkflowCustomFieldsColumns(ZGrid grid, IBusinessObjectCollection collection, bool isVisible = false)
		{
			WorkflowActiveGridCustomColumnsInitializer container = new WorkflowActiveGridCustomColumnsInitializer(grid, collection, isVisible);
			container.HookCollection();
			return container;
		}

		#region WorkflowActiveGridCustomColumnsInitializer

		class WorkflowActiveGridCustomColumnsInitializer : ZActiveGridCustomColumnsInitializer
		{
			public WorkflowActiveGridCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, bool isVisible = false)
				: base(grid, collection, Res.GetData("02e92e7c-d4c7-40e1-8d41-32dd8f0ac043", "Workflow Custom Fields"), isVisible, false)
			{ }

			protected override System.ComponentModel.PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
			{
				return new WorkflowCustomPropertyDescriptor(property);
			}

			protected override void HookBusinessObjectCore(IEnumerable<BusinessObject> businessObjects)
			{
				base.HookBusinessObjectCore(businessObjects);

				foreach (var businessObject in businessObjects)
				{
					IWorkflowProviderCore workflowProvider = businessObject as IWorkflowProviderCore;
					IWorkflowDescriptor workflowDescriptor = workflowProvider != null ? new WorkflowDescriptor.Loader().GetWorkflowDescriptor(workflowProvider.WorkflowType) : null;
					if (workflowDescriptor != null)
					{
						foreach (string propertyName in workflowDescriptor.GetPropertiesThatAffectWorkflow())
						{
							ZPropertyInfo propertyInfo = businessObject.FindPropertyInfo(propertyName);
							if (propertyInfo != null)
							{
								UnSubscribePropertySetChanged(businessObject, handler => propertyInfo.ValueChanged -= handler);
								SubscribePropertySetChanged(businessObject, handler => propertyInfo.ValueChanged += handler);
							}
						}

						CustomBusinessObject customBusinessObject = CustomBusinessObjectExtensions.GetCustomBusinessObject(businessObject, true);
						if (customBusinessObject != null)
						{
							var customProperties = ((ICustomPropertyContainer)customBusinessObject).CustomProperties;
							if (customProperties.Any())
							{
								AddCustomColumns(customProperties);
								businessObject.RefreshBinding();
							}
						}
					}
				}
			}

			protected override void UnhookBusinessObjectCore(BusinessObject businessObject)
			{
				base.UnhookBusinessObjectCore(businessObject);

				IWorkflowProviderCore workflowProvider = businessObject as IWorkflowProviderCore;
				IWorkflowDescriptor workflowDescriptor = workflowProvider != null ? new WorkflowDescriptor.Loader().GetWorkflowDescriptor(workflowProvider.WorkflowType) : null;
				if (workflowDescriptor != null)
				{
					foreach (string propertyName in workflowDescriptor.GetPropertiesThatAffectWorkflow())
					{
						ZPropertyInfo propertyInfo = businessObject.FindPropertyInfo(propertyName);
						if (propertyInfo != null)
						{
							UnSubscribePropertySetChanged(businessObject, handler => propertyInfo.ValueChanged -= handler);
						}
					}
				}

				CustomBusinessObject customBusinessObject = CustomBusinessObjectExtensions.GetCustomBusinessObject(businessObject);
				if (customBusinessObject != null)
				{
					businessObject.UnRegisterEditableChildObject(customBusinessObject);
				}
			}
		}

		#endregion
	}
}
