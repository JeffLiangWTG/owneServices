using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business
{
	public class MostInterestingLegProvider
	{
		public MostInterestingLegProvider(BaseJobDeclaration baseJobDeclaration)
		{
			declaration = baseJobDeclaration;
		}

		#region GetOutboundLeg

		/// <summary>
		/// Returns outbound leg from shipment/declaration transports
		/// </summary>
		public IMovementLeg GetOutboundLeg()
		{
			return GetOutboundLeg(((IRoutingSupport)declaration).Transports.Cast<IMovementLeg>());
		}

		public IMovementLeg GetOutboundLeg(IEnumerable<IMovementLeg> transports)
		{
			return GetOutboundLegCore(transports);
		}

		protected virtual IMovementLeg GetOutboundLegCore(IEnumerable<IMovementLeg> transports)
		{
			var orderedLegs = transports.ToArray();
			MovementLegComparer.SortMovementLegsByPorts(orderedLegs);
			IMovementLeg result = null;

			var countryCode = declaration.CountryCode;
			var countryOfJurisdiction = GetCustomsCountryOfJurisdictionOrCountryItself(countryCode);
			if (declaration.IsImport || declaration.IsMiscellaneous)
			{
				result = (from IMovementLeg trans in orderedLegs
						  where trans.Load.Left(2) != countryCode && GetCustomsCountryOfJurisdictionOrCountryItself(trans.Discharge.Left(2)) == countryOfJurisdiction
						  select trans).FirstOrDefault();
				if (result == null && Core.Constants.CountryCodes.IsInEuropeanCustomsUnion(countryCode))
				{
					result = (from IMovementLeg trans in orderedLegs
							  where Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(trans.Discharge.Left(2)) == Core.Constants.CountryCodes.EuropeanUnion
							  select trans).FirstOrDefault();
				}
			}
			if (declaration.IsExport)
			{
				result = (from IMovementLeg trans in orderedLegs
						  where GetCustomsCountryOfJurisdictionOrCountryItself(trans.Load.Left(2)) == countryOfJurisdiction && trans.Discharge.Left(2) != countryCode
						  select trans).LastOrDefault();
				if (result == null && Core.Constants.CountryCodes.IsInEuropeanCustomsUnion(countryCode))
				{
					result = (from IMovementLeg trans in orderedLegs
							  where Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(trans.Load.Left(2)) == Core.Constants.CountryCodes.EuropeanUnion
							  select trans).LastOrDefault();
				}
			}
			return result;
		}

		#endregion

		#region GetInboundLeg

		/// <summary>
		/// Returns inbound leg from shipment/declaration transports
		/// </summary>
		public IMovementLeg GetInboundLeg()
		{
			return GetInboundLeg(((IRoutingSupport)declaration).Transports.Cast<IMovementLeg>());
		}

		public IMovementLeg GetInboundLeg(IEnumerable<IMovementLeg> transports)
		{
			return GetInboundLegCore(transports);
		}

		protected virtual IMovementLeg GetInboundLegCore(IEnumerable<IMovementLeg> transports)
		{
			var orderedLegs = transports.ToArray();
			MovementLegComparer.SortMovementLegsByPorts(orderedLegs);
			IMovementLeg result = null;

			var countryCode = declaration.CountryCode;
			var countryOfJurisdiction = GetCustomsCountryOfJurisdictionOrCountryItself(declaration.CountryCode);
			if (declaration.IsImport)
			{
				result = (from IMovementLeg trans in orderedLegs
						  where trans.Load.Left(2) != countryCode && GetCustomsCountryOfJurisdictionOrCountryItself(trans.Discharge.Left(2)) == countryOfJurisdiction
						  select trans).FirstOrDefault();
				if (result == null && Core.Constants.CountryCodes.IsInEuropeanCustomsUnion(countryCode))
				{
					result = (from IMovementLeg trans in orderedLegs
							  where Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(trans.Discharge.Left(2)) == Core.Constants.CountryCodes.EuropeanUnion
							  select trans).FirstOrDefault();
				}
			}
			if (declaration.IsExport || declaration.IsMiscellaneous)
			{
				result = (from IMovementLeg trans in orderedLegs
						  where GetCustomsCountryOfJurisdictionOrCountryItself(trans.Load.Left(2)) == countryOfJurisdiction && trans.Discharge.Left(2) != countryCode
						  select trans).FirstOrDefault();
				if (result == null && Core.Constants.CountryCodes.IsInEuropeanCustomsUnion(countryCode))
				{
					result = (from IMovementLeg trans in orderedLegs
							  where Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(trans.Load.Left(2)) == Core.Constants.CountryCodes.EuropeanUnion
							  select trans).FirstOrDefault();
				}
			}
			return result;
		}

		#endregion

		protected virtual ZString GetCustomsCountryOfJurisdictionOrCountryItself(ZString countryCode) => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);

		protected readonly BaseJobDeclaration declaration;
	}
}
