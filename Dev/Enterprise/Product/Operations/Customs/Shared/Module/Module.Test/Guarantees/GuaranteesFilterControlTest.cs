using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module.Testing
{
	sealed class GuaranteesFilterControlTest : TestCaseWithFactory
	{
		public void TestCreationCodeColumn()
		{
			var collection = new CusGuaranteeHeaderCollection(Factory);
			var filterBizO = new GuaranteesFilterStripBusinessObject();
			using (var form = new ZForm())
			using (var filterControl = new GuaranteesFilterControl(collection, filterBizO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertEquals("Column name", "Creation Country", filterControl.Grid.GetColumnCaption(CusPermitHeaderSchema.Constants.CPH_RN_NKCountryCode));
			}
		}
	}
}
