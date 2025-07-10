using System;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsDataObjectReaderHelper
	{
		public WhsDataObjectReaderHelper(WhsWarehouse warehouse)
		{
			TimeZone = new Lazy<ITimeZone>(
				() => warehouse?.RelatedCompanyBranch?.HomePort?.TimeZoneSet?.GetCalculationTimeZone());
		}

		Lazy<ITimeZone> TimeZone { get; }

		// Tested in WhsReceiveDataObjectReaderTest.cs + WhsAdjustmentLineDataObjectReaderTest.cs
		public ZDateTimeOffset? ConvertToZDateTimeOffset(ZDateTime? dateTime)
		{
			ZDateTimeOffset? dateTimeOffset = null;
			if (dateTime.HasValue)
			{
				if (dateTime.Value != ZDateTime.Empty)
				{
					var calculationTimeZone = TimeZone?.Value;
					if (calculationTimeZone != null)
					{
						var offset = calculationTimeZone.GetUtcOffsetBasedOnLocal(dateTime.Value.ToDateTime());
						dateTimeOffset = new ZDateTimeOffset(dateTime.Value, DateTimeKind.Local, offset);
					}
					else
					{
						dateTimeOffset = new ZDateTimeOffset(dateTime.Value, DateTimeKind.Local);
					}
				}
				else
				{
					dateTimeOffset = ZDateTimeOffset.Empty;
				}
			}
			return dateTimeOffset;
		}
	}
}
