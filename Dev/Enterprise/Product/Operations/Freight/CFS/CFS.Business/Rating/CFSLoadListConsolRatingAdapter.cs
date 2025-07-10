using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.CFS.Business.Rating
{
	public class CFSLoadListConsolRatingAdapter : ConsolRatingAdapter<CFSLoadListConsol>
	{
		public CFSLoadListConsolRatingAdapter(IRatingRoute<IRoutingSupport> ratingRoute) : base(ratingRoute) { }

		public override AdapterType AdapterType => AdapterType.LoadList;

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return new ChargeCodeGroupCollection { ChargeCodeGroupList.Codes.CFSLoadList }; }
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = new JobServicesCollection();

				foreach (CodeDescriptionPair subGroup in ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.CFSLoadList))
				{
					var services = GetServiceInfos(subGroup.Code);
					result.AddRange(services);
				}

				return result;
			}
		}

		IEnumerable<JobServiceInfo> GetServiceInfos(string serviceCode)
		{
			var containers = Parent.Containers.Cast<CommonContainer>();
			var services = FreightRatingHelper.GetServiceInfosFromContainers(containers, serviceCode, GetServiceInfos);

			foreach (var service in services)
			{
				service.ChargeCodeGroup = ChargeCodeGroupList.Codes.CFSLoadList;
			}

			return services;
		}

		IEnumerable<JobServiceInfo> GetServiceInfos(CommonContainer container, ZString serviceName)
		{
			var cfsContainer = (CFSContainer)container;
			switch (serviceName)
			{
				case ChargeCodeSubGroupList.PackingCharges:
					return new[] { new JobServiceInfo(cfsContainer.HasPackDate, "", serviceName, "") };

				case ChargeCodeSubGroupList.UnpackingCharges:
					return new[] { new JobServiceInfo(cfsContainer.HasUnpackDate, "", serviceName, "") };

				default:
					return FreightRatingHelper.GetServiceInfoDefault(container, serviceName);
			}
		}

		protected override void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			var listOfContainerWrappers = Parent.Containers
				.Cast<CommonContainer>()
				.Select(x => new ContainerAndMassAndVolumeHelper(x));

			FreightRatingHelper.SetContainers(measures, null, listOfContainerWrappers, null);
		}

		public override FreightMode FreightMode
		{
			get
			{
				return TransportMode == Constants.TransportModes.Sea
						&& ContainerMode == Constants.ContainerModes.Groupage
					? FreightMode.GRP
					: base.FreightMode;
			}
		}

		public override ZString ContainerMode
		{
			get { return Parent.JK_ConsolMode; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				result[RatingDebtorOrgTypes.CNE] = Parent?.InvoicingSupporter?.Consignee;
				result[RatingDebtorOrgTypes.CNR] = Parent?.InvoicingSupporter?.Consignor;

				return result;
			}
		}

		public override IDocAddress DeliveryAddress => Parent?.InvoicingSupporter?.Consignee?.MainAddress;

		public override IDocAddress PickupAddress => Parent?.InvoicingSupporter?.Consignor?.MainAddress;

		public override RateType RateTypeToUse => RateType.CFS;

		public override AutoRatingStatusInfo StatusInformation => new AutoRatingStatusInfo(true, ZString.Empty);

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.CFSLoadList;
	}
}
