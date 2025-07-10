using System;

namespace Enterprise.HRM.Common
{
	public struct Bucket
	{
		public Bucket(string type, DateTime? forfeiture, decimal value)
		{
			Type = type;
			Forfeiture = forfeiture;
			Value = value;
		}

		public string Type { get; }
		public DateTime? Forfeiture { get; }
		public decimal Value { get; }
	}
}
