using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Vessel : DocDataObject, IVessel
	{
		public Vessel(object identifier = default)
			: base(identifier)
		{
		}

		#region Name

		public ZString Name
		{
			get => name;
			set
			{
				if (SetNonPersistentPropertyValue(NameInfo, ref name, value))
				{
					Validate(NameInfo);
				}
			}
		}

		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		#endregion

		#region LloydsIMO

		public ZString LloydsIMO
		{
			get => lloydsIMO;
			set
			{
				if (SetNonPersistentPropertyValue(LloydsIMOInfo, ref lloydsIMO, value))
				{
					Validate(LloydsIMOInfo);
				}
			}
		}

		ZString lloydsIMO;

		public ZPropertyInfo LloydsIMOInfo => GetZPropertyInfo(nameof(LloydsIMO));

		#endregion

		#region Radio Callsign

		public ZString RadioCallSign
		{
			get => radioCallSign;
			set
			{
				if (SetNonPersistentPropertyValue(RadioCallSignInfo, ref radioCallSign, value))
				{
					Validate(RadioCallSign);
				}
			}
		}

		ZString radioCallSign;

		public ZPropertyInfo RadioCallSignInfo => GetZPropertyInfo(nameof(RadioCallSign));

		#endregion

		#region Type

		public ICodeDescription Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		ICodeDescription type;

		#endregion

		#region CountryOfRegistration

		public ICountry CountryOfRegistration
		{
			get => countryOfRegistration;
			set => countryOfRegistration = SetChild(countryOfRegistration, value);
		}

		ICountry countryOfRegistration;

		#endregion

	}
}
