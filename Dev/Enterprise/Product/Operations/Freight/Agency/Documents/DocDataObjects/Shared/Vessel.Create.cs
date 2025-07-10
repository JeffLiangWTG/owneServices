using System;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Vessel
	{
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
	}
}
