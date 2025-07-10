using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	class RefJobEquipmentFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumns()
		{
			var searchBO = new RefJobEquipmentFilterBusinessObject();
			var collection = new JobEquipmentCollection(Factory);

			using (var filterControl = new RefJobEquipmentFilterControl(collection, searchBO))
			{
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible).Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();

				AssertArrayEqualsByElements(new[]
				{
					$"{JobEquipmentSchema.Constants.JEQ_Code} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{JobEquipmentSchema.Constants.JEQ_Description} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{JobEquipmentSchema.Constants.JEQ_IsActive} (ZCheckBoxColumnStyleInfo) IsVisible:True"
				}, columns);
			}
		}
	}
}
