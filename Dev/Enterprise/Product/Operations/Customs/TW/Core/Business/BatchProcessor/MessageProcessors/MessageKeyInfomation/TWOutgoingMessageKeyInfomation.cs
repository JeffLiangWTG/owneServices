using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	class TWOutgoingMessageKeyInfomation : MessageKeyInfomation
	{
		public TWOutgoingMessageKeyInfomation(string type, ZString xml) : base(type, xml)
		{
		}

		protected override void InitData()
		{
		}

		protected override ImmutableDictionary<string, Action<ZString>> CreateDataGeneratorMap()
		{
			return ImmutableDictionary.CreateRange(new Dictionary<string, Action<ZString>>
					{
						{ MessageTypeList.Codes.ECD, CreateByN5203 },
						{ MessageTypeList.Codes.ICD, CreateByNX5105 },
						{ MessageTypeList.Codes.CAA, CreateByNX5105 }
					});
		}

		void CreateByN5203(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5203.Declaration>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
			}
		}

		void CreateByNX5105(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX5105.Declaration>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
			}
		}
	}
}
