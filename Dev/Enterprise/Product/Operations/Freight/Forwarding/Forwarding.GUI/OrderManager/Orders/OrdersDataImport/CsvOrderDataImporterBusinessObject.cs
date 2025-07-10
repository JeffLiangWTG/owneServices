using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Freight.Forwarding.GUI.Orders.DataTransfer
{
	public class CsvOrderDataImporterBusinessObject : DataImporterBusinessObject
	{
		public CsvOrderDataImporterBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZString ProgressMessageForFatalError
		{
			get
			{
				return RecordsAdded > 0 || RecordsUpdated > 0
					? (ZString)("\r\n" + Res.GetString("05f43269-dabd-4b66-b78c-8039ed78d701", "Some Orders could not be imported due to the above errors.") + "\r\n")
					: base.ProgressMessageForFatalError;
			}
		}
	}
}
