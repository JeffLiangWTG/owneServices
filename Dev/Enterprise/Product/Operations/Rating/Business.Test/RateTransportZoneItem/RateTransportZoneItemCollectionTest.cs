
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTransportZoneItemCollection))]
	public class RateTransportZoneItemCollectionTest : ActiveBusinessObjectCollectionTestCase<RateTransportZoneItemCollection>
	{
		protected override RateTransportZoneItemCollection GetCollectionToTest()
		{
			var zone = Factory.New<RateTransportZone>();
			return new RateTransportZoneItemCollection(zone);
		}

		public void TestCollectionIsIImportWizardProvider()
		{
			var collection = GetCollectionToTest();
			var provider = Factory.New<RateTransportProvider>();
			(collection.Relationship.Master as RateTransportZone).TZ_TP = provider.PK;
			Assert(collection is IImportWizardProvider);
			AssertType<RateTransportZoneItemImportWizard>(((IImportWizardProvider)collection).GetImportWizard(new ImportCollectionInfoImpl(collection), null, null));
		}

		public void TestCollectionImportWithEmptyValues()
		{
			TestCollectionImportWithEmptyValue_WithColumn(RateTransportZoneItem.Schema.TQ_FromDistance);
			TestCollectionImportWithEmptyValue_WithColumn(RateTransportZoneItem.Schema.TQ_ToDistance);
			TestCollectionImportWithEmptyValue_WithColumn(RateTransportZoneItem.Schema.TQ_IsBeyond);
			TestCollectionImportWithEmptyValue_WithColumn(RateTransportZoneItem.Schema.TQ_IsExcludingPostCode);
			TestCollectionImportWithEmptyValue_WithColumn(RateTransportZoneItem.Schema.TQ_FromPostCode);
			TestCollectionImportWithEmptyValue_WithColumn(RateTransportZoneItem.Schema.TQ_ToPostCode);
			TestCollectionImportWithEmptyValue_WithColumn(RateTransportZoneItem.Schema.TQ_R9_CityTown);
		}

		void TestCollectionImportWithEmptyValue_WithColumn(string columnName)
		{
			var collection = GetCollectionToTest();
			var collectionWizardProvider = (IImportWizardProvider)collection;
			var provider = Factory.New<RateTransportProvider>();
			(collection.Relationship.Master as RateTransportZone).TZ_TP = provider.PK;

			var collectionInfoForImport = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<RateTransportZoneItem>(columnName) { HeaderText = columnName }
			};

			var wizard = (collectionWizardProvider).GetImportWizard(collectionInfoForImport, null, new FileMapperForTest());

			for (int i = 0; i < wizard.Mapping.Count; i++)
			{
				wizard.Mapping[i].AddFileColumnIndex(i);
			}

			using (var xls = CreateTestXls
			(
				new object[,] {
					{ $"{columnName}", "SomethingElse", "RocketShip" },
					{ "", 33, 56 },
					{ "", 33, 32 },
				},
				new string[,] {
					{ null, null, null },
					{ null, null, null },
				}
			))
			{
				wizard.FileName = xls.Filename;
				wizard.StartingRow = 2;
				AssertNoExceptionThrown(() => wizard.ImportIntoCollection(collection));
			}
		}

		#region Helpers

		TempFile CreateTestXls(object[,] values, string[,] formats)
		{
			using (IExcelInterface excelDoc = ExcelInterfaceFactory.New())
			{
				excelDoc.NewExcelFile(1);
				for (int i = 0; i < values.GetLength(0); i++)
				{
					for (int j = 0; j < values.GetLength(1); j++)
					{
						excelDoc.WorkSheets[0][i, j] = values[i, j];
						if (formats != null && i < formats.GetLength(0) && j < formats.GetLength(1) && !string.IsNullOrEmpty(formats[i, j]))
						{
							CellFormat cellFormat = new CellFormat();
							cellFormat.FormatPattern = formats[i, j];
							excelDoc.WorkSheets[0].SetCellFormat(i, j, cellFormat);
						}
					}
				}
				TempFile result = TempFile.NewWithExtension("xls");
				excelDoc.SaveToFile(result.Filename);

				return result;
			}
		}

		#endregion
	}
}
