using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcmPoc.Tests.Scenario.Helpers
{
	abstract class FileSystemGenerator : IGenerator
	{
		public FileSystemGenerator(string recipient, int quantity)
		{
			Recipient = recipient;
			Quantity = quantity;
		}


		protected IEnumerable<string> FileNames { get; set; }
		protected IEnumerable<string> FileContent { get; set; }
		protected string Recipient { get; }
		protected int Quantity { get; }

		public abstract IGenerator PrepareMessage(string line, int lineCount);

		public async Task GenerateMessages()
		{
			await Task.WhenAll(FileNames.Select(f => File.WriteAllLinesAsync(f, FileContent, new UTF8Encoding(false))));
		}
	}
}
