using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentCusEntryNumberProxyValidation : ForwardingShipmentCustomsEntryNumberValidation
	{
		public ForwardingShipmentCusEntryNumberProxyValidation(ForwardingShipmentCusEntryNumberProxy parent)
			: base(parent)
		{
		}

		protected new ForwardingShipmentCusEntryNumberProxy Parent { get { return (ForwardingShipmentCusEntryNumberProxy)base.Parent; } }

		protected override void CheckEntryType()
		{
			if (Parent.IsParentShipment)
			{
				base.CheckEntryType();
				MandatoryValidation.CheckEntered(Parent.EntryTypeInfo);
			}
		}

		protected override void CheckEntryNumber()
		{
			if (Parent.IsParentShipment)
			{
				if (!Parent.AllowEmptyNumber && !Parent.EntryNumberInfo.ReadOnly)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.EntryNumberInfo);
				}
				else
				{
					base.CheckEntryNumber();
				}
				CheckDuplicateNumber();
			}
		}

		void CheckDuplicateNumber()
		{
			if (!Parent.CusEntryNumber.CE_EntryType.IsEmpty && !Parent.EntryNumberInfo.ReadOnly)
			{
				if (Parent.Shipment.CusEntryNumbers.Cast<CusEntryNumber>()
					.Where(x => x.CE_EntryType == Parent.CusEntryNumber.CE_EntryType && x.PK != Parent.CusEntryNumber.PK)
					.Select(x => x.CE_EntryNum)
					.Any(x => x == Parent.CusEntryNumber.CE_EntryNum))
				{
					Parent.EntryNumberInfo.AddError(ErrorCannotEnterDuplicateNumbers(Parent.CusEntryNumber.CE_EntryType));
				}
			}
		}

		public static string ErrorCannotEnterDuplicateNumbers(string entryType)
		{
			return Res.GetString("eebcc5f8-c0e4-49dd-af76-ece32826383a", "Entry Type {0}, however the same number already exists. Please enter a different {1} number.", entryType, entryType);
		}

		protected override void CheckIssueDate()
		{
			if (Parent.IsParentShipment)
			{
				base.CheckIssueDate();
			}
		}

		protected override void CheckExpiryDate()
		{
			if (Parent.IsParentShipment)
			{
				base.CheckExpiryDate();
			}
		}
	}
}
