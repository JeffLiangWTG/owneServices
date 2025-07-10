using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml();
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml()
		{
			string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var outputFolder = ApplicationConfig.OutputFileFolderPath;

			string fileCode = "E21";
			IExDocsParser parser = new E21_ProductTypeParser();
			var srcFile = Utils.GetFileName(fileCode);
			var dstFile = Path.Combine(binPath, outputFolder, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			var result = parser.Parse(srcFile);
			parser.ExportToXml(result, dstFile, false);

			fileCode = "E01";
			parser = new E01_AqisPlaceParser();
			srcFile = Utils.GetFileName(fileCode);
			dstFile = Path.Combine(binPath, outputFolder, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			result = parser.Parse(srcFile);
			parser.ExportToXml(result, dstFile, false);

			fileCode = "E29";
			parser = new E29_AqisPlaceParser();
			srcFile = Utils.GetFileName(fileCode);
			dstFile = Path.Combine(binPath, outputFolder, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			result = parser.Parse(srcFile);
			parser.ExportToXml(result, dstFile, false);

			fileCode = "E07";
			parser = new E07_CutCodeParser();
			srcFile = Utils.GetFileName(fileCode);
			dstFile = Path.Combine(binPath, outputFolder, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			result = parser.Parse(srcFile);
			parser.ExportToXml(result, dstFile, false);

			fileCode = "E25";
			parser = new E25_SupplementaryCodeParser();
			srcFile = Utils.GetFileName(fileCode);
			dstFile = Path.Combine(binPath, outputFolder, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			result = parser.Parse(srcFile);
			parser.ExportToXml(result, dstFile, false);

			fileCode = "E38";
			parser = new E38_DominantProductParser();
			srcFile = Utils.GetFileName(fileCode);
			dstFile = Path.Combine(binPath, outputFolder, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			result = parser.Parse(srcFile);
			parser.ExportToXml(result, dstFile, false);

			fileCode = "E39";
			parser = new E39_ApprovedCertifierParser();
			srcFile = Utils.GetFileName(fileCode);
			dstFile = Path.Combine(binPath, outputFolder, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			result = parser.Parse(srcFile);
			parser.ExportToXml(result, dstFile, false);

			//generate sql scripts
			fileCode = "E21";
			parser = new E21_ProductTypeParser();
			var filePath = Utils.GetFileName(fileCode);
			result = parser.Parse(filePath);
			string sqlString = Utils.GenerateSqlStringForE21(result);
			File.WriteAllText($@"{outputFolder}\{fileCode}.txt", sqlString);

			File.WriteAllText($@"{outputFolder}\codetype.txt", @"
			INSERT INTO RefCusCodeType (ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly)
			SELECT ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly
			FROM
			(
				SELECT 'PRODD' AS ZZK_CodeType, 'EXDOCS Dairy Product Types' AS ZZK_Description, 1 AS ZZK_IsReadOnly
				UNION SELECT 'PRODE', 'EXDOCS Egg Product Types', 1
				UNION SELECT 'PRODF', 'EXDOCS Fish Product Types', 1
				UNION SELECT 'PRODG', 'EXDOCS Grain and Seed Product Types', 1
				UNION SELECT 'PRODH', 'EXDOCS Horticulture Product Types', 1
				UNION SELECT 'PRODI', 'EXDOCS Inedible Meat Product Types', 1
				UNION SELECT 'PRODM', 'EXDOCS Meat Product Types', 1
				UNION SELECT 'PRODS', 'EXDOCS Skins and Hides Product Types', 1
				UNION SELECT 'PRODW', 'EXDOCS Wool Product Types', 1
				UNION SELECT 'CUTCD', 'EXDOCS Dairy Cut Codes', 1
				UNION SELECT 'CUTCE', 'EXDOCS Egg Cut Codes', 1
				UNION SELECT 'CUTCF', 'EXDOCS Fish Cut Codes', 1
				UNION SELECT 'CUTCG', 'EXDOCS Grain and Seed Cut Codes', 1
				UNION SELECT 'CUTCH', 'EXDOCS Horticulture Cut Codes', 1
				UNION SELECT 'CUTCI', 'EXDOCS Inedible Meat Cut Codes', 1	
				UNION SELECT 'CUTCM', 'EXDOCS Meat Cut Codes', 1
				UNION SELECT 'CUTCS', 'EXDOCS Skins and Hides Cut Codes', 1
				UNION SELECT 'CUTCW', 'EXDOCS Wool Cut Codes', 1
				UNION SELECT 'DOMP', 'EXDOCS Dominant Product', 1
				UNION SELECT 'SUPP', 'EXDOCS Supplementary Code', 1
				UNION SELECT 'AQISP', 'EXDOCS Quarantine Place Code', 1 
				UNION SELECT 'ACERT', 'EXDOCS Approved Certifiers', 1
			) AS Data
			WHERE NOT EXISTS (SELECT NULL FROM RefCusCodeType WHERE Data.ZZK_CodeType = ZZK_CodeType AND Data.ZZK_Description = ZZK_Description AND Data.ZZK_IsReadOnly = ZZK_IsReadOnly)
			");

			fileCode = "E01";
			parser = new E01_AqisPlaceParser();
			filePath = Utils.GetFileName(fileCode);
			result = parser.Parse(filePath);

			sqlString = Utils.GenerateSqlStringForE01(result);
			File.WriteAllText($@"{outputFolder}\{fileCode}.txt", sqlString);

			fileCode = "E29";
			parser = new E29_AqisPlaceParser();
			filePath = Utils.GetFileName(fileCode);
			result = parser.Parse(filePath);
			sqlString = Utils.GenerateSqlStringForE29(result);
			File.WriteAllText($@"{outputFolder}\{fileCode}.txt", sqlString);

			fileCode = "E07";
			parser = new E07_CutCodeParser();
			filePath = Utils.GetFileName(fileCode);
			result = parser.Parse(filePath);
			sqlString = Utils.GenerateSqlStringForE07(result);
			File.WriteAllText($@"{outputFolder}\{fileCode}.txt", sqlString);

			fileCode = "E25";
			parser = new E25_SupplementaryCodeParser();
			filePath = Utils.GetFileName(fileCode);
			result = parser.Parse(filePath);
			sqlString = Utils.GenerateSqlStringForE25(result);
			File.WriteAllText($@"{outputFolder}\{fileCode}.txt", sqlString);

			fileCode = "E38";
			parser = new E38_DominantProductParser();
			filePath = Utils.GetFileName(fileCode);
			result = parser.Parse(filePath);
			sqlString = Utils.GenerateSqlStringForE38(result);
			File.WriteAllText($@"{outputFolder}\{fileCode}.txt", sqlString);

			fileCode = "E39";
			parser = new E39_ApprovedCertifierParser();
			filePath = Utils.GetFileName(fileCode);
			result = parser.Parse(filePath);
			sqlString = Utils.GenerateSqlStringForE39(result);
			File.WriteAllText($@"{outputFolder}\{fileCode}.txt", sqlString);
		}
	}
}
