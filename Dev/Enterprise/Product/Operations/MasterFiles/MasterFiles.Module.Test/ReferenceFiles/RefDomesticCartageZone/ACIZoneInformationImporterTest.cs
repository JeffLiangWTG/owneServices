using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ACIZoneInformationImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			using (var form = new ZForm())
			{
				var zones = Factory.Load<RefDomesticCartageZone>(new ZQuery());
				AssertEquals(0, zones.Length);

				var importer = new ACIZoneInformationImporter(form);
				importer.Import(() => { return File.OpenRead("dummy.txt"); }, "dummy.txt");
				AssertContains("dummy.txt could not be opened due to the error -", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (var tmpFile = TempFile.New())
				using (var tmpFile2 = TempFile.New())
				{
					CreateTestACIFile(tmpFile.Filename);
					CreateTestACIFileWithMod(tmpFile2.Filename);

					importer.Import(() => { return File.OpenRead(tmpFile.Filename); }, tmpFile.Filename);
					AssertEquals("The file has been imported successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					zones = Factory.Load<RefDomesticCartageZone>(new ZQuery());
					AssertEquals(6, zones.Length);
					foreach (var zone in zones)
					{
						AssertZoneExists(Factory, "Abbot", "ME", "04406", "BGR", "USBGR", "BANGOR / BGR", "04401", 56.50m, "A", false);
						AssertZoneExists(Factory, "Abbotsford", "WI", "54405", "ATW", "USATW", "APPLETON  / ATW", "54911", 130.00m, "ABCDE", false);
						AssertZoneExists(Factory, "Abbottstown", "PA", "17301", "ABE", "USABE", "ALLENTOWN / ABE", "18109", 123.00m, "B", false);
						AssertZoneExists(Factory, "Abbyville", "KS", "67510", "ICT", "USICT", "WICHITA / ICT", "67209", 59.60m, "F", false);
						AssertZoneExists(Factory, "Abell", "MD", "20606", "BWI", "USBAL", "BALTIMORE / BWI", "21240", 80.60m, "E", true);
						AssertZoneExists(Factory, "Aberdeen Proving Ground", "MD", "21005", "ABE", "USABE", "ALLENTOWN / ABE", "18109", 128.00m, "", false);
					}

					importer.Import(() => { return File.OpenRead(tmpFile.Filename); }, tmpFile.Filename);
					AssertEquals("The file has been imported successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					zones = Factory.Load<RefDomesticCartageZone>(new ZQuery());
					AssertEquals(6, zones.Length);

					importer.Import(() => { return File.OpenRead(tmpFile2.Filename); }, tmpFile2.Filename);
					zones = Factory.Load<RefDomesticCartageZone>(new ZQuery());
					AssertEquals(7, zones.Length);
					var factory2 = new BusinessObjectFactory();
					AssertZoneExists(factory2, "Abbottstown", "PA", "17301", "ABE", "USABE", "ALLENTOWN / ABE", "18109", 123.00m, "B", true);
					AssertZoneExists(factory2, "Abell", "MD", "20606", "BWI", "USBAL", "BALTIMORE / BWI", "21240", 200.60m, "G", true);
					AssertZoneExists(factory2, "Zubin", "MD", "99999", "ABE", "USABE", "ALLENTOWN / ABE", "18109", 128.00m, "", false);
				}
			}
		}

		public void TestImportWithWrongCharacters()
		{
			using (var form = new ZForm())
			{
				var importer = new ACIZoneInformationImporter(form);
				using (TempFile tmpFile = TempFile.New())
				{
					CreateTestACIFileWithWrongCharacters(tmpFile.Filename);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					importer.Import(() => { return File.OpenRead(tmpFile.Filename); }, tmpFile.Filename);
					var zones = Factory.Load<RefDomesticCartageZone>(new ZQuery());
					AssertEquals(0, zones.Length);
					AssertContains("Answer 'Yes' if you want the system to automatically remove this and all following incorrect characters during import.\r\nAnswer 'No' to stop import process.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					importer.Import(() => { return File.OpenRead(tmpFile.Filename); }, tmpFile.Filename);
					AssertContains("Answer 'Yes' if you want the system to automatically remove this and all following incorrect characters during import.\r\nAnswer 'No' to stop import process.", UnitTestUserNotification.Instance.LastMessage.Text);

					zones = Factory.Load<RefDomesticCartageZone>(new ZQuery());
					AssertEquals(2, zones.Length);
					foreach (var zone in zones)
					{
						AssertZoneExists(Factory, "Magness?", "AR", "72553", "LIT", "USLIT", "LIT / LITTLE ROCK", "72202", 97.50m, "G", false);
						AssertZoneExists(Factory, "Magness1?", "AR", "72553", "LIT", "USLIT", "LIT / LITTLE ROCK", "72202", 97.50m, "G", false);
					}
				}
			}
		}

		public void TestImportWithIncorrectFormat()
		{
			using (var form = new ZForm())
			{
				var importer = new ACIZoneInformationImporter(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (var tmpFile = TempFile.New())
				{
					CreateTestACIFileWithIncorrectFormat(tmpFile.Filename);
					importer.Import(() => { return File.OpenRead(tmpFile.Filename); }, tmpFile.Filename);
					AssertEquals("The file format is not correct. Import has been canceled. Please review the file and import again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation

		void CreateTestACIFile(string fileName)
		{
			var contents = "";
			contents += "\"Abbot, ME\",\"04406\",\"BGR\",\"BANGOR / BGR\",\"04401\",56.50,\"A\"\r\n";
			contents += "\"Abbotsford, WI\",\"54405\",\"ATW\",\"APPLETON  / ATW\",\"54911\",130.00,\"ABCDEFG\"\r\n";
			contents += "\"Abbottstown, PA\",\"17301\",\"ABE\",\"ALLENTOWN / ABE\",\"18109\",123.00,\"B\",0\r\n";
			contents += "\"Abbyville, KS\",\"67510\",\"ICT\",\"WICHITA / ICT\",\"67209\",59.60,\"F\"\r\n";
			contents += "\"Abell, MD\",\"20606\",\"BWI\",\"BALTIMORE / BWI\",\"21240\",80.60,\"E\",1\r\n";
			contents += "\"Aberdeen Proving Ground, MD\",\"21005\",\"ABE\",\"ALLENTOWN / ABE\",\"18109\",128.00,";

			File.WriteAllText(fileName, contents);
		}

		void CreateTestACIFileWithIncorrectFormat(string fileName)
		{
			var contents = "";
			contents += "\"Abbot, ME\",\"04406\",\"BGR\",\"BANGOR / BGR\",\"04401\",56.50,56.50,\"A\"\r\n";
			contents += "\"Abbotsford, WI\",\"54405\",\"ATW\",\"APPLETON  / ATW\",\"54911\",130.00,130.00,\"ABCDEFG\"\r\n";
			contents += "\"Abbottstown, PA\",\"17301\",\"ABE\",\"ALLENTOWN / ABE\",\"18109\",123.00,123.00,\"B\",0\r\n";
			contents += "\"Abbyville, KS\",\"67510\",\"ICT\",\"WICHITA / ICT\",\"67209\",59.60,59.60,\"F\"\r\n";
			contents += "\"Abell, MD\",\"20606\",\"BWI\",\"BALTIMORE / BWI\",\"21240\",80.60,80.60,\"E\",1\r\n";
			contents += "\"Aberdeen Proving Ground, MD\",\"21005\",\"ABE\",\"ALLENTOWN / ABE\",\"18109\",128.00,128.00,";

			File.WriteAllText(fileName, contents);
		}

		void CreateTestACIFileWithMod(string fileName)
		{
			var contents = "";
			contents += "\"Abbot, ME\",\"04406\",\"BGR\",\"BANGOR / BGR\",\"04401\",56.50,\"A\"\r\n";
			contents += "\"Abbotsford, WI\",\"54405\",\"ATW\",\"APPLETON  / ATW\",\"54911\",130.00,\"A\"\r\n";
			contents += "\"Abbottstown, PA\",\"17301\",\"ABE\",\"ALLENTOWN / ABE\",\"18109\",123.00,\"B\",\"YES\"\r\n";
			contents += "\"Abbyville, KS\",\"67510\",\"ICT\",\"WICHITA / ICT\",\"67209\",59.60,\"F\"\r\n";
			contents += "\"Abell, MD\",\"20606\",\"BWI\",\"BALTIMORE / BWI\",\"21240\",200.60,\"G\"\r\n";
			contents += "\"Aberdeen Proving Ground, MD\",\"21005\",\"ABE\",\"ALLENTOWN / ABE\",\"18109\",128.00,\r\n";
			contents += "\"Zubin, MD\",\"99999\",\"ABE\",\"ALLENTOWN / ABE\",\"18109\",128.00,";

			File.WriteAllText(fileName, contents);
		}

		void CreateTestACIFileWithWrongCharacters(string fileName)
		{
			var contents = "";
			var incorrectChar = char.ConvertFromUtf32(0xA0)[0];
			contents += "\"Magness" + incorrectChar + ", AR\",\"72553\",\"LIT\",\"LIT / LITTLE ROCK\",\"72202\",97.50,\"G\"\r\n";
			contents += "\"Magness1" + incorrectChar + ", AR\",\"72553\",\"LIT\",\"LIT / LITTLE ROCK\",\"72202\",97.50,\"G\"";

			File.WriteAllText(fileName, contents, System.Text.Encoding.GetEncoding(1251));
		}

		void AssertZoneExists(BusinessObjectFactory factory, string cityTown, string state, string postcode, string iATA, string uNLOCO, string airport, string airportPostcode, decimal distance, string zone, bool isBeyond)
		{
			var query = new ZQuery(RefDomesticCartageZoneSchema.F1_CityTown, cityTown);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_RW_NKState, state);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_CityTownPostCode, postcode);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_PortCode, iATA);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_RL_NKLoco, uNLOCO);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_AirportCity, airport);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_AirportPostcode, airportPostcode);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_Distance, distance);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_DistanceUQ, "MI");
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_Zone, zone);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_DirectionToAirport, "");
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_DataSource, "ACI");
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_IsBeyond, isBeyond);

			var zones = factory.Load<RefDomesticCartageZone>(query);
			AssertEquals(1, zones.Length);
		}

		#endregion
	}
}
