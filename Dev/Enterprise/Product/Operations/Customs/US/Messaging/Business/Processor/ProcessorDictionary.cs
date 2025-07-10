using System;
using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class ProcessorDictionary
	{
		public ProcessorDictionary()
		{
			applicationCodeDictionaryWithInnerDictionary = new Dictionary<string, Dictionary<string, Dictionary<Type, Type>>>();
			applicationCodeDictionaryWithAppCode = new Dictionary<string, Dictionary<string, Type>>();
		}

		public void Add(string applicationCode, string applicationIdentifier, Type processorType)
		{
			GetDictionaryWithAppCodeFor(applicationCode).Add(applicationIdentifier, processorType);
		}

		public bool TryGetValue(string applicationCode, string applicationIdentifier, out Type processorType)
		{
			return GetDictionaryWithAppCodeFor(applicationCode).TryGetValue(applicationIdentifier, out processorType);
		}

		public void Add(string applicationCode, string applicationIdentifier, Type messageBlockType, Type processorType)
		{
			var applicationCodeDictionaryWithInnerDictionary = GetDictionaryWithInnerDictionaryFor(applicationCode);
			Dictionary<Type, Type> typeDictionary;
			if (!applicationCodeDictionaryWithInnerDictionary.TryGetValue(applicationIdentifier, out typeDictionary))
			{
				typeDictionary = new Dictionary<Type, Type>();
				applicationCodeDictionaryWithInnerDictionary.Add(applicationIdentifier, typeDictionary);
			}
			typeDictionary.Add(messageBlockType, processorType);
		}

		public bool TryGetValue(string applicationCode, string applicationIdentifier, Type messageBlockType, out Type processorType)
		{
			bool result = false;
			processorType = null;
			var applicationCodeDictionaryWithInnerDictionary = GetDictionaryWithInnerDictionaryFor(applicationCode);
			var aCEApplicationCodeDictionaryWithInnerDictionary = applicationCode == CBPEDIInterchange.ApplicationCodes.USCustomsImport ? GetDictionaryWithInnerDictionaryFor(MessageBlockDictionary.ACEApplicationCode) : null;
			Dictionary<Type, Type> innerDictionary;
			if (applicationCodeDictionaryWithInnerDictionary.TryGetValue(applicationIdentifier, out innerDictionary))
			{
				result = innerDictionary.TryGetValue(messageBlockType, out processorType);
			}
			else if (aCEApplicationCodeDictionaryWithInnerDictionary != null && aCEApplicationCodeDictionaryWithInnerDictionary.TryGetValue(applicationIdentifier, out innerDictionary))
			{
				result = innerDictionary.TryGetValue(messageBlockType, out processorType);
			}

			if (!result)
			{
				if (applicationCodeDictionaryWithInnerDictionary.TryGetValue(ApplicationIdentifierCodeList.Codes.AllMessages, out innerDictionary))
				{
					result = innerDictionary.TryGetValue(messageBlockType, out processorType);
				}
				if (!result && aCEApplicationCodeDictionaryWithInnerDictionary != null && aCEApplicationCodeDictionaryWithInnerDictionary.TryGetValue(ApplicationIdentifierCodeList.Codes.AllMessages, out innerDictionary))
				{
					result = innerDictionary.TryGetValue(messageBlockType, out processorType);
				}
			}
			return result;
		}

		Dictionary<string, Dictionary<Type, Type>> GetDictionaryWithInnerDictionaryFor(string applicationCode)
		{
			Dictionary<string, Dictionary<Type, Type>> result;
			if (!applicationCodeDictionaryWithInnerDictionary.TryGetValue(applicationCode, out result))
			{
				result = new Dictionary<string, Dictionary<Type, Type>>();
				applicationCodeDictionaryWithInnerDictionary.Add(applicationCode, result);
			}
			return result;
		}

		Dictionary<string, Type> GetDictionaryWithAppCodeFor(string applicationCode)
		{
			Dictionary<string, Type> result;
			if (!applicationCodeDictionaryWithAppCode.TryGetValue(applicationCode, out result))
			{
				result = new Dictionary<string, Type>();
				applicationCodeDictionaryWithAppCode.Add(applicationCode, result);
			}
			return result;
		}

		readonly Dictionary<string, Dictionary<string, Dictionary<Type, Type>>> applicationCodeDictionaryWithInnerDictionary;
		readonly Dictionary<string, Dictionary<string, Type>> applicationCodeDictionaryWithAppCode;
	}
}
