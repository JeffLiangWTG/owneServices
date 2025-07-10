using System.Collections;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.Customs.DataTransfer
{
	public class XlsInvoiceDataFileReader : FileDataReader
	{
		public XlsInvoiceDataFileReader(string fileName) : base(fileName)
		{
		}

		public override string[][] Records
		{
			get
			{
				if (fRecords == null)
				{
					ArrayList recordsArray = new ArrayList();
					using (ExcelInterface excelDoc = new ExcelInterface())
					{
						excelDoc.LoadExcelFile(FileName);

						for (int workSheetIndex = 0; workSheetIndex < excelDoc.WorkSheets.Count; workSheetIndex++)
						{
							excelDoc.ActiveWorksheet = workSheetIndex;
							int maxRow = excelDoc.WorkSheets[0].RowCount;
							int maxCol = excelDoc.WorkSheets[0].ColumnCount;

							for (int rowIndex = 0; rowIndex < maxRow; rowIndex++)
							{
								string[] columns = new string[maxCol];
								for (int columnIndex = 0; columnIndex < maxCol; columnIndex++)
								{
									columns[columnIndex] = excelDoc.WorkSheets[workSheetIndex][rowIndex, columnIndex].ToString().Trim(' ');
								}
								recordsArray.Add(columns);
							}
						}

						fRecords = new string[recordsArray.Count][];
						for (int i = 0; i < recordsArray.Count; i++)
						{
							fRecords[i] = (string[])recordsArray[i];
						}
					}
				}

				return fRecords;
			}
		}
		string[][] fRecords;
	}
}
