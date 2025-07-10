using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARPaymentCycle : ARTermsCycle
	{
		public ARPaymentCycle()
		{
			SetDefaults();
		}

		public ARPaymentCycle(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			SetDefaults();
		}

		void SetDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				ToDay = 1;
				PaymentDay = 1;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ARPaymentCycle(fallbackLevel, factory);
		}

		#region Bound Properties

		#region ToDay

		public override void ValidateToDay()
		{
			ToDayInfo.ClearAllNotifications();

			if (ToDay < 1)
			{
				ToDayInfo.AddError(Res.GetString("80CB5296-1DC8-40CC-9D9B-54BD119D40B4", "The value must be 1 or greater."));
			}
			if (ParentCollection != null)
			{
				if (ParentCollection.OfType<ARPaymentCycle>().Count(x => x.ToDay == this.ToDay) >= 2)
				{
					ToDayInfo.AddError(Res.GetString("FE602F75-2821-41A4-B8AD-C4FFC8E3F3EE", "Payment cycle with the same 'Cycle #' already exists."));
				}
			}
		}

		protected override bool ToDay_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region PaymentDay

		public override ZByte PaymentDay
		{
			get { return fPaymentDay; }
			set
			{
				var oldValue = PaymentDay;
				SetNonPersistentPropertyValue(PaymentDayInfo, ref fPaymentDay, value);
				if (!IsValidationSuspended)
				{
					ValidatePaymentDay();
				}

				if (ParentCollection != null && (oldValue != value || (oldValue == 1 && ToDay == 1))) //so making a new row of 1 will reshuffle
				{
					foreach (ARPaymentCycle paymentCycle in ParentCollection)
					{
						paymentCycle.ValidatePaymentDay();

						var cycle = (ZByte)(ParentCollection.OfType<ARPaymentCycle>().Count(x => x.HasChanges && x.PaymentDay < paymentCycle.PaymentDay) + 1);
						if (paymentCycle.ToDay != cycle)
						{
							paymentCycle.ToDay = cycle;
							paymentCycle.ToDayInfo.RefreshBinding();
							paymentCycle.PaymentDayInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public override void ValidatePaymentDay()
		{
			PaymentDayInfo.ClearAllNotifications();

			base.ValidatePaymentDay();

			if (ParentCollection != null)
			{
				if (ParentCollection.OfType<ARPaymentCycle>().Count(x => x.PaymentDay == this.PaymentDay) >= 2)
				{
					PaymentDayInfo.AddError(Res.GetString("139294A0-C1A8-43AE-8167-31742BF4461A", "Payment cycle with the same 'Payment Day' value already exists."));
				}
			}
		}

		#endregion

		#region FromDayCalculated

		//should not be called on ARPaymentCycle
		public override ZByte FromDayCalculated
		{
			get
			{
				return ZByte.Zero;
			}
		}

		#endregion

		#endregion

		#region Implementation

		public override BusinessObjectCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ARPaymentCycleCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		#endregion
	}
}
