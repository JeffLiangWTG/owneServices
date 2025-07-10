using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Edifact.Auto;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	abstract class MessageBuilder
	{
		public ForwardingShipment Shipment { get; set; }
		public INotifications Notifications { get; set; }

		public string MessageText
		{
			get { return CreateMessage().ToString(new CargoIMPPhase2CharacterSet()).Replace("\n", "\r\n"); }
		}

		protected abstract SegmentGroup CreateMessage();

		protected ZString MessageVersion { get { return "3"; } }

		protected void FillMessageDetail(MSGSegment mSGSegment)
		{
			mSGSegment.MessageTimestamp = DataTypeDefinitions.MessageTimestamp.Format(ZDateTime.Now, null);
		}

		protected ZString ConvertAndCheckValue(InformationResult<ZString> data, bool isMandatory, DataFormat<ZString> format)
		{
			return ConvertAndCheckValue<ZString>(data, isMandatory, format);
		}

		protected ZString ConvertAndCheckValue(InformationResult<ZString> data, bool isMandatory, DataFormat<ZString> format, out bool isFormattedCorrectly)
		{
			return ConvertAndCheckValue<ZString>(data, isMandatory, format, out isFormattedCorrectly);
		}

		protected ZString ConvertAndCheckValue(InformationResult<ZInt> data, bool isMandatory, DataFormat<ZInt> format)
		{
			return ConvertAndCheckValue<ZInt>(data, isMandatory, format);
		}

		protected ZString ConvertAndCheckValue(InformationResult<ZDecimal> data, bool isMandatory, DataFormat<ZDecimal> format)
		{
			return ConvertAndCheckValue<ZDecimal>(data, isMandatory, format);
		}

		protected ZString ConvertAndCheckValue(InformationResult<ZDateTime> data, bool isMandatory, DataFormat<ZDateTime> format)
		{
			return ConvertAndCheckValue<ZDateTime>(data, isMandatory, format);
		}

		protected ZString ConvertAndCheckValue<T>(InformationResult<T> data, bool isMandatory, DataFormat<T> format) where T : struct
		{
			bool isFormattedCorrectly;
			return ConvertAndCheckValue(data, isMandatory, format, out isFormattedCorrectly);
		}

		protected ZString ConvertAndCheckValue<T>(InformationResult<T> data, bool isMandatory, DataFormat<T> format, out bool isFormattedCorrectly) where T : struct
		{
			FormattingResult formattingResult = new FormattingResult();
			ZString result = format.Format(data.Value, formattingResult);
			if (!formattingResult.IsFormattedCorrectly)
			{
				if (isMandatory)
				{
					Notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("d54fe7a7-aaa6-489a-a193-1e2b4aaa56f6", "{0}: value can't be formatted, {1}", data.Name, formattingResult.ErrorMessage)));
				}
				else
				{
					Notifications.Notify(new WarningNotification(WarningType.Warning, Res.GetString("d54fe7a7-aaa6-489a-a193-1e2b4aaa56f6", "{0}: value can't be formatted, {1}", data.Name, formattingResult.ErrorMessage)));
				}
			}

			if (isMandatory && result.IsEmpty)
			{
				Notifications.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, data.Name));
			}

			isFormattedCorrectly = formattingResult.IsFormattedCorrectly;
			return result;
		}

		protected TResult ConvertAndCheckValue<TSource, TResult>(InformationResult<TSource> data, bool isMandatory, ListConverter<TSource, TResult> converter) where TSource : struct
		{
			FormattingResult formattingResult = new FormattingResult();
			TResult result = converter.Convert(data.Value, formattingResult);
			if (!formattingResult.IsFormattedCorrectly)
			{
				if (isMandatory)
				{
					Notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, Res.GetString("7edd40da-fc55-4fa0-a53d-e8b138afc80e", "{0}: value can't be converted, {1}", data.Name, formattingResult.ErrorMessage)));
				}
				else
				{
					Notifications.Notify(new WarningNotification(WarningType.Warning, Res.GetString("7edd40da-fc55-4fa0-a53d-e8b138afc80e", "{0}: value can't be converted, {1}", data.Name, formattingResult.ErrorMessage)));
				}
			}

			if (isMandatory && result == null)
			{
				Notifications.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, data.Name));
			}

			return result;
		}
	}
}
