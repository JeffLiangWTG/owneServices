using System;
using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business
{
	class MessageBlockDictionaryGenerator
	{
		public IReadOnlyList<MessageBlockDictionary> GenerateBlockDictionaries(Type attribute)
		{
			var dictionaryArray = new MessageBlockDictionary[10];
			for (var i = 1; i < dictionaryArray.Length; i++)
			{
				dictionaryArray[i] = new MessageBlockDictionary(i);
			}

			foreach (MessageBlockTypeProvider typeProvider in GetMessageBlockTypes(attribute))
			{
				MessageBlockDictionary dictionary = dictionaryArray[typeProvider.MandatoryCharacters.Length];
				AddType(dictionary, typeProvider);
			}

			var dictionaries = new List<MessageBlockDictionary>();

			for (int i = dictionaryArray.Length - 1; i > 0; i--)
			{
				MessageBlockDictionary dictionary = dictionaryArray[i];
				if (dictionary.HasElements)
				{
					dictionaries.Add(dictionary);
				}
			}
			return dictionaries;
		}

		#region Implementation

#if DEBUG
		protected virtual
#endif
 void AddType(MessageBlockDictionary dictionary, MessageBlockTypeProvider typeProvider)
		{
			dictionary.AddType(typeProvider);
		}

#if DEBUG
		protected virtual
#endif
 IEnumerable<MessageBlockTypeProvider> GetMessageBlockTypes(Type attributeType)
		{
			foreach (ABIMessageBlockTypeProviderAttribute attribute in AssemblyMetaDataReader.GetAttributes<ABIMessageBlockTypeProviderAttribute>())
			{
				foreach (Type type in attribute.Type.Assembly.GetTypes())
				{
					if (typeof(MessageBlock).IsAssignableFrom(type) && !type.IsAbstract)
					{
						string version;
						string mandatoryCharacters = GetMandatoryCharacters(type, attributeType, out version);
						if (mandatoryCharacters.Length > 0)
						{
							foreach (ApplicationIdentifierAttribute applicationIdentifierAttribute in GetApplicationIdentifiers(type))
							{
								yield return new MessageBlockTypeProvider(type, applicationIdentifierAttribute.ApplicationCode, applicationIdentifierAttribute.ApplicationIdentifier, mandatoryCharacters, version);
							}
						}
					}
				}
			}
		}

		IEnumerable<ApplicationIdentifierAttribute> GetApplicationIdentifiers(Type type)
		{
			List<ApplicationIdentifierAttribute> result = new List<ApplicationIdentifierAttribute>();

			AddApplicationIdentifierAttributes(result, type.GetCustomAttributes(typeof(ApplicationIdentifierAttribute), true));
			var baseType = type.BaseType;
			var messageBlockType = typeof(MessageBlock);
			var objectType = typeof(System.Object);
			while (baseType != messageBlockType && baseType != objectType) // get attributes from base type
			{
				AddApplicationIdentifierAttributes(result, baseType.GetCustomAttributes(typeof(ApplicationIdentifierAttribute), false));
				baseType = baseType.BaseType;
			}
			if (result.Count == 0)
			{
				result.Add(new ApplicationIdentifierAttribute(MessageBlockDictionary.EmptyApplicationIdentifier));
			}
			return result;
		}

		void AddApplicationIdentifierAttributes(List<ApplicationIdentifierAttribute> result, object[] applicationIdentifierAttributes)
		{
			foreach (ApplicationIdentifierAttribute attr in applicationIdentifierAttributes)
			{
				if (!result.Contains(attr))
				{
					result.Add(attr);
				}
			}
		}

		string GetMandatoryCharacters(Type type, Type attributeType, out string version)
		{
			string result = "";
			version = "";
			object[] outputBlockAttributes = type.GetCustomAttributes(attributeType, true);
			if (outputBlockAttributes.Length > 0)
			{
				BlockAttribute outputBlockAttribute = (BlockAttribute)outputBlockAttributes[0];
				result = outputBlockAttribute.MandatoryCharacters;
				version = outputBlockAttribute.Version;
			}
			return result;
		}

		#endregion
	}
}
