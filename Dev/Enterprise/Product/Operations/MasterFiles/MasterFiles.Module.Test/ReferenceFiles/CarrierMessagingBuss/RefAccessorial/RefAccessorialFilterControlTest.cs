using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	class RefAccessorialFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumns()
		{
			var searchBO = new RefAccessorialFilterBusinessObject();
			var collection = new RefAccessorialCollection(Factory);

			using (var filterControl = new RefAccessorialFilterControl(collection, searchBO))
			{
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();

				AssertArrayEqualsByElements(new[]
				{
					$"{RefAccessorialSchema.Constants.ASI_Code} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{RefAccessorialSchema.Constants.ASI_Description} (ZTextBoxColumnStyleInfo) IsVisible:True",
				}, columns);
			}
		}
	}
}
