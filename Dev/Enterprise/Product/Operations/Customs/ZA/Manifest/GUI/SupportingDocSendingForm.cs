using CargoWise.Types;
using Enterprise.Customs.ZA.Manifest.Business;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public partial class SupportingDocSendingForm : Customs.GUI.SupportingDocSendingForm
	{
		public SupportingDocSendingForm()
		{
		}

		public SupportingDocSendingForm(ManifestSupportingDocSendingObjectParent manifestWrapper)
			: base(manifestWrapper)
		{
		}

		public override string FormHeading
		{
			get { return Res.GetString("0ED25BAC-496D-45A5-84E1-1F3469184A34", "Send Supporting Documents"); }
		}

		protected override ZBool LRNColumnVisible => ZBool.False;

		protected override ZBool CaseNumberColumnVisible => ZBool.True;
	}
}
