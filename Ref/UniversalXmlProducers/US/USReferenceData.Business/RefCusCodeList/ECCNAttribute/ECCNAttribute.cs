using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.USReferenceData.Services;
using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class ECCNAttribute
	{
		[Name("Code")]
		public string Code { get; set; }
		[Name("AttributeName")]
		public string AttributeName { get; set; }
		[Name("AttributeValue")]
		public string AttributeValue { get; set; }
	}

	public static class ECCNAttributeList
	{
		public static List<ECCNAttribute> GetMEU(ICsvParser parser)
		{
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.Instance.MEUAttributeFileName);
			return parser.Parse<ECCNAttribute>(filePath);
		}

		public static List<ECCNAttribute> GetLicenseType(ICsvParser parser)
		{
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.Instance.LicenseTypeAttributeFileName);
			return parser.Parse<ECCNAttribute>(filePath);
		}
	}
}
