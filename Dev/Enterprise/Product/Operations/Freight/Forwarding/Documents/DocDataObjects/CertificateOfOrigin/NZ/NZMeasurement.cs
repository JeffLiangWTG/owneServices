using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ
{
	public class NZMeasurement : DocDataObject, IMeasurement
	{
		readonly ZDecimal _originalValue;
		readonly ZString _originalCode;

		public NZMeasurement(ZDecimal value, ZString code)
		{
			_originalValue = value;
			_originalCode = code;

			Value = value;

			var codeDescription = new CodeDescription(new NZCustomsTariffQuantityWeightUnits())
			{
				Code = code
			};

			codeDescription.PropertyValueChanged += OnCodeDescriptionValueChanged;

			Unit = codeDescription;
		}

		void OnCodeDescriptionValueChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			if (e.Property.Name != nameof(Unit.Code))
			{
				return;
			}

			var newValue = e.Property.Value.ToString();

			if (_originalCode.ToString() == newValue)
			{
				Value = _originalValue;
			}
			else
			{
				Value = Core.Constants.Weight.Convert(_originalValue, _originalCode, newValue);
			}
		}

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
	}
}
