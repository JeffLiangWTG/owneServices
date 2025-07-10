using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	class ZDropDownListTestClass : ZDropDownList
	{
		public string CachedSelectedValueExposedForTest
		{
			get { return CachedSelectedValue; }
			set { CachedSelectedValue = value; }
		}
	}
}
