using System;
using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class MessageBlockDictionary
	{
		public const string EmptyApplicationIdentifier = "  ";

		/// <summary>
		/// A, B, Y and Z blocks are slightly different between ACS and ACE and 
		/// </summary>
		public const string ACEApplicationCode = "ACE";

		public MessageBlockDictionary(int mandatoryCharactersLength)
		{
			dictionary = new Dictionary<string, Type>();
			explicitDictionary = new Dictionary<string, bool>();
			MandatoryCharactersLength = mandatoryCharactersLength;
		}

		public void CheckForDuplicate(MessageBlockTypeProvider messageBlockTypeProvider)
		{
			// Check for crossover duplicates
			bool containsKey = explicitDictionary.ContainsKey(GetExplicitKey(messageBlockTypeProvider));
			if (containsKey)
			{
				bool isDefinedExplicit = explicitDictionary[GetExplicitKey(messageBlockTypeProvider)];
				if (messageBlockTypeProvider.ApplicationIdentifier == EmptyApplicationIdentifier)   // Check for non explicit definition
				{
					if (isDefinedExplicit)
					{
						throw new Exception("Must define ApplicationIdentifier attribute on : " + messageBlockTypeProvider.MessageBlockType.FullName);
					}
				}
				else
				{
					if (!isDefinedExplicit)
					{
						Type messageBlockType;
						TryGetValue(messageBlockTypeProvider.ApplicationCode, EmptyApplicationIdentifier, messageBlockTypeProvider.MandatoryCharacters, messageBlockTypeProvider.Version, out messageBlockType);
						throw new Exception("Must define ApplicationIdentifier attribute on : " + messageBlockType.FullName);
					}
				}
			}

			Type duplicateMessageBlockType;
			if (TryGetValue(messageBlockTypeProvider.ApplicationCode, messageBlockTypeProvider.ApplicationIdentifier, messageBlockTypeProvider.MandatoryCharacters, messageBlockTypeProvider.Version, out duplicateMessageBlockType))
			{
				throw new Exception("Duplicate types found\r\nPlace an ApplicationIdentifier attribute on BOTH types\r\nType 1 : " + duplicateMessageBlockType.FullName + System.Environment.NewLine + "Type 2 : " + messageBlockTypeProvider.MessageBlockType.FullName);
			}
			// Check for complete duplicate
		}

		string GetExplicitKey(MessageBlockTypeProvider messageBlockTypeProvider)
		{
			return messageBlockTypeProvider.ApplicationCode + messageBlockTypeProvider.MandatoryCharacters + messageBlockTypeProvider.Version;
		}

		readonly Dictionary<string, bool> explicitDictionary;
		readonly Dictionary<string, Type> dictionary;

		public bool HasElements
		{
			get { return dictionary.Count > 0; }
		}

		public void AddType(MessageBlockTypeProvider messageBlockTypeProvider)
		{
			if (messageBlockTypeProvider.MandatoryCharacters.Length != MandatoryCharactersLength)
			{
				throw new ArgumentException("Invalid MandatoryCharacters length", paramName: nameof(messageBlockTypeProvider));
			}

			CheckForDuplicate(messageBlockTypeProvider);
			dictionary.Add(messageBlockTypeProvider.FullKey, messageBlockTypeProvider.MessageBlockType);
			if (!explicitDictionary.ContainsKey(GetExplicitKey(messageBlockTypeProvider)))
			{
				explicitDictionary.Add(GetExplicitKey(messageBlockTypeProvider), messageBlockTypeProvider.ApplicationIdentifier != EmptyApplicationIdentifier);
			}
		}

		public readonly int MandatoryCharactersLength;

		internal bool TryGetValue(string applicationCode, string applicationIdentifier, string mandatoryCharacters, string version, out Type messageBlockType)
		{
			ApplicationIdentifierCodeList.EnsureIsValidFormat(applicationIdentifier);
			return dictionary.TryGetValue(applicationCode + applicationIdentifier + mandatoryCharacters + version, out messageBlockType);
		}
	}
}
