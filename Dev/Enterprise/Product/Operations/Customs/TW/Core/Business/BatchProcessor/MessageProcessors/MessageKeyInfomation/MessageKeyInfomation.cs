using System;
using System.Collections.Immutable;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public abstract class MessageKeyInfomation
	{
		public MessageKeyInfomation(string type, ZString xml)
		{
			MessageType = type;
			InitData();
			if (DataGeneratorMap.ContainsKey(MessageType))
			{
				DataGeneratorMap[MessageType].Invoke(xml);
			}
		}

		ImmutableDictionary<string, Action<ZString>> DataGeneratorMap
		{
			get
			{
				if (dataGeneratorMap == null)
				{
					dataGeneratorMap = CreateDataGeneratorMap();
				}
				return dataGeneratorMap;
			}
		}
		ImmutableDictionary<string, Action<ZString>> dataGeneratorMap;

		protected abstract ImmutableDictionary<string, Action<ZString>> CreateDataGeneratorMap();

		protected abstract void InitData();

		public ZString ErrorText { get; set; }

		public object Result
		{
			get;
			set;
		}

		public ZString MessageType { get; }

		protected class SafeXmlReader<T> where T : class
		{
			public SafeXmlReader(MessageKeyInfomation infomation)
			{
				this.infomation = infomation;
			}

			readonly MessageKeyInfomation infomation;

			public T Result
			{
				get;
				private set;
			}

			public bool TryReadFromXML(string xmlText)
			{
				Result = null;
				var error = ZString.Empty;
				var serializer = ZXmlSerializer.New(typeof(T));
				var settings = new XmlReaderSettings();
				try
				{
					using (var input = new StringReader(xmlText))
					{
						using (var reader = XmlReader.Create(input, settings))
						{
							var events = new XmlDeserializationEvents();
							Result = (T)serializer.Deserialize(reader, events);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					error = ex.Message;
				}

				infomation.ErrorText = error;
				return error.IsEmpty;
			}
		}
	}
}
