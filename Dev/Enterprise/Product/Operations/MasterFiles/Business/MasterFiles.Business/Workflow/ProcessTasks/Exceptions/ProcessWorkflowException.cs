using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(ProcessTask), "PK")]
	public class ProcessWorkflowException : AutoProcessWorkflowException
	{
		public ProcessWorkflowException(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var cause = Factory.NewWithValidTestData<ProcessWorkflowExceptionCause>();
			var resolution = Factory.NewWithValidTestData<ProcessWorkflowExceptionResolution>();

			base.FillWithValidTestDataCore(kind, propertyPath);

			WEX_P9_ProcessTask = task.PK;
			WEX_WEC_Cause = cause.PK;
			WEX_WER_Resolution = resolution.PK;
		}
#endif
		
		public override bool IsSavedByFactory => base.IsSavedByFactory && HasChanges;
	}
}
