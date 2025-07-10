using CargoWise.Interop.DataObjects;

namespace Enterprise.MasterFiles.GUI
{
	public class ZOutlookEmailSenderDataObject : ZDataObject
	{
		protected ZOutlookEmailSenderDataObject(string format, object data)
			: base(format, data)
		{
		}

		public new static ZOutlookEmailSenderDataObject FromData(string format, object data)
		{
			return new ZOutlookEmailSenderDataObject(format, data);
		}
	}
}
