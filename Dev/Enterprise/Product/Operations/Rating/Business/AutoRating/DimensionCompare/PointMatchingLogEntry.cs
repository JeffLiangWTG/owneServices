namespace Enterprise.Rating.Business
{
	public class PointMatchingLogEntry
	{
		public PointMatchingLogEntry(
			string rateLineInfo,
			string measureType,
			string measureDimension,
			string entityName,
			string pointValue,
			string rateLineValue,
			bool pointRejected)
		{
			this.rateLineInfo = rateLineInfo;
			this.measureType = measureType;
			this.measureDimension = measureDimension;
			this.entityName = entityName ?? string.Empty;
			this.pointValue = pointValue ?? string.Empty;
			this.rateLineValue = rateLineValue ?? string.Empty;
			this.pointRejected = pointRejected;
		}

		public string GetLogString()
		{
			if (!pointRejected)
			{
				return "\t\t\t\t" + LogMessages.AddingChargeableAmount(entityName, pointValue, measureType);
			}
			else
			{
				return LogMessages.PointMismatch(rateLineInfo, entityName, measureType, measureDimension, pointValue, rateLineValue);
			}
		}

		readonly bool pointRejected;
		readonly string rateLineInfo;
		readonly string measureType;
		readonly string measureDimension;
		readonly string pointValue;
		readonly string rateLineValue;
		readonly string entityName;

		public string RateLineInfo
		{
			get { return rateLineInfo; }
		}

		public bool PointRejected
		{
			get { return pointRejected; }
		}

		public string EntityName
		{
			get { return entityName; }
		}
	}
}
