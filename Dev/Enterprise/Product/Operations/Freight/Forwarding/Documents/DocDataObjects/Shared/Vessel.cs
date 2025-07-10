using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class Vessel : DocDataObject, IVessel
	{
		public Vessel(object identifier = default)
			: base(identifier)
		{
		}

		public static Vessel Create(IContext context, Freight.Business.Transport transport)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new Vessel
			{
				Name = transport?.JW_Vessel ?? ZString.Empty,
				LloydsIMO = transport?.Vessel?.RV_LloydsNumber ?? ZString.Empty,
				RadioCallSign = transport?.Vessel?.RV_RadioCallSign ?? ZString.Empty,
				Type = new CodeDescription(transport?.Vessel?.Lookups?.RV_VesselType_List ?? new CodeDescriptionPairList(OLookUpEditType.VesselType))
				{
					Code = transport?.Vessel?.RV_VesselType ?? ZString.Empty,
				},
				CountryOfRegistration = DocumentVisualizer.DocDataObjects.Country.Create(context, transport?.Vessel?.CountryOfReg),
			};
		}

		public static Vessel Create(IContext context, TransportLeg transportLeg)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new Vessel
			{
				Name = (transportLeg?.VesselName).GetValueOrDefault(),
				LloydsIMO = (transportLeg?.VesselLloydsIMO).GetValueOrDefault(),
				Type = new CodeDescription(new CodeDescriptionPairList(OLookUpEditType.VesselType))
				{
					Code = ZString.Empty
				}
			};
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
