using System;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public enum FillType { AlwaysZeroFill, AlwaysSpaceFill, ZeroFillUnlessEmpty }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public abstract class MessageBlockAttribute : Attribute
	{
		public static class FieldStatus
		{
			public const string Mandatory = "M";
			public const string Conditional = "C";
			public const string Optional = "O";
		}

		protected MessageBlockAttribute(byte length, byte position, string status)
		{
			if (length < 0 || length > 80)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}
			Length = length;

			if (position < 1 || position + length - 1 > 80)
			{
				throw new ArgumentOutOfRangeException(nameof(position));
			}
			this.position = position;

			Status = status;
		}

		protected MessageBlockAttribute(byte length, byte position, string status, FillType fillType)
			: this(length, position, status)
		{
			this.fillType = fillType;
		}

		protected MessageBlockAttribute(byte length, byte position, string status, string fillTypePropertyName)
			: this(length, position, status)
		{
			this.fillTypePropertyName = fillTypePropertyName;
		}

		public string Serialise(MessageBlock block, IZType value, bool humanFriendly)
		{
			string result = SerialiseCore(block, value, humanFriendly);
			if (!humanFriendly)
			{
				if (result.Length > Length)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Data provided exceeded maximum field length");
				}
				else if (result.Length < Length)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Data returned was not padded to correct length");
				}
			}
			return result;
		}

		public string Serialise(MessageBlock block, IZType value)
		{
			return Serialise(block, value, false);
		}

		public IZType DeSerialise(string eightyCharacterBlock)
		{
			string rawData = GetRawData(eightyCharacterBlock);
			return DeSerialiseCore(rawData);
		}

		public string GetRawData(string eightyCharacterBlock)
		{
			return eightyCharacterBlock.Substring(Offset, Length).TrimEnd();
		}

		public int Offset
		{
			get { return position - 1; }
		}

		public readonly int Length;
		public readonly string Status;

		protected FillType GetFillTypeCore(MessageBlock block)
		{
			var result = fillType;
			if (!string.IsNullOrEmpty(fillTypePropertyName))
			{
				var propertyInfo = block.GetType().GetProperty(fillTypePropertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) ?? throw new InvalidMessageFormatException("there is no property named " + fillTypePropertyName);
				result = (FillType)propertyInfo.GetValue(block, Array.Empty<object>());
			}
			return result;
		}

		readonly FillType fillType;
		readonly string fillTypePropertyName;

		#region Implementation

		protected abstract string SerialiseCore(MessageBlock block, IZType value, bool humanFriendly);
		protected abstract IZType DeSerialiseCore(string value);

		protected const char ZeroPaddingCharacter = '0';
		protected const char SpacePaddingCharacter = ' ';
		protected const char InvalidPaddingCharacter = '*';

		protected char GetPaddingCharacter(FillType fillType)
		{
			return fillType == FillType.AlwaysSpaceFill ? SpacePaddingCharacter : ZeroPaddingCharacter;
		}

		readonly int position;

		#endregion
	}
}
