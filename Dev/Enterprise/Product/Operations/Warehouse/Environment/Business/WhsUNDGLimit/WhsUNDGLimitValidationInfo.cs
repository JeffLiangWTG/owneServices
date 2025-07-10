using System;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsUNDGLimitValidationInfo
	{
		public WhsUNDGLimitValidationInfo(UNDGLimitType limitType, string code, decimal totalWeight, decimal totalVolume)
		{
			LimitType = limitType;
			Code = code;
			TotalWeight = totalWeight;
			TotalVolume = totalVolume;
		}

		public UNDGLimitType LimitType { get; }

		public string Title => LimitType switch
		{
			UNDGLimitType.DG => Res.GetString("77ca7807-66cb-406d-9096-881cd22d5bcc", "DG"),
			UNDGLimitType.CountryReference => Res.GetString("c6c89310-9ec4-41b5-9dea-8a08f8e744f5", "Country Reference"),
			UNDGLimitType.UNDGClass => Res.GetString("7eedd846-368a-47b2-898b-f7dbdb6a3863", "UNDG Class"),
			_ => throw new ArgumentException("Invalid value", nameof(LimitType))
		};

		public string Code { get; }
		public decimal TotalWeight { get; }
		public decimal TotalVolume { get; }
	}
}
