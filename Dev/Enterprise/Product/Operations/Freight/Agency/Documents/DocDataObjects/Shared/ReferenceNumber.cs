using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class ReferenceNumber : DocDataObject, IReferenceNumber
	{
		#region Value

		public ZString Value
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

		ZString _value;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));

		#endregion

		#region Type

		public ICodeDescription Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		ICodeDescription type;

		#endregion

		#region CountryOfIssue

		public ICountry CountryOfIssue
		{
			get => countryOfIssue;
			set => countryOfIssue = SetChild(countryOfIssue, value);
		}

		ICountry countryOfIssue;

		#endregion
	}
}
