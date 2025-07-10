using System;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public abstract class CMMBaseCountryProcessor : ICMMCountryProcessor
	{
		protected CMMBaseCountryProcessor(CMMEmailGenerator emailBuilder, ICMMProcessingAdapter adapter)
		{
			if (emailBuilder == null)
			{
				throw new ArgumentNullException(nameof(emailBuilder));
			}

			if (adapter == null)
			{
				throw new ArgumentNullException(nameof(adapter));
			}

			this.EmailBuilder = emailBuilder;
			this.Adapter = adapter;
		}

		protected CMMEmailGenerator EmailBuilder { get; private set; }
		protected ICMMProcessingAdapter Adapter { get; private set; }

		public abstract void UpdateContainer(CMMMessageContainer containerData, AgencyShipmentContainer container);
		public abstract void ValidateContainer(CMMMessageContainer cmmMessageContainer, AgencyShipmentContainer container);
	}
}


