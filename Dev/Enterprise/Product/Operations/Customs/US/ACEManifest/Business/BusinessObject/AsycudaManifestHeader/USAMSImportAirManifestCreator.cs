using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	/// <summary>
	/// Creates US AMS Import Air Manifest for a Consol using mutex locks.
	/// </summary>
	public class USAMSImportAirManifestCreator : Integration.Customs.ASYCUDA.ACEManifest.IUSAMSImportAirManifestCreator
	{
		public USAMSImportAirManifestCreator(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}
		readonly ForwardingConsol consol;

		public ZString CheckSupported
		{
			get
			{
				var result = string.Empty;
				var headerWrapper = new ManifestHeadersWrapper(consol);
				if (consol.IsDeleted)
				{
					result = Res.GetString("31BFA83B-1F5B-44A2-B048-A7CA69B9B343", "This Consol {0} has been deleted, please check again.", consol.HumanReadableName);
				}
				else if (!consol.IsAir)
				{
					result = Res.GetString("1FB32DE8-C753-40D7-AF2D-84A218203E8D", "The automatic creation of US AMS is not available for this transport mode by {0}.", consol.TransportMode);
				}
				else if (headerWrapper.GetConsolCountriesAndTransportModesThatMightNeedManifest().Count == 0)
				{
					result = Res.GetString("4D7A34A4-6B73-4849-B0A4-5629BCCE802A", "This Consol does not require a manifest.");
				}
				return result;
			}
		}

		public DisposableAction TryCreateUSAMSData(INotifications notifications)
		{
			var result = DisposableAction.NoAction;
			if (CheckSupported.IsEmpty)
			{
				var mutexForConsol = AsycudaManifestHeader.CreateMutex(consol.PK);
				if (mutexForConsol.Lock())
				{
					CreateAMS(notifications);
				}
				else
				{
					notifications?.AddError(Res.GetString("4601E23E-3F3F-4BC2-AAEE-7CA0B1D57C0F", "Someone else is already in the process of creating a Customs Manifest for this Consol {0}.\r\nYou should be able to access the Customs Manifest when the person has saved the record. Please try later.", consol.HumanReadableName));
				}
				result = new DisposableAction(() => ((IDisposable)mutexForConsol).Dispose());
			}
			return result;
		}

		void CreateAMS(INotifications notifications)
		{
			var useConsolFactory = !consol.IsInDatabase;
			var factory = useConsolFactory ? consol.Factory : new BusinessObjectFactory();
			var header = GetExistingAMS(factory);
			if (header == null)
			{
				header = factory.New<AsycudaManifestHeader>();
				header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
				header.AMA_ParentId = consol.PK;
				header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
				header.SynchroniseWithSourceIfNeeded();
			}

			if (!useConsolFactory)
			{
				try
				{
					factory.Save();
				}
				catch (Exception ex)
				{
					notifications?.AddError(Res.GetString("0B6F3F78-8421-4BA2-8DD1-6CBB01FD1ED7", "An error occurred when creating an US AMS Import Air Manifest for this consol {0}. {1}", consol.HumanReadableName, ex.Message));
				}
			}
		}

		public AsycudaManifestHeader GetExistingAMS(BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_TransportMode, Core.Constants.TransportModes.Air);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, ACEManifestTypes.Codes.IAM);
			query.FetchOnlyFromLocalCache = !consol.IsInDatabase;
			query.ReLoadExistingRows = true;
			return factory.LoadTop1<AsycudaManifestHeader>(query);
		}
	}
}
