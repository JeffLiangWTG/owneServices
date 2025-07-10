using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using static System.FormattableString;
using static Enterprise.Messaging.Business.AWB.ValueElement;
using ValueType = Enterprise.Messaging.Business.AWB.ValueType;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB
{
	public abstract class AWBMessageBlock
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected AWBMessageBlock()
		{
			SetupSpecialFields();
			if (!GetFieldInfos().Any())
			{
				throw new InvalidAWBMessageBlockStructureException(Invariant($"{GetType().FullName} does not have any field Info."));
			}
		}

		public (bool success, ZString leftoverLineData) Deserialise(ZString data)
		{
			var lineDatas = new List<ZString>();
			var noOfCRLF = SpecialFieldInfos.Count(x => x.Value == SpecialChars.CRLF);
			var start = 0;
			for (var i = 0; i < noOfCRLF; i++)
			{
				var crlfIndex = data.IndexOf(SpecialChars.CRLF, start, StringComparison.OrdinalIgnoreCase);
				if (crlfIndex > -1)
				{
					lineDatas.Add(data.SubstringSafe(start, crlfIndex - start));
					start = crlfIndex + 2;
				}
				else
				{
					throw new InvalidMessageFormatException(Invariant($"Data Mismatch in Message.\r\nError reading CRLF #{i + 1} from block {GetType().Name}."));
				}
			}

			var leftoverLineData = data.SubstringSafe(start);
			if (IsAddMultiLineDataEnabled)
			{
				start = 0;
				for (var i = 0; i < addMultiLineData.MaximumOccurrence; i++)
				{
					var crlfIndex = leftoverLineData.IndexOf(SpecialChars.CRLF, start, StringComparison.OrdinalIgnoreCase);
					if (crlfIndex > -1)
					{
						var lineData = leftoverLineData.SubstringSafe(start, crlfIndex - start);
						if (!lineData.StartsWith(SpecialChars.Slant, StringComparison.OrdinalIgnoreCase) || lineData.Length > addMultiLineData.LengthPerOccurence + 1)
						{
							break;
						}
						lineDatas.Add(lineData);
						start = crlfIndex + 2;
					}
					else
					{
						break;
					}
				}

				leftoverLineData = leftoverLineData.SubstringSafe(start);
			}

			var success = Deserialise(lineDatas);
			return (success, leftoverLineData);
		}

		bool Deserialise(List<ZString> lineDatas)
		{
			var success = true;
			var columnIsValid = true;
			var fieldInfos = GetFieldInfos().ToArray();
			var fieldInfosLength = fieldInfos.Length;
			var crlfIndex = 0;
			var lineData = lineDatas[crlfIndex];
			var start = 0;

			for (var index = 0; index < fieldInfosLength; index++)
			{
				var fieldInfo = fieldInfos[index];
				if (fieldInfo is SpecialFieldInfo mandatoryFieldInfo)
				{
					if (mandatoryFieldInfo.Value == SpecialChars.CRLF)
					{
						crlfIndex++;
						lineData = crlfIndex < lineDatas.Count ? lineDatas[crlfIndex] : ZString.Empty;
						start = 0;
					}
					else
					{
						columnIsValid = columnIsValid || mandatoryFieldInfo.ValueType == ValueType.ColumnIdentifier;
						if (columnIsValid)
						{
							var value = mandatoryFieldInfo.Value;
							var actualValue = lineData.SubstringSafe(start, value.Length);
							if (actualValue == value)
							{
								start += value.Length;
								continue;
							}
							else if (mandatoryFieldInfo.ValueType == ValueType.ColumnIdentifier && mandatoryFieldInfo.Status != StatusType.Mandatory)
							{
								columnIsValid = false;
							}
							else if (mandatoryFieldInfo.Status == StatusType.Mandatory)
							{
								throw new InvalidMessageFormatException(Invariant($"Data Mismatch in Message.\r\nError reading Line {crlfIndex + 1} Element {mandatoryFieldInfo.Position} from block {GetType().Name}\r\nExpected Value = '{mandatoryFieldInfo.Value}' Actual Value = '{actualValue}'"));
							}
						}
					}
				}
				else if (columnIsValid && fieldInfo is AttributeFieldInfo attributeFieldInfo)
				{
					var currentLineData = lineData.SubstringSafe(start, attributeFieldInfo.Attribute.MaxLength);
					var nextIndex = index + 1;
					var nextFieldInfo = nextIndex < fieldInfosLength ? fieldInfos[nextIndex] as SpecialFieldInfo : null;
					if (nextFieldInfo != null && nextFieldInfo.Value != SpecialChars.CRLF)
					{
						var nextFieldInfoIndex = currentLineData.IndexOf(nextFieldInfo.Value, StringComparison.OrdinalIgnoreCase);
						if (nextFieldInfoIndex > -1)
						{
							currentLineData = currentLineData.Left(nextFieldInfoIndex);
						}
					}

					try
					{
						IZType value = attributeFieldInfo.Attribute.DeSerialise(currentLineData);
						attributeFieldInfo.FieldInfo.SetValue(this, value);
						start += currentLineData.Length;
					}
					catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
					{
						var fieldName = attributeFieldInfo.FieldInfo.Name;
						var fieldFullName = GetType().FullName + "." + fieldName;
						int position = attributeFieldInfo.Attribute.Position;
						int length = attributeFieldInfo.Attribute.MaxLength;

						var innerException = new InvalidMessageFormatException(Invariant($"Deserialising to {fieldFullName}\r\nPosition={position}, Length={length}, Value='{currentLineData}'\r\nBlockData='{lineData}'"), ex);
						throw new InvalidMessageFormatException(Invariant($"Data Mismatch in Message.\r\n\r\nError reading {fieldName} from block {GetType().Name}.\r\n{ex.Message}"), innerException);
					}
				}
			}

			if (IsAddMultiLineDataEnabled)
			{
				var data = new ZStringBuilder();
				while (crlfIndex < lineDatas.Count)
				{
					data.Append(lineDatas[crlfIndex++].SubstringSafe(1));
				}

				AdditionalMultiLineDataValue = data.ToString();
			}

			return success;
		}

		/// <summary>
		/// Has no data? It serialises and check if there is any other data than its mandatory character.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				var infos = GetAttributeFieldInfos().ToArray();
				var result = !IsAddMultiLineDataEnabled || AdditionalMultiLineDataValue.IsEmpty;
				if (result)
				{
					foreach (var info in infos)
					{
						IZType fieldValue = (IZType)info.FieldInfo.GetValue(this);
						if (!fieldValue.IsEmpty)
						{
							result = false;
							break;
						}
					}
				}

				return result;
			}
		}

		class SerialisedValues
		{
			public string Title
			{
				get { return title; }
				set
				{
					title = value.TrimEnd();
				}
			}

			public string Value
			{
				get { return _value; }
				set { _value = value.TrimEnd(); }
			}

			string _value;
			string title;
		}

		public ElementList CreateElements()
		{
			string blockName = GetType().Name;
			var result = new ElementList();
			foreach (var fieldInfo in GetFieldInfos())
			{
				if (fieldInfo is SpecialFieldInfo mandatoryFieldInfo)
				{
					result.Add(new ValueElement(mandatoryFieldInfo.Status, new Format(mandatoryFieldInfo.Value.Length, mandatoryFieldInfo.CharType), mandatoryFieldInfo.Value, mandatoryFieldInfo.ValueType));
				}
				else if (fieldInfo is AttributeFieldInfo attributeFieldInfo)
				{
					string fieldInfoName = attributeFieldInfo.FieldInfo.Name;
					var fieldValue = (IZType)attributeFieldInfo.FieldInfo.GetValue(this);

					try
					{
						result.Add(attributeFieldInfo.Attribute.GetElement(fieldValue));
					}
					catch (MessageBlockSerialisationException serialisationException)
					{
						result.Add(new ValueElement(attributeFieldInfo.Attribute.Status, new Format(attributeFieldInfo.Attribute.MaxLength, CharType.Special), attributeFieldInfo.Attribute.InvalidValue, ValueType.Value));
						ErrorReporter.ReportOnce(blockName + fieldInfoName, Invariant($"Error serialising {blockName}.{fieldInfoName}, Value = {fieldValue.ToString()}"), serialisationException);
					}
				}
			}

			if (IsAddMultiLineDataEnabled && !addMultiLineData.Value.IsEmpty)
			{
				var lineDatas = addMultiLineData.Value.Split(addMultiLineData.LengthPerOccurence);
				for (var i = 0; i < addMultiLineData.MaximumOccurrence; i++)
				{
					var lineData = lineDatas[i];
					var additionalData = new ElementList();
					additionalData.AddSlant();
					additionalData.AddValue(new Format(addMultiLineData.LengthPerOccurence, addMultiLineData.CharType), lineData);
					additionalData.AddCRLF();
					result.AddHeader(additionalData);
				}
			}

			return result;
		}

		public string Serialise()
		{
			var result = new StringBuilder();
			int resultLength = 0;

			var values = new List<SerialisedValues>();
			var blockName = GetType().Name;
			foreach (var info in GetAttributeFieldInfos())
			{
				var value = new SerialisedValues();
				values.Add(value);

				var fieldValue = (IZType)info.FieldInfo.GetValue(this);
				var fieldInfoName = info.FieldInfo.Name;
				var attribute = info.Attribute;
				string serialisedValue = null;

				try
				{
					try
					{
						serialisedValue = attribute.Serialise(fieldValue);
					}
					catch (MessageBlockSerialisationException serialisationException)
					{
						ErrorReporter.ReportOnce(blockName + fieldInfoName, Invariant($"Error serialising {blockName}.{fieldInfoName}, Value = {fieldValue.ToString()}"), serialisationException);
						serialisedValue = serialisationException.NewInvalidFormat;
					}
					value.Value = serialisedValue;
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException(Invariant($"Error serialising {blockName}.{fieldInfoName}, Value = {fieldValue.ToString()}"), ex);
				}

				bool shouldBeMoreExplanatory = serialisedValue.Trim().Length > 0;
				string fieldValueToAppend = shouldBeMoreExplanatory ? serialisedValue : serialisedValue.PadLeft
					(
						attribute.Position > resultLength ? serialisedValue.Length + attribute.Position - resultLength : serialisedValue.Length
					);

				resultLength += fieldValueToAppend.Length;
				value.Title = fieldInfoName + Invariant($" ({attribute.MinLength}-{attribute.MaxLength}{GetCharType(attribute.CharType)})");
			}

			if (IsAddMultiLineDataEnabled)
			{
				var value = new SerialisedValues();
				var maxLength = addMultiLineData.LengthPerOccurence * addMultiLineData.MaximumOccurrence;
				value.Value = addMultiLineData.Value.Left(maxLength);
				value.Title = addMultiLineData.Fieldname + Invariant($" (1-{maxLength}{GetCharType(addMultiLineData.CharType)})");
				values.Add(value);
			}

			bool reportSegment = true;
			int maxVisibleCharacters = 0;
			foreach (var value in values)
			{
				if (value.Value.Trim().Length > 0)
				{
					reportSegment = true;
					maxVisibleCharacters = Math.Max(value.Title.Length, maxVisibleCharacters);
				}
			}

			if (reportSegment)
			{
				var paddingLength = Math.Max(40, maxVisibleCharacters / 2);
				ZString title = blockName;
				title = title.PadLeft(title.Length + ((paddingLength - title.Length) / 2), '-').PadRight(paddingLength, '-');
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

			return result.ToString();
		}

		public interface IFieldInfo
		{
			byte Position { get; }
		}

		public class AttributeFieldInfo : IFieldInfo
		{
			public AttributeFieldInfo(AWBMessageBlockAttribute attribute, FieldInfo fieldInfo)
			{
				Attribute = attribute;
				FieldInfo = fieldInfo;
			}
			public readonly AWBMessageBlockAttribute Attribute;
			public readonly FieldInfo FieldInfo;

			byte IFieldInfo.Position => Attribute.Position;
		}

		static Dictionary<Type, List<IFieldInfo>> Dictionary
		{
			get { return dictionary ?? (dictionary = new Dictionary<Type, List<IFieldInfo>>()); }
		}

		[ThreadStatic]
		static Dictionary<Type, List<IFieldInfo>> dictionary;

		static int CompareFieldInfo(IFieldInfo x, IFieldInfo y)
		{
			if (x == null)
			{
				return (y == null) ? 0 : -1;
			}
			else
			{
				return (y == null) ? 1 : x.Position.CompareTo(y.Position);
			}
		}

		public IEnumerable<AttributeFieldInfo> GetAttributeFieldInfos()
		{
			return GetFieldInfos().OfType<AttributeFieldInfo>();
		}

		public IEnumerable<IFieldInfo> GetFieldInfos()
		{
			List<IFieldInfo> result;
			var blockType = GetType();
			if (!Dictionary.TryGetValue(blockType, out result))
			{
				result = new List<IFieldInfo>(SpecialFieldInfos);
				foreach (var fieldInfo in blockType.GetFields())
				{
					var attributes = (AWBMessageBlockAttribute[])fieldInfo.GetCustomAttributes(typeof(AWBMessageBlockAttribute), true);
					if (attributes.Length > 1)
					{
						throw new InvalidAWBMessageBlockStructureException(Invariant($"{blockType.FullName}.{fieldInfo.Name} has too many 'AWBMessageBlockAttribute'; it should have one."));
					}
					else if (attributes.Length == 1)
					{
						result.Add(new AttributeFieldInfo(attributes[0], fieldInfo));
					}
				}

				result.Sort(CompareFieldInfo);
				var lastFieldInfo = result.Last() as SpecialFieldInfo;
				if (lastFieldInfo == null || lastFieldInfo.Value != SpecialChars.CRLF)
				{
					throw new InvalidAWBMessageBlockStructureException(Invariant($"{blockType.FullName} last Field Info is not CRLF."));
				}

				Dictionary.Add(blockType, result);
			}

			return result;
		}

		public override bool Equals(object obj)
		{
			bool result = false;
			if (obj != null && obj.GetType() == GetType())
			{
				result = Serialise() == ((AWBMessageBlock)obj).Serialise();
			}

			return result;
		}

		public override int GetHashCode()
		{
			return Serialise().GetHashCode();
		}

		protected abstract void SetupSpecialFields();

		protected bool IsAddMultiLineDataEnabled => addMultiLineData != null;
		protected void SetupAdditionalMultiLineData(string fieldName, int lengthPerOccurence, int maximumOccurrence, CharType charType)
		{
			addMultiLineData = new AdditionalMultiLineData(fieldName, lengthPerOccurence, maximumOccurrence, charType);
		}

		protected ZString AdditionalMultiLineDataValue
		{
			get => addMultiLineData?.Value ?? ZString.Empty;
			set
			{
				if (addMultiLineData == null)
				{
					throw new NotSupportedException(Invariant($"{nameof(addMultiLineData)} is not setup for {GetType().FullName}."));
				}
				addMultiLineData.Value = value.KeepChars(CharTypes.Text, " ");
			}
		}
		AdditionalMultiLineData addMultiLineData;

		protected void AddSpecialField(byte position, StatusType status, CharType charType, ValueType valueType, ZString chars)
		{
			SpecialFieldInfos.Add(new SpecialFieldInfo() { Position = position, Status = status, CharType = charType, ValueType = valueType, Value = chars });
		}

		class AdditionalMultiLineData
		{
			public AdditionalMultiLineData(string fieldName, int lengthPerOccurence, int maximumOccurrence, CharType charType)
			{
				this.Fieldname = Argument.NotNullOrEmpty(fieldName, nameof(fieldName));
				this.LengthPerOccurence = Argument.GreaterThanOrEqual(lengthPerOccurence, 1, nameof(lengthPerOccurence));
				this.MaximumOccurrence = Argument.GreaterThanOrEqual(maximumOccurrence, 1, nameof(maximumOccurrence));
				this.CharType = charType;
			}

			public string Fieldname { get; private set; }
			public int MaximumOccurrence { get; private set; }
			public int LengthPerOccurence { get; private set; }
			public CharType CharType { get; private set; }
			public ZString Value { get; set; }
		}

		public class SpecialFieldInfo : IFieldInfo
		{
			public byte Position { get; set; }
			public StatusType Status { get; set; }
			public CharType CharType { get; set; }
			public ValueType ValueType { get; set; }
			public ZString Value { get; set; }
		}

		List<SpecialFieldInfo> SpecialFieldInfos => specialFieldInfos ?? (specialFieldInfos = new List<SpecialFieldInfo>());
		List<SpecialFieldInfo> specialFieldInfos;

		string GetCharType(CharType charType)
		{
			var result = "T";
			switch (charType)
			{
				case CharType.Alpha:
					result = "A";
					break;
				case CharType.AlphaNumeric:
					result = "AN";
					break;
				case CharType.Numeric:
				case CharType.NumericWithDecimal:
					result = "N";
					break;
				case CharType.Special:
					result = "X";
					break;
			}

			return result;
		}
	}
}
