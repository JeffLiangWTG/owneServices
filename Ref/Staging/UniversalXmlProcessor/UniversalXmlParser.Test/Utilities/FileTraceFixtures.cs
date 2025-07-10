using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.UniversalXmlParser.Utilities;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Utilities
{
	[TestFixture]
	public class FileTraceFixtures : BaseUnitTestFixture
	{
		[Test]
		public async Task Remove()
		{
			var fileName = Path.Combine(FolderHelper.GetBinFolder(), Guid.NewGuid().ToString());
			var fileTrace = new FileTrace(fileName);
			Assert.IsNotNull(fileTrace);
			await fileTrace.SaveLastSuccessLineNumberAsync(1);
			Assert.AreEqual(1, await fileTrace.GetLastSuccessLineNumberAsync());
			fileTrace.Remove();
			Assert.AreEqual(0, await fileTrace.GetLastSuccessLineNumberAsync());
		}

		[Test]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "No security needs in this test")]
		public async Task TestUpdateLastSuccessLineNumber()
		{
			var fileName = Path.Combine(FolderHelper.GetBinFolder(), Guid.NewGuid().ToString());
			var fileTrace = new FileTrace(fileName);
			Assert.IsNotNull(fileTrace);
			var lineNumber = await fileTrace.GetLastSuccessLineNumberAsync();
			Assert.AreEqual(0, lineNumber);

			for (var i = 0; i < 10; i++)
			{
				var nextNumber = new Random().Next(1000, 9999);
				await fileTrace.SaveLastSuccessLineNumberAsync(nextNumber);

				lineNumber = await fileTrace.GetLastSuccessLineNumberAsync();
				Assert.AreEqual(nextNumber, lineNumber);
			}
		}
	}
}
