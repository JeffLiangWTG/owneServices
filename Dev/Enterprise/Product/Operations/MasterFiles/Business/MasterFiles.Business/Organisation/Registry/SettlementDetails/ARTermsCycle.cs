using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARTermsCycle : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ToDay = "ToDay";
			public const string PaymentDay = "PaymentDay";
			public const string FromDayCalculated = "FromDayCalculated";
		}

		#endregion

		public ARTermsCycle()
		{
			SetDefaults();
		}

		public ARTermsCycle(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			SetDefaults();
		}

		void SetDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				ToDay = 31;
				PaymentDay = 1;
				fromDayCalculated = (ZByte)255;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ARTermsCycle(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateToDay();
			ValidatePaymentDay();
		}

		#region Bound Properties

		#region ToDay

		public virtual ZByte ToDay
		{
			get { return fToDay; }
			set
			{
				var oldValue = ToDay;
				SetNonPersistentPropertyValue(ToDayInfo, ref fToDay, value);

				if (!IsValidationSuspended)
				{
					if (ParentCollection != null && oldValue != value)
					{
						foreach (ARTermsCycle termsCycle in ParentCollection)
						{
							termsCycle.ValidateToDay();
							termsCycle.fromDayCalculated = (ZByte)255;
							termsCycle.FromDayCalculatedInfo.RefreshBinding();
						}
					}
				}
			}
		}

		protected ZByte fToDay;

		protected virtual bool ToDay_ReadOnly
		{
			get { return false; }
		}

		public ZPropertyInfo ToDayInfo
		{
			get { return GetZPropertyInfo(Schema.ToDay); }
		}

		public virtual void ValidateToDay()
		{
			ToDayInfo.ClearAllNotifications();

			if (ToDay < 1 || ToDay > 31)
			{
				ToDayInfo.AddError(Res.GetString("978f010f-8021-4454-a99d-c56f40b55720", "The value must be between 1 and 31."));
			}
			if (ParentCollection != null)
			{
				if (ParentCollection.OfType<ARTermsCycle>().Count(x => x.ToDay == this.ToDay) >= 2)
				{
					ToDayInfo.AddError(Res.GetString("963C7FC7-5474-4426-AF12-0BBFA157A03F", "Terms cycle with the same 'To Day' value already exists."));
				}
			}
		}

		#endregion

		#region PaymentDay

		public virtual ZByte PaymentDay
		{
			get { return fPaymentDay; }
			set
			{
				SetNonPersistentPropertyValue(PaymentDayInfo, ref fPaymentDay, value);
				if (!IsValidationSuspended)
				{
					ValidatePaymentDay();
				}
			}
		}

		protected ZByte fPaymentDay;

		protected bool PaymentDay_ReadOnly
		{
			get { return false; }
		}

		public ZPropertyInfo PaymentDayInfo
		{
			get { return GetZPropertyInfo(Schema.PaymentDay); }
		}

		public virtual void ValidatePaymentDay()
		{
			PaymentDayInfo.ClearAllNotifications();

			if (PaymentDay < 1 || PaymentDay > 31)
			{
				PaymentDayInfo.AddError(Res.GetString("25731183-921F-4BA8-BA65-C98735910E1B", "The value must be between 1 and 31."));
			}
		}

		#endregion

		#region FromDayCalculated

		public virtual ZByte FromDayCalculated
		{
			get
			{
				if (fromDayCalculated == (ZByte)255)
				{
					//fromDayCalculated = () =>
					{
						ZByte result = ToDay;
						if (ParentCollection != null)
						{
							ZByte previousARTermsCycleToDay = (
								from termsCycle in ParentCollection.OfType<ARTermsCycle>()
								where termsCycle.ToDay < ToDay
								orderby termsCycle.ToDay descending
								select termsCycle.ToDay)
								.FirstOrDefault();
							result = previousARTermsCycleToDay == ZByte.Zero ? ParentCollection.OfType<ARTermsCycle>().Max(termsCycle => termsCycle.ToDay) : previousARTermsCycleToDay;
						}
						result++;
						fromDayCalculated = result > 31 ? (ZByte)1 : result;
					}
				}

				return fromDayCalculated;
			}
		}
		ZByte fromDayCalculated;

		public ZPropertyInfo FromDayCalculatedInfo
		{
			get { return GetZPropertyInfo(Schema.FromDayCalculated); }
		}

		#endregion

		#endregion

		#region Implementation

		public virtual BusinessObjectCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ARTermsCycleCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ToDay, ToDay.ToString());
			writer.WriteElementString(Schema.PaymentDay, PaymentDay.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ToDay = new ZByte(reader.ReadElementString(Schema.ToDay));
			PaymentDay = new ZByte(reader.ReadElementString(Schema.PaymentDay));
		}

		#endregion
	}
}
