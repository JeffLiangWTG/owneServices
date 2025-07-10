using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USExportClassificationFilterControlTest : TestCase
	{
		public void TestScheduleBCaptionExists()
		{
			var factory = new BusinessObjectFactory();
			var filterBO = new USExportClassificationFilterBusinessObject();
			var filterCollection = new Customs.Business.BaseClassificationCollection<CusClassification>(factory);
			using (var control = new USExportClassificationFilterControl(filterCollection, filterBO))
			{
				Core.Forms.ZGridColumnInfo columnInfo1 = null;
				foreach (Core.Forms.ZGridColumnInfo columnInfo in control.FilteredGrid.ColumnStyles)
				{
					if (columnInfo.ColumnName == "CC_FormattedTariffNum")
					{
						columnInfo1 = columnInfo;
						break;
					}
				}

				AssertNotNull("Schedule B column should have been found", columnInfo1);
				AssertEquals("Schedule B", columnInfo1.Caption);
			}
		}
	}
}
