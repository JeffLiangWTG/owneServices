using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageHostCollection : SailingEndPointWrapperCollection<PortMessageHost>
	{
		public PortMessageHostCollection(JobVoyage voyage)
			: base(voyage)
		{ }

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return CreateNewElement(new NullEndPoint(Factory));
		}

		protected override PortMessageHost CreateNewElement(ISailingEndPoint endPoint)
		{
			return new PortMessageHost(endPoint);
		}

		protected override object GetKey(PortMessageHost bizObj)
		{
			return bizObj.EndPointPK;
		}

		#endregion

		#region NullEndPoint

		/// <summary>
		/// For retarded binding.
		/// </summary>
		internal class NullEndPoint : NonPersistentBusinessObject, ISailingEndPoint
		{
			public NullEndPoint(BusinessObjectFactory factory)
				: base(factory) { }

			#region ISailingEndPoint Members

			public ZString Direction
			{
				get { throw new NotSupportedException("The null endpoint exists only because .NET binding sux, you should never actually use these"); }
			}

			public ZDateTime EstimatedDate
			{
				get { throw new NotSupportedException("The null endpoint exists only because .NET binding sux, you should never actually use these"); }
			}

			public ZString Port
			{
				get { throw new NotSupportedException("The null endpoint exists only because .NET binding sux, you should never actually use these"); }
			}

			public ZString Vessel
			{
				get { throw new NotSupportedException("The null endpoint exists only because .NET binding sux, you should never actually use these"); }
			}

			public ZString Voyage
			{
				get { throw new NotSupportedException("The null endpoint exists only because .NET binding sux, you should never actually use these"); }
			}

			#endregion

			#region IEDIMessageCollectionProvider Members

			public EDIMessageCollection Messages
			{
				get { throw new NotSupportedException("The null endpoint exists only because .NET binding sux, you should never actually use these"); }
			}

			#endregion
		}

		#endregion
	}

	public abstract class SailingEndPointWrapperCollection<WrapperType> : NonPersistentBusinessObjectCollection<WrapperType> where WrapperType : NonPersistentBusinessObject
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected SailingEndPointWrapperCollection(JobVoyage jobVoyage)
			: base(jobVoyage.Factory)
		{
			voyage = jobVoyage;
			jobVoyage.Origins.CountChanged += EndPortsCountChanged<VoyageOrigin>;
			jobVoyage.Destinations.CountChanged += EndPortsCountChanged<VoyageDestination>;
			Rebuild();
		}

		#region Rebuild

		public void Rebuild()
		{
			RemoveAllButLeaveRelationshipsIntact();

			foreach (VoyageOrigin origin in voyage.Origins)
			{
				Add(CachedWrapElement(origin));
			}

			foreach (VoyageDestination destination in voyage.Destinations)
			{
				Add(CachedWrapElement(destination));
			}
		}

		#endregion

		#region Collection Overrides

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var host = (WrapperType)bizOAdded;
			base.OnAdded(host);
			var key = GetKey(host);

			if (!cache.ContainsKey(key))
			{
				cache.Add(key, host);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			var host = (WrapperType)bizO;
			base.OnRemoved(host);
			var key = GetKey(host);

			if (cache.ContainsKey(key))
			{
				cache.Remove(key);
			}
		}

		#endregion

		#region Implementation

		void EndPortsCountChanged<T>(object sender, CollectionCountChangedEventArgs e)
			where T : BusinessObject, ISailingEndPoint
		{
			var endPoint = (T)e.BizObject;

			if (e.ItemAdded)
			{
				Add(CachedWrapElement(endPoint));
			}
			else if (e.ItemRemoved)
			{
				var key = GetKey(endPoint);
				if (cache.ContainsKey(key))
				{
					Remove(cache[key]);
				}
			}
		}

		WrapperType CachedWrapElement<T>(T endPoint)
			where T : BusinessObject, ISailingEndPoint
		{
			WrapperType result;
			var key = GetKey(endPoint);

			if (!cache.TryGetValue(key, out result))
			{
				result = CreateNewElement(endPoint);
				cache.Add(key, result);
			}

			return result;
		}

		object GetKey(ISailingEndPoint endPoint)
		{
			return GetKey(CreateNewElement(endPoint));
		}

		#endregion

		protected abstract WrapperType CreateNewElement(ISailingEndPoint endPoint);
		protected abstract object GetKey(WrapperType bizObj);

		protected readonly JobVoyage voyage;
		readonly Dictionary<object, WrapperType> cache = new Dictionary<object, WrapperType>();
	}
}



