using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CSTRateEntryCollection : RateEntryCollection
	{
		public CSTRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}

		#region Rate Entry Type

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.CST; }
		}

		#endregion
	}
}

