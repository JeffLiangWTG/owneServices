using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business.Data
{
	public class DataImporter : IDataTransfer
	{
		#region Constants

		public static class Constants
		{
			public static class PackingLineFields
			{
				public const int Driver = 0;
				public const int TruckRego = 1;
				public const int ReceivalTimeStamp = 2;
				public const int FirstRefNo = 3;
				public const int FirstRefType = 4;
				public const int SecondRefNo = 5;
				public const int SecondRefType = 6;
				public const int Location = 7;
				public const int PackageCount = 8;
				public const int Length = 9;
				public const int Width = 10;
				public const int Height = 11;
				public const int UM = 12;
				public const int Weight = 13;
				public const int UW = 14;
				public const int MinColumnNo = 15;
			}

			public class PackingLineReferenceTypes
			{
				readonly string[] referenceTypes =
					{
						"INT", // Interim Receipt
						"HBL", // House Bill
						"BKR", // Booking Reference
						"SHP", // Shipment Reference
						"JOB", // Job Reference
						"CON", // Consol Reference
						"OTH"  // Other Reference
					};

				protected string[] GetReferenceTypes()
				{
					return (string[])referenceTypes.Clone();
				}

				public static ZPropertyInfo GetReferencePropertyInfo(CFSShipment shipment, string refType)
				{
					int index = Array.IndexOf(new PackingLineReferenceTypes().GetReferenceTypes(), refType);
					if (index == -1 || shipment == null)
					{
						return null;
					}

					switch (index)
					{
						case 0:
							return shipment.JS_InterimReceiptInfo;
						case 1:
							return shipment.JS_HouseBillInfo;
						case 3:
							return shipment.JS_UniqueConsignRefInfo;
						case 5:
							return shipment.JS_ConsolReferenceInfo;
						default:
							return null;
					}
				}

				public static string ConvertToRefName(string columnName)
				{
					switch (columnName)
					{
						case JobShipmentSchema.Constants.JS_HouseBill:
							return (NoResString)"House Bill";
						case JobShipmentSchema.Constants.JS_InterimReceipt:
							return (NoResString)"Interim Receipt";
						case JobShipmentSchema.Constants.JS_ConsolReference:
							return (NoResString)"Client Reference";
						case JobShipmentSchema.Constants.JS_UniqueConsignRef:
							return (NoResString)"Shipment Reference";
						default:
							return "";
					}
				}
			}
		}

		#endregion

		public DataImporter(CFSShipment shipment)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment), "Shipment cannot be a null reference");
			}

			this.Shipment = shipment;
			this.Factory = shipment.Factory;
		}

		public event ProcessedEventHandler Processed;
		public event EventHandler ProcessCompleted;

		public ZBool ImportPackLinesFromFile(string fileName)
		{
			ZBool result = true;
			if (!File.Exists(fileName))
			{
				fErrorMessage = Res.GetString("05360a27-e5a3-42df-a2be-6b7d8c9407a2", "The File '{0}' does not exist", fileName);
				result = false;
			}
			else
			{
				try
				{
					Shipment.OuterPackLines.SuspendValidation();
					ArrayList fieldValues = ConvertRecordsIntoArrays(fileName);
					TotalRecordCount = fieldValues.Count;

					if (TotalRecordCount > 0)
					{
						string[] line;
						FailedCount = 0;
						ProcessedCount = 0;

						String errorMessage = "";
						ImportCancelled = false;

						for (int rowCounter = 1; rowCounter <= TotalRecordCount; rowCounter++)
						{
							if (ImportCancelled)
							{
								break;
							}

							line = (string[])fieldValues[rowCounter - 1];
							errorMessage = ProcessLineFromFile(line);
							if (errorMessage == null)
							{
								ProcessedCount++;
							}
							else
							{
								ProcessedCount++;
								FailedCount++;
							}

							string strLine = ConvertToCommaDelimitedString(line);
							string logEntry = (errorMessage == null) ? "" : Res.GetString("e8bf3db5-9055-4b24-a69d-9e6488faadca", "Row {0}: {1}. Content: {2}", rowCounter, errorMessage, strLine);
							OnProcessed(logEntry);
						}

						if (!ImportCancelled)
						{
							OnProcessCompleted();
						}
					}
					else
					{
						OnProcessCompleted();
					}
				}
				finally
				{
					Shipment.OuterPackLines.ResumeValidation();
				}
			}

			return result;
		}

		public void CancelImport()
		{
			ImportCancelled = true;
		}

		#region Properties

		string fErrorMessage;
		public string ErrorMessage
		{
			get
			{
				if (fErrorMessage == null)
				{
					fErrorMessage = "";
				}

				return fErrorMessage;
			}
		}

		#endregion

		#region Implementation

		protected int FailedCount;
		protected int ProcessedCount;
		protected int TotalRecordCount;
		protected int PercentageComplete
		{
			get { return ((int)(((float)ProcessedCount) / TotalRecordCount) * 100); }
		}

		protected void OnProcessCompleted()
		{
			if (ProcessCompleted != null)
			{
				ProcessCompleted(this, new EventArgs());
				ProcessedEventArgs e = new ProcessedEventArgs(PercentageComplete, ProcessedCount, FailedCount, Res.GetString("6c525dfd-8d48-4361-b798-6eece9f9b95f", "Number of Pack Lines imported = {0}", (ProcessedCount - FailedCount)));
				Processed(this, e);
			}
		}

		protected void OnProcessed(string logEntry)
		{
			if (Processed != null)
			{
				ProcessedEventArgs e = new ProcessedEventArgs(PercentageComplete, ProcessedCount, FailedCount, logEntry);
				Processed(this, e);
			}
		}

		protected ArrayList ConvertRecordsIntoArrays(ZString fileName)
		{
			ArrayList fieldValues;
			using (ExcelInterface excelDoc = new ExcelInterface())
			{
				try
				{
					excelDoc.LoadExcelFile(fileName);
					fieldValues = ConvertXLSFileIntoArrays(excelDoc);
				}
				catch (ExcelInterfaceException)
				{
					fieldValues = ConvertCSVFileIntoArrays(fileName);
				}
			}

			return fieldValues;
		}

		protected internal ArrayList ConvertXLSFileIntoArrays(ExcelInterface excelDoc)
		{
			ArrayList fieldValues = new ArrayList();

			for (int i = 0; i < excelDoc.WorkSheets.Count; i++)
			{
				excelDoc.ActiveWorksheet = i;
				int maxRow = excelDoc.WorkSheets[i].RowCount;
				int maxCol = excelDoc.WorkSheets[i].ColumnCount;

				for (int j = 0; j < maxRow; j++)
				{
					string[] columns = new string[maxCol];
					for (int k = 0; k < maxCol; k++)
					{
						columns[k] = excelDoc.WorkSheets[i][j, k].ToString().Trim(' ');
					}

					fieldValues.Add(columns);
				}
			}

			return fieldValues;
		}

		protected ArrayList ConvertCSVFileIntoArrays(string fileName)
		{
			ArrayList fieldValues = new ArrayList();
			using (StreamReader reader = new StreamReader(fileName))
			{
				string strLine;
				while ((strLine = reader.ReadLine()) != null)
				{
					// Try to parse csv lines with three different delimiters (~ or | or ,) ordered by preference.
					for (int i = 0; i < 3; i++)
					{
						char delimiter;
						if (i == 0)
						{
							delimiter = '~';
						}
						else if (i == 1)
						{
							delimiter = '|';
						}
						else
						{
							delimiter = ',';
						}

						var csvLine = new OCsvLine(strLine, delimiter);
						if (csvLine.FieldValues.Length >= Constants.PackingLineFields.MinColumnNo)
						{
							fieldValues.Add(csvLine.FieldValues);
							break;
						}
					}
				}
			}

			return fieldValues;
		}

		protected internal string ProcessLineFromFile(string[] line)
		{
			var errorMessage = ValidateLine(line);
			if (errorMessage == null)
			{
				CFSPackLine packLine = Shipment.OuterPackLines.AddNew();
				PopulatePackLine(packLine, line);
				return null;
			}
			else
			{
				return errorMessage;
			}
		}

		protected internal string ValidateLine(string[] line)
		{
			if (line.Length < Constants.PackingLineFields.MinColumnNo)
			{
				return Res.GetString("98f105f9-8b70-4366-9956-14f43b3d2190", "Record does not have all of the required fields");
			}

			ZString[] referenceTypes =
				{
					(ZString)line[Constants.PackingLineFields.FirstRefType],
					(ZString)line[Constants.PackingLineFields.SecondRefType]
				};

			if (referenceTypes[0].IsEmpty && referenceTypes[1].IsEmpty)
			{
				return Res.GetString("cacd6ab0-12ee-47d5-a516-18c4b22949ed", "No reference provided. Pack line has to be associated with a shipment");
			}

			if (referenceTypes[0] == referenceTypes[1])
			{
				return Res.GetString("abe3b9aa-2716-4248-a169-1de2b10526f2", "Pack line cannot have two references of the same type");
			}

			string errorMessage = null;
			for (int i = 0; i < 2; i++)
			{
				if (!referenceTypes[i].IsEmpty)
				{
					ZPropertyInfo propertyInfo = Constants.PackingLineReferenceTypes.GetReferencePropertyInfo(Shipment, referenceTypes[i]);
					if (propertyInfo == null)
					{
						errorMessage = Res.GetString("9a805b95-995d-4c41-889c-79252de3e220", "Reference type is invalid");
						break;
					}
					else if (!propertyInfo.Value.IsEmpty)
					{
						if ((ZString)propertyInfo.Value != line[((i == 0) ? Constants.PackingLineFields.FirstRefNo : Constants.PackingLineFields.SecondRefNo)])
						{
							errorMessage = Res.GetString("2def1dc1-7092-4b0d-abcd-1a4f92cdf1c6", "{0} does not match with the shipment record selected", Constants.PackingLineReferenceTypes.ConvertToRefName(propertyInfo.Name));
							break;
						}

						errorMessage = null;
					}
					else
					{
						errorMessage = Res.GetString("60b67d9f-55d4-4ea9-983d-7e0149c2094a", "No matching reference found");
					}
				}
			}

			return errorMessage;
		}

		protected internal void SetValue(ZPropertyInfo propertyInfo, string[] fieldValues, int fieldIndex)
		{
			if (fieldValues.Length > fieldIndex)
			{
				ZString fieldValue = fieldValues[fieldIndex];
				if (!fieldValue.IsEmpty)
				{
					if (propertyInfo.PropertyType == typeof(ZString))
					{
						propertyInfo.Value = fieldValue.Left(propertyInfo.MaxLength);
					}
					else if (propertyInfo.PropertyType == typeof(ZDecimal))
					{
						ZDecimal result = 0;
						if (ZDecimal.TryParse(fieldValue, out result))
						{
							propertyInfo.Value = result;
						}
					}
					else if (propertyInfo.PropertyType == typeof(ZDateTime))
					{
						if (fieldValue.Length == 12)
						{
							//CCYYMMDDHHMM DateTimeFormat for CSV files
							try
							{
								propertyInfo.Value = (ZDateTime)DateTime.ParseExact(fieldValue, "yyyyMMddHHmm", Thread.CurrentThread.CurrentCulture);
							}
							catch (FormatException) { }
						}
					}
					else if (propertyInfo.PropertyType == typeof(ZShort))
					{
						ZShort result = 0;
						if (ZShort.TryParse(fieldValue, out result))
						{
							propertyInfo.Value = result;
						}
					}
					else if (propertyInfo.PropertyType == typeof(ZInt))
					{
						ZInt result = 0;
						if (ZInt.TryParse(fieldValue, out result))
						{
							propertyInfo.Value = result;
						}
					}
				}
			}
		}

		protected string ConvertToCommaDelimitedString(string[] line)
		{
			System.Text.StringBuilder lineString = new System.Text.StringBuilder();
			if (line.Length > 0)
			{
				lineString.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", line[0]);
				for (int i = 1; i < line.Length; i++)
				{
					lineString.Append(", \"");
					lineString.Append(line[i]);
					lineString.Append('"');
				}
			}

			return lineString.ToString();
		}

		#region Populate Business Objects

		CommonPickupDeliveryConfirm GetLegWithDetails(ZString driver, ZDateTime deliveredDate, ZString vehicleReg)
		{
			//Is this only for Delivering to the CFS ??? or import outgoing(gatepassing) too?

			CommonPickupDeliveryConfirm result = null;
			CommonPickupDeliveryConfirmCollection pickupDeliveryConfirms = Shipment.IsImport() ? Shipment.DestinationCFSDepartures : Shipment.OriginCFSArrivals;

			foreach (CommonPickupDeliveryConfirm leg in pickupDeliveryConfirms)
			{
				if (!leg.IsInDatabase &&
					leg.EU_DriversName == driver &&
					vehicleReg == leg.EU_VehicleRegistration &&
					deliveredDate == leg.EU_PickupDeliveryTime
					)
				{
					result = leg;
					break;
				}
			}

			if (result == null)
			{
				result = pickupDeliveryConfirms.AddNew();
			}

			result.EU_DriversName = driver.Left(result.EU_DriversNameInfo.MaxLength);
			result.EU_VehicleRegistration = vehicleReg.Left(result.EU_VehicleRegistrationInfo.MaxLength);
			result.EU_PickupDeliveryTime = deliveredDate;

			return result;
		}

		protected virtual internal void PopulatePackLine(CFSPackLine packLine, string[] line)
		{
			ZString driver = ZString.Empty;
			ZString vehicleReg = ZString.Empty;
			ZDateTime deliveryDate = ZDateTime.Empty;

			if (Constants.PackingLineFields.Driver < line.Length)
			{
				driver = line[Constants.PackingLineFields.Driver];
			}

			if (Constants.PackingLineFields.TruckRego < line.Length)
			{
				vehicleReg = line[Constants.PackingLineFields.TruckRego];
			}

			if (Constants.PackingLineFields.ReceivalTimeStamp < line.Length)
			{
				try
				{
					deliveryDate = (ZDateTime)DateTime.ParseExact(line[Constants.PackingLineFields.ReceivalTimeStamp], "yyyyMMddHHmm", Thread.CurrentThread.CurrentCulture);
				}
				catch (FormatException) { }
			}

			packLine.JL_F3_NKPackType = "PKG";
			SetValue(packLine.JL_PackageCountInfo, line, Constants.PackingLineFields.PackageCount);
			SetValue(packLine.JL_ActualWeightInfo, line, Constants.PackingLineFields.Weight);
			SetValue(packLine.JL_WidthInfo, line, Constants.PackingLineFields.Width);
			SetValue(packLine.JL_LengthInfo, line, Constants.PackingLineFields.Length);
			SetValue(packLine.JL_HeightInfo, line, Constants.PackingLineFields.Height);
			SetValue(packLine.JL_UnitOfDimensionInfo, line, Constants.PackingLineFields.UM);
			SetValue(packLine.JL_ActualWeightUQInfo, line, Constants.PackingLineFields.UW);

			CommonPickupDeliveryConfirm leg = GetLegWithDetails(driver, deliveryDate, vehicleReg);
			CommonConfirmDivot divot = leg.GetDivot(packLine);
			divot.J8_PackagesDelivered = packLine.JL_PackageCount;
		}

		#endregion

		protected CFSShipment Shipment;
#if DEBUG
		public CFSShipment GetShipmentInternal() => Shipment;
#endif
		protected BusinessObjectFactory Factory;
		protected bool ImportCancelled;

		#endregion
	}
}
