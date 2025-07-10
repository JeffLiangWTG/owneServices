using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class USImportClassificationFilterControlTest : TestCase
	{
		public void TestFormattedTariffCaptionExists()
		{
			var factory = new BusinessObjectFactory();
			var filterBO = new USImportClassificationFilterBusinessObject();
			var filterCollection = new Customs.Business.BaseClassificationCollection<CusClassification>(factory);
			using (var control = new USImportClassificationFilterControl(filterCollection, filterBO))
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

				AssertNotNull("Formatted Tariff column should have been found", columnInfo1);
				AssertEquals("Tariff No.", columnInfo1.Caption);
			}
		}
	}
}
