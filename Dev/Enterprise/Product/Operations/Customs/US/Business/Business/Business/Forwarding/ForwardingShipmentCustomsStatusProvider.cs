using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ForwardingShipmentCustomsStatusProvider : Forwarding.Business.ForwardingShipmentCustomsStatusProvider, Integration.Customs.US.IForwardingShipmentCustomsStatusProvider
	{
		public ForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
			: base(shipment)
		{
			var factory = shipment.Factory;

			var declarationQuery = JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, shipment.PK);
			declarationQuery.FetchOnlyFromLocalCache = !shipment.IsInDatabase;
			var declaration = factory.LoadTop1<JobDeclaration>(declarationQuery);

			customsCargoStatus = "";
			customsMessageStatus = "";

			if (declaration != null)
			{
				if (declaration.IsExport)
				{
					customsMessageStatus = factory.GetCachedValue<AESDirectCustomsEntryStatus>().GetDescriptionFromCode(declaration.ExportStatus);
					expStatus = declaration.ExportStatus;
				}
				else
				{
					if (declaration.IsCargoReleaseValidationMode)
					{
						customsCargoStatus = factory.GetCachedValue<ImportMessageStatusList>().GetDescriptionFromCode(declaration.CargoReleaseStatus);
						crlStatus = declaration.CargoReleaseStatus;
					}

					if (declaration.IsEntrySummaryValidationMode)
					{
						customsMessageStatus = factory.GetCachedValue<ImportMessageStatusList>().GetDescriptionFromCode(declaration.EntrySummaryStatus);
						ensStatus = declaration.EntrySummaryStatus;
					}

					if (declaration.IsACECargoCertificationMode)
					{
						customsCargoStatus = factory.GetCachedValue<ImportMessageStatusList>().GetDescriptionFromCode(declaration.CargoReleaseStatus);
						seBillStatus = declaration.SimplifiedEntryBillStatusDescription;
					}
					hldOrEXMStatus = declaration.HLDOrEXMStatus;
				}
			}
		}

		readonly ZString customsCargoStatus;
		readonly ZString customsMessageStatus;
		readonly ZString crlStatus;
		readonly ZString ensStatus;
		readonly ZString expStatus;
		readonly ZString seBillStatus;
		readonly ZString hldOrEXMStatus;

		public override ZString CustomsCargoStatus()
		{
			return customsCargoStatus;
		}

		public override ZString CustomsMessageStatus()
		{
			return customsMessageStatus;
		}

		public override ZString CRLStatus()
		{
			return crlStatus;
		}

		public override ZString ENSStatus()
		{
			return ensStatus;
		}

		public override ZString EXPStatus()
		{
			return expStatus;
		}

		public override ZString SEBillStatus()
		{
			return seBillStatus;
		}

		public override ZString HLDOrEXMStatus()
		{
			return hldOrEXMStatus;
		}
	}
}
