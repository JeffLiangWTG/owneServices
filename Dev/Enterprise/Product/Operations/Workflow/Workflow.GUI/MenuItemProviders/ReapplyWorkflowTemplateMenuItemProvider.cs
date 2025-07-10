using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.GUI
{
	public class ReapplyWorkflowTemplateMenuItemProvider : IFilterGridMenuItemProvider, IWorkflowReapplyTemplatesMenuItem
	{
		public const string MenuItemName = "ReapplyWorkflowTemplate";

		IEnumerable<MenuItem> IFilterGridMenuItemProvider.GetMenuItems(ZFilterGridModule module)
		{
			if (!string.IsNullOrEmpty(module.WorkflowType))
			{
				yield return GetReapplyWorkflowTemplateMenuItem(new ReapplyWorkflowTemplateInServiceTaskConfiguration(), () => module.GetSelectedBusinessObjects());
			}
		}

		public IMenuItem GetReapplyWorkflowTemplateMenuItemForBusinessObjectFrom(BusinessObject businessObject)
		{
			return GetReapplyWorkflowTemplateMenuItem(new ReapplyWorkflowTemplateInGUIConfiguration(), () => new[] { businessObject } );
		}

		public static ZMenuItem GetReapplyWorkflowTemplateMenuItemForFilterGrid(BusinessObject[] businessObjects)
		{
			return GetReapplyWorkflowTemplateMenuItem(new ReapplyWorkflowTemplateInServiceTaskConfiguration(), () => businessObjects);
		}

		static ZMenuItem GetReapplyWorkflowTemplateMenuItem(IReapplyWorkflowTemplateConfiguration config, Func<BusinessObject[]> businessObjectsGetter)
		{
			var workflowProviderGetter = new IWorkflowProviderCollectionGetter(() =>
			{
				var bizObjs = businessObjectsGetter.Invoke();
				var isSupported = bizObjs.All(bizO => bizO is IWorkflowProvider);

				var workflowProviders = isSupported
					? bizObjs.Cast<IWorkflowProvider>()
					: Enumerable.Empty<IWorkflowProvider>();

				var notSupportedBizObjNames = bizObjs.Where(bizO => !(bizO is IWorkflowProvider)).Select(bizO => bizO.HumanReadableShortcutName);

				var workflowProvidersInfo = new WorkflowProvidersInfo
				{
					WorkflowProviders = workflowProviders,
					IsSupported = isSupported,
					NotSupportedBizObjNames = notSupportedBizObjNames
				};

				return workflowProvidersInfo;
			});

			return new ReapplyWorkflowTemplateMenuItemTree(config, workflowProviderGetter) { Name = MenuItemName };
		}
	}

	public class WorkflowProvidersInfo
	{
		public IEnumerable<IWorkflowProvider> WorkflowProviders { get; set; }
		public bool IsSupported { get; set; }
		public IEnumerable<ZString> NotSupportedBizObjNames { get; set; }
	}

	public delegate WorkflowProvidersInfo IWorkflowProviderCollectionGetter();
}
