using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.USReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	public class FirmsCodeMessageProcessor : USIncomingMessageProcessor
	{
		public FirmsCodeMessageProcessor(string outputPath) : base(outputPath)
		{
		}

		public override string MetaDataPattern => @"F[234]11.{75} ";

		public override string OutputFileName => "USFirmsCodes.xml";

		public override string XMLWriterDataSource => "US FIRMS Codes";

		public override UpdateType UpdateType => UpdateType.Partial;

		protected override void ProcessCore(MatchCollection matchMetaDatas)
		{
			var refCusCodeLists = new List<RefCusCodeList>();
			var lastUpdateDate = DateTime.MinValue;
			var currentUSTime = DateTime.UtcNow.AddHours(-5).Date;

			RefCusCodeList refCusCodeList = null;
			var currentAttributeList = new List<RefCusCodeListAttribute>();
			foreach (Match matchMetaData in matchMetaDatas)
			{
				var matchValue = matchMetaData.Value;
				var identifier = matchValue.Substring(0, 4);
				if (identifier == USIncomingMessageRequest.F211)
				{
					if (refCusCodeList != null)
					{
						refCusCodeList.RefCusCodeListAttributes = currentAttributeList.ToArray();
						refCusCodeLists.Add(refCusCodeList);
						refCusCodeList = null;
						currentAttributeList.Clear();
					}

					var firmsCode = matchValue.Substring(8, 4).Trim();
					if (!string.IsNullOrEmpty(firmsCode))
					{
						var districtPortCode = matchValue.Substring(4, 4).Trim();
						var status = matchValue.Substring(12, 1).Trim();
						var facilityType = matchValue.Substring(13, 2).Trim();
						var nameOfFacility = matchValue.Substring(15, 35).Trim();
						var endDate = status == FirmsCode.Active ? DefaultValues.MaxDateTime : currentUSTime;

						refCusCodeList = new RefCusCodeList
						{
							ZZD_Code = firmsCode,
							ZZD_ZZK_NKCodeType = CodeType.FIRMS,
							ZZD_Description = nameOfFacility,
							ZZD_EndDate = endDate
						};

						AddAttribute(currentAttributeList, AttributeNames.FacilityType, facilityType);
						AddAttribute(currentAttributeList, AttributeNames.DistrictPortCode, districtPortCode);

						var updateDate = DateTime.ParseExact(matchValue.Substring(50, 6), MMDDYY, CultureInfo.InvariantCulture);
						if (updateDate > lastUpdateDate)
						{
							lastUpdateDate = updateDate;
						}
					}
				}

				if (refCusCodeList != null)
				{
					if (identifier == USIncomingMessageRequest.F311)
					{
						var facilityAddress = matchValue.Substring(4, 35).Trim();
						var city = matchValue.Substring(39, 35).Trim();
						var state = matchValue.Substring(74, 2).Trim();
						AddAttribute(currentAttributeList, AttributeNames.FacilityAddress, facilityAddress);
						AddAttribute(currentAttributeList, AttributeNames.City, city);
						AddAttribute(currentAttributeList, AttributeNames.State, state);
					}
					else if (identifier == USIncomingMessageRequest.F411)
					{
						var zipCode = matchValue.Substring(4, 9).Trim();
						var country = matchValue.Substring(13, 2).Trim();
						AddAttribute(currentAttributeList, AttributeNames.ZIPCode, zipCode);
						AddAttribute(currentAttributeList, AttributeNames.Country, country);
					}
				}
			}

			if (refCusCodeList != null)
			{
				refCusCodeList.RefCusCodeListAttributes = currentAttributeList.ToArray();
				refCusCodeLists.Add(refCusCodeList);
			}

			SaveXML(refCusCodeLists.DistinctBy(x => x.ZZD_Code).ToList(), DateTime.UtcNow.Date, () => XmlWriterHelper.GetRefCusCodeListWriterConfiguration(CodeType.FIRMS, isKeyColumnForAttributeValue: false));

			if (lastUpdateDate != DateTime.MinValue)
			{
				var lastUpdateDateStorage = new LocalFileStorage(FirmsCode.LastUpdateDate);
				lastUpdateDateStorage.Save(lastUpdateDate, MMDDYY);
			}
		}

		static void AddAttribute(List<RefCusCodeListAttribute> attributeList, string name, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				var attribute = new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = name,
					ZZE_Value = value
				};
				attributeList.Add(attribute);
			}
		}
	}
}
