using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.CFS.Business
{
	class CFSContainerRatingAdapter : RatingAdapter<CFSContainer>
	{
		public CFSContainerRatingAdapter(CFSContainer parent)
			: base(parent)
		{
		}

		#region RatingAdapter

		public override AdapterType AdapterType => AdapterType.CFSContainer;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new CFSContainerStorageJobDateProvider(Parent); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.Add(ChargeCodeGroupList.Codes.ContainerStorage);

				return result;
			}
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var jobServices = new JobServicesCollection();
				jobServices.Add(GetFreeStorageInfo);
				jobServices.Add(GetBondedStorageInfo);

				return jobServices;
			}
		}

		JobServiceInfo GetFreeStorageInfo
		{
			get
			{
				var freeStorageDays = GetFreeStorageDays();
				var duration = new TimeInfo(freeStorageDays, 0, 0).Span;
				var description = Res.GetString("765cb6f4-6354-4573-9108-46bd67ef8bf1", "Free Storage");

				return new JobServiceInfo(freeStorageDays > 0, ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage, description, null, duration);
			}
		}

		JobServiceInfo GetBondedStorageInfo
		{
			get
			{
				var bondedStorageDays = GetBondedStorageDays();
				var duration = new TimeInfo(bondedStorageDays, 0, 0).Span;
				var description = Res.GetString("e70051fc-4fbc-4a2f-a675-52522858e0db", "Bonded Storage");

				return new JobServiceInfo(bondedStorageDays > 0, ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage, description, null, duration);
			}
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.CFS; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.FCLStorage; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent.InvoicingSupporter; }
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		public override FreightMode FreightMode
		{
			get { return FreightMode.FCL; }
		}

		public override ZString ContainerMode
		{
			get { return Core.Constants.ContainerModes.FCL; }
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get { return new ServiceLevelRatingInformation(System.Array.Empty<ServiceLevelInfo>()); }
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				result.AddContainer(Parent.JC_RC);
				return result;
			}
		}

		ZDate StorageEndDate
		{
			get { return Parent.JC_DepartureTime.IsValid ? Parent.JC_DepartureTime.Date.AddDays(1) : ZDateTime.Today.Date.AddDays(1); }
		}

		#region Free Storage

		int GetFreeStorageDays()
		{
			var result = 0;

			var freeStorageStartDate = ZDate.Empty;

			if (Parent.JC_FCLStorageArrivedUnderbond)
			{
				if (Parent.JC_FCLStorageUnderbondCleared.IsValid)
				{
					freeStorageStartDate = Parent.JC_FCLStorageUnderbondCleared.Date.AddDays(1);
				}
			}
			else if (Parent.JC_ArrivalCTOStorageStartDate.IsValid)
			{
				freeStorageStartDate = Parent.JC_ArrivalCTOStorageStartDate.Date;
			}

			if (freeStorageStartDate.IsValid && freeStorageStartDate < StorageEndDate)
			{
				result = (StorageEndDate - freeStorageStartDate).Days;
			}

			return result;
		}

		#endregion

		#region Bonded Storage

		int GetBondedStorageDays()
		{
			var result = 0;

			var bondedStorageStartDate = ZDate.Empty;

			if (Parent.JC_FCLStorageArrivedUnderbond)
			{
				var bondedStorageEndDate = Parent.JC_FCLStorageUnderbondCleared.IsEmpty ? StorageEndDate : Parent.JC_FCLStorageUnderbondCleared.Date.AddDays(1);

				if (Parent.JC_ArrivalCTOStorageStartDate.IsValid)
				{
					bondedStorageStartDate = Parent.JC_ArrivalCTOStorageStartDate.Date;
				}

				if (bondedStorageStartDate.IsValid && bondedStorageStartDate < bondedStorageEndDate)
				{
					result = (bondedStorageEndDate - bondedStorageStartDate).Days;
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
