using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ProducerStatusCodeListGenerator
{
	public class ProducerStatusCodeListGenerator
	{
		const string JsonFileName = "producerStatusCodeList.json";
		readonly string _saveTo;

		public ProducerStatusCodeListGenerator(string saveTo)
		{
			_saveTo = Path.Combine(saveTo, JsonFileName);
		}

		public void Run()
		{
			var fs = new FileStream(_saveTo, FileMode.Create, FileAccess.ReadWrite);
			fs.Close();

			var codeList = Enum.GetValues(typeof(ProducerStatus)).Cast<ProducerStatus>()
				.Select(x => new StatusFlag(x.ToString(), (int)x))
				.ToList();
			codeList.RemoveAt(0);

			var content = JsonConvert.SerializeObject(codeList);

			if (File.Exists(_saveTo))
			{
				File.WriteAllText(_saveTo, content);
			}
			else
			{
				Console.WriteLine("File create failed.");
			}
		}
	}

	public class StatusFlag
	{
		public string flagName { get; set; }
		public int flagValue { get; set; }
		public StatusFlag(string name, int value)
		{
			flagName = name;
			flagValue = value;
		}
	}
}
