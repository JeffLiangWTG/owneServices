using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ExternalRequestInfoTemplateForm : ZTemplateForm
	{
		public readonly ExternalRequestInfoTemplate ExternalRequestInfoTemplate;

		public ExternalRequestInfoTemplateForm(ExternalRequestInfoTemplate externalRequestInfoTemplate)
			: base(externalRequestInfoTemplate)
		{
			ExternalRequestInfoTemplate = externalRequestInfoTemplate;
		}

		#region Captions

		public override string FormCaption
		{
			get { return Enterprise.MasterFiles.GUI.Res.GetString("ExternalRequestInfoTemplateForm|FormCaption", "Request Template") + " " + ExternalRequestInfoTemplate.RIT_Code; }
		}

		#endregion

		protected override bool SupportsEDocs => false;
	}
}
