using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	[ModuleID(ModuleId.JobConsol)]
	public class ColoadConsolCollection : ActiveBusinessObjectCollection<ForwardingConsol>, IFilterModuleExtraNotificationProvider
	{
		public ColoadConsolCollection(ForwardingConsol masterConsol)
			: base(masterConsol.Factory, masterConsol, new ZQuery(), JobConsolSchema.JK_JK_MasterConsol)
		{
		}

		ColoadConsolCollection(BusinessObjectFactory factory, ZString freightNumber, ZDateTime cutOffDate)
			: base(factory)
		{
			FreightNumber = freightNumber;
			CutOffDate = cutOffDate;
		}

		public static ColoadConsolCollection GetColoadConsolsList(BusinessObjectFactory factory, ZString freightNumber, ZDateTime cutOffDate)
		{
			var additionalFilter = new ZQuery(JobConsolSchema.JK_AgentType, Constants.AgentType.AWBCoload);
			additionalFilter.AddToFilter(new ZQuery(JobConsolSchema.JK_AgentType, Constants.AgentType.Direct), JoinCondition.Or);
			additionalFilter.AddToFilter(JobConsolSchema.JK_JK_MasterConsol, SQLComparisonOperator.Equal, null);

			var collection = new ColoadConsolCollection(factory, freightNumber, cutOffDate);
			collection.AdditionalFilter.AddToFilter(additionalFilter);

			return collection;
		}

		readonly ZString FreightNumber;

		#region Implementation

		readonly ZDateTime CutOffDate;

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region AddNotificationWhenAdditionalFilterNotMet

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			var consol = (ForwardingConsol)selectedBusinessObject;
			if (!consol.CouldBeAttachedToMultiAWBMaster)
			{
				errors.Add(UnableToAttachToCLMConsolErrorMessage);
			}
			else if (!consol.JK_JK_MasterConsol.IsEmpty && consol.MasterConsol != null)
			{
				errors.Add(ResString.GetMultilingualString("f697bc12-20f6-4d77-892e-23fd9383a4c7", "This consol is already attached to Multi AWB Master {0}.", consol.MasterConsol.JK_UniqueConsignRef));
			}
		}

		ResourceString UnableToAttachToCLMConsolErrorMessage => ResString.GetMultilingualString("dfdccbd1-a3fe-92b6-4841-193b430d4bfb", "Only AWB Coload consols and Direct consols can be attached to Multi AWB Master.");

		#endregion

		#region IFilterModuleExtraNotificationProvider Members

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			var consol = businessObject as ForwardingConsol;

			if (consol != null)
			{
				if (consol.Transports.Cast<Transport>().All(x => x.JW_VoyageFlight != FreightNumber))
				{
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("7f2d30d7-bf5e-4c00-b50e-30e9fc07ffac", "Only AWB on the same flight can be consolidated together."));
				}

				if (!Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed && CutOffDate.IsValid && CutOffDate < ZDateTime.UtcNow)
				{
					var notificationType = CargoWise.ComponentModel.NotificationType.Error;
					var message = ZString.Format(Res.GetString("5b55ac89-370b-4cd2-a244-05af0c8e663b", "Cannot attach this consol to the AWB Master consol as the Cut Off Date ({0}) has passed."), new ZDateTime(Env.Time.GetLocalTimeFromUtc(CutOffDate.ToDateTime())).ToBestReadableDateTimeString());

					return new Notification(notificationType, message);
				}
			}

			return null;
		}

		#endregion

	}
}
