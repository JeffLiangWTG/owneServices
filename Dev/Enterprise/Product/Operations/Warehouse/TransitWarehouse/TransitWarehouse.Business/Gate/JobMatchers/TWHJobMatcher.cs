using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public abstract class TWHJobMatcher : ITWHJobMatcher
	{
		protected ITWHJobMatcher NextJobMatcher;
		protected BusinessObjectFactory Factory;
		protected WhsWarehouse TransitWarehouse;
		protected string ReferenceNumber;
		protected ReferenceNumberTypes ReferenceNumberType;

		public void SetNextJobMatcher(ITWHJobMatcher matcher)
		{
			NextJobMatcher = matcher;
		}

		public TWHJobMatcherResult Process()
		{
			if (IsReferenceTypeMatch())
			{
				var result = InternalProcess();

				if (NextJobMatcher != null && result.ErrorCode == ValidationErrorCode.JNF && ReferenceNumberType == ReferenceNumberTypes.Unknown)
				{
					return NextJobMatcher.Process();
				}

				return result;
			}
			else if (NextJobMatcher != null)
			{
				return NextJobMatcher.Process();
			}
			else
			{
				return new TWHJobMatcherResult()
				{
					ReferenceNumber = ReferenceNumber,
					ReferenceNumberType = ReferenceNumberType,
					ErrorCode = ValidationErrorCode.JNF
				};
			}
		}

		protected abstract TWHJobMatcherResult InternalProcess();

		protected abstract bool IsReferenceTypeMatch();
	}
}
