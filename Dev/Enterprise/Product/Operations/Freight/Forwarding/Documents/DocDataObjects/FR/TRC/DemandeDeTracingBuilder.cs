using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public sealed class DemandeDeTracingBuilder
	{
		public DemandeDeTracingBuilder(ITRCDetails trcDetails, IReadOnlyCollection<CommonContainer> containers, DemandeDeTracingDirection direction)
		{
			this.trcDetails = Argument.NotNull(trcDetails, nameof(trcDetails));
			this.containers = Argument.NotNull(containers, nameof(containers));
			this.direction = direction;
			this.context = new CommonContext(trcDetails.BusinessObject.Factory.GetCachedReadOnlyFactory());
		}

		readonly ITRCDetails trcDetails;
		readonly IReadOnlyCollection<CommonContainer> containers;
		readonly DemandeDeTracingDirection direction;
		readonly IContext context;

		public DemandeDeTracing Build()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				var demandeDeTracing = new DemandeDeTracing(
					trcDetails.SourceType,
					trcDetails.SourceID);

				demandeDeTracing.ConsolNumber = trcDetails.SourceID;

				demandeDeTracing.PortOfOrigin = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = trcDetails.PortOfOrigin
				};

				demandeDeTracing.PortOfDestination = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = trcDetails.PortOfDestination
				};

				demandeDeTracing.OperationalPort = direction == DemandeDeTracingDirection.Import
					? Unloco.Create(context, trcDetails.OperationalPortImport)
					: Unloco.Create(context, trcDetails.OperationalPortExport);

				demandeDeTracing.BookingConfirmationReference = trcDetails.BookingConfirmationReference;

				demandeDeTracing.ContainerMode = new CodeDescription(trcDetails.ContainerModeList)
				{
					Code = trcDetails.ContainerMode
				};

				demandeDeTracing.ShipmentType = new CodeDescription(trcDetails.ShipmentTypeList)
				{
					Code = trcDetails.ShipmentType
				};

				demandeDeTracing.WaybillNumber = trcDetails.WaybillNumber;

				PopulateAddresses(demandeDeTracing);
				PopulateContainers(demandeDeTracing);
				PopulateDates(demandeDeTracing);

				demandeDeTracing.ValidateAllIncludingChildren();

				return demandeDeTracing;
			}
		}

		#region Implementation

		void PopulateAddresses(DemandeDeTracing demandeDeTracing)
		{
			switch (direction)
			{
				case DemandeDeTracingDirection.Import:
					demandeDeTracing.ReceivingForwarder = AddressBuilder.Create(context, trcDetails.ReceivingForwarder);
					demandeDeTracing.Carrier = AddressBuilder.Create(context, trcDetails.Carrier);
					break;

				case DemandeDeTracingDirection.Export:
					demandeDeTracing.SendingForwarder = AddressBuilder.Create(context, trcDetails.SendingForwarder);
					demandeDeTracing.Carrier = AddressBuilder.Create(context, trcDetails.Carrier);
					break;
			}

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress
				?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;

			demandeDeTracing.SendingParty = AddressBuilder.CreateForCurrentUser(context);
			demandeDeTracing.SendingPartySON = proxyMainAddress.GetRegistrationNumberFromRegistry(context, OrgCusCode.FranceCodeTypes.SON, true, demandeDeTracing.OperationalPort);
			demandeDeTracing.SendingPartyCI5 = proxyMainAddress.GetRegistrationNumberFromRegistry(context, OrgCusCode.FranceCodeTypes.CI5, true, demandeDeTracing.OperationalPort);
		}

		void PopulateContainers(DemandeDeTracing demandeDeTracing)
		{
			var demandeDeTracingContainers = new List<DemandeDeTracingContainer>();

			foreach (var forwardingContainer in containers)
			{
				var container = new DemandeDeTracingContainer(forwardingContainer.PK)
				{
					ContainerNumber = forwardingContainer.JC_ContainerNum,
					IsNonOperativeReefer = forwardingContainer.JC_IsNonOperativeReefer
				};

				container.ContainerNumberInfo.AddMessageErrorIfEmpty(Res.GetString("053c2048-1542-4408-9df4-2540cf2a4716", "Container Number is required"));
				demandeDeTracingContainers.Add(container);
			}

			demandeDeTracing.Containers = demandeDeTracingContainers;
		}

		void PopulateDates(DemandeDeTracing demandeDeTracing)
		{
			switch (direction)
			{
				case DemandeDeTracingDirection.Export:
					demandeDeTracing.ETD = trcDetails.ETD;
					break;

				case DemandeDeTracingDirection.Import:
					demandeDeTracing.ETA = trcDetails.ETA;
					break;
			}
		}
		#endregion
	}
}
