using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Freight.Business
{
	public abstract class CollectionLimitHelperForPotentialHVLV
	{
		protected CollectionLimitHelperForPotentialHVLV(BusinessObject parent)
		{
			this.parent = Argument.NotNull(parent, "parent");
		}

		readonly BusinessObject parent;

		protected abstract IList Collection { get; }
		protected abstract int Limit { get; }
		protected abstract DateTime LimitIntroductionTimeUtc { get; }
		protected abstract ZDateTime ParentCreationTimeUtc { get; }
		protected abstract string ChildPlural { get; }
		protected abstract string ChildSingular { get; }
		protected abstract string ParentSingular { get; }

		public void SetCollectionLimit(ISupportMaxCountValidation collection)
		{
			if (CollectionRequiresValidation)
			{
				collection.EnableMaxCountValidation(Limit, LimitError, true);
			}
		}

		public void CheckCollectionCountOnParent()
		{
			RemoveNotifications();

			var notification = CreateNotification();

			if (notification != null)
			{
				parent.AddRowNotification(notification);
			}
		}

		void RemoveNotifications()
		{
			var errorMessage = LimitError;
			parent.RemoveRowWarning(errorMessage);
			parent.RemoveRowError(errorMessage);
		}

		public INotification CreateNotification()
		{
			return CreateNotification(Collection.Count);
		}

		public INotification CreateNotification(int collectionCount)
		{
			if (CollectionRequiresValidation)
			{
				if (collectionCount > Limit)
				{
					return new Notification(NotificationType.Error, LimitError);
				}
				else if (collectionCount > Limit / 2)
				{
					return new Notification(NotificationType.Warning, LimitError);
				}
			}

			return null;
		}

		bool CollectionRequiresValidation
		{
			get
			{
				if (parent.IsDeleted)
				{
					return false;
				}

				var createTimeUtc = ParentCreationTimeUtc.IsValid ? ParentCreationTimeUtc : ZDateTime.UtcNow;
				return LimitIntroductionTimeUtc < createTimeUtc;
			}
		}

		string LimitError
		{
			get
			{
				return Res.GetString("7d6e4073-95b4-4937-955a-54580119bf7e", @"The number of {0} on a {1} is limited for performance and database management reasons to {3} {0}. Above {4} {0} you will receive this message for every additional {2} added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.",
				ChildPlural, // 0
				ParentSingular, // 1
				ChildSingular, // 2
				Limit, // 3
				Limit / 2 // 4
				);
			}
		}
	}
}
