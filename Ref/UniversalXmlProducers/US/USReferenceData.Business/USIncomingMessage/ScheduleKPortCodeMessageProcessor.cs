using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	public class ScheduleKPortCodeMessageProcessor : USIncomingMessageProcessor
	{
		IReadOnlyList<ForeignPort> foreignPorts;

		public ScheduleKPortCodeMessageProcessor(string outputPath, IReadOnlyList<ForeignPort> foreignPorts) : base(outputPath)
		{
			this.foreignPorts = foreignPorts;
		}

		public override string MetaDataPattern => @"F104\d{5}.{70} ";

		public override string OutputFileName => @"USForeignPorts.xml";

		public override string XMLWriterDataSource => "US Foreign Ports";

		public override UpdateType UpdateType => UpdateType.Full;

		protected override void ProcessCore(MatchCollection matchMetaDatas)
		{
			var foreignPortDic = foreignPorts.ToDictionary(port => port.Code, port => port);
			var refCusCodeLists = new List<RefCusCodeList>();
			foreach (Match match in matchMetaDatas)
			{
				var matchValue = match.Value.Trim();
				var code = matchValue.Substring(4, 5);
				refCusCodeLists.Add(GenerateCusCode(code, matchValue.Substring(9), attributeList =>
				{
					if (foreignPortDic.TryGetValue(code, out var foreignPort))
					{
						AddAESOrInBondAttribute(attributeList, foreignPort.Type);
						foreignPortDic.Remove(code);
					}
					else
					{
						AddAttribute(attributeList, Constants.AttributeValues.Common);
					}
				}));
			}

			foreach (var foreignPort in foreignPortDic)
			{
				refCusCodeLists.Add(GenerateCusCode(foreignPort.Key, foreignPort.Value.Description, attributeList => AddAESOrInBondAttribute(attributeList, foreignPort.Value.Type)));
			}
			SaveXML(refCusCodeLists, DateTime.Now.Date, () => XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.CodeType.PORT));
		}

		static RefCusCodeList GenerateCusCode(string code, string description, Action<List<RefCusCodeListAttribute>> attributesPopulator)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = code,
				ZZD_Description = description
			};
			var attributeList = new List<RefCusCodeListAttribute>();
			attributesPopulator(attributeList);
			refCusCodeList.RefCusCodeListAttributes = attributeList.ToArray();
			return refCusCodeList;
		}

		static void AddAttribute(List<RefCusCodeListAttribute> attributeList, string type)
		{
			var attribute = new RefCusCodeListAttribute()
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.PortValidType,
				ZZE_Value = type
			};
			attributeList.Add(attribute);
		}

		static void AddAESOrInBondAttribute(List<RefCusCodeListAttribute> attributeList, string type)
		{
			if (type == Constants.AttributeValues.AES)
			{
				AddAttribute(attributeList, Constants.AttributeValues.AES);
			}
			else
			{
				AddAttribute(attributeList, Constants.AttributeValues.InBond);
				AddAttribute(attributeList, Constants.AttributeValues.Common);
			}
		}
	}
}
