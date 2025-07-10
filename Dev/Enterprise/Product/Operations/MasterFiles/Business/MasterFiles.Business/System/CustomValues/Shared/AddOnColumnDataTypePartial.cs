using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	partial class AddOnColumnDataType
	{
		public static string PartIdentifier = "PART";

		public static Type GetTypeFromCode(string code)
		{
			Argument.NotNullOrEmpty(code, "code");
			switch (code.ToUpperInvariant())
			{
				case Codes.Boolean:
					return typeof(ZBool);
				case Codes.Byte:
					return typeof(ZByte);
				case Codes.ComboBox:
					return typeof(CodeDescriptionPair);
				case Codes.Date:
					return typeof(ZDate);
				case Codes.Datetime:
					return typeof(ZDateTime);
				case Codes.Decimal:
					return typeof(ZDecimal);
				case Codes.Guid:
					return typeof(ZGuid);
				case Codes.Integer:
					return typeof(ZInt);
				case Codes.Short:
					return typeof(ZShort);
				case Codes.String:
					return typeof(ZString);
				default:
					throw new ArgumentException("Unknown code : " + code, nameof(code));
			}
		}

		public static bool IsMultiPartCode(string code)
		{
			switch (code.ToUpperInvariant())
			{
				case Codes.ComboBox:
					return true;
				default:
					return false;
			}
		}

		public static string GetCodeFromObject(object zTypeObject)
		{
			return GetCodeFromType(zTypeObject.GetType());
		}

		public static string GetCodeFromType(Type type)
		{
			Argument.NotNull(type, "type");
			if (type == typeof(ZBool))
			{
				return Codes.Boolean;
			}

			if (type == typeof(ZByte))
			{
				return Codes.Byte;
			}

			if (type == typeof(CodeDescriptionPair))
			{
				return Codes.ComboBox;
			}

			if (type == typeof(ZDate))
			{
				return Codes.Date;
			}

			if (type == typeof(ZDateTime))
			{
				return Codes.Datetime;
			}

			if (type == typeof(ZDecimal))
			{
				return Codes.Decimal;
			}

			if (type == typeof(ZGuid))
			{
				return Codes.Guid;
			}

			if (type == typeof(ZInt))
			{
				return Codes.Integer;
			}

			if (type == typeof(ZShort))
			{
				return Codes.Short;
			}

			if (type == typeof(ZString))
			{
				return Codes.String;
			}

			throw new ArgumentException("Unsupported Type : " + type.Name, nameof(type));
		}
	}
}