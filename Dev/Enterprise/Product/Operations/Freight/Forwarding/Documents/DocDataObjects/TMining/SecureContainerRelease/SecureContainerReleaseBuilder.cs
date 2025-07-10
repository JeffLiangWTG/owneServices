using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SecureContainerReleaseBuilder
	{
		public SecureContainerReleaseBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			Argument.NotNull(consol, nameof(consol));

			this.consol = consol;
			this.parameters = parameters;
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public SecureContainerRelease Build()
		{
			var scr = new SecureContainerRelease(nameof(ForwardingConsol), consol.JK_UniqueConsignRef);

			scr.FormMode = GetFormMode();

			scr.OperationalPort = Unloco.Create(context, consol.DischargePort);
			scr.BillOfLading = consol.JK_MasterBillNum;

			PopulateParties(scr);
			PopulateContainers(scr);
			PopulateDataObjectWriterFields(scr);

			AddValidations(scr);

			scr.ValidateAllIncludingChildren();

			return scr;
		}

		void PopulateParties(SecureContainerRelease scr)
		{
			var sendingPartyAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			scr.SendingParty = AddressBuilder.Create(context, sendingPartyAddress);
			scr.SendingPartyIdEOR = sendingPartyAddress.GetEoriNumber(context);
			scr.SendingPartyIdDUN = sendingPartyAddress.GetDunsNumber(context, false);

			scr.Forwarder = AddressBuilder.Create(context, consol.ReceivingForwarderWithContact);
			scr.ForwarderIdEOR = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetEoriNumber(context);
			scr.ForwarderIdDUN = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetDunsNumber(context, false);

			scr.TransportCompany = AddressBuilder.Create(context, consol.ArrivalUnpackCFSTransportAddress);
			scr.TransportCompanyIdEOR = consol.ArrivalUnpackCFSTransportAddress.GetEoriNumber(context);
			scr.TransportCompanyIdDUN = consol.ArrivalUnpackCFSTransportAddress.GetDunsNumber(context, false);
		}

		void PopulateContainers(SecureContainerRelease scr)
		{
			var containerBuilder = new SecureContainerReleaseContainerBuilder();
			var containerDOs = new List<SecureContainerReleaseContainer>();
			var selectedContainerPKs = GetSelectedContainerPKs();

			foreach (ForwardingContainer containerBO in consol.Containers)
			{
				if (selectedContainerPKs.Contains(containerBO.PK))
				{
					containerDOs.Add(containerBuilder.Build(containerBO, scr.FormMode));
				}
			}

			scr.Containers = containerDOs.OrderBy(container => container.Number).ToArray();
		}

		void PopulateDataObjectWriterFields(SecureContainerRelease scr)
		{
			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Other
			};

			scr.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol.JK_ConsolMode)
					? (ZString)Core.Constants.ContainerModes.FCL
					: consol.JK_ConsolMode
			};
		}

		void AddValidations(SecureContainerRelease scr)
		{
			((Unloco)scr.OperationalPort).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("3B47228F-04F6-4E0B-92FD-6483A72F891F", "Operational Port is required."));
			scr.BillOfLadingInfo.AddMessageErrorIfEmpty(Res.GetString("DE6D7E6D-85B3-4A17-A062-821C7E0E4C37", "BOL Number is required."));

			scr.ForwarderIdEOR.ValueInfo.AddMessageError(() =>
				scr.Containers.Any(c => c.IsTranferToForwarder)
				&& scr.ForwarderIdEOR.Value.IsEmpty
				&& scr.ForwarderIdDUN.Value.IsEmpty,
				Res.GetString("4225F28C-33D3-4CDB-8AA9-08E58CA4B9A5", "Forwarder ID is missing from this organization. Provide Registration Number EOR or DUN."));
			scr.ForwarderIdDUN.ValueInfo.AddMessageError(() =>
				scr.Containers.Any(c => c.IsTranferToForwarder)
				&& scr.ForwarderIdEOR.Value.IsEmpty
				&& scr.ForwarderIdDUN.Value.IsEmpty,
				Res.GetString("4225F28C-33D3-4CDB-8AA9-08E58CA4B9A5", "Forwarder ID is missing from this organization. Provide Registration Number EOR or DUN."));
			scr.TransportCompanyIdEOR.ValueInfo.AddMessageError(() =>
				scr.Containers.Any(c => !c.IsTranferToForwarder)
				&& scr.TransportCompanyIdEOR.Value.IsEmpty
				&& scr.TransportCompanyIdDUN.Value.IsEmpty,
				Res.GetString("4C258122-CAC9-49A4-9CB5-8D89ED4081D7", "Transport Company ID is missing from this organization. Provide Registration Number EOR or DUN."));
			scr.TransportCompanyIdDUN.ValueInfo.AddMessageError(() =>
				scr.Containers.Any(c => !c.IsTranferToForwarder)
				&& scr.TransportCompanyIdEOR.Value.IsEmpty
				&& scr.TransportCompanyIdDUN.Value.IsEmpty,
				Res.GetString("4C258122-CAC9-49A4-9CB5-8D89ED4081D7", "Transport Company ID is missing from this organization. Provide Registration Number EOR or DUN."));

			foreach (var container in scr.Containers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("028835E8-1CBF-4E5A-AFBC-E147813D3DC5", "Container Number is required."));
				container.ReleaseIdentificationInfo.AddMessageErrorIfEmpty(Res.GetString("B345E7C7-2DD2-4FC6-A33A-73F5CC98108B", "Release Identification is required."));

				container.OnValueChanged(nameof(container.IsTranferToForwarder)).Do(() =>
				{
					scr.ForwarderIdEOR.Validate(nameof(scr.ForwarderIdEOR.Value));
					scr.ForwarderIdDUN.Validate(nameof(scr.ForwarderIdDUN.Value));
					scr.TransportCompanyIdEOR.Validate(nameof(scr.TransportCompanyIdEOR.Value));
					scr.TransportCompanyIdDUN.Validate(nameof(scr.TransportCompanyIdDUN.Value));
				});
			}
		}

		#region Implement

		IEnumerable<ZGuid> GetSelectedContainerPKs()
		{
			if (parameters?.Data is IReadOnlyCollection<ForwardingContainer> containers)
			{
				return containers.Select(c => c.PK);
			}

			return new List<ZGuid>();
		}

		string GetFormMode()
		{
			switch (parameters.DocumentTitle)
			{
				case TMiningConstants.DocumentNames.TMiningSecureContainerReleaseRevoke:
					return SecureContainerRelease.FormModeRevoke;
				default:
					return SecureContainerRelease.FormModeTransfer;
			}
		}

		#endregion
	}
}
