using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class OffsetFromUtcTransform : ITransformStrategy
	{
		static OffsetFromUtcTransform transform;

		OffsetFromUtcTransform() { }

		public static OffsetFromUtcTransform Instance()
		{
			if (transform == null)
			{
				transform = new OffsetFromUtcTransform();
			}
			return transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return dataSetType == typeof(RefUNLOCO) || dataSetType == typeof(RefTimeZoneSet);
		}

		public T Transform<T>(T data)
		{
			if (data is RefUNLOCOUtcOffset unlocoUtcOffset)
			{
				unlocoUtcOffset.RLO_OffsetFromUtc = decimal.Divide(unlocoUtcOffset.RLO_OffsetMinutesFromUtc, 60);
			}
			else if (data is RefTimeZone timeZone)
			{
				timeZone.R2_OffsetFromUTC = decimal.Divide(timeZone.R2_OffsetMinutesFromUTC, 60);
			}
			return data;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefUNLOCOUtcOffset),
					typeof(RefTimeZone)
				};
			}
		}
	}
}
