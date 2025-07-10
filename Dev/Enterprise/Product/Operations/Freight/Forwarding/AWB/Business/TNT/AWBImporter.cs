using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.TNT
{
	public interface IImportLogger
	{
		void LogInfo(string message);
		void LogWarning(string message);
		void LogError(string message);
	}

	public interface IAWBProcessedInfo
	{
		bool ErrorsFound { get; }
		string MAWBNumber { get; }
		ZGuid MAWBRecordPK { get; }
		string FlightDate { get; }
		string DestinationPort { get; }
		List<string> HAWBNumbers { get; }
	}

	public interface IAWBImporter
	{
		/// <summary>
		/// Parses the quantum file and extracts key info from the file. Parsing progress will be logged to the provided logger, including errors.
		/// </summary>
		IAWBProcessedInfo Validate(OrgContact webUser, Stream quantumFileContent, IImportLogger logger);

		/// <summary>
		/// Parses the quantum file and extracts key info from the file. Imports the file into AWB tables. Parsing progress will be logged to the provided logger, including errors.
		/// </summary>
		IAWBProcessedInfo Import(OrgContact webUser, Stream quantumFileContent, IImportLogger logger);
	}

	class AWBProcessedInfo : IAWBProcessedInfo
	{
		public AWBProcessedInfo()
		{
			ErrorsFound = false;
			MAWBNumber = string.Empty;
			MAWBRecordPK = ZGuid.Empty;
			FlightDate = string.Empty;
			DestinationPort = string.Empty;
		}

		public bool ErrorsFound { get; set; }
		public string MAWBNumber { get; set; }
		public ZGuid MAWBRecordPK { get; set; }
		public string FlightDate { get; set; }
		public string DestinationPort { get; set; }

		public List<string> HAWBNumbers
		{
			get
			{
				return HAWBNumbersList.ToList();
			}
		}

		public List<string> HAWBNumbersList = new List<string>();
	}

	public class AWBImporter : IAWBImporter
	{
		#region IAWBImporter Members

		public IAWBProcessedInfo Validate(OrgContact webUser, Stream quantumFileContent, IImportLogger logger)
		{
			logger.LogInfo(Res.GetString("B7D4492F-6F4E-4828-9350-D3FB51CB9876", "Validating Quantum File with size: {0} bytes", quantumFileContent.Length));
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			var info = ProcessInternal(webUser, factory, quantumFileContent, logger);

			if (info.ErrorsFound)
			{
				logger.LogInfo(Res.GetString("2961C510-F4E2-4E6D-904A-F3258EFE80D5", "Validation failed."));
			}
			else
			{
				logger.LogInfo(Res.GetString("9E95ADE7-1BAB-45F7-8100-788DD3E8FA3C", "MAWB and HAWBs successfully validated."));
			}

			return info;
		}

		public IAWBProcessedInfo Import(OrgContact currentUser, Stream quantumFileContent, IImportLogger logger)
		{
			logger.LogInfo(Res.GetString("3EB9B61D-5B96-4EF8-9791-42A158777230", "Importing from Quantum File with size: {0} bytes", quantumFileContent.Length));

			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			var info = ProcessInternal(currentUser, factory, quantumFileContent, logger);

			if (!info.ErrorsFound)
			{
				try
				{
					factory.Save();
					logger.LogInfo(Res.GetString("E59D2753-09F8-40F5-B571-65E9464AA504", "MAWB and HAWBs successfully saved to database."));
				}
				catch (ZSaveException e)
				{
					info.ErrorsFound = true;
					StringBuilder errorBuilder = new StringBuilder();
					errorBuilder.AppendLine(Res.GetString("fe0956bf-74ac-4a13-a3b5-a777682ccc20", "Error encountered while saving records to the database. Details are:"));
					errorBuilder.AppendLine(e.Message);
					logger.LogError(errorBuilder.ToString());
				}
			}

			if (info.ErrorsFound)
			{
				logger.LogInfo(Res.GetString("392BCF0B-C2BD-47BE-8754-612EB871C0D2", "Import failed."));
			}

			return info;
		}

		#endregion

		AWBProcessedInfo ProcessInternal(OrgContact currentUser, BusinessObjectFactory factory, Stream quantumFileContent, IImportLogger logger)
		{
			var info = new AWBProcessedInfo();

			using (var reader = new StreamReader(quantumFileContent))
			{
				ExportAWBHeader mawbHeader = null;
				var mawbNumbers = new List<string>();
				var hawbNumbers = new Dictionary<string, int>();

				do
				{
					var quantumLine = reader.ReadLine();

					if (string.IsNullOrEmpty(quantumLine) || !quantumLine.StartsWith("100", StringComparison.Ordinal))
					{
						info.ErrorsFound = true;
						logger.LogError((NoResString)"File contains the wrong RECORD_TYPE. 100 is the expected value."); // Error message
						return info;
					}

					TNTCustomsRowData line = null;

					try
					{
						line = new TNTCustomsRowData();
						line.ReadRow(quantumLine);
					}
					catch (Exception e)
					{
						if (e.IsCriticalException())
						{
							throw;
						}

						info.ErrorsFound = true;
						logger.LogError((NoResString)"Error parsing file. See error details below:"); // Error message
						logger.LogError(e.Message);
						return info;
					}

					if (line.RECORD_TYPE != 100)
					{
						info.ErrorsFound = true;
						logger.LogError((NoResString)"File contains the wrong RECORD_TYPE. 100 is the expected value."); // Error message
						return info;
					}

					string lineMAWBNumber = GetMAWBNumber(line);
					CheckForDuplicateMAWB(factory, lineMAWBNumber, info, logger);
					if (!mawbNumbers.Contains(lineMAWBNumber))
					{
						mawbNumbers.Add(lineMAWBNumber);
					}

					CheckValidIATAPort(factory, "MOV_DEST_BUL_ID", line.MOV_DEST_BUL_ID, info, logger, line.CON_ID); // not a schema column name
					CheckValidIATAPort(factory, "MOV_ORIG_BUL_ID", line.MOV_ORIG_BUL_ID, info, logger, line.CON_ID); // not a schema column name
					CheckValidCountryCode(factory, "SEN_COU_ID", line.SEN_COU_ID, info, logger, line.CON_ID); // not a schema column name
					CheckValidCountryCode(factory, "REC_COU_ID", line.REC_COU_ID, info, logger, line.CON_ID); // not a schema column name

					if (info.ErrorsFound)
					{
						return info;
					}

					try
					{
						if (mawbHeader == null)
						{
							mawbHeader = CreateMAWB(currentUser, factory, line);

							if (info.ErrorsFound)
							{
								return info;
							}

							info.MAWBNumber = mawbHeader.EH_WayBillNumber;
							info.MAWBRecordPK = mawbHeader.PK;
							info.DestinationPort = mawbHeader.EH_AirportOfDestinationCode;
							info.FlightDate = line.MOV_DEP_DT;
							mawbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
							logger.LogInfo(Res.GetString("b2f32342-b8cb-4672-852e-f8ee4d70c415", "MAWB processed: {0}", info.MAWBNumber));
						}

						string sourceHawbNumber = line.CON_ID;
						int hawbCount;
						hawbNumbers.TryGetValue(sourceHawbNumber, out hawbCount);
						hawbNumbers[sourceHawbNumber] = hawbCount + 1;

						if (hawbNumbers[sourceHawbNumber] == 1)
						{
							var hawbHeader = CreateHAWB(mawbHeader, line);
							info.HAWBNumbersList.Add(hawbHeader.EH_WayBillNumber);
							logger.LogInfo(Res.GetString("ed0f64f6-21d8-4a4d-a38b-2428ee0d924c", "HAWB processed: {0}", hawbHeader.EH_WayBillNumber));
						}
						else
						{
							ExportAWBHeader existingHawb = (ExportAWBHeader)mawbHeader.ChildBills.Find(new ZQuery(ExportAWBHeaderSchema.EH_WayBillNumber, sourceHawbNumber))[0];
							CheckCanMerge(existingHawb, line, logger, info);

							if (info.ErrorsFound)
							{
								return info;
							}

							MergeHAWB(existingHawb, line);
							logger.LogInfo(Res.GetString("d4af727b-3ab9-43ae-9898-57f947c28245", "HAWB merged: {0}, Sequence: {1}", sourceHawbNumber, line.CON_SEQ_ID));
						}
					}
					catch (Exception e)
					{
						if (e.IsCriticalException())
						{
							throw;
						}

						info.ErrorsFound = true;
						StringBuilder errorBuilder = new StringBuilder();
						errorBuilder.Append((NoResString)"Error encountered while processing record: "); // Exception Only
						errorBuilder.AppendLine(quantumLine);
						errorBuilder.AppendLine((NoResString)"Error details are:"); // Exception Only
						errorBuilder.AppendLine(e.Message);
						logger.LogError(errorBuilder.ToString());

						return info;
					}
				} while (quantumFileContent.Position < quantumFileContent.Length);

				CheckForMultipleMAWBs(mawbNumbers, info, logger);

				if (!info.ErrorsFound && mawbHeader != null)
				{
					var awbRateLine = mawbHeader.AWBRateLines[0];

					int mawbTotalPieces = 0;
					var totalWeight = ZDecimal.Zero;
					var totalShippingLoadAndCount = ZInt.Zero;

					foreach (ExportAWBHeader hawbHeader in mawbHeader.ChildBills)
					{
						totalWeight += hawbHeader.AWBRateLines[0].ER_GrossWeight;

						int pieces = 0;
						if (int.TryParse(hawbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP, out pieces))
						{
							mawbTotalPieces += pieces;
						}

						totalShippingLoadAndCount += hawbHeader.EH_ShippingLoadAndCount;
					}

					awbRateLine.ER_GrossWeight = totalWeight;
					awbRateLine.ER_RateClass = GetRateClass(totalWeight);
					awbRateLine.ER_ChargeableWeight = totalWeight;
					awbRateLine.ER_NoOfPiecesOrRCPInfo.SetValueTruncateToFit(mawbTotalPieces.ToString(Culture.Invariant));

					mawbHeader.EH_ShippingLoadAndCount = totalShippingLoadAndCount;
				}
			}

			return info;
		}

		void CheckCanMerge(ExportAWBHeader existingHawb, TNTCustomsRowData line, IImportLogger logger, AWBProcessedInfo info)
		{
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			var mawbHeader = tempFactory.New<ExportAWBHeader>();
			var tempHAWBHeader = CreateHAWB(mawbHeader, line);

			StringBuilder errorBuilder = new StringBuilder();

			ValidateMergeFieldsAreTheSame(existingHawb, tempHAWBHeader, ExportAWBHeader.Schema.EH_ShipperName, errorBuilder);
			ValidateMergeFieldsAreTheSame(existingHawb, tempHAWBHeader, ExportAWBHeader.Schema.EH_ConsigneeName, errorBuilder);
			ValidateMergeFieldsAreTheSame(existingHawb, tempHAWBHeader, ExportAWBHeader.Schema.EH_HouseDeclaredValueCurrency, errorBuilder);
			ValidateMergeFieldsAreTheSame(existingHawb, tempHAWBHeader, ExportAWBHeader.Schema.EH_HouseCustomsValueCurrency, errorBuilder);

			if (errorBuilder.Length > 0)
			{
				info.ErrorsFound = true;
				logger.LogError(Res.GetString("2ea2270e-8116-448d-9a89-3a87c9f4f49c", "Error while merging HAWB records. HAWB Number: {0}, Sequence: {1}. {2}", tempHAWBHeader.EH_WayBillNumber, line.CNA_CON_SEQ_ID, errorBuilder.ToString()));
			}
		}

		void ValidateMergeFieldsAreTheSame(ExportAWBHeader existingHawb, ExportAWBHeader tempHAWBHeader, string fieldName, StringBuilder errorBuilder)
		{
			IZType tempValue = tempHAWBHeader[fieldName] as IZType ?? ZString.Empty;
			IZType existingValue = existingHawb[fieldName] as IZType ?? ZString.Empty;

			if (!tempValue.Equals(existingValue))
			{
				errorBuilder.AppendLine(Res.GetString("30ca621a-4d87-4129-8577-f898ac46237b", "Cannot merge HAWB with {0}: {1} into HAWB with {0}: {2}", fieldName, tempValue.ToString(), existingValue.ToString()));
			}
		}

		void MergeHAWB(ExportAWBHeader existingHawb, TNTCustomsRowData line)
		{
			var header = existingHawb;

			header.SetNatureAndQtyOfGoodsBreakingIntoLines(MergeWithoutDuplicate(header.NatureAndQtyOfGoods, line.ADS_DS, line.CON_GOODS_DS));
			header.EH_ShippingLoadAndCount += line.CON_TOT_PIECE_QT;

			var awbRateLine = header.AWBRateLines[0];
			awbRateLine.ER_GrossWeight += line.CON_OPSA_TGRS_WT;

			int pieces = int.TryParse(awbRateLine.ER_NoOfPiecesOrRCP, out pieces) ? pieces : 0;
			awbRateLine.ER_NoOfPiecesOrRCPInfo.SetValueTruncateToFit((pieces + line.CON_TOT_PIECE_QT).ToString(CultureInfo.InvariantCulture));
		}

		string MergeWithoutDuplicate(params string[] stringsToMerge)
		{
			string result = stringsToMerge.Length > 0 ? stringsToMerge[0] : string.Empty;

			if (stringsToMerge.Length > 1)
			{
				for (int i = 1; i < stringsToMerge.Length; i++)
				{
					string trimmedString = stringsToMerge[i].Trim();
					if (!result.Contains(trimmedString))
					{
						result += "  " + trimmedString;
					}
				}
			}

			return result.Trim();
		}

		void CheckForMultipleMAWBs(List<string> mawbNumbers, AWBProcessedInfo info, IImportLogger logger)
		{
			if (mawbNumbers.Count > 1)
			{
				info.ErrorsFound = true;
				var errorBuilder = new StringBuilder();
				errorBuilder.AppendLine(Res.GetString("6ad1819a-a816-4748-9da0-bf120c1b329e", @"Import file expected to have HAWBs that belong to a single MAWB, but more than 1 MAWB is found in this file."));
				errorBuilder.AppendLine(Res.GetString("c7115fcb-2d09-451c-ab35-8cad190104ea", @"Split this file into multiple files each containing HAWBs that belong to a single MAWB only, then try again."));
				errorBuilder.Append(Res.GetString("fccea768-fac6-46ee-a9ed-e49ac884d329", @"MAWBs found are:") + " ");

				foreach (var mawbNumber in mawbNumbers)
				{
					errorBuilder.Append(mawbNumber);
					errorBuilder.Append(", ");
				}

				logger.LogError(errorBuilder.ToString().TrimEnd(',', ' '));
			}
		}

		void CheckForDuplicateMAWB(BusinessObjectFactory factory, string mawbNumber, AWBProcessedInfo info, IImportLogger logger)
		{
			bool mawbExists = factory.ExistsInDatabase(ExportAWBHeaderSchema.Constants.TableName, new ZQuery(ExportAWBHeaderSchema.EH_WayBillNumber, mawbNumber));
			if (mawbExists)
			{
				info.ErrorsFound = true;
				logger.LogError(Res.GetString("CCCCB256-983A-4123-886D-CFCC963F4630", "File with MAWB Number {0} has already been imported. Duplicate import is not allowed.", mawbNumber));
			}
		}

		void CheckValidCountryCode(BusinessObjectFactory factory, string fieldName, string fieldValue, AWBProcessedInfo info, IImportLogger logger, string consignmentId)
		{
			var countries = factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, fieldValue));

			if (countries.Length == 0)
			{
				info.ErrorsFound = true;
				logger.LogError(Res.GetString("CB7A18FB-EDC6-4873-8E58-16E7FC158C9C", "File contains an invalid country/region code '{0}' in field {1}. Line containing the error has consignment ID: {2}", fieldValue, fieldName, consignmentId));
			}
		}

		void CheckValidIATAPort(BusinessObjectFactory factory, string fieldName, string fieldValue, AWBProcessedInfo info, IImportLogger logger, string consignmentId)
		{
			var locos = factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATA, fieldValue));
			if (locos.Length == 0)
			{
				info.ErrorsFound = true;
				logger.LogError(Res.GetString("7575ba52-df1c-4c48-8990-47dd21bf9ecc", "File contains an invalid IATA port code '{0}' in field {1}. Line containing the error has consignment ID: {2}", fieldValue, fieldName, consignmentId));
			}
		}

		string GetMAWBNumber(TNTCustomsRowData line)
		{
			return !string.IsNullOrEmpty(line.MOV_MAWB_CD) && line.MOV_MAWB_CD.Length > 3 ? line.MOV_MAWB_CD.Substring(0, 3) + "-" + line.MOV_MAWB_CD.Substring(3) : line.MOV_MAWB_CD;
		}

		internal ExportAWBHeader CreateMAWB(OrgContact currentUser, BusinessObjectFactory factory, TNTCustomsRowData line)
		{
			var header = factory.New<ExportAWBHeader>();

			header.EH_WayBillNumberInfo.SetValueTruncateToFit(GetMAWBNumber(line));

			ImportCommonFields(factory, line, header);

			header.EH_ShipperNameInfo.SetValueTruncateToFit(GlbCompany.CurrentCompany.GC_Name);
			header.EH_ShipperAddressInfo.SetValueTruncateToFit(GlbBranch.CurrentBranch.GB_Address1);
			header.EH_ShipperAddress2Info.SetValueTruncateToFit(GlbBranch.CurrentBranch.GB_Address2);
			header.EH_ShipperPostCodeInfo.SetValueTruncateToFit(GlbBranch.CurrentBranch.GB_PostCode);
			header.EH_ShipperPlaceInfo.SetValueTruncateToFit(GlbBranch.CurrentBranch.GB_City);
			header.EH_ShipperStateInfo.SetValueTruncateToFit(GlbBranch.CurrentBranch.GB_State);

			var orgProxyCountryCode = (GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy).CountryCode;
			header.EH_ShipperCountryCode = orgProxyCountryCode;

			RefUNLOCO[] unlocos = factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATA, line.MOV_DEST_BUL_ID));
			header.EH_ConsigneeCountryCode = unlocos[0].RL_RN_NKCountryCode;
			header.EH_AirportOfDestinationText = unlocos[0].RL_PortName;

			header.EH_ChargesCode = "PP";
			header.AWBRateLine1.NatureAndQtyOfGoodsDescription = Core.Constants.AWB.NatureAndQtyOfGoodsDetails.ConsolAsPerList;

			header.EH_AWBIssueDate = ZDateTime.Today;
			header.EH_AWBIssuePlace = GetAWBIssuePlace();
			header.EH_AWBAgentsSignatureInfo.SetValueTruncateToFit(currentUser.OC_ContactName);
			header.EH_ShippersSignature = ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName).Left(35);

			var accountingInfo = header.AWBAccountingInformations.AddNew();
			accountingInfo.EA_Information = Res.GetString("b75ece35-fcad-41ad-bc3b-1baea56f991b", "FREIGHT PREPAID");

			return header;
		}

		internal ExportAWBHeader CreateHAWB(ExportAWBHeader mawbHeader, TNTCustomsRowData line)
		{
			var header = mawbHeader.ChildBills.AddNew();

			header.EH_WayBillNumberInfo.SetValueTruncateToFit(line.CON_ID);

			ImportCommonFields(mawbHeader.Factory, line, header);

			header.EH_ShipperAccountInfo.SetValueTruncateToFit(line.SEN_ACC_ID);
			header.EH_ShipperNameInfo.SetValueTruncateToFit(line.SEN_NM);
			string[] shipperAddressLines = GetTwoAddressLines(line.SEN_ADDR_1_DS, line.SEN_ADDR_2_DS, line.SEN_ADDR_3_DS);
			header.EH_ShipperAddressInfo.SetValueTruncateToFit(shipperAddressLines[0]);
			header.EH_ShipperAddress2Info.SetValueTruncateToFit(shipperAddressLines[1]);

			header.EH_ShipperPlaceInfo.SetValueTruncateToFit(line.SEN_CITY_NM);
			header.EH_ShipperStateInfo.SetValueTruncateToFit(line.SEN_PRV_NM);
			header.EH_ShipperCountryCodeInfo.SetValueTruncateToFit(line.SEN_COU_ID);
			header.EH_ShipperPostCodeInfo.SetValueTruncateToFit(line.SEN_PCODE_CD);

			string shipperContactNumber = GetContactNumber(line.SEN_CPF_TEL_1_ID, line.SEN_CPF_TEL_2_ID);
			if (shipperContactNumber.Length > 0)
			{
				header.EH_ShipperContactCode = "TE";
				header.EH_ShipperContactDetailInfo.SetValueTruncateToFit(shipperContactNumber);
			}

			header.EH_ConsigneeAccountInfo.SetValueTruncateToFit(line.REC_ACC_ID);
			header.EH_ConsigneeNameInfo.SetValueTruncateToFit(line.REC_NM);
			string[] consigneeAddressLines = GetTwoAddressLines(line.REC_ADDR_1_DS, line.REC_ADDR_2_DS, line.REC_ADDR_3_DS);
			header.EH_ConsigneeAddressInfo.SetValueTruncateToFit(consigneeAddressLines[0]);
			header.EH_ConsigneeAddress2Info.SetValueTruncateToFit(consigneeAddressLines[1]);
			header.EH_ConsigneePostCodeInfo.SetValueTruncateToFit(line.REC_PCODE_CD);
			header.EH_ConsigneePlaceInfo.SetValueTruncateToFit(line.REC_CITY_NM);
			header.EH_ConsigneeStateInfo.SetValueTruncateToFit(line.REC_PRV_NM);
			header.EH_ConsigneeCountryCodeInfo.SetValueTruncateToFit(line.REC_COU_ID);
			header.EH_ConsigneePostCodeInfo.SetValueTruncateToFit(line.REC_PCODE_CD);

			string consigneeContactNumber = GetContactNumber(line.REC_CPF_TEL_1_ID, line.REC_CPF_TEL_2_ID);
			if (consigneeContactNumber.Length > 0)
			{
				header.EH_ConsigneeContactCode = "TE";
				header.EH_ConsigneeContactDetailInfo.SetValueTruncateToFit(consigneeContactNumber);
			}

			header.EH_DeclaredValue = line.CON_VAL_OF_GOODS_AM;
			header.EH_HouseDeclaredValueCurrencyInfo.SetValueTruncateToFit(line.CON_CUY_ID_VAL_OF_GOODS);
			header.EH_CustomsValue = line.CON_VAL_OF_GOODS_AM;
			header.EH_HouseCustomsValueCurrencyInfo.SetValueTruncateToFit(line.CON_CUY_ID_VAL_OF_GOODS);
			header.SetNatureAndQtyOfGoodsBreakingIntoLines(MergeWithoutDuplicate(line.ADS_DS, line.CON_GOODS_DS));

			header.EH_ShippingLoadAndCount = line.CON_TOT_PIECE_QT;
			var awbRateLine = header.AWBRateLines[0];
			awbRateLine.ER_GrossWeight = line.CON_OPSA_TGRS_WT;
			awbRateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			awbRateLine.ER_NoOfPiecesOrRCPInfo.SetValueTruncateToFit(line.CON_TOT_PIECE_QT.ToString(Culture.Invariant));
			return header;
		}

		ZString GetRateClass(ZDecimal grossWeight)
		{
			return (grossWeight >= 45M) ? Core.Constants.AWB.RateClass.QuantityRate : Core.Constants.AWB.RateClass.NormalCharge;
		}

		string GetContactNumber(string areaCode, string number)
		{
			ZString result = (areaCode + number + string.Empty).Trim();
			return result.KeepNumericCharacters();
		}

		void ImportCommonFields(BusinessObjectFactory factory, TNTCustomsRowData line, ExportAWBHeader header)
		{
			var carrier = RefAirline.LoadFromAirline2LetterCode(factory, line.MOV_CRR_ID);
			header.EH_IssuingAgentNameInfo.SetValueTruncateToFit(carrier.RM_AirlineName1);
			header.EH_IssuingAgentAddress1Info.SetValueTruncateToFit(carrier.RM_AddressLine1);
			header.EH_IssuingAgentAddress2Info.SetValueTruncateToFit(carrier.RM_AddressLine2 + ", " + carrier.RM_AirlineCity + ", " + carrier.RM_AirlinePostalCode);

			header.EH_AirportOfDepartureAndRequestRouteTextInfo.SetValueTruncateToFit(line.MOV_ORIG_BUL_ID);
			header.EH_To1stInfo.SetValueTruncateToFit(line.MOV_DEST_BUL_ID);
			header.EH_By1stInfo.SetValueTruncateToFit(line.MOV_CRR_ID);
			header.EH_Booking1stCarrierInfo.SetValueTruncateToFit(line.MOV_CRR_ID);
			header.EH_Booking1stFlightInfo.SetValueTruncateToFit(line.MOV_NR);
			header.EH_Booking1stFlightDate = !string.IsNullOrEmpty(line.MOV_DEP_DT) && line.MOV_DEP_DT.Length > 2 ? line.MOV_DEP_DT.Substring(line.MOV_DEP_DT.Length - 2, 2) : "";
			header.EH_AirportOfDestinationCodeInfo.SetValueTruncateToFit(line.MOV_DEST_BUL_ID);
			header.EH_AWBOriginCodeInfo.SetValueTruncateToFit(line.MOV_ORIG_BUL_ID);

			header.EH_CurrencyInfo.SetValueTruncateToFit(line.LAV_VALUE_DS1);

			header.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			header.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
		}

		ZString GetAWBIssuePlace()
		{
			ZString result = GlbBranch.CurrentBranch.GB_City;
			OrgHeader orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			if (orgProxy != null && orgProxy.UNLOCO != null)
			{
				result = orgProxy.UNLOCO.RL_PortName;
			}
			return result.Left(17);
		}

		string[] GetTwoAddressLines(string address1, string address2, string address3)
		{
			string addressCombined = address1 + " " + address2 + " " + address3;
			string[] addressParts = addressCombined.Split(' ');
			StringBuilder line1Builder = new StringBuilder(50);
			StringBuilder line2Builder = new StringBuilder(50);
			bool needLine2 = false;

			foreach (var part in addressParts)
			{
				if (!string.IsNullOrEmpty(part) && !string.IsNullOrEmpty(part.Trim()))
				{
					if (!needLine2 && line1Builder.Length + part.Length + 1 > ExportAWBHeaderSchema.EH_ShipperAddress.MaxLength)
					{
						needLine2 = true;
					}

					if (!needLine2)
					{
						if (line1Builder.Length > 0)
						{
							line1Builder.Append(" ");
						}

						line1Builder.Append(part);
					}
					else
					{
						if (line2Builder.Length > 0)
						{
							line2Builder.Append(" ");
						}

						line2Builder.Append(part);
					}
				}
			}

			return new string[] { line1Builder.ToString(), line2Builder.ToString() };
		}
	}

	public static class ZPropertyInfoExtensions
	{
		public static void SetValueTruncateToFit(this ZPropertyInfo propertyInfo, ZString value)
		{
			propertyInfo.SetValueFromString(value.Left(propertyInfo.MaxLength));
		}
	}
}
