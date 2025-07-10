using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	[Serializable]
	class MessageBlockSerialisationException : Exception
	{
		public MessageBlockSerialisationException(string message, string newInvalidFormat)
			: base(message)
		{
			NewInvalidFormat = newInvalidFormat;
		}

#if NETFRAMEWORK
		protected MessageBlockSerialisationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string NewInvalidFormat;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Instantiated through reflection")]
	public abstract class MessageBlock
	{
		protected MessageBlock(string mandatoryCharacters)
		{
			MandatoryCharacters = mandatoryCharacters;
		}

		public readonly string MandatoryCharacters;

		public void Deserialise(string eightyCharacterBlock)
		{
			if (eightyCharacterBlock == null)
			{
				throw new ArgumentNullException(nameof(eightyCharacterBlock));
			}

			if (eightyCharacterBlock.Length != 80)
			{
				throw new ArgumentException("string was not 80 bytes", nameof(eightyCharacterBlock));
			}

			if (!eightyCharacterBlock.StartsWith(MandatoryCharacters))
			{
				throw new InvalidMessageFormatException("Deserialised data does not match mandatory values for ControlIdentifier."
					+ System.Environment.NewLine
					+ "Expected value : " + MandatoryCharacters + System.Environment.NewLine
					+ "Actual value : " + eightyCharacterBlock.Substring(0, MandatoryCharacters.Length));
			}

			foreach (AttributeFieldInfo attributeFieldInfo in GetAttributeFieldInfos())
			{
				var fieldName = attributeFieldInfo.FieldInfo.Name;

				var value = DeSerialiseValueFromAttibute(attributeFieldInfo.Attribute, fieldName, eightyCharacterBlock);
				attributeFieldInfo.FieldInfo.SetValue(this, value);
			}
			AdjustEndDates();
		}

		IZType DeSerialiseValueFromAttibute(MessageBlockAttribute attribute, ZString fieldName, ZString eightyCharacterBlock)
		{
			try
			{
				return attribute.DeSerialise(eightyCharacterBlock);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
			{
				var fieldFullName = GetType().FullName + "." + fieldName;
				var blockName = GetType().Name;
				var rawValue = attribute.GetRawData(eightyCharacterBlock);
				var offset = attribute.Offset;
				var length = attribute.Length;

				var innerMessage = ZString.Format("Deserialising to {0}\r\nOffset={1}, Length={2}, Value='{3}'\r\nBlockData='{4}'", fieldFullName, offset, length, rawValue, eightyCharacterBlock);
				var innerException = new InvalidMessageFormatException(innerMessage, ex);

				var message = ZString.Format("Data Mismatch in Message.\r\n\r\nError reading {0} from block {1}.\r\n{2}", fieldName, blockName, ex.Message);
				throw new InvalidMessageFormatException(message, innerException);
			}
		}

		protected virtual void AdjustEndDates()
		{
		}

		protected ZDate AdjustedEndDate(ZDate beginDate, ZDate endDate)
		{
			if (endDate.IsValid && beginDate.IsValid && endDate < beginDate)
			{
				var end20YY = 2000 + Convert.ToInt32(((ZString)endDate.Year.ToString()).Right(2));
				if (end20YY > System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.TwoDigitYearMax)
				{
					return new ZDate(end20YY, endDate.Month, endDate.Day);
				}
			}
			return endDate;
		}

		/// <summary>
		/// Has no data? It serialises and check if there is any other data than its mandatory character.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				foreach (AttributeFieldInfo info in GetAttributeFieldInfos())
				{
					IZType fieldValue = (IZType)info.FieldInfo.GetValue(this);
					if (!fieldValue.IsEmpty)
					{
						return false;
					}
				}
				return true;
			}
		}

		public ISerialisedValue[] GetSerialisedValues()
		{
			List<SerialisedValues> values = new List<SerialisedValues>();
			string blockName = GetType().Name;
			foreach (AttributeFieldInfo info in GetAttributeFieldInfos())
			{
				SerialisedValues value = new SerialisedValues();
				values.Add(value);

				IZType fieldValue = (IZType)info.FieldInfo.GetValue(this);
				string fieldInfoName = info.FieldInfo.Name;
				MessageBlockAttribute attribute = info.Attribute;
				string serialisedValue = null;

				try
				{
					try
					{
						serialisedValue = attribute.Serialise(this, fieldValue, true);
					}
					catch (MessageBlockSerialisationException serialisationException)
					{
						ErrorReporter.ReportOnce(blockName + fieldInfoName, "Error serialising " + blockName + "." + fieldInfoName + ", Value = " + fieldValue.ToString(), serialisationException);
						serialisedValue = serialisationException.NewInvalidFormat;
					}
					value.Value = serialisedValue;
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException("Error serialising " + blockName + "." + fieldInfoName + ", Value = " + fieldValue, ex);
				}

				value.Title = fieldInfoName;
			}

			return values.ToArray();
		}

		public string Serialise()
		{
			return Serialise(false);
		}

		public interface ISerialisedValue
		{
			string Title { get; }
			string Value { get; }
		}

		public class SerialisedValues : ISerialisedValue
		{
			public string Title
			{
				get { return title; }
				set
				{
					ZString fieldName = value;
					for (char c = (char)65; c <= 65 + 25; c++)
					{
						fieldName = fieldName.Replace(c.ToString(), " " + c);
					}
					fieldName = fieldName.Replace("of ", " of ");
					title = fieldName.TrimEnd();
				}
			}

			public string Value
			{
				get { return _value; }
				set { _value = value.Trim(); }
			}

			string _value;
			string title;
		}

		public string Serialise(bool humanFriendly)
		{
			var result = new StringBuilder();
			if (!humanFriendly)
			{
				result.Append(MandatoryCharacters);
			}
			var resultLength = MandatoryCharacters.Length;

			var values = new List<SerialisedValues>();
			var blockName = GetType().Name;
			var extendedFieldsHumanFriendlySerialiserSupporter = this as IExtendedFieldsHumanFriendlySerialiserSupporter;
			var extendedFieldNameToSerialise = extendedFieldsHumanFriendlySerialiserSupporter?.ExtendedFieldNameToSerialise ?? ZString.Empty;
			var extendedHumanFriendlyMappings = extendedFieldsHumanFriendlySerialiserSupporter?.GetExtendedFieldsMappings(extendedFieldNameToSerialise);

			foreach (AttributeFieldInfo info in GetAttributeFieldInfos())
			{
				var value = new SerialisedValues();

				var fieldValue = (IZType)info.FieldInfo.GetValue(this);
				var fieldInfoName = info.FieldInfo.Name;
				var attribute = info.Attribute;
				var serialisedValue = SerialiseValueFromAttribute(attribute, fieldInfoName, fieldValue, humanFriendly);
				value.Value = serialisedValue;

				var shouldBeMoreExplanatory = humanFriendly && serialisedValue.Trim().Length > 0;
				var fieldValueToAppend = shouldBeMoreExplanatory ? serialisedValue : serialisedValue.PadLeft
				(
					attribute.Offset > resultLength ? serialisedValue.Length + attribute.Offset - resultLength : serialisedValue.Length
				);

				resultLength += fieldValueToAppend.Length;
				if (humanFriendly)
				{
					if (extendedHumanFriendlyMappings != null && extendedHumanFriendlyMappings.ContainsKey(fieldInfoName))
					{
						values.AddRange(GetExtendedHumanFriendlySerialisedValues(extendedHumanFriendlyMappings, attribute, fieldInfoName, fieldValue));
					}
					else
					{
						values.Add(value);
						value.Title = fieldInfoName + " (" + (attribute.Offset + 1) + "-" + (attribute.Offset + attribute.Length) + ")";
					}
				}
				else
				{
					values.Add(value);
					result.Append(fieldValueToAppend);
				}
			}

			if (humanFriendly)
			{
				var reportSegment = true;
				var maxVisibleCharacters = 0;
				foreach (SerialisedValues value in values)
				{
					if (value.Value.Trim().Length > 0)
					{
						reportSegment = true;
						maxVisibleCharacters = Math.Max(value.Title.Length, maxVisibleCharacters);
					}
				}

				if (reportSegment)
				{
					var title = blockName;
					title = title.PadLeft(title.Length + ((40 - title.Length) / 2), '-').PadRight(40, '-');
					result.AppendLine(title);
					foreach (SerialisedValues value in values)
					{
						if (value.Value.Trim().Length > 0)
						{
							result.AppendLine(value.Title.PadRight(maxVisibleCharacters) + " :" + value.Value);
						}
					}
					result.AppendLine();
				}
			}
			var finalResult = humanFriendly ? result.ToString() : result.ToString().PadRight(80);
			if (!humanFriendly && finalResult.Length > 80 && !Globals.IsTest)
			{
				var exceptionMessageBuilder = new ZStringBuilder();
				exceptionMessageBuilder.AppendLine("Message Block Content: " + finalResult);
				exceptionMessageBuilder.AppendLine("Message Block Type: " + this.GetType().FullName);
				exceptionMessageBuilder.AppendLine("Result Length: " + resultLength);
				foreach (AttributeFieldInfo info in GetAttributeFieldInfos())
				{
					var fieldValue = (IZType)info.FieldInfo.GetValue(this);
					var fieldInfoName = info.FieldInfo.Name;
					var attribute = info.Attribute;
					exceptionMessageBuilder.AppendLine("Field:" + fieldInfoName + ", Value:" + fieldValue.ToString() + ", Attribute:" + attribute.ToString());
				}

				throw new DeveloperNotificationException("Message block content is longer than 80 characters. Please refer to the inner exception for more details.", new InvalidMessageFormatException(exceptionMessageBuilder.ToString()));
			}
			return finalResult;
		}

		public IEnumerable<SerialisedValues> GetExtendedHumanFriendlySerialisedValues(ZString fieldInfoName)
		{
			var blockType = GetType();
			var fieldInfo = blockType.GetFields().FirstOrDefault(f => f.Name == fieldInfoName);
			var attribute = fieldInfo?.GetCustomAttributes(typeof(MessageBlockAttribute), true).FirstOrDefault();

			var extendedFieldsHumanFriendlySerialiserSupporter = this as IExtendedFieldsHumanFriendlySerialiserSupporter;
			var extendedFieldNameToSerialise = extendedFieldsHumanFriendlySerialiserSupporter?.ExtendedFieldNameToSerialise ?? ZString.Empty;
			var extendedHumanFriendlyMappings = extendedFieldsHumanFriendlySerialiserSupporter?.GetExtendedFieldsMappings(extendedFieldNameToSerialise);

			return attribute != null && extendedHumanFriendlyMappings != null && extendedHumanFriendlyMappings.ContainsKey(fieldInfoName)
				? GetExtendedHumanFriendlySerialisedValues(extendedHumanFriendlyMappings, (MessageBlockAttribute)attribute, fieldInfoName, (IZType)fieldInfo.GetValue(this))
				: null;
		}

		IEnumerable<SerialisedValues> GetExtendedHumanFriendlySerialisedValues(Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>> mappings, MessageBlockAttribute attribute, ZString fieldInfoName, IZType fieldValue)
		{
			var result = new List<SerialisedValues>();
			if (mappings != null)
			{
				foreach (var mapping in mappings[fieldInfoName])
				{
					var extendedSerialisedValues = new SerialisedValues();
					var extentedValue = DeSerialiseValueFromAttibute(mapping.Value, mapping.Key, fieldValue.ToString().PadRight(attribute.Length).PadLeft(attribute.Offset + attribute.Length));
					extendedSerialisedValues.Value = SerialiseValueFromAttribute(mapping.Value, mapping.Key, extentedValue, true);
					extendedSerialisedValues.Title = mapping.Key + " (" + (mapping.Value.Offset + 1) + "-" + (mapping.Value.Offset + mapping.Value.Length) + ")";
					result.Add(extendedSerialisedValues);
				}
			}

			return result;
		}

		ZString SerialiseValueFromAttribute(MessageBlockAttribute attribute, ZString fieldInfoName, IZType fieldValue, bool humanFriendly)
		{
			var blockName = GetType().Name;
			try
			{
				ZString serialisedValue;
				try
				{
					serialisedValue = attribute.Serialise(this, fieldValue, humanFriendly);
				}
				catch (MessageBlockSerialisationException serialisationException)
				{
					ErrorReporter.ReportOnce(blockName + fieldInfoName, "Error serialising " + blockName + "." + fieldInfoName + ", Value = " + fieldValue.ToString(), serialisationException);
					serialisedValue = serialisationException.NewInvalidFormat;
				}

				return serialisedValue;
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException("Error serialising " + blockName + "." + fieldInfoName + ", Value = " + fieldValue.ToString(), ex);
			}
		}

		public class AttributeFieldInfo
		{
			public AttributeFieldInfo(MessageBlockAttribute attribute, FieldInfo fieldInfo)
			{
				Attribute = attribute;
				FieldInfo = fieldInfo;
			}
			public readonly MessageBlockAttribute Attribute;
			public readonly FieldInfo FieldInfo;
		}

		static Dictionary<Type, List<AttributeFieldInfo>> Dictionary
		{
			get { return dictionary ?? (dictionary = new Dictionary<Type, List<AttributeFieldInfo>>()); }
		}

		[ThreadStatic]
		static Dictionary<Type, List<AttributeFieldInfo>> dictionary;

		static int CompareAttributeFieldInfo(AttributeFieldInfo x, AttributeFieldInfo y)
		{
			if (x == null)
			{
				return (y == null) ? 0 : -1;
			}
			else
			{
				return (y == null) ? 1 : x.Attribute.Offset.CompareTo(y.Attribute.Offset);
			}
		}

		public IEnumerable<AttributeFieldInfo> GetAttributeFieldInfos()
		{
			List<AttributeFieldInfo> result;
			Type blockType = GetType();
			if (!Dictionary.TryGetValue(blockType, out result))
			{
				result = new List<AttributeFieldInfo>();
				foreach (FieldInfo fieldInfo in blockType.GetFields())
				{
					MessageBlockAttribute[] attributes = (MessageBlockAttribute[])fieldInfo.GetCustomAttributes(typeof(MessageBlockAttribute), true);
					if (attributes.Length > 1)
					{
						throw new Exception(blockType.FullName + "." + fieldInfo.Name + " has too many 'MessageBlockAttribute'; it should have one.");
					}
					else if (attributes.Length == 1)
					{
						result.Add(new AttributeFieldInfo(attributes[0], fieldInfo));
					}
				}
				result.Sort(CompareAttributeFieldInfo);
				Dictionary.Add(blockType, result);
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			bool result = false;
			if (obj != null && obj.GetType() == GetType())
			{
				result = Serialise() == ((MessageBlock)obj).Serialise();
			}
			return result;
		}

		public override int GetHashCode()
		{
			return Serialise().GetHashCode();
		}
	}
}
