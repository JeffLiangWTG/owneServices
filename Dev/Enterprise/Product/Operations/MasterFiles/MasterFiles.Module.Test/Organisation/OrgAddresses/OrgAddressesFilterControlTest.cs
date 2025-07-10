using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgAddressesFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGridColumnsExactlyLikeAddressUserControlGrid()
		{
			using (var addressesUserControl = new AddressesUserControl())
			using (var filterControl = new OrgAddressesFilterControl(Factory))
			{
				var expectedColumns = (typeof(AddressesUserControl).GetField("OrgAddressBoundGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(addressesUserControl) as ZGrid).ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				var actualColumns = filterControl.Grid.ColumnStyles;

				foreach (ZGridColumnInfo actual in actualColumns)
				{
					if (actual.ColumnName == "OA_SystemCreateTimeUtc" ||
						actual.ColumnName == "OA_SystemCreateUser" ||
						actual.ColumnName == "OA_SystemLastEditTimeUtc" ||
						actual.ColumnName == "OA_SystemLastEditUser")
					{
						continue;
					}

					Assert(expectedColumns.Any(col => col.ColumnName == actual.ColumnName && col.Caption == actual.Caption && col.IsMandatory == actual.IsMandatory));
				}
			}
		}
	}
}
