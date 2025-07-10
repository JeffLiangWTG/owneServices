using System.Collections;
using System.Collections.Specialized;
using System.IO;
using CargoWise.Common;

namespace Enterprise.Customs.DataTransfer
{
	public class CsvInvoiceDataFileReader : FileDataReader
	{
		public CsvInvoiceDataFileReader(string fileName) : base(fileName)
		{
		}

		public override string[][] Records
		{
			get
			{
				if (fRecords == null)
				{
					ArrayList recordsArray = new ArrayList();
					foreach (string lineFromFile in FileLines)
					{
						OCsvLine csvLine = new OCsvLine(lineFromFile);
						AddRecordsForLine(recordsArray, csvLine);
					}

					fRecords = new string[recordsArray.Count][];
					for (int i = 0; i < recordsArray.Count; i++)
					{
						fRecords[i] = (string[])recordsArray[i];
					}
				}
				return fRecords;
			}
		}
		string[][] fRecords;

		protected virtual void AddRecordsForLine(ArrayList recordsArrayOfStringArrays, OCsvLine csvLine)
		{
			recordsArrayOfStringArrays.Add(csvLine.FieldValues);
		}

		protected StringCollection FileLines
		{
			get
			{
				if (fFileLines == null)
				{
					fFileLines = new StringCollection();
					using (StreamReader reader = new StreamReader(FileName))
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							fFileLines.Add(line);
						}
					}
				}
				return fFileLines;
			}
		}
		StringCollection fFileLines;
	}
}
