using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	public class RegionDistrictPortCodeMessageProcessor : USIncomingMessageProcessor
	{
		public RegionDistrictPortCodeMessageProcessor(string outputPath) : base(outputPath)
		{
		}

		public override string MetaDataPattern => @"F[123]01\d{5}.{70} ";

		public override string OutputFileName => @"USRegionDistrictPortCode.xml";

		public override string XMLWriterDataSource => "US Region District Port Code";

		public override UpdateType UpdateType => UpdateType.Partial;

		protected override void ProcessCore(MatchCollection matchMetaDatas)
		{
			var refCusCodeLists = new List<RefCusCodeList>();
			var attributeList = new List<RefCusCodeListAttribute>();
			var transportModeList = new List<RefCusCodeOrAttributeTransportMode>();
			string code = string.Empty;
			string desc = string.Empty;
			foreach (Match match in matchMetaDatas)
			{
				var matchValue = match.Value;
				var identifier = matchValue.Substring(0, 4);

				if (identifier == Constants.USIncomingMessageRequest.F101)
				{
					code = matchValue.Substring(4, 5);
					desc = matchValue.Substring(9, 24).Trim();

					var transportMode = matchValue.Substring(65, 1);
					AddTransportMode(transportModeList, transportMode);

					string address1 = matchValue.Substring(33, 32).Trim();
					string unlading = matchValue.Substring(74, 1).Trim();
					AddAttribute(attributeList, Constants.AttributeNames.Address1, address1);
					if (unlading == Constants.RegionDistrictPortCode.return1)
					{
						AddAttribute(attributeList, Constants.AttributeNames.Unlading, Constants.AttributeValues.Y);
					}
					AddAttribute(attributeList, Constants.AttributeNames.ROLE, Constants.AttributeValues.EXP);
				}
				else if (identifier == Constants.USIncomingMessageRequest.F201)
				{
					var f201Code = matchValue.Substring(4, 5);
					if (f201Code == code)
					{
						string address2 = matchValue.Substring(9, 32).Trim();
						string address3 = matchValue.Substring(41, 32).Trim();
						AddAttribute(attributeList, Constants.AttributeNames.Address2, address2);
						AddAttribute(attributeList, Constants.AttributeNames.Address3, address3);
					}
					else
					{
						throw new InvalidOperationException($"The Region/District/Port Code {f201Code} in F201 does not match the code {code} in F101. The message structure may be changed.");
					}
				}
				else if (identifier == Constants.USIncomingMessageRequest.F301)
				{
					var f301Code = matchValue.Substring(4, 5);
					if (f301Code == code)
					{
						string city = matchValue.Substring(9, 15).Trim();
						string state = matchValue.Substring(24, 2).Trim();
						string postCode = matchValue.Substring(26, 9).Trim();
						AddAttribute(attributeList, Constants.AttributeNames.City, city);
						AddAttribute(attributeList, Constants.AttributeNames.State, state);
						AddAttribute(attributeList, Constants.AttributeNames.PostCode, postCode);

						refCusCodeLists.Add(GenerateCusCode(code, desc, transportModeList, attributeList));

						attributeList.Clear();
						transportModeList.Clear();
					}
					else
					{
						throw new InvalidOperationException($"The Region/District/Port Code {f301Code} in F301 does not match the code {code} in F101. The message structure may be changed.");
					}
				}
			}
			SaveXML(refCusCodeLists, DateTime.UtcNow.Date, () => XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.CodeType.CUSOF, enableTransport: true));
		}

		static RefCusCodeList GenerateCusCode(string code, string description, List<RefCusCodeOrAttributeTransportMode> refCusCodeOrAttributeTransportModeList, List<RefCusCodeListAttribute> refCusCodeListAttributeList)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = code.Substring(1, 4),
				ZZD_Description = description
			};

			refCusCodeList.RefCusCodeOrAttributeTransportModes = refCusCodeOrAttributeTransportModeList.ToArray();
			refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributeList.ToArray();
			return refCusCodeList;
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

		static void AddTransportMode(List<RefCusCodeOrAttributeTransportMode> transportModeList, string transportMode)
		{
			var modes = new List<string>
			{
				Constants.TransportMode.AIR,
				Constants.TransportMode.FIX,
				Constants.TransportMode.INW,
				Constants.TransportMode.MAI,
				Constants.TransportMode.RAI,
				Constants.TransportMode.ROA,
				Constants.TransportMode.SEA,
			};

			switch (transportMode)
			{
				case Constants.TransportMode.AllModesValid:
					break;
				case Constants.TransportMode.ExceptionOfAir:
					modes.Remove(Constants.TransportMode.AIR);
					break;
				case Constants.TransportMode.ExceptionOfVessel:
				case Constants.TransportMode.SpaceFill:
					modes.Remove(Constants.TransportMode.SEA);
					break;
				default:
					throw new InvalidOperationException($"Transport mode {transportMode} does not match. The new transport mode code is introduced.");
			}

			foreach (var mode in modes)
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode { ZZU_TransportMode = mode });
			}
		}
	}
}
