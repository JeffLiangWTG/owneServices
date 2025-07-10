using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer
{
	public class FlatFileInvoiceDataImporter : DeclarationDataImporter
	{
		public FlatFileInvoiceDataImporter(string fileName)
		{
			this.FileName = fileName;
		}

		public FlatFileInvoiceDataImporter(string fileName, BaseJobDeclaration toJobDec)
			: base(toJobDec)
		{
			this.FileName = fileName;
		}
		protected readonly string FileName;

		protected FileDataReader dataReader;
		protected BaseJobComInvoiceHeader mostRecentlyAddedInvHead;
		protected ZString currentDefaultOrderNo;

		/// <summary>
		/// Override passing the concrete type of product that will be loaded
		/// </summary>
		/// <returns></returns>
		protected virtual Type GetOrgSupplierPartType()
		{
			return null;
		}

		#region Import

		List<IDisposable> listChangedSuspenders;

		public override void Import()
		{
			listChangedSuspenders = new List<IDisposable>();
			if (toJobDec != null)
			{
				toJobDec.Factory.SuspendValidation();

				listChangedSuspenders.Add(toJobDec.FilteredInvoiceLines.SuspendListChanged());
			}
			try
			{
				bool isSuccessful = false;

				base.Import();
				SetDataReader();

				mostRecentlyAddedInvHead = null;
				if (IsDataValid)
				{
					BeforeImport();
					chargesToPost = new ArrayList();

					foreach (string[] record in dataReader.Records)
					{
						ProcessRecord(record);
						if (CancelImport)
						{
							FireLogEvent(Res.GetString("b7b54b29-e51d-4641-8d52-55ea49165dc5", "Import Canceled"));
							break;
						}
					}
					PostCharges();

					//If Invoice Amount has not been calculated, then default to the sum of the invoice line totals
					foreach (BaseJobComInvoiceHeader invHead in InvoiceHeaders)
					{
						invHead.BalancePrice();
					}

					if (!CancelImport)
					{
						FireLogEvent("--------------------\n" + Res.GetString("c8cdfaeb-b868-4d79-b8bf-81b31f9d9f9a", "Import Successful"));
						isSuccessful = true;
					}
				}
				else
				{
					FireLogEvent(Res.GetString("9c66f996-11ca-49b3-b28d-01e6f5d148d0", "File is not valid for import."));
				}

				AfterImport(isSuccessful);
			}
			finally
			{
				foreach (IDisposable disposable in listChangedSuspenders)
				{
					disposable.Dispose();
				}
				if (toJobDec != null)
				{
					toJobDec.Factory.ResumeValidation();
				}
			}
		}
		#endregion

		#region SetDataReader
		protected virtual void SetDataReader()
		{
			if (Path.GetExtension(FileName).ToUpper() == ".XLS")
			{
				dataReader = new XlsInvoiceDataFileReader(FileName);
			}
			else
			{
				dataReader = new CsvInvoiceDataFileReader(FileName);
			}
		}
		#endregion

		#region Virtual BeforeImport and AfterImport methods
		protected virtual void BeforeImport()
		{
			Type productType = GetOrgSupplierPartType();
			if (productType != null)
			{
				var partNumbers = new Dictionary<string, bool>();
				foreach (string[] record in dataReader.Records)
				{
					if (IsInvoiceLine(record) && record.Length > Constants.InvoiceLineFields.ProductCode)
					{
						// Leave as fetch hint
						// Do not change to Factory.Load
						// Barfs with stack overflow
						ZString productCode = record[Constants.InvoiceLineFields.ProductCode];
						factory.AddFetchHint(productType, OrgSupplierPartSchema.OP_PartNum, productCode);
					}
				}
			}
		}

		protected virtual void AfterImport(bool isImportSuccessful)
		{
		}
		#endregion

		#region IsDataValid
		protected virtual bool IsDataValid
		{
			get
			{
				var result = true;
				var headerFound = false;
				int headerCount = 0;
				var errorOnMultipleInvoiceHeaders = !SupportMultipleInvoiceHeaders;
				foreach (string[] record in dataReader.Records)
				{
					if (IsInvoiceHeader(record))
					{
						if (errorOnMultipleInvoiceHeaders && ++headerCount > 1)
						{
							result = false;
							FireLogEvent(Res.GetString("{9DF17E27-B466-43AF-8D48-70910E882F7F}", "Multiple invoice header lines found. Only one invoice header line is allowed."));
							break;
						}
						headerFound = true;
					}
					else if (IsInvoiceCharge(record))
					{
						if (!headerFound)
						{
							result = false;
							FireLogEvent(Res.GetString("494a1d07-c09e-4fc5-9420-e4c9cd62ae2c", "An invoice charge line found without invoice header line. Invoice charge line has to be preceded by an invoice header line or another invoice charge line."));
							break;
						}
					}
					else if (IsInvoiceLine(record))
					{
						headerFound = false;
					}
				}

				return result;
			}
		}

		protected virtual bool SupportMultipleInvoiceHeaders => true;
		#endregion

		#region TotalRecordCount
		public override int TotalRecordCount
		{
			get { return dataReader.Records.Length; }
		}
		#endregion

		#region FailedRecordCount
		public override int FailedRecordCount
		{
			get { return fFailedRecordCount; }
		}
		int fFailedRecordCount;
		#endregion

		#region ProcessedRecordCount
		public override int ProcessedRecordCount
		{
			get { return fProcessedRecordCount; }
		}
		int fProcessedRecordCount;
		#endregion

		#region ProcessRecord
		protected void ProcessRecord(string[] record)
		{
			if (IsInvoiceHeader(record))
			{
				mostRecentlyAddedInvHead = AddNewInvoice();
				listChangedSuspenders.Add(mostRecentlyAddedInvHead.JobComInvoiceLines.SuspendListChanged());
				PopulateInvoiceHeader(mostRecentlyAddedInvHead, record);
			}
			else if (IsInvoiceCharge(record))
			{
				if (mostRecentlyAddedInvHead != null)
				{
					AddChargeToPost(mostRecentlyAddedInvHead, record);
				}
			}
			else if (IsInvoiceLine(record))
			{
				BaseJobComInvoiceLine invLine;
				if (mostRecentlyAddedInvHead != null)
				{
					invLine = mostRecentlyAddedInvHead.JobComInvoiceLines.AddNew();
				}
				else
				{
					var lineInvoiceNumber = record.Length > Constants.InvoiceLineFields.InvoiceNo ? record[Constants.InvoiceLineFields.InvoiceNo] : "UNKNOWN";
					var invoiceHeaders = FindInvoicesFromInvoiceNumber(lineInvoiceNumber);
					BaseJobComInvoiceHeader invHead = null;
					if (invoiceHeaders.Length > 0)
					{
						invHead = invoiceHeaders[0];
					}
					else
					{
						invHead = AddNewInvoice();
						invHead.JZ_InvoiceNumber = lineInvoiceNumber.Substring(0, Math.Min(lineInvoiceNumber.Length, BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumberMaxLength));
					}
					invLine = invHead.JobComInvoiceLines.AddNew();
				}
				PopulateInvoiceLine(invLine, record);
			}
			else if (record.Length > 0)
			{
				fFailedRecordCount++;
				FireLogEvent(Res.GetString("3d6b3fa4-dbe7-430e-aa13-cccdd209d40b", "Invalid invoice record type. Row excluded. Content: {0}", RecordAsString(record)));
			}
			fProcessedRecordCount++;
			FireLogEvent("");
		}
		#endregion

		#region Charge Recording and Posting
		ArrayList chargesToPost;

		void AddChargeToPost(BaseJobComInvoiceHeader mostRecentlyAddedInvHead, string[] record)
		{
			chargesToPost.Add(new ChargeToPost(mostRecentlyAddedInvHead, record));
		}

		struct ChargeToPost
		{
			public ChargeToPost(BaseJobComInvoiceHeader mostRecentlyAddedInvHead, string[] record)
			{
				this.MostRecentlyAddedInvHead = mostRecentlyAddedInvHead;
				this.Record = record;
			}
			public readonly BaseJobComInvoiceHeader MostRecentlyAddedInvHead;
			public string[] Record;
		}

		void PostCharges()
		{
			foreach (ChargeToPost charge in chargesToPost)
			{
				BaseInvoiceCharge invCharge = charge.MostRecentlyAddedInvHead.Charges.AddNew();
				invCharge.J7_ParentID = charge.MostRecentlyAddedInvHead.PK;
				PopulateInvoiceCharges(invCharge, charge.Record);
			}
		}
		#endregion

		#region AddNewInvoice

		protected virtual BaseJobComInvoiceHeader AddNewInvoice()
		{
			return toJobDec.Invoices.AddNew();
		}

		protected virtual BaseJobComInvoiceLine AddNewInvoiceLine()
		{
			return toJobDec.InvoiceLines.AddNew();
		}

		#endregion

		#region GenerateInvoiceNumberIfNoneSpecified

		protected void GenerateInvoiceNumberIfNoneSpecified(string[] fieldValues)
		{
			var invoiceNumberString = new ZString(fieldValues[1]);
			if (invoiceNumberString.IsEmpty)
			{
				//search for last invoice with EDI prefix
				foreach (BaseJobComInvoiceHeader header in InvoiceHeaders)
				{
					if (header.JZ_InvoiceNumber.StartsWith("EDIIMPORTED"))
					{
						invoiceNumberString = header.JZ_InvoiceNumber.Replace("EDIIMPORTED", "");
					}
				}

				if (!invoiceNumberString.IsEmpty && ZInt.TryParse(invoiceNumberString, out var invoiceNumber))
				{
					invoiceNumber++;
					invoiceNumberString = "EDIIMPORTED" + invoiceNumber.ToString();
				}
				else
				{
					invoiceNumberString = "EDIIMPORTED1";
				}

				fieldValues[1] = invoiceNumberString;
			}
		}
		#endregion

		protected string FieldStringValue(int field, string[] fieldValues)
		{
			return fieldValues.Length > field ? fieldValues[field] : "";
		}

		#region PopulateInvoiceHeader
		protected virtual void PopulateInvoiceHeader(BaseJobComInvoiceHeader invHead, string[] fieldValues)
		{
			GenerateInvoiceNumberIfNoneSpecified(fieldValues);

			if (!invHead.IsAttachedToPersistentDeclaration)
			{
				SetValue(invHead.JZ_MessageTypeInfo, fieldValues, Constants.InvoiceHeaderFields.ShipmentType);
			}

			SetValue(invHead.JZ_InvoiceNumberInfo, fieldValues, Constants.InvoiceHeaderFields.InvoiceNo);
			SetValue(invHead.JZ_InvoiceDateInfo, fieldValues, Constants.InvoiceHeaderFields.InvoiceDate);
			SetValue(invHead.JZ_InvoiceAmountInfo, fieldValues, Constants.InvoiceHeaderFields.InvoiceAmount);

			if (fieldValues.Length > Constants.InvoiceHeaderFields.SupplierCode)
			{
				ZGuid orgPK;
				if (FindOrganisation((ZString)fieldValues[Constants.InvoiceHeaderFields.SupplierCode], out orgPK))
				{
					invHead.JZ_OH_Supplier = orgPK;
				}
				else
				{
					UnknownOrganisationCodeEventArgs args = new UnknownOrganisationCodeEventArgs();
					args.Name = FieldStringValue(Constants.InvoiceHeaderFields.SupplierName, fieldValues);
					args.RegistrationNo = FieldStringValue(Constants.InvoiceHeaderFields.SupplierRegNumber, fieldValues);
					args.Street = FieldStringValue(Constants.InvoiceHeaderFields.SupplierStreet, fieldValues);
					args.Street2 = FieldStringValue(Constants.InvoiceHeaderFields.SupplierStreet2, fieldValues);
					args.PostCode = FieldStringValue(Constants.InvoiceHeaderFields.SupplierPostCode, fieldValues);
					args.City = FieldStringValue(Constants.InvoiceHeaderFields.SupplierCity, fieldValues);
					args.Country = FieldStringValue(Constants.InvoiceHeaderFields.SupplierCountry, fieldValues);
					args.Code = FieldStringValue(Constants.InvoiceHeaderFields.SupplierCode, fieldValues);
					FireUnknownOrganisationCodeFound(args);

					if (!args.Code.IsEmpty)
					{
						supplierCodeMap.Add(fieldValues[Constants.InvoiceHeaderFields.SupplierCode], args.Code);
						if (FindOrganisation(args.Code, out orgPK))
						{
							invHead.JZ_OH_Supplier = orgPK;
						}
					}
				}
			}

			if (fieldValues.Length > Constants.InvoiceHeaderFields.BuyerCode)
			{
				ZGuid orgPK;
				if (FindOrganisation((ZString)fieldValues[Constants.InvoiceHeaderFields.BuyerCode], out orgPK))
				{
					invHead.JZ_OH_Buyer = orgPK;
				}
			}

			if (fieldValues.Length > Constants.InvoiceHeaderFields.OrderNo)
			{
				currentDefaultOrderNo = fieldValues[Constants.InvoiceHeaderFields.OrderNo];
			}

			if (fieldValues.Length > Constants.InvoiceHeaderFields.InvoiceCurrency)
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, (ZString)fieldValues[Constants.InvoiceHeaderFields.InvoiceCurrency]);
				if (currency != null)
				{
					invHead.JZ_RX_NKInvoice_Currency = currency.RX_Code;
				}
			}
			SetValue(invHead.JZ_IncoTermInfo, fieldValues, Constants.InvoiceHeaderFields.IncoTerm);
		}
		#endregion

		#region PopulateInvoiceCharges
		protected virtual void PopulateInvoiceCharges(BaseInvoiceCharge invCharge, string[] fieldValues)
		{
			SetValue(invCharge.J7_AmountInfo, fieldValues, Constants.InvoiceChargeFields.Amount);
			SetValue(invCharge.J7_ChargeTypeInfo, fieldValues, Constants.InvoiceChargeFields.ChargeType);
			SetValue(invCharge.J7_RX_NKCurrencyInfo, fieldValues, Constants.InvoiceChargeFields.Currency);
			SetValue(invCharge.J7_IsDutiableInfo, fieldValues, Constants.InvoiceChargeFields.IsDutiable);
			SetValue(invCharge.J7_IsGSTApplicableInfo, fieldValues, Constants.InvoiceChargeFields.IsGSTApplicable);
			SetValue(invCharge.J7_IsIncludedInITOTInfo, fieldValues, Constants.InvoiceChargeFields.IsIncludedInITOT);
			SetValue(invCharge.J7_PrepaidCollectInfo, fieldValues, Constants.InvoiceChargeFields.PrepaidCollect);
		}

		protected virtual internal void PopulateInvoiceLine(BaseJobComInvoiceLine invLine, string[] fieldValues)
		{
			invLine.Charges.SuspendValidation();
			if (invLine.JI_OrderNumber.IsEmpty && fieldValues.Length > Constants.InvoiceLineFields.OrderNo)
			{
				ZString orderNo = (ZString)fieldValues[Constants.InvoiceLineFields.OrderNo];
				if (orderNo.IsEmpty)
				{
					orderNo = currentDefaultOrderNo;
				}

				invLine.JI_OrderNumber = orderNo.Left(invLine.JI_OrderNumberInfo.MaxLength);
			}

			SetValue(invLine.JI_OrderNumberInfo, fieldValues, Constants.InvoiceLineFields.OrderNo);
			SetValue(invLine.JI_PartNoInfo, fieldValues, Constants.InvoiceLineFields.ProductCode);
			SetValue(invLine.JI_PartAttrib1Info, fieldValues, Constants.InvoiceLineFields.ProductAttrib1);
			SetValue(invLine.JI_PartAttrib2Info, fieldValues, Constants.InvoiceLineFields.ProductAttrib2);
			SetValue(invLine.JI_PartAttrib3Info, fieldValues, Constants.InvoiceLineFields.ProductAttrib3);
			SetValue(invLine.JI_DescriptionInfo, fieldValues, Constants.InvoiceLineFields.ProductDescription);
			SetValue(invLine.JI_InvoiceQuantityInfo, fieldValues, Constants.InvoiceLineFields.Quantity);
			SetValue(invLine.JI_InvoiceUQInfo, fieldValues, Constants.InvoiceLineFields.UnitofQty);
			SetValue(invLine.JI_VolumeInfo, fieldValues, Constants.InvoiceLineFields.Volume);
			SetValue(invLine.JI_VolumeUQInfo, fieldValues, Constants.InvoiceLineFields.UnitofVol);
			SetValue(invLine.JI_WeightInfo, fieldValues, Constants.InvoiceLineFields.Weight);
			SetValue(invLine.JI_WeightUQInfo, fieldValues, Constants.InvoiceLineFields.UnitOfWgt);
			SetValue(invLine.JI_LinePriceInfo, fieldValues, Constants.InvoiceLineFields.LinePrice);
			if (invLine.Part == null)
			{
				if (fieldValues.Length > Constants.InvoiceLineFields.TariffLookup)
				{
					var classification = BaseCusClassification.LoadFromLookupCode(factory, (ZString)fieldValues[Constants.InvoiceLineFields.TariffLookup], GetClassificationTypeMatching(invLine), GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					if (classification != null)
					{
						invLine.JI_CC = classification.PK;
					}
				}

				if (fieldValues.Length > Constants.InvoiceLineFields.TariffCode)
				{
					SetValue(invLine.JI_TariffInfo, fieldValues, Constants.InvoiceLineFields.TariffCode);
				}
			}

			SetValue(invLine.JI_CustomsUnitQtyInfo, fieldValues, Constants.InvoiceLineFields.CustomsUnit);
			SetValue(invLine.JI_CustomsQuantityInfo, fieldValues, Constants.InvoiceLineFields.CustomsQty);

			if (invLine.JI_CustomsQuantity == 0 && invLine.JI_CustomsUnitQty == invLine.JI_InvoiceUQ)
			{
				invLine.JI_CustomsQuantity = invLine.JI_InvoiceQuantity;
			}

			SetValue(invLine.CountryOfOriginFieldInfo, fieldValues, Constants.InvoiceLineFields.Origin);
			SetValue(invLine.JI_ConcessionOrderInfo, fieldValues, Constants.InvoiceLineFields.Concession);
			SetValue(invLine.JI_CustomAttrib1Info, fieldValues, Constants.InvoiceLineFields.CustomText1);
			SetValue(invLine.JI_CustomAttrib2Info, fieldValues, Constants.InvoiceLineFields.CustomText2);
			SetValue(invLine.JI_CustomAttrib3Info, fieldValues, Constants.InvoiceLineFields.CustomText3);
			SetValue(invLine.JI_CustomDate1Info, fieldValues, Constants.InvoiceLineFields.CustomDate1);
			SetValue(invLine.JI_CustomDate2Info, fieldValues, Constants.InvoiceLineFields.CustomDate2);
			SetValue(invLine.JI_CustomDate3Info, fieldValues, Constants.InvoiceLineFields.CustomDate3);
			SetValue(invLine.JI_CustomFlag1Info, fieldValues, Constants.InvoiceLineFields.CustomFlag1);
			SetValue(invLine.JI_CustomFlag2Info, fieldValues, Constants.InvoiceLineFields.CustomFlag2);
			SetValue(invLine.JI_CustomFlag3Info, fieldValues, Constants.InvoiceLineFields.CustomFlag3);
			SetValue(invLine.JI_CustomDecimal1Info, fieldValues, Constants.InvoiceLineFields.CustomDecimal1);
			SetValue(invLine.JI_CustomDecimal2Info, fieldValues, Constants.InvoiceLineFields.CustomDecimal2);
			SetValue(invLine.JI_CustomDecimal3Info, fieldValues, Constants.InvoiceLineFields.CustomDecimal3);
		}
		#endregion

		#region Record Type Checkers
		protected bool IsInvoiceHeader(string[] record)
		{
			return record.Length > 0 && record[0].Equals(Constants.InvoiceRecordType.Head, StringComparison.OrdinalIgnoreCase);
		}

		protected bool IsInvoiceCharge(string[] record)
		{
			return record.Length > 0 && record[0].Equals(Constants.InvoiceRecordType.Charge, StringComparison.OrdinalIgnoreCase);
		}

		protected bool IsInvoiceLine(string[] record)
		{
			return record.Length > 0 && record[0].Equals(Constants.InvoiceRecordType.Line, StringComparison.OrdinalIgnoreCase);
		}
		#endregion

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Constants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldNotHaveConstructors")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
			public class InvoiceHeaderFields
			{
				public const int RecordLength = 48;

				public const int Type = 0;
				public const int InvoiceNo = 1;
				public const int OrderNo = 2;
				public const int InvoiceDate = 3;
				public const int InvoiceAmount = 4;
				public const int InvoiceCurrency = 5;
				public const int IncoTerm = 6;
				public const int Preference = 7;
				public const int Related = 8;
				public const int TaxExempt = 9;
				public const int SupplierCode = 10;
				public const int SupplierName = 11;
				public const int SupplierRegNumber = 12;
				public const int SupplierPhone = 13;
				public const int SupplierStreet = 14;
				public const int SupplierStreet2 = 15;
				public const int SupplierCity = 16;
				public const int SupplierPostCode = 17;
				public const int SupplierCountry = 18;
				public const int BuyerCode = 19;
				public const int BuyerName = 20;
				public const int BuyerRegNumber = 21;
				public const int BuyerPhone = 22;
				public const int BuyerStreet = 23;
				public const int BuyerStreet2 = 24;
				public const int BuyerCity = 25;
				public const int BuyerPostCode = 26;
				public const int BuyerCountry = 27;
				public const int CustomText1 = 28;
				public const int CustomText2 = 29;
				public const int CustomDate1 = 30;
				public const int CustomDate2 = 31;
				public const int CustomFlag1 = 32;
				public const int CustomFlag2 = 33;
				public const int CustomDecimal1 = 34;
				public const int CustomDecimal2 = 35;
				// at the time of adding this column, CA had most columns and it had 47 columns.
				public const int ShipmentType = 47;
			}

			public static class InvoiceChargeFields
			{
				public const int RecordLength = 8;

				public const int Type = 0;
				public const int ChargeType = 1;
				public const int Amount = 2;
				public const int Currency = 3;
				public const int IsDutiable = 4;
				public const int IsGSTApplicable = 5;
				public const int IsIncludedInITOT = 6;
				public const int PrepaidCollect = 7;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldNotHaveConstructors")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
			public class InvoiceLineFields
			{
				public const int RecordLength = 37;

				public const int Type = 0;
				public const int LineNo = 1;
				public const int InvoiceNo = 2;
				public const int OrderNo = 3;
				public const int ProductCode = 4;
				public const int ProductAttrib1 = 5;
				public const int ProductAttrib2 = 6;
				public const int ProductAttrib3 = 7;
				public const int ProductDescription = 8;
				public const int Quantity = 9;
				public const int UnitofQty = 10;
				public const int Volume = 11;
				public const int UnitofVol = 12;
				public const int Weight = 13;
				public const int UnitOfWgt = 14;
				public const int LinePrice = 15;
				public const int TariffCode = 16;
				public const int TariffLookup = 17;
				public const int CustomsQty = 18;
				public const int CustomsUnit = 19;
				public const int Origin = 20;
				public const int OriginState = 21;
				public const int TreatmentCode = 22;
				public const int Preference = 23;
				public const int Concession = 24;
				public const int CustomText1 = 25;
				public const int CustomText2 = 26;
				public const int CustomText3 = 27;
				public const int CustomDate1 = 28;
				public const int CustomDate2 = 29;
				public const int CustomDate3 = 30;
				public const int CustomFlag1 = 31;
				public const int CustomFlag2 = 32;
				public const int CustomFlag3 = 33;
				public const int CustomDecimal1 = 34;
				public const int CustomDecimal2 = 35;
				public const int CustomDecimal3 = 36;
			}

			public static class InvoiceRecordType
			{
				public const string Head = "HEAD";
				public const string Line = "LINE";
				public const string Charge = "CHRG";
			}
		}
		#endregion

		#region RecordAsString
		protected string RecordAsString(string[] record)
		{
			StringBuilder result = new StringBuilder(record.Length);

			foreach (string s in record)
			{
				if (s.Length > 0)
				{
					result.Append(s);
					result.Append(",");
				}
			}

			return result.ToString();
		}
		#endregion

		#region SetValue
		protected void SetValue(ZPropertyInfo propertyInfo, string[] fieldValues, int fieldIndex)
		{
			if (fieldValues.Length > fieldIndex)
			{
				ZString fieldValue = fieldValues[fieldIndex];
				if (!fieldValue.IsEmpty)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime))
					{
						SetDateTimeValue(propertyInfo, fieldValue);
					}
					else
					{
						SetPropertyInfoValue(propertyInfo, fieldValue);
					}
				}
			}
		}

		protected virtual void SetDateTimeValue(ZPropertyInfo propertyInfo, ZString fieldValue)
		{
			if (fieldValue.Length == 14)
			{
				try
				{
					//CCYYMMDDHHMMSS DateTimeFormat for CSV files
					Regex dateExpression = new Regex("[12][0-9]{3}[01][0-9][0-3][0-9][012][0-9][0-5][0-9][0-6][0-9]");
					if (dateExpression.IsMatch(fieldValue))
					{
						propertyInfo.Value = new ZDateTime(
							ZInt.Parse(fieldValue.Left(4)),
							ZInt.Parse(fieldValue.SubstringSafe(4, 2)),
							ZInt.Parse(fieldValue.SubstringSafe(6, 2)),
							ZInt.Parse(fieldValue.SubstringSafe(8, 2)),
							ZInt.Parse(fieldValue.SubstringSafe(10, 2)),
							ZInt.Parse(fieldValue.SubstringSafe(12, 2)));
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					//assume datetime format is correct - else do nothing
				}
			}
		}
		#endregion

		#region Implementation

		protected virtual BaseJobComInvoiceHeader[] InvoiceHeaders
		{
			get { return toJobDec.Invoices.ToArray(); }
		}

		protected virtual string GetClassificationTypeMatching(BaseJobComInvoiceLine invoiceLine)
		{
			bool isImport = false;

			if (toJobDec != null)
			{
				isImport = toJobDec.IsImport;
			}
			else
			{
				var invoice = invoiceLine.InvoiceHeader;
				if (invoice != null)
				{
					if (!invoice.JZ_MessageType.IsEmpty)
					{
						isImport = invoice.IsImport;
					}
					else
					{
						var buyer = invoice.Buyer;
						isImport = buyer != null && buyer.CountryCode == invoice.CountryCode;
					}
				}
			}

			return isImport ? BaseCusClassification.ClassificationType.IMP : BaseCusClassification.ClassificationType.EXP;
		}

		protected virtual BaseJobComInvoiceHeader[] FindInvoicesFromInvoiceNumber(ZString invoiceNumber)
		{
			ZQuery query = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);
			return new List<BaseJobComInvoiceHeader>(toJobDec.Invoices.Find(query)).ToArray();
		}

		#endregion
	}
}
