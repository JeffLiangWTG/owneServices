using System;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportCommon.Module
{
	public abstract class DtbTransportController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region GetNewBusinessEntityInLocalFactory

		protected sealed override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var transport = (DtbTransport)base.GetNewBusinessEntityInLocalFactory();
			var consolidation = (DtbTransportConsolidation)Factory.New(TransportConsolidationType);
			using (transport.SuspendSettingHasChanges())
			{
				SetupNewBusinessEntity(transport);
				consolidation.Bookings.Add(transport);
			}
			return transport;
		}

		protected abstract Type TransportConsolidationType { get; }

		protected virtual void SetupNewBusinessEntity(DtbTransport transport)
		{
		}

		#endregion
	}
}
