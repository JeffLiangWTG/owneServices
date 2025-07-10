using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ConsolRelatedShipmentsOSMGSecurityCheckpoint : SecurityCheckpoint
	{
		internal ConsolRelatedShipmentsOSMGSecurityCheckpoint(BusinessObject businessEntity)
			: base("ConsolShipmentOSMGSecurityCheckpoint", ResString.GetMultilingualString("9418c49e-3dd8-44c5-9dfe-6c1cb7eb0f65", "Cannot access consol"), null, null, false)
		{
			this.businessEntity = businessEntity;
		}

		readonly BusinessObject businessEntity;

		(bool IsRestricted, MultilingualString RestrictionMessage) Restriction
		{
			get
			{
				if (restriction == null)
				{
					restriction = GetRestriction(businessEntity);
				}
				return restriction.Value;
			}
		}
		(bool IsRestricted, MultilingualString RestrictionMessage)? restriction;

		public override bool IsAllowed => !Restriction.IsRestricted;

		public override bool Visible => false;

		public override void ShowError() => Globals.Message.ShowError(ErrorMessageForNotAllowed, Res.GetString("7a046a87-6c16-484d-9880-a628d18cd66a", "Access denied"));

		public override MultilingualString ErrorMessageForNotAllowed => Restriction.RestrictionMessage;

		(bool IsRestricted, MultilingualString RestrictionMessage) GetRestriction(BusinessObject sourceEntity)
		{
			if (sourceEntity is ForwardingConsol consol && !consol.IsTemplateRecord && !FreightDataRegistry.Instance.ConsolAllowAccessRegardlessOfShipmentsOSMGRights.Value)
			{
				var securytiProvider = new JobShipmentCRMSecurityProvider();
				if (securytiProvider.HasRestrictions)
				{
					var factory = new BusinessObjectFactory();
					var filter = new ShipmentsOfConsolFilter(JobConsolFilterBusinessObject.Descriptions.RelatedShipmentsSecurity, () => new ForwardingShipmentCollection(factory));
					filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
					var query = new ZQuery(JobConsolSchema.PK, consol.PK);
					query.AddToFilter(filter.Query);
					if (!factory.Exists(typeof(ForwardingConsol), query))
					{
						return (true, GetRestrictedConsolDeniedAccessError(securytiProvider.DeniedSecurityCheckpointsPaths));
					}
				}
			}

			return (false, null);
		}

		MultilingualString GetRestrictedConsolDeniedAccessError(IEnumerable<MultilingualString> deniedSecurityCheckpointsPaths)
		{
			return ResString.GetMultilingualString("0b827385-1c6c-44ba-bd12-985cdbc7eacf", @"You do not have the appropriate security rights to run this function.

At least one of the shipments on this consolidation has its access restricted.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
{0}", string.Join(System.Environment.NewLine, deniedSecurityCheckpointsPaths));
		}
	}
}
