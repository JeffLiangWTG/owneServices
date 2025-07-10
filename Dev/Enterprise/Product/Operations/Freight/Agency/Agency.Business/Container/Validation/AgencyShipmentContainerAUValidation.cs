using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentContainerAUValidation : JobContainerValidation
	{
		public AgencyShipmentContainerAUValidation(AgencyShipmentContainer container)
			: base(container)
		{
		}

		public void ValidateCustomsEntryNumberType()
		{
			ValidateCalculatedProperty(Parent.CustomsEntryNumberTypeInfo);
		}

		protected virtual void CheckCustomsEntryNumberType()
		{
			if (Parent.CustomsEntryNumberType_List.Count > 0)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CustomsEntryNumberTypeInfo, Parent.CustomsEntryNumberType_List);
			}
			else
			{
				if (!Parent.CustomsEntryNumberType.IsEmpty)
				{
					Parent.CustomsEntryNumberTypeInfo.AddError(Res.GetString("a682c9e9-4440-4fd4-9ff8-681f2baf8fe3", "Entry Type should be Empty"));
				}
			}
		}

		public void ValidateCustomsEntryNumber()
		{
			ValidateCalculatedProperty(Parent.CustomsEntryNumberInfo);
		}

		protected virtual void CheckCustomsEntryNumber()
		{
			if (Parent.Booking != null)
			{
				if (Parent.Booking.CusEntryNumbers.Count > 0 && !Parent.CustomsEntryNumberType.IsEmpty)
				{
					Parent.CustomsEntryNumberInfo.AddError(Res.GetString("e45b63a5-141c-4d73-823a-ef5e3b18f8d2", "CAN is available on either Shipment OR Container level"));
				}
			}

			if (!Parent.CustomsEntryNumberInfo.ReadOnly && !Parent.CustomsEntryNumberType.IsEmpty)
			{
				if (Parent.CustomsEntryNumber.IsEmpty)
				{
					Parent.CustomsEntryNumberInfo.AddWarning(Res.GetString("bc513cae-263a-4284-8478-5bbe24d58c25", "Entry Number is not specified"));
				}
				else if (Parent.CustomsEntryNumberType == CANType.CustomsAuthorityNumber.Code)
				{
					ZString invalidReason = new CANValidation().GetInvalidReason(Parent.CustomsEntryNumber);
					if (!invalidReason.IsEmpty)
					{
						Parent.CustomsEntryNumberInfo.AddWarning(invalidReason);
					}
				}
			}
		}

		#region Implementation

		protected new AgencyShipmentContainer Parent
		{
			get { return (AgencyShipmentContainer)base.Parent; }
		}

		#endregion
	}
}
