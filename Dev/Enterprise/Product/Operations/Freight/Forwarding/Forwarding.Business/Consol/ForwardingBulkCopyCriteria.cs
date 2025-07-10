using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingBulkCopyCriteria : BulkCopyCriteria, Enterprise.Integration.Forwarding.IForwardingBulkCopyCriteria
	{
		public ForwardingBulkCopyCriteria(ZGuid sailingPK)
			: base(sailingPK)
		{
		}

		public ForwardingBulkCopyCriteria(ZGuid sailingPK, bool onlyThisSailing)
			: base(sailingPK, onlyThisSailing)
		{
		}

		protected override BulkSailingConsolGenerator GetNewSailingConsolGenerator()
		{
			return new ForwardingBulkSailingConsolGenerator(Factory);
		}

		protected override void Dispose()
		{
			(consolDetails as IDisposable)?.Dispose();
			base.Dispose();
		}
	}
}
