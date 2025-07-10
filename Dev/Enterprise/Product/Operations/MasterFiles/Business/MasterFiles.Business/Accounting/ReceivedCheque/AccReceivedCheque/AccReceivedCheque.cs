using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccReceivedCheque : AutoAccReceivedCheque
	{
		public AccReceivedCheque(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("30ea4397-7ef5-44a7-b3b7-23f0c3370ec7", "Received Cheque");

		#region Set Default Values

		const string defaultStatus = "COH";  // TODO: Cheque statuses will be defined as Constants

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			RCH_Status = defaultStatus;
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			RCH_Amount = 1M;
		}
#endif
	}
}
