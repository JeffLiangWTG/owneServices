using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Common
{
	public interface IConfigProvider
	{
		string ConfigFile { get; }
	}

	public class ConfigProvider : IConfigProvider
	{
		public ConfigProvider(string fileName)
		{
			configFileName = fileName;
		}
		private readonly string configFileName;

		public string ConfigFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
	}
}
