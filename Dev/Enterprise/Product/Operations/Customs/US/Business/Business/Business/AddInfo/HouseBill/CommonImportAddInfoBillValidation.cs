using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class CommonImportAddInfoBillValidation : AddInfoBillValidation
	{
		public CommonImportAddInfoBillValidation(AddInfoBill addInfoHouseBill)
			: base(addInfoHouseBill)
		{
		}

		#region US_UI_NKBillIssuerSCAC

		protected override void CheckUS_UI_NKBillIssuerSCAC()
		{
			base.CheckUS_UI_NKBillIssuerSCAC();

			if (IsMasterBill || IsHouseBill)
			{
				if (ShouldValidateUS_UI_NKBillIssuerSCAC)
				{
					var declaration = Parent.Declaration;
					if (Parent.US_UI_NKBillIssuerSCAC.IsEmpty)
					{
						if ((IsMasterBill && declaration.IsMasterBillSCACRequired) || (IsHouseBill && declaration.IsHouseBillSCACRequired))
						{
							Parent.US_UI_NKBillIssuerSCACInfo.AddMessageError(SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
						}
					}
					else
					{
						new IssuerCarrierSCACValidator(Parent.Factory).ValidateSCACCode(Parent.US_UI_NKBillIssuerSCACInfo, declaration.JE_TransportMode, "Issuer", !declaration.IsACEAutoRoadAndPedTransportMode);

						switch (Parent.Bill.CU_BillType)
						{
							case BillTypeList.Codes.MasterBill:
								Parent.Declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
								break;
							case BillTypeList.Codes.HouseBill:
								Parent.Declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
								break;
						}
					}
				}
				Parent.Bill.Validation.ValidateCU_BillNum();
			}

			CheckIssuerSCACNotAllowed();
		}
		internal const string SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail = "Standard Carrier Alpha Code (SCAC) required for Master Bill (when Mode of Transport is Sea, Rail, Air, Truck or BWB) and House Bill (when Mode Of Transport is Sea or Rail.)";
		internal const string IssuerSCACiSNotPermitted = "Issuer SCAC not allowed for this Transport Mode";

		protected virtual bool ShouldValidateUS_UI_NKBillIssuerSCAC
		{
			get { return true; }
		}

		protected virtual void CheckIssuerSCACNotAllowed()
		{
		}

		#endregion

		#region US_VolumeUQ

		protected override void CheckUS_VolumeUQ()
		{
			base.CheckUS_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_VolumeUQInfo, Parent.Parent.Lookups.VolumeUQList);

			if (!Parent.US_Volume.IsEmpty && Parent.US_VolumeUQ.IsEmpty)
			{
				Parent.US_VolumeUQInfo.AddMessageError("Volume UQ is required when Volume has a value.");
			}
		}

		#endregion

		#region US_WeightUQ

		protected override void CheckUS_WeightUQ()
		{
			base.CheckUS_WeightUQ();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_WeightUQInfo, Parent.Parent.Lookups.WeightUQList);
		}

		#endregion

		public Bill Bill
		{
			get { return Parent.Parent; }
		}
		#region Boolean Flags

		protected bool IsMasterBill
		{
			get { return Bill.IsMasterBill; }
		}

		protected bool IsHouseBill
		{
			get { return Bill.IsHouseBill; }
		}

		#endregion
	}
}
