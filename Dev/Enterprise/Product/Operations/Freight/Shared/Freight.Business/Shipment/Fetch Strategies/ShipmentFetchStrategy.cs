using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ShipmentFetchStrategy(CommonShipment shipment)
			: base(shipment)
		{
		}

		CommonShipment Shipment
		{
			get { return BusinessObject as CommonShipment; }
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(JobPackLinesSchema.JL_JS, Shipment.PK);
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Shipment.PK);
			Factory.AddFetchHint(JobConShipLinkSchema.JN_JS, Shipment.PK);
			Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, Shipment.PK);
			Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, Shipment.PK);
			Factory.AddFetchHint(JobDeclarationSchema.JE_JS, Shipment.PK);
			Factory.AddFetchHint(JobPackLinesSchema.JL_JS, Shipment.PK);
			Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, Shipment.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, Shipment.PK);
			Factory.AddFetchHint(GenCustomAddOnValueSchema.XV_ParentID, Shipment.PK);

			var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, Shipment.PK);
			jobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			Factory.AddFetchHint(typeof(JobHeader), jobQuery);

			var cusEntryNumRequired = false;
			var cusEntryNumWithDeclarationRequired = false;
			var cusHawbRequired = false;
			var cusSCAHouseRequired = false;
			var jobConsolTransportRequired = false;
			var jobDocAddressRequired = false;
			var requiresJobContainerPackPivot = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case CommonShipment.Schema.JS_InspectionTypeCode:
					case nameof(CommonShipment.NumbersAsString):
						cusEntryNumRequired = true;
						break;

					case CommonShipment.Schema.CustomsEntryNumberExpiryDate:
					case CommonShipment.Schema.CustomsEntryNumberIssueDate:
						cusEntryNumRequired = true;
						cusHawbRequired = true;
						cusSCAHouseRequired = true;
						break;

					case CommonShipment.Schema.CustomsEntryNumber:
					case CommonShipment.Schema.CustomsEntryNumberType:
						cusEntryNumRequired = true;
						cusHawbRequired = true;
						cusSCAHouseRequired = true;
						cusEntryNumWithDeclarationRequired = true;
						break;

					case CommonShipment.Schema.JS_Calc_CurrentVoyageFlight:
					case CommonShipment.Schema.JS_Calc_CurrentVessel:
					case CommonShipment.Schema.JS_Calc_CurrentLoadPort:
					case CommonShipment.Schema.JS_Calc_CurrentETD:
					case CommonShipment.Schema.JS_Calc_CurrentETA:
					case CommonShipment.Schema.JS_Calc_CurrentDischargePort:
						jobConsolTransportRequired = true;
						break;

					case CommonShipment.Schema.ConsignorContact:
					case CommonShipment.Schema.ConsignorPK:
					case CommonShipment.Schema.JS_Calc_ConsignorCompanyCode:
					case CommonShipment.Schema.JS_Calc_ConsignorCompanyName:
					case nameof(CommonShipment.JS_Calc_PickupCartageZone):
						jobDocAddressRequired = true;
						break;

					case nameof(CommonShipment.JS_Calc_20GPCount):
					case nameof(CommonShipment.JS_Calc_20RECount):
					case nameof(CommonShipment.JS_Calc_40GPCount):
					case nameof(CommonShipment.JS_Calc_40RECount):
					case nameof(CommonShipment.JS_Calc_ContainerCount):
					case nameof(CommonShipment.JS_Calc_OtherContainerCount):
					case nameof(CommonShipment.JS_Calc_TEUCount):
						requiresJobContainerPackPivot = true;
						break;
				}
			}

			if (jobDocAddressRequired)
			{
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, Shipment.PK);
			}

			if (cusEntryNumRequired)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, Shipment.PK);
				Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, Shipment.PK);
			}

			if (cusEntryNumWithDeclarationRequired)
			{
				var declarations = Shipment.Declarations;
				var parentIDClearedOnlyFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, Shipment.PK);
				parentIDClearedOnlyFilter.DefaultJoinCondition = JoinCondition.Or;
				foreach (BusinessObject decl in declarations)
				{
					var queryDecl = new ZQuery(CusEntryNumSchema.CE_ParentID, decl.PK);
					parentIDClearedOnlyFilter.AddToFilter(queryDecl);
				}

				var query = new ZQuery();
				query.AddToFilter(parentIDClearedOnlyFilter);
				query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				Factory.AddFetchHint(typeof(CusEntryNumber), query);
			}

			if (cusHawbRequired)
			{
				AddCusHawbFetchHint();
			}

			if (cusSCAHouseRequired)
			{
				AddSCAHouseFetchHint();
			}

			if (jobConsolTransportRequired)
			{
				Factory.AddFetchHint(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID, Shipment.PK);
			}

			if (requiresJobContainerPackPivot)
			{
				AddConsolFetchHint();

				foreach (var packLine in Shipment.OuterPackLines)
				{
					Factory.AddFetchHint(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL, packLine.PK);
				}
			}
		}

		protected virtual void AddCusHawbFetchHint()
		{
		}

		protected virtual void AddSCAHouseFetchHint()
		{
		}

		protected void AddConsolFetchHint()
		{
			var consolShipmentPivotQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
			consolShipmentPivotQuery.AddToFilter(JobConShipLinkSchema.JN_JS, Shipment.PK);

			var consolsQuery = new ZDBOnlyQuery(typeof(CommonConsol));
			consolsQuery.AddSubQuery(consolShipmentPivotQuery, JoinCondition.And);

			Factory.AddFetchHint(typeof(CommonConsol), consolsQuery);
		}
	}
}
