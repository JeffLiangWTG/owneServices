using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowEventContextBizo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public WorkflowEventContextBizo(WorkflowDescriptor workflowDescriptor)
		{
			this.workflowDescriptor = workflowDescriptor;
		}

		readonly WorkflowDescriptor workflowDescriptor;

		public WorkflowEventContextCollection ContextCollection
		{
			get
			{
				if (contextCollection == null)
				{
					contextCollection = new WorkflowEventContextCollection(workflowDescriptor);
					RegisterEditableChildObject(contextCollection);

					ContextCollection.Refreshed +=
						(sender, e) =>
						{
							ContextPathCodeInfo.RefreshBinding();
							ContextPathDescriptiveInfo.RefreshBinding();
						};
				}
				return contextCollection;
			}
		}
		WorkflowEventContextCollection contextCollection;

		#region ContextPathCode

		[MaxLength(ProcessTasks.Schema.P9_CascadedEventsContextMaxLength)]
		public ZString ContextPathCode
		{
			get { return workflowDescriptor.GetContextStringFromPath(ContextCollection.GetPathUpToElement(null)); }
		}

		public ZPropertyInfo ContextPathCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ContextPathCode)); }
		}

		#endregion

		#region ContextPathDescriptive

		public ZString ContextPathDescriptive
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				foreach (var contextPair in ContextCollection.GetPathUpToElement(null))
				{
					if (sb.Length > 0)
					{
						sb.Append(WorkflowDescriptor.ContextStepsSeparator);
					}
					if (!string.IsNullOrEmpty(contextPair.MasterClassifier.Description))
					{
						sb.Append(contextPair.MasterClassifier.Description).Append(WorkflowDescriptor.ContextPartsSeparator);
					}
					sb.Append(contextPair.MasterType.Description);
				}
				return sb.ToString();
			}
		}

		public ZPropertyInfo ContextPathDescriptiveInfo
		{
			get { return GetZPropertyInfo(nameof(ContextPathDescriptive)); }
		}

		#endregion
	}
}
