using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public sealed class TradersRemarkValidation : CusSupportingInfoValidation
	{
		public TradersRemarkValidation(TradersRemark parent)
		: base(parent)
		{
			this.parent = parent;
		}
		readonly TradersRemark parent;
		JobDeclaration declaration => parent.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			TradersRemarksRowValidation();
		}

		void TradersRemarksRowValidation()
		{
			if (!declaration.HasCofO && declaration.TradersRemarks.Count > 2)
			{
				Parent.AddRowMessageError(Res.GetString("BFC2D8CF-DA3A-4D4A-BC23-68913C36B063", "Only two lines of Traders Remarks are allowed, except for Outward (OUT) with COO."));
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			if (declaration.IsStandAloneCertificateOfOrigin)
			{
				Parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("3430C4C5-8D0A-4718-963D-B4564A908914", "Traders Remarks are NOT sent in a stand-alone Certificate of Origin (COO) declaration. If you need to send free text information for a COO, you should enter it in the Additional Information field on the Certificate of Origin tab."));
			}
		}
	}
}
