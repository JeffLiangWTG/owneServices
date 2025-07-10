using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	public sealed class BulkCopyCriteriaForTest : BulkCopyCriteria
	{
		public BulkCopyCriteriaForTest(ZGuid sailingPK)
			: base(sailingPK)
		{
		}

		public BulkCopyCriteriaForTest(ZGuid sailingPK, bool onlyThisSailing)
			: base(sailingPK, onlyThisSailing)
		{
		}

		protected override BulkSailingConsolGenerator GetNewSailingConsolGenerator()
		{
			return new BulkSailingConsolGeneratorForTest(Factory);
		}

		public Action DisposeImplementaion { get; set; }
		protected override void Dispose()
		{
			if (DisposeImplementaion != null)
			{
				DisposeImplementaion();
			}
		}
	}
}
