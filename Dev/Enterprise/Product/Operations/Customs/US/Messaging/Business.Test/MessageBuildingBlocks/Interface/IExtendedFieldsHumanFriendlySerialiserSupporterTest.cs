using System;
using System.Linq;
using System.Text;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class IExtendedFieldsHumanFriendlySerialiserSupporterTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestExtendedFieldToSerialiseShouldNotBeTrimed()
		{
			var failures = new StringBuilder();
			var retriever = new SubClassRetriever(GetType().Assembly, typeof(IExtendedFieldsHumanFriendlySerialiserSupporter));
			retriever.IncludeAbstractClasses = false;
			foreach (var type in retriever.Retrieve())
			{
				var block = (MessageBlock)Activator.CreateInstance(type);
				var serialiserSupporter = block as IExtendedFieldsHumanFriendlySerialiserSupporter;
				if (serialiserSupporter != null)
				{
					var extendedField = serialiserSupporter.ExtendedFieldNameToSerialise;
					if (!extendedField.IsEmpty)
					{
						var fieldWithAttribute = block.GetAttributeFieldInfos().FirstOrDefault(x => x.FieldInfo.Name == extendedField);
						if (fieldWithAttribute != null)
						{
							var messageBlockStringAttribute = fieldWithAttribute.Attribute as MessageBlockStringAttribute;
							if (messageBlockStringAttribute != null && messageBlockStringAttribute.ShouldTrimBegining)
							{
								failures.AppendLine("ShouldTrimBegining should be set to false in " + type.FullName + " -> " + extendedField + " field.");
							}
						}
					}
				}
			}

			if (failures.Length > 0)
			{
				Fail(failures.ToString());
			}
			else
			{
				Assert("All good.", true);
			}
		}
	}
}
