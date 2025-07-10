using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CartageZoneCollection : NonPersistentBusinessObjectCollection<CartageZone>
	{
		public CartageZoneCollection(RateLine master)
			: base(master.Factory)
		{
			Master = master;
		}

		public readonly RateLine Master;

		#region Load

		public override void Load()
		{
			using (SuspendListChanged())
			{
				SuspendValidation();
				try
				{
					RemoveAll();

					var newItem = AddNew();

					foreach (ICodeDescription pair in Master.Lookups.Zones)
					{
						newItem = AddNew();

						if (pair.PK != null)
						{
							newItem.InitialiseForTransportZone((ZGuid)pair.PK, pair.Code);
						}
						else
						{
							newItem.InitialiseForACIZone(pair.Code);
						}
					}
				}
				finally
				{
					ResumeValidation();
				}
			}
		}

		#endregion

		#region Delete

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var zone = (CartageZone)elementToDelete;
			if (!zone.ZonePK.IsEmpty && !Master.Lookups.Zones.ContainsCode(zone.ZoneName))
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		#endregion

		#region Find

		public bool ContainsZone(ZGuid zonePK)
		{
			return FindZone(zonePK) != null;
		}

		public bool ContainsZone(ZString zoneName)
		{
			return FindZone(zoneName) != null;
		}

		public CartageZone FindZone(ZGuid zonePK)
		{
			return this.Cast<CartageZone>().FirstOrDefault(x => x.ZonePK == zonePK);
		}

		public CartageZone FindZone(ZString zoneName)
		{
			return this.Cast<CartageZone>().FirstOrDefault(x => x.ZoneName == zoneName);
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CartageZone(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

