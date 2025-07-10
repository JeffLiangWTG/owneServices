using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ExternalRequestTypeForm : ZTemplateForm
	{
		public readonly ExternalRequestType ExternalRequestType;

		public ExternalRequestTypeForm(ExternalRequestType externalRequestType)
			: base(externalRequestType)
		{
			ExternalRequestType = externalRequestType;
		}

		#region Captions

		public override string FormCaption
		{
			get { return Enterprise.MasterFiles.GUI.Res.GetString("ExternalRequestTypeForm|FormCaption", "Request Type") + " " + ExternalRequestType.RQT_Code; }
		}

		#endregion

		protected override bool SupportsEDocs => false;
	}
}
