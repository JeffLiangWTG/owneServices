using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	class ReconEntryFeeComparer : IComparer<IReconciliationImportEntryFee>
	{
		public int Compare(IReconciliationImportEntryFee x, IReconciliationImportEntryFee y)
		{
			return GetOrder(x.FeeClass).CompareTo(GetOrder(y.FeeClass));
		}

		int GetOrder(ZString feeClass)
		{
			switch (feeClass)
			{
				case ReconDeclaration.Constants.DutyAccountingClassCode:
					return 1;
				case Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing:
					return 2;
				default:
					return 3;
			}
		}
	}
}
