using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class NonSecurityJobConsolAWBSpecialHandlingCollection : DependentBusinessObjectCollection<NonSecurityJobConsolAWBSpecialHandling, ForwardingConsol>
	{
		public NonSecurityJobConsolAWBSpecialHandlingCollection(ForwardingConsol consol)
			: base(consol, consol.Factory)
		{
			this.EnableMaxCountValidation(9, false, NotificationType.Error, Res.GetString("58f7814f-7f3d-f3bb-48aa-64e2501c6c90", "A maximum of nine Special Handling Codes is possible for the FWB message"));
			this.CountChanged += NonSecurityJobConsolAWBSpecialHandlingCollection_CountChanged;
		}

		/// <summary>
		/// During Data Refresh, duplicate specail handlling codes will be added. Delete the duplicate entires if they are added by data refresh bus.
		/// Maybe this could be made generic/put in Core somewhere/opted into by Collections, but at the moment architecture team suggested to fix it inside the specific collection.
		/// </summary>
		void NonSecurityJobConsolAWBSpecialHandlingCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && IsUpdatingByDataRefreshBus
				&& e.BizObject is NonSecurityJobConsolAWBSpecialHandling newlyAddedItem
				&& this.Cast<NonSecurityJobConsolAWBSpecialHandling>().Count(s => s.JKH_Code == newlyAddedItem.JKH_Code) > 1)
			{
				RemoveAndDelete(e.BizObject);
			}
		}

		protected override string FkColumnName => JobConsolAWBSpecialHandlingSchema.Constants.JKH_JK_Consol;

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			var invalidCodes = new[]
			{
				AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
				SecurityJobConsolAWBSpecialHandling.NotSecured,
				string.Empty
			};

			var codeFilter = new ZQuery(JobConsolAWBSpecialHandlingSchema.JKH_Code, SQLComparisonOperator.NotEqual, invalidCodes);
			result.AddToFilter(codeFilter);

			return result;
		}
	}
}
