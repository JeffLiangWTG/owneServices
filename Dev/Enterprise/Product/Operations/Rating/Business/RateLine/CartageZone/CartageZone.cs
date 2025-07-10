using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Represents a single Cartage Zone. This could be a Transport Zone set
	/// or an ACI Zone.
	///
	/// Within this class there's also the listing of RateLineItems that
	/// correspond to this Cartage Zone.
	/// </summary>
	public class CartageZone : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string ZoneName = "ZoneName";
			public const string ZonePK = "ZonePK";
			public const string Description = "Description";
		}

		#endregion

		public CartageZone(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void InitialiseForTransportZone(ZGuid zonePK, ZString zoneName)
		{
			if (isInitialised)
			{
				throw new InvalidOperationException("Can only be initialised once");
			}

			ZonePK = zonePK;
			ZoneName = zoneName;

			isInitialised = true;
		}

		public void InitialiseForACIZone(ZString zoneName)
		{
			if (isInitialised)
			{
				throw new InvalidOperationException("Can only be initialised once");
			}

			ZoneName = zoneName;
			isInitialised = true;
		}

		#region ZoneName and ZonePK

		/// <summary>
		/// Both ACI Zone and TransportZone have a ZoneName
		/// </summary>
		public ZString ZoneName
		{
			get { return fZoneName; }
			private set
			{
				CheckMaximumLength(ZoneNameInfo, value);
				fZoneName = value;
				if (!IsValidationSuspended)
				{
					ValidateZoneName();
					ValidateDescription();
				}
				ZoneNameInfo.RefreshBinding();
			}
		}
		ZString fZoneName;

		public ZPropertyInfo ZoneNameInfo
		{
			get { return GetZPropertyInfo(Schema.ZoneName); }
		}

		public int ZoneName_MaxLength
		{
			get
			{
				return IsACIZone
					? RefDomesticCartageZone.Schema.F1_ZoneMaxLength
					: RateTransportZone.Schema.TZ_ZoneNameMaxLength;
			}
		}

		public void ValidateZoneName()
		{
			ZoneNameInfo.ClearAllNotifications();
		}

		/// <summary>
		/// Only Transport Zone has ZonePK
		/// </summary>
		public ZGuid ZonePK { get; private set; }

		public bool IsACIZone
		{
			get { return ZonePK.IsEmpty; }
		}

		#endregion

		#region Description

		[BusinessObjectTestExclude]
		[MaxLength(RateTransportZone.Schema.TZ_ZoneNameMaxLength)]
		public ZString Description
		{
			get { return (ZoneName.IsEmpty && ZonePK.IsEmpty) ? (ZString)Res.GetString("7955ecf4-c5b0-46ea-9949-320dde307d11", "Standard") : ZoneName; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();

			if (!ZoneName.IsEmpty && !Parent.Lookups.Zones.ContainsCode(ZoneName))
			{
				DescriptionInfo.AddError(ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		#endregion

		#region RateLineItems

		public RateLineItemsView ZoneRateLineItems
		{
			get
			{
				if (fRateLineItems == null && Parent != null)
				{
					fRateLineItems = new RateLineItemsView(this);
					RegisterEditableChildObject(fRateLineItems);
				}

				return fRateLineItems;
			}
		}

		RateLineItemsView fRateLineItems;

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsDeleted)
			{
				return;
			}

			ZoneRateLineItems.RemoveAndDeleteAll();

			base.Delete();
		}

		#endregion

		#region Parent

		public RateLine Parent
		{
			get
			{
				if (fParent == null)
				{
					foreach (var parentCollection in ParentCollections)
					{
						if (parentCollection is CartageZoneCollection)
						{
							fParent = ((CartageZoneCollection)parentCollection).Master;
							break;
						}
					}
				}

				return fParent;
			}
		}

		RateLine fParent;

		#endregion

		bool isInitialised;
	}
}

