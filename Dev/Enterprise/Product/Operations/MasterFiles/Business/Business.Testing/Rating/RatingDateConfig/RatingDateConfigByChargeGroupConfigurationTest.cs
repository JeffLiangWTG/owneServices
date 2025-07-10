using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDateConfigByChargeGroupConfiguration))]
	public class RatingDateConfigByChargeGroupConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRatingDateConfigByChargeGroupConfiguration()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfig11 = orgHeader.MiscServ.RatingDateConfigs.AddNew();
			ratingDateConfig11.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ratingDateConfig11.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			ratingDateConfig11.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Import;

			var ratingDateConfig12 = orgHeader.MiscServ.RatingDateConfigs.AddNew();
			ratingDateConfig12.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			ratingDateConfig12.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			ratingDateConfig12.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Export;

			var ratingDateConfig21 = orgHeader.MiscServ.RatingDateConfigs.AddNew();
			ratingDateConfig21.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig21.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsol.Code;
			ratingDateConfig21.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Other;

			AssertContainsExactElementsInAnyOrder
			(
				expected: new[]
				{
					"BRK => ",
					"BON => ",
					"CLL => ",
					"CSH => ",
					"CST => ",
					"DST => FCN-EXP",
					"FRT => GCN-OTH",
					"INS => ",
					"LHR => ",
					"LOD => ",
					"ORG => FCN-IMP",
					"OBR => ",
					"OBO => ",
					"SDS => ",
					"TRN => ",
					"TBC => ",
					"TDC => ",
					"TDL => ",
					"TDU => ",
					"TRC => ",
					"TRU => ",
					"UNL => ",
					"WAH => ",
					"WIN => ",
					"WOU => ",
					"WST => ",
					"CYI => ",
					"CYO => ",
					"CYS => ",
					"YRA => ",
					"YRE => ",
					"YTU => ",
					"MWO => ",
					"CGI => ",
					"CGO => ",
				},
				actual: orgHeader.MiscServ.RatingDateConfigByChargeGroupConfiguration.RatingDateConfigByChargeGroups.Select(x => $"{x.ChargeGroup} => {string.Join(",", x.ChargeGroupSettings.Select(y => $"{y.JobType}-{y.DirectionCode}"))}")
			);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfigCollection = new RatingDateConfigCollection(orgHeader);
			return new RatingDateConfigByChargeGroupConfiguration(ratingDateConfigCollection);
		}

		#endregion
	}
}
