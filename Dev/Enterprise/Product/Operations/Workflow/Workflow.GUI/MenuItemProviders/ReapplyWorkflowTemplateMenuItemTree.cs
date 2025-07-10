using System;
using System.Linq;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public class ReapplyWorkflowTemplateMenuItemTree : ZMenuItem
	{
		public ReapplyWorkflowTemplateMenuItemTree(IReapplyWorkflowTemplateConfiguration config, IWorkflowProviderCollectionGetter iWorkflowProviderCollectionGetter)
			: base(ResString.GetMultilingualString("CDD851BA-6613-4B7A-8F7A-3F0D78F1E2B8", "Reapply Workflow Templates"))
		{
			this.iWorkflowProviderCollectionGetter = iWorkflowProviderCollectionGetter;
			this.config = config;
			Click += MenuItem_Click;
		}

		readonly IWorkflowProviderCollectionGetter iWorkflowProviderCollectionGetter;
		readonly IReapplyWorkflowTemplateConfiguration config;

		void MenuItem_Click(object sender, EventArgs e)
		{
			if (!EnvProxy.Instance.Security.WorkflowTaskTemplatesReapply.IsAllowed)
			{
				EnvProxy.Instance.Security.WorkflowTaskTemplatesReapply.ShowError();
				return;
			}

			var getter = iWorkflowProviderCollectionGetter();

			if (getter.IsSupported)
			{
				var workflowProviders = getter.WorkflowProviders.ToArray();

				if (workflowProviders.Length != 0)
				{
					var options = new ReapplyWorkflowTemplateUserOptions(config);
					using (var optionsForm = new ReapplyWorkflowTemplatesConfigurationForm(options))
					{
						ZFormModaliser.ShowDialogAndDispose(optionsForm);
					}

					if (options.ShouldPerformReapplication)
					{
						using (var progressReporter = GetProgressFromProvider().CreateProgressReporter(Res.GetString("CD21d25a-2809-42ee-806d-9e607c241ec7", "Preparing to reapply Workflow Templates"), workflowProviders.Length))
						{
							WorkflowReapplicationStrategy.ReapplyWorkflowTemplates(config, options, workflowProviders, progressReporter);
						}
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("27A1BB7E-BD39-42A4-AE67-EF89A89DC940", "Reapplication of workflow templates is not supported for:\r\n{0}", String.Join("\r\n", getter.NotSupportedBizObjNames)));
			}
		}

		IProgressReporterProvider GetProgressFromProvider()
		{
			return new DefaultProgressReporterProvider(System.Windows.Forms.Form.ActiveForm);
		}
	}
}
