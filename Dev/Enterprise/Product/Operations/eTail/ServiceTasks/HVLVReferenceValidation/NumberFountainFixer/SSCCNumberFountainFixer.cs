using Enterprise.NumberFountain;

namespace Enterprise.eTail.ServiceTasks
{
	class SSCCNumberFountainFixer : NumberFountainFixer
	{
		public SSCCNumberFountainFixer(NumberFountainInfo fountainInfo, IHVLVReferenceInfo referenceInfo)
			: base(fountainInfo, referenceInfo)
		{
		}

		protected override string GetExclusiveLowerBoundSearchValue(long number) => SSCCBarCodeChecker.GetSSCCWithoutCheckDigit(number, FountainInfo.Prefix);

		protected override string GetExclusiveUpperBoundSearchValue(long number) => SSCCBarCodeChecker.GetSSCCWithoutCheckDigit(number + 1, FountainInfo.Prefix);

		protected override int GetFullIdLengthFromSearchValue(string searchValue) => searchValue.Length + 1;
	}
}
