using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryHeaderCharges : AutoCusEntryHeaderCharges, Integration.Customs.TW.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.ChargeTypeList))]
		[ResourceStringData("0EB3C089-FD7C-4BA6-8F85-6936A4DC94E7", Caption = "Type")]
		public override ZString C1_ChargeType { get => base.C1_ChargeType; set => base.C1_ChargeType = value; }

		[ResourceStringData("93107331-BDD7-43F5-A964-0DB1AC2A6278", Caption = "Type Description", ShortCaption = "Type Desc.")]
		public ZString C1_ChargeTypeDesc { get => Lookups.ChargeTypeList.GetDescriptionFromCode(C1_ChargeType); }

		[ResourceStringData("1E5DBC6D-276E-47C2-8033-B414EAD1EE27", Caption = "Payment Method Description", ShortCaption = "Payment Method Desc.")]
		public ZString C1_MethodOfPaymentDesc { get => Lookups.PaymentMethodsList.GetDescriptionFromCode(C1_MethodOfPayment); }

		public override ZString C1_MethodOfPayment
		{
			get => base.C1_MethodOfPayment;
			set
			{
				var methodOfPayment = value;
				CheckChargeTypeUniqueness(C1_ChargeType, methodOfPayment);
				base.C1_MethodOfPayment = methodOfPayment;
			}
		}

		protected override void CheckChargeTypeUniqueness(ZString value)
		{
			CheckChargeTypeUniqueness(value, C1_MethodOfPayment);
		}

		void CheckChargeTypeUniqueness(ZString chargeType, ZString methodOfPayment)
		{
			if (EntryHeader != null && EntryHeader.Charges.GetChargesByTypeAndPaymentMethod(chargeType, methodOfPayment).Any(x => x.PK != PK))
			{
				ErrorReporter.ReportOnce("Not unique in this CusEntryHeaderChargesCollection.", chargeType + "-" + methodOfPayment + " is not unique in this CusEntryHeaderChargesCollection.");
			}
		}
	}
}
