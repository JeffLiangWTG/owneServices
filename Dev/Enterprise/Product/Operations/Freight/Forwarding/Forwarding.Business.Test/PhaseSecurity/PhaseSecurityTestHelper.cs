using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public static class PhaseSecurityTestHelper
	{
		public static class Shipment
		{
			public static PhaseSecurity SetupRestricted(string restrictedPhaseCode)
			{
				return SetupPhase(restrictedPhaseCode, false, EntityType.Shipment);
			}

			public static PhaseSecurity SetupAllowed(string allowedPhaseCode)
			{
				return SetupPhase(allowedPhaseCode, true, EntityType.Shipment);
			}
		}

		public static class Consol
		{
			public static PhaseSecurity SetupRestricted(string restrictedPhaseCode)
			{
				return SetupPhase(restrictedPhaseCode, false, EntityType.Consol);
			}

			public static PhaseSecurity SetupAllowed(string allowedPhaseCode)
			{
				return SetupPhase(allowedPhaseCode, true, EntityType.Consol);
			}
		}

		#region Implementation

		enum EntityType
		{
			Shipment,
			Consol
		}

		static PhaseSecurity SetupPhase(string phaseCode, bool isAllowedForCurrentDepartment, EntityType entityType)
		{
			var locationsList = entityType == EntityType.Shipment ? PhaseConstants.GetShipmentLocationsList() : PhaseConstants.GetConsolLocationsList();

			PhaseSecurity security = new PhaseSecurity(locationsList);
			security.IsEnabled = true;

			Phase phase = security.Phases.AddNew();
			phase.Code = phaseCode;
			phase.Description = (NoResString)"Hello";

			PhaseRule rule = phase.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			if (isAllowedForCurrentDepartment)
			{
				rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			}
			else
			{
				rule.DepartmentPK = new BusinessObjectFactory().LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK)).PK;
			}

			if (entityType == EntityType.Shipment)
			{
				ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);
			}
			else
			{
				ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);
			}

			return security;
		}

		#endregion
	}
}
