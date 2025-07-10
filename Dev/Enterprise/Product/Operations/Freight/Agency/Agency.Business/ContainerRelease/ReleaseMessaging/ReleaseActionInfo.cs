using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	class ReleaseActionInfo : IUniversalActionInfo
	{
		public ReleaseActionInfo(ReleaseHeader header)
		{
			this.header = Argument.NotNull(header, "header");
		}

		readonly ReleaseHeader header;

		ZString IUniversalActionInfo.ActionType
		{
			get { return ZString.Empty; }
		}

		BusinessObjectFactory IUniversalActionInfo.FactoryForProcessing
		{
			get { return header.Factory; }
		}

		BusinessObject IUniversalActionInfo.ParentBO
		{
			get { return header.Shipment; }
		}

		ZString IUniversalActionInfo.PurposeCode
		{
			get { return ZString.Empty; }
		}

		RecipientRoleDetail[] IUniversalActionInfo.RecipientRoleDetails
		{
			get { return header.MessageStrategies.SelectMany(x => x.RecipientRoles.ToRecipientRoleDetails()).ToArray(); }
		}

		ZDateTimeOffset IUniversalActionInfo.TriggerActualDate
		{
			get { return ZDateTimeOffset.Now; }
		}

		ZInt IUniversalActionInfo.TriggerCount
		{
			get { return 1; }
		}

		ZString IUniversalActionInfo.TriggerDescription
		{
			get { return ZString.Empty; }
		}

		ZString IUniversalActionInfo.TriggerEventCode
		{
			get { return Events.ReleaseRequestedCode; }
		}

		ZString IUniversalActionInfo.TriggerReference
		{
			get { return ZString.Empty; }
		}

		ZDateTimeOffset IUniversalActionInfo.TriggerScheduledDate
		{
			get { return ZDateTimeOffset.Now; }
		}

		TriggerType IUniversalActionInfo.TriggerType
		{
			get { return TriggerType.Manual; }
		}

		IStmALog IUniversalActionInfo.TriggeringEvent
		{
			get { return null; }
		}

		INotifications IUniversalActionInfo.Notifications { get; set; }

		public IOrgHeader RecipientOrganization => null;

		public void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode) => throw new NotImplementedException("If this happens then there is something wrong with the grouping code in WorkflowTriggerActionManager"); // Neve ever my friends :).
	}
}
