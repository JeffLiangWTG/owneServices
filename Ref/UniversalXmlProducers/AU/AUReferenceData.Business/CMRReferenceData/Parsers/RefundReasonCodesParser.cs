using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using System;
using System.IO;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class RefundReasonCodesParser : BaseCMRReferenceDataParser
	{
		public RefundReasonCodesParser(IDateTimeProvider dateProvider)
		{
			this.dateProvider = dateProvider;
		}

		protected override string FileNamePrefix => ApplicationConfig.RefundReasonCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_RefundReasonCodes.xml";

		protected override string DataSource => "AU CMR Refund Reason Codes";

		const bool IsAutoExpiryOn = false;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate, new DateTime(2000, 1, 1, 0, 0, 0));
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRRR, defaultStartDate);
		}

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 10),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 71, 250),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_StartDate, 12, 8),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_EndDate, 21, 8, Constants.RefData_Common.MaximumDateTime),
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var code = codeListConverter.Convert(line);
						if (IsCodeValid(code, line))
						{ 
							xmlWriter.PopulateData(code);
						}
					}
				}
			}
		}

		bool IsCodeValid(RefCusCodeList code, string lineToOutput)
		{
			return Helper.IsConditionValidAndReportError((code.ZZD_Code != null && code.ZZD_Description != null), InsufficientInfoErrorMessage, lineToOutput)
				&& Helper.IsConditionValidAndReportError((code.ZZD_StartDate < code.ZZD_EndDate), InvalidTimeErrorMessage, lineToOutput)
				&& Helper.IsConditionValidAndReportError((code.ZZD_EndDate.Date >= dateProvider.CurrentLocalDateTime.Date), CodeHasExpiredErrorMessage, lineToOutput);
		}

		IDateTimeProvider dateProvider;
	}
}
