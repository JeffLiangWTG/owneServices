using CargoWise.EntityFramework;
using Enterprise.Tracking.Web.Quotes;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class QuotationTestPage : Quotation
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		internal ZDropDownList VolumeDropDownInternal
		{
			get { return VolumeDropDown; }
			set { VolumeDropDown = value; }
		}

		internal BusinessObject GetNewDataSourceInternal() => GetNewDataSource();
	}
}
