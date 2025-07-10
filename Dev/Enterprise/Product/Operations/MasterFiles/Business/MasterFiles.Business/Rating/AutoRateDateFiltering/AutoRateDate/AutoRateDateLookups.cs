using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AutoRateDateLookups : ZLookups
	{
		public AutoRateDateLookups(AutoRateDate parent)
			: base(parent)
		{
		}

		protected new AutoRateDate Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AutoRateDate)base.Parent; }
		}

		public CodeDescriptionPairList DirectionList => AutoRateDateLookupsHelper.DirectionList;

		public CodeDescriptionPairList ContainerModeList => AutoRateDateLookupsHelper.ContainerModeList;

		public CodeDescriptionPairList DateTypeList => AutoRateDateLookupsHelper.DateTypeList;

		public LocationCollection AutoRatingLocationCollection => AutoRateDateLookupsHelper.AutoRatingLocationCollection;

		public CodeDescriptionPairList RateTypeList => AutoRateDateLookupsHelper.RateTypeList;

		#region Implementation

		AutoRateDateLookupsHelper AutoRateDateLookupsHelper => autoRateDateLookupsHelper ??= new AutoRateDateLookupsHelper(Parent);
		AutoRateDateLookupsHelper autoRateDateLookupsHelper;

		#endregion
	}
}
