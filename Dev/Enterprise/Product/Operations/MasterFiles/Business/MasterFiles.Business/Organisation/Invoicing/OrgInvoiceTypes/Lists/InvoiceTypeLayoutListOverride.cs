using CargoWise.Common;

namespace Enterprise.MasterFiles.Business
{
	public class InvoiceTypeLayoutList : AutoInvoiceTypeLayoutList
	{
		protected InvoiceTypeLayoutList()
			: base()
		{
		}

		public static InvoiceTypeLayoutList New()
		{
			InvoiceTypeLayoutList result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				result = new InvoiceTypeLayoutList();
			}
			return result;
		}

		protected delegate InvoiceTypeLayoutList NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
	}
}
