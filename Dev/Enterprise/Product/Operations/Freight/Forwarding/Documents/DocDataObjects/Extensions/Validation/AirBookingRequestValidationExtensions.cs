using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirBookingRequestValidation
{
	static class AirBookingRequestValidationExtensions
	{
		#region AddIATAFormatValidation

		public static Unloco AddIATAValidation(this Unloco unloco, ZString portName)
		{
			if (unloco == null)
			{
				return null;
			}

			unloco.IATACodeInfo.AddMessageErrorIfEmpty(Res.GetString("dd40ea16-b43f-4e7b-956e-084ecfaf0d13", "{0} IATA code is mandatory", portName));
			unloco.IATACodeInfo.AddMessageError(() => !unloco.IATACode.IsEmpty && (unloco.IATACode.Length != 3 || !unloco.IATACode.IsLettersOnlyOrEmpty)
						, Res.GetString("7e0ee985-5eb3-46c4-9956-d58e30d5d690", "{0} IATA code must consist of 3 letters.", portName));

			return unloco;
		}
		#endregion

		#region AddMaximumLengthValidation

		public static ZPropertyInfo AddMaximumLengthValidation(this ZPropertyInfo info, ZString propertyName, ZInt maximumLength)
		{
			if (info == null || maximumLength <= 0 || propertyName.IsEmpty)
			{
				return null;
			}

			if (info.Value is ZInt)
			{
				var maxValue = Math.Pow(10, maximumLength) - 1;
				info.AddMessageError(() => (ZInt)info.Value > maxValue, Res.GetString("61bec342-e548-4cd3-989a-9f32a4aee98f", "{0} must not exceed the maximum {1}.", propertyName, maxValue));
			}

			if (info.Value is ZDecimal)
			{
				info.AddMessageError(() => info.Value.ToString().Trim(' ', '0').Length > maximumLength
													, Res.GetString("a13b0e69-970a-4ead-825c-0af955fb3026"
																	, "{0} must not exceed the maximum length {1}."
																	, propertyName
																	, maximumLength));
			}

			if (info.Value is ZString)
			{
				info.AddMessageError(() => !info.Value.IsEmpty && new ZString(info.Value).Length > maximumLength, Res.GetString("e34722f8-8684-4446-b4cd-ea7513a3587f", "{0} must not exceed {1} characters.", propertyName, maximumLength));
			}

			return info;
		}

		#endregion
	}
}
