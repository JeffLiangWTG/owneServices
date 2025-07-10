using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceProcessingErrorLog : AutoAccDraftInvoiceProcessingErrorLog
	{
		public AccDraftInvoiceProcessingErrorLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.ErrorCodeList")]
		public override ZString AIL_Code
		{
			get => base.AIL_Code;
			set => base.AIL_Code = value;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AIL_Code = Enterprise.ZArchitecture.Core.AccDraftInvoiceProcessingErrorCodes.SystemExceptionOccurred;
		}
#endif
	}
}
