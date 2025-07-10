using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	/// <summary>
	/// Creates US AMS for a Consol using mutex locks.
	/// </summary>
	public class CusUSAMSCreator : Integration.Customs.US.USAMS.ICusUSAMSCreator
	{
		public CusUSAMSCreator(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}
		readonly ForwardingConsol consol;

		public ZString CheckSupported
		{
			get
			{
				var result = string.Empty;
				if (consol.IsDeleted)
				{
					result = Res.GetString("90A9B169-F641-4919-8965-9C4124129129", "This Consol {0} has been deleted, please check again.", consol.HumanReadableName);
				}
				else if (!consol.IsSea && !consol.IsRail)
				{
					result = Res.GetString("611640AF-C923-43B2-A6B9-2D34B2CEA56B", "The automatic creation of US AMS is not available for this transport mode by {0}.", consol.TransportMode);
				}
				else if (!consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates)
					&& !consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.PuertoRico))
				{
					result = Res.GetString("8C17ED72-AA6B-42CA-A069-27298402B792", "The automatic creation of US AMS is only applicable to Consols transported via US/PR.");
				}
				return result;
			}
		}

		public DisposableAction TryCreateUSAMSData(INotifications notifications)
		{
			var result = DisposableAction.NoAction;
			if (CheckSupported.IsEmpty)
			{
				var mutexForConsol = CusInBondHeader.CreateMutex(consol.PK);
				if (mutexForConsol.Lock())
				{
					CreateAMS(notifications);
				}
				else
				{
					notifications?.AddError(Res.GetString("E2346FB6-66C5-44DF-91A4-78E58937224C", "Someone else is already in the process of creating an AMS for this Consol {0}.\r\nYou should be able to access the AMS when the person has saved the record. Please try later.", consol.HumanReadableName));
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
				header = factory.New<CusInBondHeader>();
				header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
				header.BH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				header.BH_ParentID = consol.PK;
				((Integration.Customs.US.USAMS.ICusInBondHeaderWithConsolSynchonisation)header).SynchroniseWithConsolIfNeeded();
			}

			if (!useConsolFactory)
			{
				try
				{
					factory.Save();
				}
				catch (Exception ex)
				{
					notifications?.AddError(Res.GetString("D62720CD-B13E-462E-9E2E-24B136F0E8C7", "An error occurred when creating an AMS for this consol {0}. {1}", consol.HumanReadableName, ex.Message));
				}
			}
		}

		CusInBondHeader GetExistingAMS(BusinessObjectFactory factory)
		{
			var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, consol.PK);
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = !consol.IsInDatabase;
			query.ReLoadExistingRows = true;
			return factory.LoadTop1<CusInBondHeader>(query);
		}
	}
}
