using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class EstimateSalesAnalysisFilter : AutoEstimateSalesAnalysisFilter
	{
		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Status = EstimateSalesAnalysisStatusFilterList.Codes.All;
			ShouldMatchOnBuyerSupplier = true;
		}

		#endregion

		#region Properties

		[List("StatusFilters")]
		public override ZString Status
		{
			get { return base.Status; }
			set { base.Status = value; }
		}

		#endregion

		#region Lookups

		public ICodeDescriptionPairList StatusFilters
		{
			get
			{
				if (statusFilters == null)
				{
					statusFilters = new EstimateSalesAnalysisStatusFilterList();
				}

				return statusFilters;
			}
		}
		ICodeDescriptionPairList statusFilters;

		#endregion
	}
}
