using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ProductImageForTest : ProductImage
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public ProductImageForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
		}
	}
}
