using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(ProcessWorkflowExceptionType), "PK")]
	public class ProcessWorkflowExceptionResolution : AutoProcessWorkflowExceptionResolution
	{
		public ProcessWorkflowExceptionResolution(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			base.FillWithValidTestDataCore(kind, propertyPath);
			WER_WET_Type = type.PK;
		}

		public override bool CanDelete => !HasProcessWorkflowException;

		bool HasProcessWorkflowException
		{
			get
			{
				var query = new ZQuery(ProcessWorkflowExceptionSchema.WEX_WER_Resolution, PK);
				return Factory.LoadTop1<ProcessWorkflowException>(query) != null;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete =>
			HasProcessWorkflowException
			? ResString.GetMultilingualString("BF3D84BE-516D-4D00-A16A-2B7DB93A0D5C", "Unable to delete Resolution {0} because it is currently in use.", WER_Code)
			: base.ReasonForNotAbleToDelete;
#endif
	}
}
