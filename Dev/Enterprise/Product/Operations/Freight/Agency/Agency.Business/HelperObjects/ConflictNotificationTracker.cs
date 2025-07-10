using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public static class ConflictNotificationTracker
	{
		public static void AddWarning(ZPropertyInfo info, ZString notification)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			Host host = GetHost(info.BizObj.Factory);
			PropertyTag tag;

			if (!host.PropertyTags.TryGetValue(info, out tag))
			{
				tag = new PropertyTag(host, info);
				host.PropertyTags.Add(info, tag);
			}

			tag.Notifications.Add(notification);
		}

		static Host GetHost(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(typeof(Host).FullName, () => new Host(factory));
		}

		sealed class Host
		{
			public Host(BusinessObjectFactory factory)
			{
				Factory = factory;
				PropertyTags = new Dictionary<ZPropertyInfo, PropertyTag>();
				Factory.Saving += Factory_Saving;
			}

			public BusinessObjectFactory Factory { get; private set; }
			public Dictionary<ZPropertyInfo, PropertyTag> PropertyTags { get; private set; }

			void Factory_Saving(BusinessObjectFactory factory)
			{
				foreach (PropertyTag tag in PropertyTags.Values)
				{
					tag.Close();
					((IBusinessObjectInternals)tag.Info.BizObj).Validate(tag.Info);
				}

				PropertyTags.Clear();
			}
		}

		sealed class PropertyTag
		{
			public PropertyTag(Host host, ZPropertyInfo info)
			{
				Host = host;
				Info = info;
				Notifications = new List<ZString>();
				Info.AdditionalValidation += Info_AdditionalValidation;
				Info.ValueChanged += Info_ValueChanged;
			}

			public void Close()
			{
				Info.AdditionalValidation -= Info_AdditionalValidation;
				Info.ValueChanged -= Info_ValueChanged;
			}

			public Host Host { get; private set; }
			public ZPropertyInfo Info { get; private set; }
			public List<ZString> Notifications { get; private set; }

			void Info_ValueChanged(object sender, EventArgs e)
			{
				PropertyTag tag;

				if (Host.PropertyTags.TryGetValue(Info, out tag))
				{
					tag.Close();
					Host.PropertyTags.Remove(Info);
				}
			}

			void Info_AdditionalValidation()
			{
				foreach (ZString notification in Notifications)
				{
					Info.AddWarning(notification);
				}
			}
		}
	}
}
