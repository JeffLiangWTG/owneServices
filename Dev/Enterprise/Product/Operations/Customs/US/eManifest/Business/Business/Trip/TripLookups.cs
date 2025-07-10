using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class TripLookups : CusInBondHeaderLookups
	{
		public TripLookups(Trip parent)
			: base(parent)
		{
		}

		new Trip Parent
		{
			get { return (Trip)base.Parent; }
		}

		public ICodeDescriptionPairList TransitDirectionCodes
		{
			get { return Factory.GetCachedValue<TransitDirectionCodes>(); }
		}

		public ICodeDescriptionPairList TransportModes
		{
			get { return Factory.GetCachedValue<TransportModes>(); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleDPortCodes
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = Parent;
				if (parent.PortUnladingDCodeIsDropEdit)
				{
					result = parent.PortUnladingDRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			}
		}

		public ZZRefCusCodeListCombinedCollection ScheduleKPortCodes
		{
			get
			{
				return Factory.GetCachedValue("ScheduleKPortCodes", delegate()
				{
					var foreignPorts = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
					var result = new ZZRefCusCodeListCombinedCollection(Factory);
					foreach (var foreignPort in foreignPorts)
					{
						result.Add(foreignPort);
					}
					return result;
				});
			}
		}

		public IBusinessObjectCollection SCACCarrierCodes
		{
			get { return new USCarrierCombinedCollection(Factory, Trip.Truck); }
		}

		public IBusinessObjectCollection Organizations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public new IBusinessObjectCollection Importers
		{
			get { return new DebtorCollection(Factory); }
		}

		public IBusinessObjectCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public IBusinessObjectCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public ICodeDescriptionPairList MessageStatusList
		{
			get
			{
				var last = Parent.Messages.Find((x) =>
				{
					return x.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.USeManifest)
						&& x.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Transmit)
						&& !x.IsCancelled
						&& !x.IsPending;
				}).OrderByDescending(x => x.EM_SystemCreateTimeUtc).ThenByDescending(x => x.EM_MessageNum).FirstOrDefault();

				var type = last == null ? ZString.Empty : last.EM_MessageType;
				return Factory.GetCachedValue(
					string.Format("US.eManifest.Trip.MessageStatusList.{0}", type),
					() => new MessageStatusList(new MessageTypes().GetMultilingualDescriptionFromCode(type)));
			}
		}

		public ICodeDescriptionPairList ReleaseStatusList
		{
			get
			{
				return Factory.GetCachedValue(
					"US.eManifest.Trip.ReleaseStatusList",
					() =>
					{
						var result = new TripEntryStatusList();
						result.AddRange(new EntryStatusList());
						result.AddRange(new MessageTypes());
						return result;
					});
			}
		}
	}
}
