using System;
using System.IO;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DataLoadWithFlexibleColumnsTest : TestCase
	{
		public void TestDataImportedRegardlessOfHeaderOrder()
		{
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Header3, Header2, Header1,, NewHeader4, NewHeader5");
					sw.WriteLine("3,2,1,,string,20210305");
					sw.Flush();
				}
				DataLoad dataLoad = new MessedUpDataLoad();
				dataLoad.ImportData(testFileName.Filename, "TestingType");  // Asserted behaviour is in ProcessDataForThisLine
				Assert(true);
			}
		}

		sealed class MessedUpDataLoad : DataLoadWithFlexibleColumns
		{
			public override string CSVTemplateHeading
			{
				get { return "HEADER1,HEADER2,HEADER3,NewHeader4,NewHeader5"; }
			}

			protected override void ProcessDataForThisLine(OCsvLine line)
			{
				AssertEquals("1", TryGetStringValue(line, "HEaDeR1"));
				AssertEquals("2", TryGetStringValue(line, "HEADER2"));
				AssertEquals("3", TryGetStringValue(line, "HEAdER3"));
				AssertEquals("", TryGetStringValue(line, "HEADER4"));  // this header is not in the import file, so defaults to blank
				AssertEquals("string", TryGetStringValue(line, "NewHeader4"));
#if NETFRAMEWORK
				AssertExceptionThrown<ArgumentOutOfRangeException>("Exceed max length", "Specified argument was out of the range of valid values.\r\nParameter name: Value of NewHeader4 exceeds the max length(3): string", () => { TryGetStringValue(line, "NewHeader4", 3); });
#else
				AssertExceptionThrown<ArgumentOutOfRangeException>("Exceed max length", "Specified argument was out of the range of valid values. (Parameter 'Value of NewHeader4 exceeds the max length(3): string')", () => { TryGetStringValue(line, "NewHeader4", 3); });
#endif
				AssertEquals(null, TryGetDateValue(line, "NewHeader6"));
			}
		}
	}
}
