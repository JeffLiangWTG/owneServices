using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class Measurement : DocDataObject, IMeasurement
	{
		#region Value

		public ZDecimal Value
		{
			get => _value;
			set
			{
				if (SetNonPersistentPropertyValue(ValueInfo, ref _value, value))
				{
					Validate(ValueInfo);
				}
			}
		}

		ZDecimal _value;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));

		#endregion

		#region Unit

		public ICodeDescription Unit
		{
			get => unit;
			set => unit = SetChild(unit, value);
		}

		ICodeDescription unit;

		#endregion

		#region ToString

		public override string ToString() => FormattableString.Invariant($"{Value:0.00} {Unit?.Code}");

		#endregion
	}
}
