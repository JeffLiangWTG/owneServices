using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBRateLineCollection : DependentBusinessObjectCollection<ExportAWBRateLine, ExportAWBHeader>
	{
		public ExportAWBRateLineCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		public ZBool Contains(string propertyName, object value)
		{
			foreach (ExportAWBRateLine rateLine in this)
			{
				if (rateLine.ZPropertyInfoHash[propertyName].Value.Equals(value))
				{
					return ZBool.True;
				}
			}

			return ZBool.False;
		}

		public ExportAWBRateLine this[string propertyName, object value]
		{
			get
			{
				foreach (ExportAWBRateLine rateLine in this)
				{
					if (rateLine.ZPropertyInfoHash[propertyName].Value.Equals(value))
					{
						return rateLine;
					}
				}
				return null;
			}
		}

		public ZDecimal SumOfDecimal(string propertyName)
		{
			ZDecimal result = 0M;

			foreach (ExportAWBRateLine rateLine in this)
			{
				result += (ZDecimal)rateLine.ZPropertyInfoHash[propertyName].Value;
			}

			return result;
		}

		public ZInt SumOfInt(string propertyName)
		{
			ZInt result = 0;

			foreach (ExportAWBRateLine rateLine in this)
			{
				ZInt parsedResult = 0;
				if (ZInt.TryParse(rateLine.ZPropertyInfoHash[propertyName].Value.ToString(), out parsedResult))
				{
					result += parsedResult;
				}
			}

			return result;
		}

		// normal readonly does not work because it causes a list reset which stuffs up the binding to ratelines
		public void SetReadOnly(ZBool readOnly)
		{
			foreach (ExportAWBRateLine rateLine in this)
			{
				rateLine.SetReadOnlyIncludingChildren(readOnly);
			}
		}
	}
}
