using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(ProcessWorkflowExceptionType), "PK")]
	public class ProcessWorkflowExceptionCause : AutoProcessWorkflowExceptionCause
	{
		public ProcessWorkflowExceptionCause(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			base.FillWithValidTestDataCore(kind, propertyPath);
			WEC_WET_Type = type.PK;
		}

		public override bool CanDelete => !HasProcessWorkflowException;

		bool HasProcessWorkflowException
		{
			get
			{
				var query = new ZQuery(ProcessWorkflowExceptionSchema.WEX_WEC_Cause, PK);
				return Factory.LoadTop1<ProcessWorkflowException>(query) != null;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete =>
			HasProcessWorkflowException
			? ResString.GetMultilingualString("04BF0EB6-A8AB-47F4-A702-238B72B678E7", "Unable to delete Cause {0} because it is currently in use.", WEC_Code)
			: base.ReasonForNotAbleToDelete;
#endif
	}
}
