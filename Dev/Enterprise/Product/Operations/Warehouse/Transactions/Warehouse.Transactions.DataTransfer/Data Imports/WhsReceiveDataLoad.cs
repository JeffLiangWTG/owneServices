using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsReceiveDataLoad : DataLoad
	{
		public void ImportReceiveData(string dataLocation)
		{
			ImportReceiveData(dataLocation, false);
		}

		public void ImportReceiveData(string dataLocation, bool isDeliveranceConversion)
		{
			this.IsDeliveranceConversion = isDeliveranceConversion;
			HasError = false;

			ImportData(dataLocation, (NoResString)"Inventory"); // Its an identifier

			if (!ErrorOccurredInThisRun)
			{
				SaveFactory();
			}
		}

		void SaveFactory()
		{
			Factory.SuspendValidation();
			try
			{
				DisplayLogMessage("");
				DisplayLogMessage(Res.GetString("14a05a81-25e3-4eca-b96b-7c0e6b3bed48", "Saving. Please wait..."));

				Factory.Save();

				DisplayLogMessage(Res.GetString("a8c61adc-ce8b-4fdc-99db-ee9e85fc441e", "Saving Complete."));
			}
			catch (ZSaveException ex)
			{
				DisplayErrorMessage(Res.GetString("A34E3A1F-F688-4932-8E1D-A1CF24345336", "An exception occurred while saving the record(s) in the database."));
				DisplayErrorMessage(string.IsNullOrEmpty(ex.FriendlyMessage) ? ex.GetInnermostException().Message : ex.FriendlyMessage);
				DisplayErrorMessage(Res.GetString("3709E2E4-4D78-4DB2-972C-DE750A368B9B", "Please correct the relevant data in the file and then close and open the form to try again."));
				DisplayLogMessage(Res.GetString("DE262ACC-853F-4449-8D37-75C251AE49B0", "Saving Failed."));
			}
			finally
			{
				Factory.ResumeValidation();
			}
		}

		#region DataFields Receive

		class ReceiveDataToLoad
		{
			public ZString CustomsEntryNumber;
			public ZString ClientCode;
			public ZString Warehouse;
			public ZString Reference;
			public ZString ReceiveCategory;
			public ZDateTime ArrivalDate;
			public ZString ProductCode;
			public ZDecimal Quantity;
			public ZString QuantityUnit;
			public ZDecimal Pallets;
			public ZString Location;
			public ZString Attribute1;
			public ZString Attribute2;
			public ZString Attribute3;
			public ZString SerialNumber;
			public ZDateTime ExpiryDate;
			public ZDateTime PackingDate;
			public ZShort CustomsEntryLineNumber;
			public ZDateTime CustomsEntryDate;
			public ZString CustomsAddInfo;
			public ZDecimal CustomsQuantity;
			public ZString CustomsQuantityUnit;
			public ZDecimal ValueForDuty;
			public Money TILV;
			public ZString CountryOfOrigin;
			public ZDecimal BondedWhsQuantity;
			public ZString BondedWhsQuantityUnit;
			public ZDecimal CustomsSecondQuantity;
			public ZString CustomsSecondUnitQty;
			public ZDecimal CustomsThirdQuantity;
			public ZString CustomsThirdUnitQty;
			public ZString Tariff;
			public ZString PrimaryPreference;
			public ZString ManufacturerCode;
			public ZString ZoneStatus;
			public ZBool IsFromOtherFTZWarehouse;
			public ZString OutwardType;

			// Temp
			public ZGuid ClientPK;
			public ZGuid WarehousePK;
			public ZGuid ProductPK;
			public ZGuid LocationPK;
			public ZGuid ExistingReceivePK;
			public ZGuid ManufacturerAddressPK;
			public WhsWarehouse WhsBizo;
		}

#if DEBUG
		public
#endif
		class ReceiveHeader
		{
			public const int ColumnCount = 37;
			public string[] Title = new string[ColumnCount]
				{
					"CustomsEntryNo",
					"ClientCode",
					(NoResString)"Warehouse",  // May be an identifier or GUID.
					(NoResString)"Reference",  // May be an identifier or GUID.
					"ArrivalDate",
					"ProductCode",
					(NoResString)"Quantity", // May be an identifier or GUID.
					"QuantityUQ",
					(NoResString)"Pallets", // May be an identifier or GUID.
					(NoResString)"Location", // May be an identifier or GUID.
					"Attribute1", // May be an identifier or GUID.
					"Attribute2", // May be an identifier or GUID.
					"Attribute3", // May be an identifier or GUID.
					"ExpiryDate",
					"PackingDate",
					"CustomsEntryLineNo",
					"CustomsEntryDate",
					"CustomsAddInfo",
					"CustomsQty",
					"CustomsUQ",
					"CtryOfOrigin",
					"ValueForDuty",
					"BondedWhsQty",
					"BondedWhsUQ",
					"TILV",
					"CustomsSecondQuantity",
					"CustomsSecondUnitQty",
					(NoResString)"Tariff", // May be an identifier or GUID.
					"PrimaryPreference",
					"CustomsThirdQuantity",
					"CustomsThirdUnitQty",
					"ManufacturerCode",
					"ZoneStatus",
					"IsFromOtherFTZWarehouse",
					"OutwardType",
					"SerialNumber", // May be an identifier or GUID.
					"ReceiveCategory"
				};
		}

		#endregion

#if DEBUG
		public
#endif
		WhsReceive CurrentReceive;
		ReceiveDataToLoad CurrentRecord;
		bool IsDeliveranceConversion;

#if DEBUG
		public
#endif
		bool ErrorOccurredInThisRun => HasError || RunCounters.RecsExcluded > 0;

		bool HeaderChanged
		{
			get
			{
				return (CurrentReceive == null ||
						CurrentRecord.WarehousePK != CurrentReceive.WD_WW_Whs ||
						CurrentRecord.ClientPK != CurrentReceive.WD_OH_Client ||
						CurrentRecord.Reference != CurrentReceive.WD_ExternalReference);
			}
		}

		bool DoesRecordHaveCustomsData
		{
			get { return !CurrentRecord.CustomsEntryNumber.IsEmpty; }
		}

		#region ImportFrom .csv file

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			CurrentRecord = GetReceiveDataFromLine(line);
			ProcessReceiveData();
			OnProgressChanged();
		}

		protected override void OutputFinalTotals(string dataType)
		{
			CheckIfCalculatedWeightAndVolumeAreValid();
			DisplayLogMessage("");

			if (ErrorOccurredInThisRun)
			{
				DisplayLogMessage(Res.GetString("d2a22c02-3710-466e-8752-242c25ed54c1", "Please fix the above errors and try again."));
				DisplayLogMessage(Res.GetString("0d252c2c-1378-44a6-9c61-e0236d4b441c", "None of the records were created in this run."));
				DisplayLogMessage(Res.GetString("e84565c8-d45f-452c-8a6c-1cb349724181", "T O T A L : Record created = 0") + " ");
			}
			else
			{
				DisplayLogMessage(Res.GetString("c981c0b3-b97f-4160-be92-c5548d095b22", "All records were successfully created in this run."));
				DisplayLogMessage(Res.GetString("abeedf0a-1ea2-4101-ac57-6aba38715df9", "T O T A L : Record created(s) = {0}", RunCounters.RecsCreated));
			}
		}

		void CheckIfCalculatedWeightAndVolumeAreValid()
		{
			if (!HasError)
			{
				var dockets = Factory.Load<WhsDocket>(new ZQuery() { FetchOnlyFromLocalCache = true });

				foreach (var docket in dockets)
				{
					if (docket != null && !docket.WD_TotalWeight.IsWithinSqlPrecisionAndScale(WhsDocketSchema.WD_TotalWeight.Precision, WhsDocketSchema.WD_TotalWeight.Scale))
					{
						DisplayErrorMessage(Res.GetString("d8e6fc55-c59a-485a-8004-a523abff078a", "Calculated Total Line Weight exceeded the valid length."));
					}

					if (docket != null && !docket.WD_TotalCubic.IsWithinSqlPrecisionAndScale(WhsDocketSchema.WD_TotalCubic.Precision, WhsDocketSchema.WD_TotalCubic.Scale))
					{
						DisplayErrorMessage(Res.GetString("68b5e22f-3d88-43ff-8345-dbe8ebcfa728", "Calculated Total Line Volume exceeded the valid length."));
					}
				}
			}
		}

		void ProcessReceiveData()
		{
			FindRequiredData();
			ValidateAndDisplayAnyErrors();

			if (!ErrorOccurredInThisRun)
			{
				SetValuesForMissingData();
				CalculateHeader();
				AddNewReceiveLine();
			}
		}

		#endregion

		#region ExtractData

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "grandfathered")]
		ReceiveDataToLoad GetReceiveDataFromLine(OCsvLine line)
		{
			var record = new ReceiveDataToLoad();
			if (line.FieldValues.Length >= 1)
			{
				try
				{
					DocketDataFormatter.FormatCSVData(line);
					record.CustomsEntryNumber = line.FieldValues[0].Trim();
					if (line.FieldValues.Length > 1)
					{
						record.ClientCode = line.FieldValues[1].Trim();
					}

					if (line.FieldValues.Length > 2)
					{
						record.Warehouse = line.FieldValues[2].Trim();
					}

					if (line.FieldValues.Length > 3)
					{
						record.Reference = line.FieldValues[3].Trim();
					}

					if (line.FieldValues.Length > 4 && !new ZString(line.FieldValues[4]).IsEmpty)
					{
						record.ArrivalDate = ParseDate(line.FieldValues[4], (NoResString)"Arrival Date"); // May be an identifier or GUID.
					}
					if (line.FieldValues.Length > 5)
					{
						record.ProductCode = line.FieldValues[5].Trim();
					}

					if (line.FieldValues.Length > 6)
					{
						record.Quantity = ParseDecimal(line.FieldValues[6]);
					}

					if (line.FieldValues.Length > 7)
					{
						record.QuantityUnit = line.FieldValues[7].Trim();
					}

					if (line.FieldValues.Length > 8)
					{
						record.Pallets = ParseDecimal(line.FieldValues[8]);
					}

					if (line.FieldValues.Length > 9)
					{
						record.Location = line.FieldValues[9].Trim();
					}

					if (line.FieldValues.Length > 10)
					{
						record.Attribute1 = line.FieldValues[10].Trim();
					}

					if (line.FieldValues.Length > 11)
					{
						record.Attribute2 = line.FieldValues[11].Trim();
					}

					if (line.FieldValues.Length > 12)
					{
						record.Attribute3 = line.FieldValues[12].Trim();
					}

					if (line.FieldValues.Length > 13 && !new ZString(line.FieldValues[13]).IsEmpty)
					{
						record.ExpiryDate = ParseDate(line.FieldValues[13], (NoResString)"Expiry Date"); // May be an identifier or GUID.
					}

					if (line.FieldValues.Length > 14 && !new ZString(line.FieldValues[14]).IsEmpty)
					{
						record.PackingDate = ParseDate(line.FieldValues[14], (NoResString)"Packing Date"); // May be an identifier or GUID.
					}
					if (line.FieldValues.Length > 15)
					{
						record.CustomsEntryLineNumber = ParseShort(line.FieldValues[15]);
					}

					if (line.FieldValues.Length > 16 && !new ZString(line.FieldValues[16]).IsEmpty)
					{
						record.CustomsEntryDate = ParseDate(line.FieldValues[16], (NoResString)"Customs Entry Date"); // May be an identifier or GUID.
					}
					if (line.FieldValues.Length > 17)
					{
						record.CustomsAddInfo = line.FieldValues[17].Trim();
					}

					if (line.FieldValues.Length > 18)
					{
						record.CustomsQuantity = ParseDecimal(line.FieldValues[18]);
					}

					if (line.FieldValues.Length > 19)
					{
						record.CustomsQuantityUnit = line.FieldValues[19].Trim();
					}

					if (line.FieldValues.Length > 20)
					{
						record.CountryOfOrigin = line.FieldValues[20].Trim();
					}

					if (line.FieldValues.Length > 21)
					{
						record.ValueForDuty = ParseDecimal(line.FieldValues[21]);
					}

					if (line.FieldValues.Length > 22)
					{
						record.BondedWhsQuantity = ParseDecimal(line.FieldValues[22]);
					}

					if (line.FieldValues.Length > 23)
					{
						record.BondedWhsQuantityUnit = line.FieldValues[23].Trim();
					}

					if (line.FieldValues.Length > 24)
					{
						record.TILV = new Money(ParseDecimal(line.FieldValues[24]), GlbCompany.CurrentCompany.LocalCurrency);
					}
					if (line.FieldValues.Length > 25)
					{
						record.CustomsSecondQuantity = ParseDecimal(line.FieldValues[25]);
					}

					if (line.FieldValues.Length > 26)
					{
						record.CustomsSecondUnitQty = line.FieldValues[26].Trim();
					}

					if (line.FieldValues.Length > 27)
					{
						record.Tariff = line.FieldValues[27].Trim();
					}

					if (line.FieldValues.Length > 28)
					{
						record.PrimaryPreference = line.FieldValues[28].Trim();
					}

					if (line.FieldValues.Length > 29)
					{
						record.CustomsThirdQuantity = ParseDecimal(line.FieldValues[29]);
					}

					if (line.FieldValues.Length > 30)
					{
						record.CustomsThirdUnitQty = line.FieldValues[30].Trim();
					}

					if (line.FieldValues.Length > 31)
					{
						record.ManufacturerCode = line.FieldValues[31].Trim();
					}

					if (line.FieldValues.Length > 32)
					{
						record.ZoneStatus = line.FieldValues[32].Trim();
					}

					if (line.FieldValues.Length > 33)
					{
						record.IsFromOtherFTZWarehouse = ParseBool(line.FieldValues[33].Trim());
					}

					if (line.FieldValues.Length > 34)
					{
						record.OutwardType = line.FieldValues[34].Trim();
					}

					if (line.FieldValues.Length > 35)
					{
						record.SerialNumber = line.FieldValues[35].Trim();
					}

					if (line.FieldValues.Length > 36)
					{
						record.ReceiveCategory = line.FieldValues[36].Trim();
					}

					return record;
				}
				catch (Exception e)
				{
					if (e.IsCriticalException())
					{
						throw;
					}

					ErrorReporter.ReportOnce("WhsReceiveDataLoad.GetReceiveDataFromLine", "Exception while extracting data from CsvLine.", e);
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Validation

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			var numFields = line.FieldValues.Length;
			var isValid = numFields == ReceiveHeader.ColumnCount;

			var header = new ReceiveHeader();
			for (var h = 0; h < numFields && isValid; h++)
			{
				isValid = string.Compare(line.FieldValues[h].Trim(), header.Title[h], StringComparison.OrdinalIgnoreCase) == 0;
			}

			return isValid;
		}

		public override string CSVTemplateHeading
		{
			get
			{
				ReceiveHeader header = new ReceiveHeader();
				return string.Join(",", header.Title);
			}
		}

		#endregion

		#region Implementation

		public WhsDocketDataFormatter DocketDataFormatter
		{
			get
			{
				if (docketDataFormatter == null)
				{
					docketDataFormatter = GetNewDocketDataFormatter();
				}
				return docketDataFormatter;
			}
		}

		protected virtual WhsDocketDataFormatter GetNewDocketDataFormatter()
		{
			return new WhsDocketDataFormatter();
		}

		WhsDocketDataFormatter docketDataFormatter;

		void SetValuesForMissingData()
		{
			var client = Factory.Load<OrgHeader>(CurrentRecord.ClientPK);
			var part = Factory.Load<OrgSupplierPart>(CurrentRecord.ProductPK);
			OrgPartRelation relation = null;
			if (part != null)
			{
				relation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, OrgPartRelation.RelationshipTypes.Owner);
			}

			if (CurrentRecord.QuantityUnit.IsEmpty)
			{
				CurrentRecord.QuantityUnit = part != null ? part.OP_StockKeepingUnit : ZString.Empty;
			}

			if (!CurrentRecord.Attribute1.IsEmpty)
			{
				if (client.MiscServ.OM_IMPartAttrib1Name.IsEmpty)
				{
					client.MiscServ.OM_IMPartAttrib1Name = Res.GetString("41fadb90-5d14-4f4d-bd1c-07b501de5fc0", "Attribute 1");
				}

				if (client.MiscServ.OM_IMPartAttrib1Type.IsEmpty)
				{
					client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
				}

				if (relation != null)
				{
					relation.OU_UsePartAttrib1 = true;
				}
			}

			if (!CurrentRecord.Attribute2.IsEmpty)
			{
				if (client.MiscServ.OM_IMPartAttrib2Name.IsEmpty)
				{
					client.MiscServ.OM_IMPartAttrib2Name = Res.GetString("c437f6c6-4bdf-440a-842c-eeb3c7420759", "Attribute 2");
				}

				if (client.MiscServ.OM_IMPartAttrib2Type.IsEmpty)
				{
					client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
				}

				if (relation != null)
				{
					relation.OU_UsePartAttrib2 = true;
				}
			}

			if (!CurrentRecord.Attribute3.IsEmpty)
			{
				if (client.MiscServ.OM_IMPartAttrib3Name.IsEmpty)
				{
					client.MiscServ.OM_IMPartAttrib3Name = Res.GetString("30a44ea4-4c7b-45a4-86f0-aa36ebef9b6a", "Attribute 3");
				}

				if (client.MiscServ.OM_IMPartAttrib3Type.IsEmpty)
				{
					client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;
				}

				if (relation != null)
				{
					relation.OU_UsePartAttrib3 = true;
				}
			}

			if (!CurrentRecord.SerialNumber.IsEmpty)
			{
				client.MiscServ.OM_IMUseSerialNumber = true;
				if (relation != null)
				{
					relation.OU_UseSerialNumber = true;
				}
			}

			if (!CurrentRecord.ExpiryDate.IsEmpty && !client.MiscServ.OM_IMUseExpiryDate)
			{
				client.MiscServ.OM_IMUseExpiryDate = true;
				if (relation != null)
				{
					relation.OU_UseExpiryDate = true;
				}
			}

			if (!CurrentRecord.PackingDate.IsEmpty && !client.MiscServ.OM_IMUsePackingDate)
			{
				client.MiscServ.OM_IMUsePackingDate = true;
				if (relation != null)
				{
					relation.OU_UsePackingDate = true;
				}
			}

			SetValuesForMissingCustomsData();
		}

		void SetValuesForMissingCustomsData()
		{
			if (DoesRecordHaveCustomsData)
			{
				if (CurrentRecord.CustomsEntryDate.IsEmpty)
				{
					CurrentRecord.CustomsEntryDate = CurrentRecord.ArrivalDate.ToZDateTime();
				}

				if (CurrentRecord.CustomsQuantity <= 0)
				{
					CurrentRecord.CustomsQuantity = CurrentRecord.Quantity;
				}

				if (CurrentRecord.CustomsQuantityUnit.IsEmpty)
				{
					CurrentRecord.CustomsQuantityUnit = CurrentRecord.QuantityUnit;
				}

				if (CurrentRecord.BondedWhsQuantityUnit.IsEmpty && CurrentRecord.BondedWhsQuantity > 0m)
				{
					CurrentRecord.BondedWhsQuantityUnit = CurrentRecord.QuantityUnit;
				}
			}
		}

		void DisplayErrorMessage(string message)
		{
			DisplayLogMessage(message);
			HasError = true;
		}

		bool HasError;

		#region ValidateAndDisplayAnyErrors

		void ValidateAndDisplayAnyErrors()
		{
			var currentRowNumber = RunCounters.CurrentRow.ToString(CultureInfo.CurrentCulture);
			if (CurrentRecord == null)
			{
				DisplayErrorMessage(Res.GetString("33b2be38-7e39-404d-8b32-3b698173212a", "Row {0} Error.... data is inconsistent with required format.", currentRowNumber));
			}
			else
			{
				DisplayErrorsForDates(currentRowNumber);
				DisplayErrorsForDuplicates(currentRowNumber);
				DisplayErrorsForPks(currentRowNumber);
				DisplayErrorsForProduct(currentRowNumber);

				if (!CurrentRecord.Location.IsEmpty && !CurrentRecord.LocationPK.IsValid)
				{
					DisplayErrorMessage(Res.GetString("e753292a-01a8-4b4d-bdf0-89cadf54b6e4", "Row {0} Error.... location ({1}) in warehouse ({2}), could not be found.", currentRowNumber, CurrentRecord.Location, CurrentRecord.Warehouse));
				}

				if (CurrentRecord.Quantity <= 0m)
				{
					DisplayErrorMessage(Res.GetString("3416fd38-d9d2-40aa-9608-04aef58a87a1", "Row {0} quantity ({1}) is not a valid amount.", currentRowNumber, CurrentRecord.Quantity.ToString()));
				}

				DisplayErrorsForUnits(currentRowNumber);
				DisplayErrorsForAttributes(currentRowNumber);
				DisplayErrorsForSerialNumber(currentRowNumber);
				DisplayErrorsForReference(currentRowNumber);
				DisplayErrorsForCustomsData(currentRowNumber);
				DisplayErrorsForReceiveCategory(currentRowNumber);
			}

			if (HasError)
			{
				RunCounters.RecsExcluded++;
			}
		}

		void DisplayErrorsForDates(string currentRowNumber)
		{
			if (!CurrentRecord.ArrivalDate.IsEmpty && !CurrentRecord.ArrivalDate.IsValidSmallDateTime)
			{
				DisplayErrorMessage(Res.GetString("48C6126A-3876-4560-A008-1AC8DD28AE10", "Row {0} Error.... arrival date ({1}) is invalid.", currentRowNumber, CurrentRecord.ArrivalDate));
			}

			if (!CurrentRecord.CustomsEntryDate.IsEmpty && !CurrentRecord.CustomsEntryDate.IsValidSmallDateTime)
			{
				DisplayErrorMessage(Res.GetString("67EBCFB1-5A16-49B3-86E7-5865365EEDF3", "Row {0} Error.... customs entry date ({1}) is invalid.", currentRowNumber, CurrentRecord.CustomsEntryDate));
			}

			if (!CurrentRecord.ExpiryDate.IsEmpty && !CurrentRecord.ExpiryDate.IsValidSmallDateTime)
			{
				DisplayErrorMessage(Res.GetString("0681DB0F-4E05-4A1A-BD85-4DD4CE3033AF", "Row {0} Error.... expiry date ({1}) is invalid.", currentRowNumber, CurrentRecord.ExpiryDate));
			}

			if (!CurrentRecord.PackingDate.IsEmpty && !CurrentRecord.PackingDate.IsValidSmallDateTime)
			{
				DisplayErrorMessage(Res.GetString("99FA5B44-5058-48EC-BA1A-E43755DF63C1", "Row {0} Error.... packing date ({1}) is invalid.", currentRowNumber, CurrentRecord.PackingDate));
			}
		}

		void DisplayErrorsForDuplicates(string currentRowNumber)
		{
			if (!CurrentRecord.ClientPK.IsValid)
			{
				var clientCode = CurrentRecord.ClientCode;
				if (duplicateDeliveranceCodes)
				{
					DisplayErrorMessage(Res.GetString("7f6f2319-1a35-44f4-9632-55ff4ce282c7", "Row {0} Error.... more than one organization was found with the client deliverance code ({1}). Please remove or change the duplicate code.", currentRowNumber, clientCode));
				}
				if (duplicateLegacyCodes)
				{
					DisplayErrorMessage(Res.GetString("95c65bdb-a958-4486-8b2e-bdefda5dc5ed", "Row {0} Error.... more than one organization was found with the legacy client code ({1}). Please remove or change the duplicate code.", currentRowNumber, clientCode));
				}
				if (duplicateBusinessRegCodes)
				{
					DisplayErrorMessage(Res.GetString("ada53a6b-13d5-43c6-be0e-8aea8965ad5f", "Row {0} Error.... more than one organization was found with the ABN code ({1}). Please remove or change the duplicate code.", currentRowNumber, clientCode));
				}
				if (duplicateClientCodes)
				{
					DisplayErrorMessage(Res.GetString("662c760c-8ead-45d1-bf06-61e7c4378a3e", "Row {0} Error.... more than one organization was found with the customs client code ({1}). Please remove or change the duplicate code.", currentRowNumber, clientCode));
				}
				if (!duplicateDeliveranceCodes && !duplicateLegacyCodes && !duplicateBusinessRegCodes && !duplicateClientCodes)
				{
					DisplayErrorMessage(Res.GetString("eded84bc-8ff6-4e46-8980-cd9e69b85bac", "Row {0} Error.... client code ({1}) could not be found.", currentRowNumber, clientCode));
				}
			}
		}

		void DisplayErrorsForPks(string currentRowNumber)
		{
			if (!CurrentRecord.WarehousePK.IsValid)
			{
				DisplayErrorMessage(Res.GetString("b475bbc7-52c4-4cb4-af20-b2becb738726", "Row {0} Error.... warehouse ({1}) could not be found.", RunCounters.CurrentRow.ToString(CultureInfo.CurrentCulture), CurrentRecord.Warehouse));
			}

			if (CurrentRecord.ExistingReceivePK.IsValid)
			{
				WhsReceive receive = Factory.Load<WhsReceive>(CurrentRecord.ExistingReceivePK);
				if (receive.IsFinalised)
				{
					DisplayErrorMessage(Res.GetString("7466c9a8-694a-44a3-bb61-879539b39368", "Row {0} Error.... an Receive for client ({1}), warehouse ({2}), reference ({3}) already exists.", currentRowNumber, CurrentRecord.ClientCode, CurrentRecord.Warehouse, CurrentRecord.Reference));
				}
			}
		}

		void DisplayErrorsForProduct(string currentRowNumber)
		{
			var createMissingWhsProduct = SystemDataRegistry.Instance.CreateMissingWarehouseProduct.Value;
			if (createMissingWhsProduct)
			{
				if (CurrentRecord.ProductCode.IsEmpty)
				{
					DisplayProductCodeCannotBeFound(currentRowNumber);
				}
				else if (CurrentRecord.ProductCode.Length > OrgSupplierPartSchema.OP_PartNum.MaxLength)
				{
					DisplayErrorMessage(Res.GetString("0F5B31D4-A3BC-4A18-A182-0021A5075445", "Row {0} Error.... The maximum length for Product Code ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.ProductCode, OrgSupplierPartSchema.OP_PartNum.MaxLength));
				}
			}
			else
			{
				if (!CurrentRecord.ProductPK.IsValid)
				{
					DisplayProductCodeCannotBeFound(currentRowNumber);
				}
			}
		}

		void DisplayProductCodeCannotBeFound(string currentRowNumber)
		{
			DisplayErrorMessage(Res.GetString("7e639eb5-0f7e-42bd-9393-3bfa216b0e71", "Row {0} Error.... product code ({1}) could not be found.", currentRowNumber, CurrentRecord.ProductCode));
		}

		void DisplayErrorsForUnits(string currentRowNumber)
		{
			if (!CurrentRecord.QuantityUnit.IsEmpty)
			{
				if (CurrentRecord.QuantityUnit.Length > WhsDocketLineSchema.WE_F3_NKPackType.MaxLength)
				{
					DisplayErrorMessage(
						Res.GetString("2AA18AF1-C254-4AD9-B7D5-E5530FCB9F5A", "Row {0} Error.... The maximum length for Quantity Unit ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.QuantityUnit, WhsDocketLineSchema.WE_F3_NKPackType.MaxLength.ToString(CultureInfo.CurrentCulture)));
				}
				else if (!LookupsHelper.PackTypesWithStandardUnits(Factory).ContainsCode(CurrentRecord.QuantityUnit))
				{
					DisplayErrorMessage(Res.GetString("537FD954-54DF-427C-B2A4-A50F29FE7618", "Row {0} Error.... Quantity Unit ({1}) is not a valid Unit.", currentRowNumber, CurrentRecord.QuantityUnit));
				}
			}

			if (!CurrentRecord.CustomsQuantityUnit.IsEmpty && CurrentRecord.CustomsQuantityUnit.Length > WhsBondedWarehouseAttributeSchema.WB_CustomsUnitOfQty.MaxLength)
			{
				DisplayErrorMessage(
					Res.GetString("53E70A24-9ECC-4E9D-9A2A-0B6B5D7E9B0A", "Row {0} Error.... The maximum length for Customs Unit Quantity ({1}) has been exceeded. The maximum length of this property is {2} characters.",
					currentRowNumber, CurrentRecord.CustomsQuantityUnit, WhsBondedWarehouseAttributeSchema.WB_CustomsUnitOfQty.MaxLength.ToString(CultureInfo.CurrentCulture)));
			}

			if (!CurrentRecord.CustomsSecondUnitQty.IsEmpty && CurrentRecord.CustomsSecondUnitQty.Length > WhsBondedWarehouseAttributeSchema.WB_CustomsSecondUnitQty.MaxLength)
			{
				DisplayErrorMessage(
					Res.GetString("4F3F905E-DBCC-4547-976B-D6BD2D8B539A", "Row {0} Error.... The maximum length for Customs Second Unit Quantity ({1}) has been exceeded. The maximum length of this property is {2} characters.",
					currentRowNumber, CurrentRecord.CustomsSecondUnitQty, WhsBondedWarehouseAttributeSchema.WB_CustomsSecondUnitQty.MaxLength.ToString(CultureInfo.CurrentCulture)));
			}

			if (!CurrentRecord.BondedWhsQuantityUnit.IsEmpty && CurrentRecord.BondedWhsQuantityUnit.Length > WhsBondedWarehouseAttributeSchema.WB_BondedWhsUnitOfQty.MaxLength)
			{
				DisplayErrorMessage(
					Res.GetString("9B35EF5B-BC5C-40FF-9F1F-EF9ED187319D", "Row {0} Error.... The maximum length for Bonded Warehouse Unit Quantity ({1}) has been exceeded. The maximum length of this property is {2} characters.",
					currentRowNumber, CurrentRecord.BondedWhsQuantityUnit, WhsBondedWarehouseAttributeSchema.WB_BondedWhsUnitOfQty.MaxLength));
			}
		}

		void DisplayErrorsForAttributes(string currentRowNumber)
		{
			if (!CurrentRecord.Attribute1.IsEmpty && CurrentRecord.Attribute1.Length > WhsDocketLineSchema.WE_PartAttrib1.MaxLength)
			{
				DisplayErrorMessage(Res.GetString("CCF385A7-2B0B-47DA-853A-9B4DA6B1A52F", "Row {0} Error.... The maximum length for Attribute 1 ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.Attribute1, WhsDocketLineSchema.WE_PartAttrib1.MaxLength));
			}

			if (!CurrentRecord.Attribute2.IsEmpty && CurrentRecord.Attribute2.Length > WhsDocketLineSchema.WE_PartAttrib2.MaxLength)
			{
				DisplayErrorMessage(Res.GetString("0C15745D-F69E-46F1-81A2-C579839FA3DA", "Row {0} Error.... The maximum length for Attribute 2 ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.Attribute2, WhsDocketLineSchema.WE_PartAttrib2.MaxLength));
			}

			if (!CurrentRecord.Attribute3.IsEmpty && CurrentRecord.Attribute3.Length > WhsDocketLineSchema.WE_PartAttrib3.MaxLength)
			{
				DisplayErrorMessage(Res.GetString("0B73DFB2-E853-4024-A50B-2EF626AC0F4A", "Row {0} Error.... The maximum length for Attribute 3 ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.Attribute3, WhsDocketLineSchema.WE_PartAttrib3.MaxLength));
			}
		}

		void DisplayErrorsForSerialNumber(string currentRowNumber)
		{
			if (!CurrentRecord.SerialNumber.IsEmpty && CurrentRecord.SerialNumber.Length > WhsDocketLineSchema.WE_SerialNumber.MaxLength)
			{
				DisplayErrorMessage(Res.GetString("128944ce-90a6-4d66-a3c7-4d719c86bec7", "Row {0} Error.... The maximum length for Serial Number ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.SerialNumber, WhsDocketLineSchema.WE_SerialNumber.MaxLength));
			}
		}

		void DisplayErrorsForReference(string currentRowNumber)
		{
			if (!CurrentRecord.Reference.IsEmpty && CurrentRecord.Reference.Length > WhsDocketSchema.WD_ExternalReference.MaxLength)
			{
				DisplayErrorMessage(Res.GetString("7E8E3E3A-9A3E-4AE2-935F-52208F833A0B", "Row {0} Error.... The maximum length for Reference ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.Reference, WhsDocketSchema.WD_ExternalReference.MaxLength));
			}
		}

		void DisplayErrorsForReceiveCategory(string currentRowNumber)
		{
			if (!CurrentRecord.ReceiveCategory.IsEmpty && CurrentRecord.ReceiveCategory.Length > WhsDocketSchema.WD_ReceiveCategory.MaxLength)
			{
				DisplayErrorMessage(Res.GetString("f42629e0-c574-4512-998d-248bc3a277e9", "Row {0} Error.... The maximum length for Receive Category ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, CurrentRecord.ReceiveCategory, WhsDocketSchema.WD_ReceiveCategory.MaxLength));
			}
		}

		void DisplayErrorsForCustomsData(string currentRowNumber)
		{
			if (DoesRecordHaveCustomsData)
			{
				var customsEntryKey = WhsBondedWarehouseAttribute.BuildKey(CurrentRecord.CustomsEntryNumber, CurrentRecord.CustomsEntryLineNumber);
				if (customsEntryKey.Length > WhsDocketLineSchema.WE_BondedEntryKey.MaxLength)
				{
					DisplayErrorMessage(Res.GetString("70E74EC5-0121-49C2-9007-60AF0742D28D", "Row {0} Error.... The maximum length for Customs Entry Key ({1}) has been exceeded. The maximum length of this property is {2} characters.",
						currentRowNumber, customsEntryKey, WhsDocketSchema.WD_ExternalReference.MaxLength));
				}

				if (CurrentRecord.CustomsEntryLineNumber <= 0)
				{
					DisplayErrorMessage(Res.GetString("d295eee3-819f-4bc7-997f-4d6758e89532", "Row {0} Customs Entry Line Number ({1}) is not a valid for a bonded inventory.", currentRowNumber, CurrentRecord.CustomsEntryLineNumber.ToString()));
				}
			}
		}

		#endregion

		void CalculateHeader()
		{
			if (HeaderChanged)
			{
				if (CurrentRecord.ExistingReceivePK.IsValid)
				{
					CurrentReceive = Factory.Load<WhsReceive>(CurrentRecord.ExistingReceivePK);
				}
				else
				{
					AddNewReceive();
				}
			}
		}

		void AddNewReceive()
		{
			CurrentReceive = Factory.New<WhsReceive>();
			using (CurrentReceive.GetValidationSuspender())
			{
				CurrentReceive.WD_WW_Whs = CurrentRecord.WarehousePK;
				CurrentReceive.WD_OH_Client = CurrentRecord.ClientPK;
				CurrentReceive.WD_DocketType = DocketType.Codes.Receive;
				CurrentReceive.WD_ReceiveCategory = CurrentRecord.ReceiveCategory;

				if (DoesRecordHaveCustomsData)
				{
					CurrentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
				}

				CurrentReceive.WD_ExternalReference = CurrentRecord.Reference;
				var arrivalDate = CurrentRecord.WhsBizo.GetWarehouseBranchLocalDateTimeOffset(CurrentRecord.ArrivalDate);
				CurrentReceive.WD_ArrivalDate = arrivalDate;

				if (CurrentRecord.ArrivalDate.IsEmpty)
				{
					CurrentReceive.WD_BookingDate = CurrentRecord.WhsBizo.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
				}
				else
				{
					CurrentReceive.WD_BookingDate = arrivalDate;
				}

				if (CurrentRecord.Reference.IsEmpty)
				{
					CurrentReceive.WD_ExternalReference = "INWARDS";
					CurrentReceive.IsUniqueExternalReferenceCreatedOnSave = true;
				}
			}
		}

		void AddNewReceiveLine()
		{
			CurrentReceive.Lines.Factory.SuspendValidation();
			try
			{
				var line = CurrentReceive.Lines.AddNew();
				try
				{
					using (line.GetValidationSuspender())
					{
						if (CurrentRecord.ProductPK == ZGuid.Empty)
						{
							line.ProductCode = CurrentRecord.ProductCode;
							line.ProductDesc = CurrentRecord.ProductCode;
						}
						else
						{
							line.WE_OP = CurrentRecord.ProductPK;
						}

						line.WE_F3_NKPackType = CurrentRecord.QuantityUnit;
						line.WE_ClientOrderedUnits = CurrentReceive.AsnLines.Count > 0 ? 0 : CurrentRecord.Quantity;
						line.WE_PackQuantity = CurrentRecord.Quantity;

						if (!CurrentRecord.LocationPK.IsEmpty)
						{
							line.WE_WL = CurrentRecord.LocationPK;
						}

						line.WE_ExpiryDate = CurrentRecord.ExpiryDate.Date;
						line.WE_PackingDate = CurrentRecord.PackingDate.Date;
						line.WE_PartAttrib1 = CurrentRecord.Attribute1;
						line.WE_PartAttrib2 = CurrentRecord.Attribute2;
						line.WE_PartAttrib3 = CurrentRecord.Attribute3;
						line.WE_SerialNumber = CurrentRecord.SerialNumber;

						line.WE_LineNo = (short)(CurrentReceive.Lines.Count + 1);

						line.Docket.WD_DocketType = DocketType.Codes.Receive;
						line.WE_WE_OriginalDocketLineForRating = line.PK;

						if (DoesRecordHaveCustomsData)
						{
							line.WE_BondedEntryKey = WhsBondedWarehouseAttribute.BuildKey(CurrentRecord.CustomsEntryNumber, CurrentRecord.CustomsEntryLineNumber);
							AddCustomsData(line);
						}

						var docket = line.Docket;
						if (docket.WD_ArrivalDate.IsEmpty && line.Location != null)
						{
							docket.WD_ArrivalDate = docket.Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
						}
					}
				}
				finally
				{
					RunCounters.RecsCreated++;
				}
			}
			finally
			{
				CurrentReceive.IsImportingData = true;
				CurrentReceive.Lines.Factory.ResumeValidation();
				CurrentReceive.CreateProductFiles();
			}
		}

		void AddCustomsData(WhsReceiveLine line)
		{
			//new WhsBondedWarehouseAttribute
			WhsBondedWarehouseAttribute bondData = Factory.New<WhsBondedWarehouseAttribute>();
			using (bondData.GetValidationSuspender())
			{
				bondData.WB_ParentID = line.PK;
				bondData.WB_ParentTableCode = WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode;
				bondData.WB_EntryKey = CurrentRecord.CustomsEntryNumber;
				bondData.WB_EntryLineNo = CurrentRecord.CustomsEntryLineNumber;
				bondData.WB_EntryDate = CurrentRecord.CustomsEntryDate;
				bondData.WB_RN_NKCountryOfOrigin = CurrentRecord.CountryOfOrigin;
				bondData.WB_AddInfo = CurrentRecord.CustomsAddInfo;
				bondData.WB_CustomsQty = CurrentRecord.CustomsQuantity;
				bondData.WB_CustomsUnitOfQty = CurrentRecord.CustomsQuantityUnit;
				bondData.WB_BondedWhsQty = CurrentRecord.BondedWhsQuantity;
				bondData.WB_BondedWhsUnitOfQty = CurrentRecord.BondedWhsQuantityUnit;
				bondData.WB_ValueForDuty = CurrentRecord.ValueForDuty;
				bondData.WB_TILV = CurrentRecord.TILV.Amount;
				bondData.WB_RX_NKTILVCurrency = (CurrentRecord.TILV.Currency == null ? "" : CurrentRecord.TILV.Currency.Code);
				bondData.WB_CustomsSecondQuantity = CurrentRecord.CustomsSecondQuantity;
				bondData.WB_CustomsSecondUnitQty = CurrentRecord.CustomsSecondUnitQty;
				bondData.WB_Tariff = CurrentRecord.Tariff;
				bondData.WB_PrimaryPreference = CurrentRecord.PrimaryPreference;
				bondData.WB_CustomsThirdQuantity = CurrentRecord.CustomsThirdQuantity;
				bondData.WB_CustomsThirdUnitQty = CurrentRecord.CustomsThirdUnitQty;
				bondData.WB_OA_ManufacturerAddress = CurrentRecord.ManufacturerAddressPK;
				bondData.WB_ZoneStatus = CurrentRecord.ZoneStatus;
				bondData.WB_IsFromAnotherFTZWhs = CurrentRecord.IsFromOtherFTZWarehouse;
				bondData.WB_OutwardType = CurrentRecord.OutwardType;
			}
		}

		void FindRequiredData()
		{
			if (CurrentRecord != null)
			{
				if (!CurrentRecord.ClientCode.IsEmpty)
				{
					FindClientPKFromCode();
				}

				if (!CurrentRecord.Warehouse.IsEmpty)
				{
					FindWarehousePKFromCode();
				}

				if (!CurrentRecord.ProductCode.IsEmpty)
				{
					SetProductPKFromCurrentRecordProductCode();
				}

				if (!CurrentRecord.Location.IsEmpty)
				{
					FindLocationPK();
				}

				if (!CurrentRecord.ManufacturerCode.IsEmpty)
				{
					FindManufacturerAddressPKFromCode();
				}

				FindExistingReceive();
			}
		}

		void FindManufacturerAddressPKFromCode()
		{
			var manufacturer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, CurrentRecord.ManufacturerCode));
			if (manufacturer != null)
			{
				CurrentRecord.ManufacturerAddressPK = manufacturer.MainAddress.PK;
			}
		}

		void FindExistingReceive()
		{
			ZQuery filter = new ZQuery(WhsDocketSchema.WD_WW_Whs, CurrentRecord.WarehousePK);
			filter.AddToFilter(WhsDocketSchema.WD_OH_Client, CurrentRecord.ClientPK);
			filter.AddToFilter(WhsDocketSchema.WD_ExternalReference, CurrentRecord.Reference.IsEmpty ? new ZString("INWARDS") : CurrentRecord.Reference);
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, Transactions.CodeLists.DocketType.Codes.Receive);
			WhsReceive receive = Factory.LoadTop1<WhsReceive>(filter);
			CurrentRecord.ExistingReceivePK = receive != null ? receive.PK : ZGuid.Empty;
		}

		OrgCusCode[] FindClientByExternalCode(string externalCodeType, string externalCode)
		{
			ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, externalCodeType);
			filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, externalCode);
			return (OrgCusCode[])Factory.Load(typeof(OrgCusCode), filter);
		}

		void FindClientPKFromCode()
		{
			duplicateDeliveranceCodes = false;
			duplicateLegacyCodes = false;
			duplicateBusinessRegCodes = false;
			duplicateClientCodes = false;
			ZGuid clientPK = ZGuid.Empty;
			OrgCusCode[] matchingClients;

			if (IsDeliveranceConversion)
			{
				matchingClients = FindClientByExternalCode(OrgCusCode.CodeTypes.DeliveranceCode, CurrentRecord.ClientCode);
				if (matchingClients.Length == 1)
				{
					clientPK = matchingClients[0].OK_OH;
				}
				duplicateDeliveranceCodes = matchingClients.Length > 1;
			}
			else
			{
				OrgHeader client = OrgHeader.LoadFromCode(Factory, CurrentRecord.ClientCode);
				if (client != null)
				{
					clientPK = client.PK;
				}
				else
				{
					matchingClients = FindClientByExternalCode(OrgCusCode.CodeTypes.LegacySystemCode, CurrentRecord.ClientCode);
					if (matchingClients.Length == 1)
					{
						clientPK = matchingClients[0].OK_OH;
					}
					else
					{
						duplicateLegacyCodes = matchingClients.Length > 1;

						matchingClients = FindClientByExternalCode(OrgCusCode.CodeTypes.GSTCode, CurrentRecord.ClientCode);
						if (matchingClients.Length == 1)
						{
							clientPK = matchingClients[0].OK_OH;
						}
						else
						{
							duplicateBusinessRegCodes = matchingClients.Length > 1;

							matchingClients = FindClientByExternalCode(OrgCusCode.CodeTypes.CustomsClientCode, CurrentRecord.ClientCode);
							if (matchingClients.Length == 1)
							{
								clientPK = matchingClients[0].OK_OH;
							}
							else
							{
								duplicateClientCodes = matchingClients.Length > 1;
							}
						}
					}
				}
			}

			CurrentRecord.ClientPK = clientPK;
		}

		bool duplicateDeliveranceCodes;
		bool duplicateLegacyCodes;
		bool duplicateBusinessRegCodes;
		bool duplicateClientCodes;

		void SetProductPKFromCurrentRecordProductCode()
		{
			OrgSupplierPart part = new OrgSupplierPart.Loader(Factory).Load(CurrentRecord.ProductCode, Factory.Load<OrgHeader>(CurrentRecord.ClientPK), null);
			CurrentRecord.ProductPK = part != null ? part.PK : ZGuid.Empty;
		}

		void FindLocationPK()
		{
			CurrentRecord.LocationPK = ZGuid.Empty;
			WhsWarehouse warehouse = Factory.Load<WhsWarehouse>(CurrentRecord.WarehousePK);
			if (warehouse != null)
			{
				WhsLocation location = warehouse.FindLocation(CurrentRecord.Location);
				if (location != null)
				{
					CurrentRecord.LocationPK = location.PK;
				}
			}
		}

		void FindWarehousePKFromCode()
		{
			WhsWarehouse warehouse;

			if (DoesRecordHaveCustomsData)
			{
				warehouse = FindBondedWarehouse(CurrentRecord.Warehouse);
			}
			else
			{
				warehouse = FindNormalWarehouse(CurrentRecord.Warehouse);
			}

			if (warehouse != null)
			{
				CurrentRecord.WhsBizo = warehouse;
			}

			CurrentRecord.WarehousePK = warehouse != null ? warehouse.PK : ZGuid.Empty;
		}

		WhsWarehouse FindBondedWarehouse(ZString warehouseName)
		{
			WhsWarehouse result = null;
			var warehouseAddress = new OrgAddress.Loader(Factory).GetWarehouseAddressBasedOnCusCode(warehouseName);

			if (warehouseAddress != null)
			{
				var warehouseFilter = GetBondedWarehouseQuery().AddToFilter(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddress.PK);

				foreach (var warehouse in Factory.Load<WhsWarehouse>(warehouseFilter))
				{
					if (warehouse.WW_GB_RelatedCompanyBranch.IsEmpty)
					{
						result = warehouse;
						break;
					}
					else if (result == null)
					{
						result = warehouse;
					}
				}
			}

			if (result == null)
			{
				var filter = GetBondedWarehouseQuery().AddToFilter(WhsWarehouseSchema.WW_WarehouseName, warehouseName);
				result = Factory.LoadTop1<WhsWarehouse>(filter);
			}

			return result;
		}

		static ZDBOnlyQuery GetBondedWarehouseQuery()
		{
			var bondedAreaQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsAreaSchema.WA_WW_Whs);
			bondedAreaQuery.AddToFilter(WhsAreaSchema.WA_AreaType, AreaTypes.Codes.Bonded);

			var exciseAreaQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsAreaSchema.WA_WW_Whs);
			exciseAreaQuery.AddToFilter(WhsAreaSchema.WA_AreaType, AreaTypes.Codes.Excise);

			var bondedWhsQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
			bondedWhsQuery.AddSubQuery(bondedAreaQuery, JoinCondition.Or);
			bondedWhsQuery.AddSubQuery(exciseAreaQuery, JoinCondition.Or);
			return bondedWhsQuery;
		}

		WhsWarehouse FindNormalWarehouse(ZString warehouseName)
		{
			var filter = new ZQuery(WhsWarehouseSchema.WW_WarehouseName, warehouseName);
			return Factory.LoadTop1<WhsWarehouse>(filter);
		}

		ZDateTime ParseDate(string date, string dateFieldName)
		{
			var d = ZDateTime.Empty;
			ZDateTime.TryParseExact(date, out d, DateFormat);
			if (!d.IsValid)
			{
				DisplayErrorMessage(Res.GetString("5e4ec92a-b2bf-42bb-902b-c971536ff29e", "Row {0} Error....Wrong format of {1}. Date format should be '{2}'. Please check the documentation.", RunCounters.CurrentRow.ToString(CultureInfo.CurrentCulture), dateFieldName, "yyyymmdd"));
			}
			else if (!d.IsValidSmallDateTime)
			{
				DisplayErrorMessage(Res.GetString("e3494b9d-fba8-443b-bf85-09321d75987b", "Row {0} Error....{1} is out of date range. Date should be between '{2}' and '{3}'.", RunCounters.CurrentRow.ToString(CultureInfo.CurrentCulture), dateFieldName,
					ZDateTime.MinSmallDateTimeValue.ToString(DateFormat, Culture.Current), ZDateTime.MaxSmallDateTimeValue.ToString(DateFormat, Culture.Current))); // Date format is explicitly specified in technical guide, and value will be saved into DB
			}
			return d;
		}

		static string DateFormat
		{
			get { return "yyyyMMdd"; }
		}

		ZDecimal ParseDecimal(string @decimal)
		{
			if (!ZDecimal.TryParse(@decimal, out ZDecimal zdecimal))
			{
				zdecimal = 0m;
			}

			return zdecimal;
		}

		ZShort ParseShort(string @short)
		{
			if (!ZShort.TryParse(@short, out ZShort zshort))
			{
				zshort = 0;
			}

			return zshort;
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "CargoWise.Types.ZBool.TryParse(System.String,CargoWise.Types.ZBool@)")]
		ZBool ParseBool(string sbool)
		{
			ZBool b = false;
			ZBool.TryParse(sbool, out b);
			return b;
		}

		#endregion
	}
}
