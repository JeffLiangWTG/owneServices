using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondBillDataCalculator
	{
		readonly ForwardingShipment shipment;
		readonly JobDeclaration declaration;
		readonly bool isAMSHBREffective;
		readonly IEnumerable<ZString> validSCACs;

		public CusInBondBillDataCalculator(ForwardingShipment shipment, JobDeclaration declaration = null)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
			this.declaration = declaration;
			this.isAMSHBREffective = ZZCustomsFunctionality.IsAMSHBREffective;
			this.validSCACs = shipment.GetValidSCACIssuerCodes(shipment.TransportMode);
		}

		public ZString GetHouseBillIssuerCode()
		{
			if (!isAMSHBREffective)
			{
				return ZString.Empty;
			}

			var result = ZString.Empty;
			if (declaration != null && declaration.IsSea)
			{
				result = declaration.JE_HouseBillIssuerSCAC.KeepValidBillNumberCharacters();
			}
			else if (shipment.IsSea)
			{
				var billNumber = shipment.JS_HouseBill.KeepValidBillNumberCharacters();
				result = billNumber.GetSCAC(validSCACs);
			}
			return result;
		}
	}
}
