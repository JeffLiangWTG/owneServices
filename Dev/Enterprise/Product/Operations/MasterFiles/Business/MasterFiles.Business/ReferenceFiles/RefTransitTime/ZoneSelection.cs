using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ZoneSelection : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZoneSelection(BusinessObject parent, string domesticColumn, string internationalColumn)
			: base(Argument.NotNull(parent, "parent").Factory)
		{
			this.parent = parent;
			this.domesticColumn = domesticColumn;
			this.internationalColumn = internationalColumn;

			SetDefaults();
		}

		readonly BusinessObject parent;
		readonly string domesticColumn;
		readonly string internationalColumn;

		#region Common

		void SetDefaults()
		{
			isDomestic = !((ZGuid)parent[domesticColumn]).IsEmpty;
			zonePK = isDomestic ? (ZGuid)parent[domesticColumn] : (ZGuid)parent[internationalColumn];
		}

		[ResourceStringData("ZoneSelection|IsDomestic", Caption = "Domestic")]
		public ZBool IsDomestic
		{
			get { return isDomestic; }
			set
			{
				if (isDomestic != value)
				{
					ZonePK = ZGuid.Empty;
					SetNonPersistentPropertyValue(IsDomesticInfo, ref isDomestic, value);
				}
			}
		}

		ZBool isDomestic;

		public ZPropertyInfo IsDomesticInfo
		{
			get { return GetZPropertyInfo(nameof(IsDomestic)); }
		}

		[List("ZoneCollection")]
		[ResourceStringData("ZoneSelection|ZonePK", Caption = "Zone")]
		public ZGuid ZonePK
		{
			get { return zonePK; }
			set
			{
				SetNonPersistentPropertyValue(ZonePKInfo, ref zonePK, value);

				var proxiedColumn = isDomestic ? domesticColumn : internationalColumn;
				parent[proxiedColumn] = value;

				if (!IsValidationSuspended)
				{
					ValidateZonePK();
				}
			}
		}

		ZGuid zonePK;

		public ZPropertyInfo ZonePKInfo
		{
			get { return GetZPropertyInfo(nameof(ZonePK)); }
		}

		public IRatingZone SelectedZone
		{
			get { return IsDomestic ? (IRatingZone)TransportZone : InternationalZone; }
		}

		public OrgHeader SelectedZoneRelatedOrg
		{
			get { return Factory.Load<OrgHeader>(RelatedOrgPK); }
		}

		ZGuid RelatedOrgPK
		{
			get { return SelectedZone != null ? SelectedZone.RelatedOrgPK : ZGuid.Empty; }
		}

		public IBusinessObjectCollection ZoneCollection
		{
			get { return isDomestic ? ObjectFactory.Get<IRateTransportZonesCollection>("IRateTransportZonesCollection", Factory) : new RefZoneHeaderCollection(Factory); }
		}

		public OrgHeaderCollection OrgCollection
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Domestic

		IRateTransportZone TransportZone
		{
			get { return IsDomestic ? Factory.Load<IRateTransportZone>(ZonePK) : null; }
		}

		[List("OrgCollection")]
		[ResourceStringData("ZoneSelection|TransportZoneOwnerPK", Caption = "Zone Owner")]
		public ZGuid TransportZoneOwnerPK
		{
			get { return RelatedOrgPK; }
		}

		[List("Countries")]
		[ResourceStringData("ZoneSelection|TransportZoneCountry", Caption = "Country/Region")]
		public ZString TransportZoneCountry
		{
			get { return TransportZone != null && TransportZone.TransportProvider != null ? TransportZone.TransportProvider.TP_RN_NKCountry : ZString.Empty; }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#region International

		RefZoneHeader InternationalZone
		{
			get { return !IsDomestic ? Factory.Load<RefZoneHeader>(ZonePK) : null; }
		}

		[List("OrgCollection")]
		[ResourceStringData("ZoneSelection|InternationalZoneCarrierPK", Caption = "Carrier/Customer")]
		public ZGuid InternationalZoneCarrierPK
		{
			get { return RelatedOrgPK; }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateZonePK();
		}

		void ValidateZonePK()
		{
			ZonePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ZonePKInfo);
			ListValidation.ErrorIfInvalidPK(ZonePKInfo);
		}

		#endregion
	}
}
