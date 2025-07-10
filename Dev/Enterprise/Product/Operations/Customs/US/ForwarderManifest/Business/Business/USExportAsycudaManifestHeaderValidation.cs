using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public USExportAsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}
		protected new USExportAsycudaManifestHeader Parent => (USExportAsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_RL_NKPortOfFinalDeparture()
		{
			base.CheckAMA_RL_NKPortOfFinalDeparture();
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_RL_NKPortOfFinalDepartureInfo);
		}

		protected override void CheckAMA_CarrierCode()
		{
		}

		protected override void CheckMasterBOL()
		{
			base.CheckMasterBOL();
			if (Parent.IsConsolidator && Parent.MasterBOL.IsEmpty != Parent.MasterBill.ABL_BillIssuer.IsEmpty)
			{
				Parent.MasterBOLInfo.AddMessageError(OnlyOneOfIssuerCodeAndBillOfLadingNumberHasValue);
			}
		}

		ZString OnlyOneOfIssuerCodeAndBillOfLadingNumberHasValue
		{
			get { return Res.GetString("B5BA0F58-811E-4D7B-B210-096C8C0033CE", "Both the Issuer Code and Bill of Lading Number should be completed if you are entering a master bill. If a master bill is entered here, all bills entered on the Bills tab will then be treated as house bills.\nIf you wish to enter Direct Bills on the Bills tab, please leave both the Issuer Code and Bill of Lading Number on this Header tab blank."); }
		}
	}
}
