using System;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using ValueType = Enterprise.Messaging.Business.AWB.ValueType;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public abstract class AWBMessageBlockAttribute : Attribute
	{
		protected AWBMessageBlockAttribute(byte position, byte minLength, byte maxLength, StatusType status, CharType charType)
		{
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(position));
			}
			Position = position;
			MinLength = minLength;
			MaxLength = maxLength;
			Status = status;
			CharType = charType;
		}

		public ValueElement GetElement(IZType value)
		{
			var serialisedValue = Serialise(value);
			return new ValueElement(Status, new Format(MaxLength, IsValid(serialisedValue) ? CharType : CharType.Special), serialisedValue, ValueType.Value);
		}

		public ZString Serialise(IZType value)
		{
			return SerialiseCore(value);
		}

		public IZType DeSerialise(ZString block)
		{
			return DeSerialiseCore(block);
		}

		public int MaxLength { get; private set; }
		public int MinLength { get; private set; }
		public byte Position { get; private set; }
		public StatusType Status { get; private set; }
		public CharType CharType { get; private set; }
		public ZString InvalidValue => new ZString(InvalidPaddingCharacter, MinLength);

		#region Implementation

		protected bool IsValid(ZString value) => !value.StartsWith(InvalidPaddingCharacter.ToString(), StringComparison.OrdinalIgnoreCase);
		protected abstract ZString SerialiseCore(IZType value);
		protected abstract IZType DeSerialiseCore(ZString value);

		protected const char InvalidPaddingCharacter = '?';
		protected const char ZeroPaddingCharacter = '0';
		protected const char SpacePaddingCharacter = ' ';

		#endregion
	}
}
