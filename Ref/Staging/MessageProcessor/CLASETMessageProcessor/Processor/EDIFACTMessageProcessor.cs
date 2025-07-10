using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CLASET;
using Enterprise.Edifact.D09B.Segments;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor
{
	public class EDIFACTMessageProcessor : BaseMessageProcessor
	{
		public EDIFACTMessageProcessor(string outputPath)
			: base(outputPath)
		{
		}

		protected override int ProcessCore(SourceData sourceData, string messageText)
		{
			var result = 0;

			var uNHIndex = messageText.IndexOf("UNH", StringComparison.InvariantCulture);
			if (uNHIndex > 0)
			{
				messageText = messageText.Substring(uNHIndex).Replace("\n", "").Replace("\r", "").Replace("\t", "");
			}

			var claset = (CLASETMessage)MessageFactory.GetMessage(CharacterSet, messageText);
			if (claset == null)
			{
				sourceData.SDA_Status = StatusProvider.GetERRStatus();
			}
			else
			{
				(ReleaseNumber, ReleaseDate) = GetReleaseDateAndNumber(claset);

				if (ReleaseDate == DateTime.MinValue)
				{
					ReleaseDate = sourceData.SDA_CreatedTime.Date;
				}

				result = ProcessCLASETData(claset);

				sourceData.SDA_Status = StatusProvider.GetMERStatus();
			}

			return result;
		}

		#region Process

		int ProcessCLASETData(CLASETMessage claset)
		{
			ProcessCLASETDataCore(claset.Group4);
			ExportToXMLFile();

			return TotalProcessed;
		}

		void ProcessCLASETDataCore(SegmentGroup4MessageSection sg4Groups)
		{
			foreach (SegmentGroup4 sg4 in sg4Groups)
			{
				Func<SegmentGroup11, int> processCodeFunc = null;

				var referenceListIdentifier = sg4.VLI[0].ValueListIdentification.ValueListIdentifier;

				switch (referenceListIdentifier)
				{
					case Constants.ReferenceTypes.PortCode:
						processCodeFunc = ProcessPortCode;
						break;

					case Constants.ReferenceTypes.FacilityCode:
						processCodeFunc = ProcessFacilityCode;
						break;

					case Constants.ReferenceTypes.CustomsProcedureCode:
						processCodeFunc = ProcessCustomsProcedureCode;
						break;

					case Constants.ReferenceTypes.CommodityCode:
						processCodeFunc = ProcessCommondityCode;
						break;
				}

				if (processCodeFunc != null)
				{
					foreach (SegmentGroup11 sg11 in sg4.Group11)
					{
						TotalProcessed += processCodeFunc(sg11);
					}
				}
			}
		}

		#endregion

		#region Facility Code

		int ProcessFacilityCode(SegmentGroup11 sg11)
		{
			var updateType = sg11.Group12[0].STS[0].Status.StatusDescriptionCode;
			var startDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.EffectiveFromDateTime);
			var endDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.ExpiryDate);

			var totalProcessed = 0;

			foreach (SegmentGroup13 sg13 in sg11.Group13)
			{
				var placeCode = sg11.Group13[0].ATT[0].AttributeDetail.AttributeDescriptionCode;
				var placeName = sg11.Group13[0].Group14[0].FTX[0].TextLiteral.FreeText1;

				if (updateType == StatusDescriptionCodeList.Terminated)
				{
					endDate = GetValidEndDate(startDate);
				}

				totalProcessed += AddFacilityCode(placeCode, placeName, startDate, endDate);
			}

			return totalProcessed;
		}

		#endregion

		#region Commodity List

		int ProcessCommondityCode(SegmentGroup11 sg11)
		{
			var updateType = sg11.Group12[0].STS[0].Status.StatusDescriptionCode;
			var startDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.EffectiveFromDateTime);
			var endDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.ExpiryDate);
			var segmentSequence = 0;
			var commodityCode = "";
			var commodityDescription = "";
			var tariffCode = "";

			foreach (SegmentGroup13 sg13 in sg11.Group13)
			{
				segmentSequence++;

				if (segmentSequence == 1)
				{
					commodityCode = sg13.ATT[0].AttributeDetail.AttributeDescriptionCode;
					var productText = sg13.Group14[0].FTX[0].TextLiteral;
					commodityDescription = productText.FreeText1 + productText.FreeText2 + productText.FreeText3 + productText.FreeText4 + productText.FreeText5;
				}
				else
				{
					tariffCode = sg13.ATT[0].AttributeDetail.AttributeDescriptionCode;
					break;
				}
			}

			(string typeOfControl, string unitOfQty) = GetControlTypeAndUnitOfQty(sg11.Group16[0].Group18[0]);

			if (updateType == StatusDescriptionCodeList.Terminated)
			{
				endDate = GetValidEndDate(startDate);
			}

			return AddCommondityCode(commodityCode, commodityDescription, tariffCode, startDate, endDate, unitOfQty, typeOfControl);
		}

		static (string controlType, string unitOfQty) GetControlTypeAndUnitOfQty(SegmentGroup18 sg18)
		{
			var foundControlType = false;
			var foundUOM = false;
			var controlType = string.Empty;
			var unitOfQty = string.Empty;
			foreach (SegmentGroup19 sg19 in sg18.Group19)
			{
				foreach (FTXSegment ftx in sg19.FTX)
				{
					if (ftx.TextLiteral.FreeText1 == Constants.TariffSegmentIdentifiers.ControlType)
					{
						controlType = ftx.TextLiteral.FreeText2;
						foundControlType = true;
					}
					else if (ftx.TextLiteral.FreeText1 == Constants.TariffSegmentIdentifiers.UnitOfMeasurement)
					{
						unitOfQty = ftx.TextLiteral.FreeText2;
						foundUOM = true;
					}

					if (foundUOM && foundControlType)
					{
						return (controlType, unitOfQty);
					}
				}
			}

			return (controlType, unitOfQty);
		}

		#endregion

		#region Customs Procedure Code

		int ProcessCustomsProcedureCode(SegmentGroup11 sg11)
		{
			var updateType = sg11.Group12[0].STS[0].Status.StatusDescriptionCode;
			var segmentSequence = 0;
			var aPCode = string.Empty;
			var aPCName = string.Empty;
			var cPCode = string.Empty;
			var startDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.EffectiveFromDateTime);
			var endDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.ExpiryDate);
			foreach (SegmentGroup13 sg13 in sg11.Group13)
			{
				segmentSequence++;
				if (segmentSequence == 1)
				{
					aPCode = sg13.ATT[0].AttributeDetail.AttributeDescriptionCode;
					aPCName = sg13.Group14[0].FTX[0].TextLiteral.FreeText1;
				}
				else if (segmentSequence == 2)
				{
					cPCode = sg13.ATT[0].AttributeDetail.AttributeDescriptionCode;
					break;
				}
			}

			if (updateType == StatusDescriptionCodeList.Terminated)
			{
				endDate = GetValidEndDate(startDate);
			}

			return AddCustomsProcedureCode(aPCode, aPCName, cPCode, startDate, endDate);
		}

		#endregion

		#region Port Code

		int ProcessPortCode(SegmentGroup11 sg11)
		{
			var updateType = sg11.Group12[0].STS[0].Status.StatusDescriptionCode;
			var startDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.EffectiveFromDateTime);
			var endDate = ExtractDate(sg11, DateOrTimeOrPeriodFunctionCodeQualifierList.ExpiryDate);
			var totalProcessed = 0;
			foreach (SegmentGroup13 sg13 in sg11.Group13)
			{
				var portCode = sg11.Group13[0].ATT[0].AttributeDetail.AttributeDescriptionCode;
				var portName = sg11.Group13[0].Group14[0].FTX[0].TextLiteral.FreeText1;

				if (updateType == StatusDescriptionCodeList.Terminated)
				{
					endDate = GetValidEndDate(startDate);
				}
				totalProcessed += AddPortCode(portCode, portName, startDate, endDate);
			}
			return totalProcessed;
		}

		#endregion

		#region GetReleaseDateAndNumber

		static (string releaseNumber, DateTime releaseDate) GetReleaseDateAndNumber(CLASETMessage claset)
		{
			var releaseNumber = "";
			var releaseDate = DateTime.MinValue;
			var releaseGroup = claset.Group1.OfType<SegmentGroup1>().FirstOrDefault(x => x.RFF.OfType<RFFSegment>().Any(r => r.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.ReleaseNumber));

			if (releaseGroup != null)
			{
				releaseNumber = releaseGroup.RFF.OfType<RFFSegment>().Where(x => x.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.ReleaseNumber).FirstOrDefault()?.Reference.ReferenceIdentifier ?? string.Empty;
				releaseDate = GetDate(releaseGroup.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.DataReleaseDate, DateFormat);
			}

			if (releaseDate == DateTime.MinValue)
			{
				(releaseNumber, releaseDate) = GetBaseReleaseDateAndNumber(claset);
			}
			return (releaseNumber, releaseDate);
		}

		static (string releaseNumber, DateTime releaseDate) GetBaseReleaseDateAndNumber(CLASETMessage claset)
		{
			var date = GetDate(claset.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.DataReleaseDate, DateFormat);
			var number = claset.UNH?.OfType<UNHSegment>().FirstOrDefault().MessageReferenceNumber ?? string.Empty;
			number = number.Replace("|", "_");
			return (number, date);
		}

		static DateTime GetDate(DTMSegmentMessageSection section, string qualifier, string dateFormat)
		{
			var result = DateTime.MinValue;
			return !(section?.OfType<DTMSegment>().FirstOrDefault(x => x.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == qualifier)?.DateTimePeriod.DateOrTimeOrPeriodText.TryParseExact(out result, dateFormat) ?? false) ? DateTime.MinValue : result;
		}

		#endregion

		#region ExtractDate

		static DateTime ExtractDate(SegmentGroup11 sg11, string qualifier) => ExtractDate(sg11, qualifier, Constants.SegmentGroup12DateFormat);

		static DateTime ExtractDate(SegmentGroup11 sg11, string qualifier, string dateFormat)
		{
			Argument.NotNull(sg11, nameof(sg11));
			Argument.NotNullOrEmpty(qualifier, nameof(qualifier));
			Argument.NotNullOrEmpty(dateFormat, nameof(dateFormat));

			var result = DateTime.MinValue;

			foreach (SegmentGroup12 sg12 in sg11.Group12)
			{
				foreach (DTMSegment dtm in sg12.DTM)
				{
					if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == qualifier)
					{
						var dateString = dtm.DateTimePeriod.DateOrTimeOrPeriodText;
						var dateYear = Convert.ToInt32(dateString.Substring(0, 4), CultureInfo.InvariantCulture);
						if (dateYear < DateTime.MaxValue.Year)
						{
							if (!dateString.Substring(0, 14).TryParseExact(out result, dateFormat))
							{
								result = DateTime.MinValue;
							}
						}
						else
						{
							result = DateTime.MaxValue;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Implement

		UNOACharacterSet CharacterSet => characterSet ?? (characterSet = new UNOASGCharacterSet());
		UNOACharacterSet characterSet;

		MessageFactory MessageFactory
		{
			get
			{
				if (messageFactory == null)
				{
					messageFactory = new MessageFactory(new Enterprise.Edifact.D09B.EdifactD09BMessageFactory());
				}
				return messageFactory;
			}
		}
		MessageFactory messageFactory;

		#endregion
	}
}
