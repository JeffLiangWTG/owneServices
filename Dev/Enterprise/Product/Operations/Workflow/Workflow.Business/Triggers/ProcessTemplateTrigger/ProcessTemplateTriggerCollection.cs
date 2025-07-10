using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessTemplateTriggerCollection : ActiveBusinessObjectCollection<ProcessTemplateTrigger>, ITemplateTriggerCollection
	{
		public ProcessTemplateTriggerCollection(ProcessTaskTemplate template)
			: base(template.Factory, template, new ZQuery(), ProcessTemplateTriggerSchema.P9T_P0_Template)
		{
		}

		#region ActiveBusinessObjectCollection Overrides

		protected override void SetDefaultsForNewElementCore(ProcessTemplateTrigger newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var lastBySequence = this.MaxBySafe(t => t.P9T_Sequence);

			newElement.P9T_Sequence = (lastBySequence?.P9T_Sequence ?? 0) + 1;
		}

		#endregion

		#region ITemplateTriggerCollection Members

		ITemplateTrigger ITemplateTriggerCollection.this[int index]
		{
			get { return this[index]; }
		}

		#endregion
	}
}
