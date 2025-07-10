using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USFDA)]
	public class USFDAAddInfo : AutoUSFDAAddInfo
	{
		public USFDAAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public override ZDecimal US_FDAValue
		{
			get { return base.US_FDAValue; }
			set
			{
				base.US_FDAValue = value;
				FDA parentFDA = Parent;
				if (parentFDA != null && parentFDA.InvoiceLine != null)
				{
					parentFDA.InvoiceLine.MarkAsNeedingValidation();
				}
			}
		}

		public new FDA Parent => base.Parent as FDA;

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
