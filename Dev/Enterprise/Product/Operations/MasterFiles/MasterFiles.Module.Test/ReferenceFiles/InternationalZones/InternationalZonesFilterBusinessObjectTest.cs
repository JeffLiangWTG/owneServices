using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(InternationalZonesFilterBusinessObject))]
	sealed class InternationalZonesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InternationalZonesFilterBusinessObject();
		}

		#endregion

		[RequiresSTA]
		public void TestColumnsPresent()
		{
			var filter = new InternationalZonesFilterBusinessObject();

			var refZoneCollection = new RefZoneHeaderCollection(Factory);

			using (var internationalZonesControl = new InternationalZonesControl(refZoneCollection, filter))
			{
				foreach (var expectedColumn in expectedColumns)
				{
					var zOrganisationColumn = (ZGridColumnInfo)internationalZonesControl.FilteredGrid.ColumnStyles.ToArray()
						.FirstOrDefault(column => column is ZGridColumnInfo && ((ZGridColumnInfo)column).ColumnName == expectedColumn.Field);

					AssertNotNull(zOrganisationColumn);
					AssertEquals(expectedColumn.Name, zOrganisationColumn.CaptionResourceString.Caption);
				}
			}
		}

		[RequiresSTA]
		public void TestFiltersPresent()
		{
			var filter = new InternationalZonesFilterBusinessObject();

			var refZoneCollection = new RefZoneHeaderCollection(Factory);

			using (var internationalZonesControl = new InternationalZonesControl(refZoneCollection, filter))
			{
				foreach (var expectedColumn in expectedColumns)
				{
					Assert(filter.ModuleFilters.Filter_List.ContainsCode(expectedColumn.Name));
				}
			}
		}

		readonly (string Field, string Name)[] expectedColumns = new []
		{
			("FZ_Code", "Code"),
			("FZ_DescriptionMultilingual", "Description"),
			("FZ_ZoneType", "Zone Type"),
			("FZ_ZoneMode", "Zone Mode"),
			("FZ_OH_RelatedParty", "Carrier/Customer/Gateway"),
		};
	}
}
