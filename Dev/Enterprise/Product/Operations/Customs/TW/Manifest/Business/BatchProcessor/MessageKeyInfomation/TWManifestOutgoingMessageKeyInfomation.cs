using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.MessageProcessors;

namespace Enterprise.Customs.TW.Manifest.MessageProcessors
{
	class TWManifestOutgoingMessageKeyInfomation : MessageKeyInfomation
	{
		public TWManifestOutgoingMessageKeyInfomation(string type, ZString xml) : base(type, xml)
		{
		}

		protected override void InitData()
		{
		}

		protected override ImmutableDictionary<string, Action<ZString>> CreateDataGeneratorMap()
		{
			return ImmutableDictionary.CreateRange(new Dictionary<string, Action<ZString>>
				{
					{ MessageTypeList.Codes.FHM, CreateByN5101H }
				});
		}

		void CreateByN5101H(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5101H.Declaration>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
			}
		}
	}
}
