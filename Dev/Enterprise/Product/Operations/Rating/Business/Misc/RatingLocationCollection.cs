using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class RatingLocationCollection : LocationCollection, IFilterModuleExtraNotificationProvider
	{
		public RatingLocationCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public RatingLocationCollection(BusinessObjectFactory factory, ZoneTypeList requiredZoneTypes)
			: base(factory, requiredZoneTypes) { }

		public RatingLocationCollection(BusinessObjectFactory factory, bool allowZones)
			: base(factory, allowZones) { }

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			if (AllowZones && businessObject is RefZoneHeader zone)
			{
				var errorMessage = new StringBuilder();
				if (!zone.IsActive)
				{
					errorMessage.AppendLine(Res.GetString("4821209C-DE27-4D9A-AC04-2F2475CFDE4B", "This Zone is inactive - it may not be used."));
				}
				if (!zone.IsRatingAvailableZone)
				{
					errorMessage.AppendLine(Res.GetString("EDBFBE70-4ACD-4300-9A11-F30F068AAD06", "This Zone is not available for Rating purposes. The Zone type is {0}.", zone.Lookups.ZoneTypes.GetDescriptionFromCode(zone.FZ_ZoneType)));
				}

				if (errorMessage.Length != 0)
				{
					return new Notification(CargoWise.EntityFramework.NotificationType.Error, errorMessage.ToString());
				}
			}
			else if (businessObject is RefCountry country && !country.IsActive)
			{
				return new Notification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("00A02856-3C5B-455C-840A-AC78012415DF", "This Country/Region is inactive - it may not be used."));
			}
			else if (businessObject is RefUNLOCO port && !port.IsActive)
			{
				return new Notification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("88BC198A-2607-4B00-908D-60FA3E95223B", "This Port is inactive - it may not be used."));
			}

			return null;
		}
	}
}

