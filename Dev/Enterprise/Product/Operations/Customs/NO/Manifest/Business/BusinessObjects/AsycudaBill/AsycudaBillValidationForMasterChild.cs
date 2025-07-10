using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Manifest.Business;

public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
{
	public AsycudaBillValidationForMasterChild(AsycudaBill parent) : base(parent)
	{
	}

	protected override void CheckABL_RL_NKPortOfLoading()
	{
	}

	protected override void CheckABL_RL_NKPortOfDischarge()
	{
	}

	protected override bool IsABL_E_DEPRequired => false;

	protected override void CheckMandatoryABL_E_ARV()
	{
	}

	protected override void CheckABL_CustomsFinalDestinationPort()
	{
		base.CheckABL_CustomsFinalDestinationPort();
		MandatoryValidation.MessageErrorIfNotEntered(Bill.ABL_CustomsFinalDestinationPortInfo);
	}

	#region Forwarder Address

	protected override void CheckABL_OA_Forwarder()
	{
		base.CheckABL_OA_Forwarder();
		CommmonBillValidation.ValidateForwarderForCusCodes();
	}

	protected override void CheckABL_ForwarderEmail()
	{
		base.CheckABL_ForwarderEmail();
		if (!Bill.ABL_OA_Forwarder.IsEmpty)
		{
			CommmonBillValidation.ValidateForwarderEmail();
		}
	}

	protected override void CheckABL_ForwarderPhone()
	{
		base.CheckABL_ForwarderPhone();
		if (!Bill.ABL_OA_Forwarder.IsEmpty)
		{
			CommmonBillValidation.ValidateForwarderPhone();
		}
	}

	#endregion

	AsycudaBillValidation CommmonBillValidation => new(Bill);

	AsycudaBill Bill => Parent as AsycudaBill;
}
