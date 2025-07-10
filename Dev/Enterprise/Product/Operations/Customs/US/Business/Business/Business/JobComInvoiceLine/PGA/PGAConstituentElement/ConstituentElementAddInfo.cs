using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USPGA)]
	public class ConstituentElementAddInfo : AutoUSConstituentElementAddInfo
	{
		public ConstituentElementAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		protected override USConstituentElementAddInfoValidation GetNewValidation()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine != null)
			{
				if (invoiceLine.IsACECargoCertificationMode)
				{
					return new ACEConstituentElementAddInfoValidation(this);
				}
				else
				{
					return new ACSConstituentElementAddInfoValidation(this);
				}
			}
			else
			{
				var constElement = (ConstituentElement)Parent;

				var aceFDA = constElement.Parent as ACEFDA;
				if (aceFDA != null)
				{
					return new ACEConstituentElementAddInfoValidation(this);
				}
				else
				{
					return new USConstituentElementAddInfoValidation(this);
				}
			}
		}

		public AutoConstituentElement ConstituentElement => Parent as AutoConstituentElement;

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				var result = Parent as ConstituentElement;
				return result != null ? result.InvoiceLine : null;
			}
		}
	}
}
