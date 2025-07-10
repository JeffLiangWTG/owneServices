using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class AgencyShipmentFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public AgencyShipmentFetchStrategy(AgencyShipment shipment)
			: base(shipment) { }

		AgencyShipment Shipment => BusinessObject as AgencyShipment;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.SeedQueryCache(JobConShipLinkSchema.Constants.TableName, new ZQuery(JobConShipLinkSchema.JN_JS, BusinessObject.PK));
			Factory.SeedQueryCache(JobDeclarationSchema.Constants.TableName, new ZQuery(JobDeclarationSchema.JE_JS, BusinessObject.PK));
			Factory.AddFetchHint(typeof(JobSailing), JobSailingSchema.PK, Shipment.JS_JX);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505: Avoid unmaintainable code")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var requireDocAddresses = false;
			var requireJobHeader = false;
			var requireJobConsolTransport = false;
			var requirePackLine = false;
			var requireJobContainer = false;
			var requireCusEntryNum = false;
			var requireJobDocsAndCartage = false;
			var requireStmNote = false;

			foreach (TableColumn column in columns)
			{
				switch (column.ColumnName)
				{
					case AgencyShipment.Schema.BookingPartyNameOrPK:
					case AgencyShipment.Schema.ConsigneeNameOrPK:
					case AgencyShipment.Schema.ConsignorContact:
					case AgencyShipment.Schema.ConsignorNameOrPK:
					case AgencyShipment.Schema.ConsigneeContact:
					case AgencyShipment.Schema.ConsigneePK:
					case AgencyShipment.Schema.JS_Calc_ConsigneeCompanyCode:
					case AgencyShipment.Schema.JS_Calc_ConsignorCompanyCode:
					case AgencyShipment.Schema.NotifyContact:
					case AgencyShipment.Schema.NotifyPartyCompanyCode:
					case AgencyShipment.Schema.PickupAgentCompanyCode:
					case AgencyShipment.Schema.ConsignorPK:
					case AgencyShipment.Schema.JS_Calc_ConsigneeCompanyName:
					case AgencyShipment.Schema.JS_Calc_ConsignorCompanyName:
					case nameof(AgencyShipment.JS_Calc_NotifyPartyDisplay):
					case nameof(AgencyShipment.ConsignorFieldType):
					case nameof(AgencyShipment.ControllingCustomerFieldType):
					case nameof(AgencyShipment.ConsigneeFieldType):
					case nameof(AgencyShipment.BookingPartyFieldType):
						requireDocAddresses = true;
						break;

					case nameof(AgencyShipment.Job) + "+" + nameof(AgencyShipment.Job.JH_Status):
					case nameof(AgencyShipment.Job) + "+" + nameof(AgencyShipment.Job.JH_HoldReason):
					case nameof(AgencyShipment.Job) + "+" + nameof(AgencyShipment.Job.JH_ProfitLossReasonCode):
					case nameof(AgencyShipment.Job) + "+" + nameof(AgencyShipment.Job.JH_TotalProfitRevenueMargin):
						requireJobHeader = true;
						break;

					case AgencyShipment.Schema.JS_Calc_CurrentDischargePort:
					case AgencyShipment.Schema.JS_Calc_CurrentETA:
					case AgencyShipment.Schema.JS_Calc_CurrentLoadPort:
					case AgencyShipment.Schema.JS_Calc_CurrentVessel:
					case AgencyShipment.Schema.JS_Calc_CurrentVoyageFlight:
					case AgencyShipment.Schema.JS_NKDischargePort:
					case AgencyShipment.Schema.JS_NKLoadPort:
					case AgencyShipment.Schema.JS_Calc_CurrentETD:
						requireJobConsolTransport = true;
						break;

					case AgencyShipment.Schema.TotalInnerPackLineLoadingMeters:
					case AgencyShipment.Schema.TotalInnerPackLineWeight:
					case AgencyShipment.Schema.TotalInnerPackLineVolume:
					case AgencyShipment.Schema.TotalInnerPackLinePackages:
					case AgencyShipment.Schema.TotalOuterPacksWeight_Imperial:
					case AgencyShipment.Schema.TotalOuterPacksWeight:
					case AgencyShipment.Schema.TotalOuterPacksVolume_Imperial:
					case AgencyShipment.Schema.TotalOuterPacksVolume:
					case AgencyShipment.Schema.TotalOuterPacksPillaged:
					case AgencyShipment.Schema.TotalOuterPacksDamaged:
					case AgencyShipment.Schema.TotalOuterPacks:
					case AgencyShipment.Schema.TotalOuterPacksLoadingMeters:
						requirePackLine = true;
						break;

					case AgencyShipment.Schema.TopLevelPacksTotalWeightInShipmentWeightUnit:
					case AgencyShipment.Schema.TopLevelPacksTotalVolumeInShipmentVolumeUnit:
					case AgencyShipment.Schema.JS_Calc_ContainerCount:
					case nameof(AgencyShipment.TopLevelPacksTotalPacks):
						requireJobContainer = true;
						break;

					case AgencyShipment.Schema.CustomsEntryNumber:
					case AgencyShipment.Schema.CustomsEntryNumberExpiryDate:
					case AgencyShipment.Schema.CustomsEntryNumberIssueDate:
					case AgencyShipment.Schema.JS_InspectionTypeCode:
					case nameof(AgencyShipment.NumbersAsString):
						requireCusEntryNum = true;
						break;

					case nameof(AgencyShipment.JS_Calc_DeliveryCartageZone):
					case nameof(AgencyShipment.JS_Calc_PickupCartageZone):
					case nameof(AgencyShipment.JS_OrderReferences):
						requireJobDocsAndCartage = true;
						break;

					case AgencyShipment.Schema.DetailedGoodsDescriptionNoteText:
					case AgencyShipment.Schema.JS_MarksAndNumbers:
					case AgencyShipment.Schema.JS_MarksAndNumbersShort:
						requireStmNote = true;
						break;
				}
			}

			if (requireJobDocsAndCartage)
			{
				Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, BusinessObject.PK);
			}

			if (requireDocAddresses)
			{
				Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			}

			if (requireJobHeader)
			{
				var jobQuery = new ZQuery();
				jobQuery.AddToFilter(JobHeaderSchema.JH_ParentID, BusinessObject.PK);
				jobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				Factory.AddFetchHint(ObjectFactory.GetType<IJobHeader>(), jobQuery);
			}

			if (requireJobConsolTransport)
			{
				Factory.AddFetchHint(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID, BusinessObject.PK);
			}

			if (requirePackLine)
			{
				Factory.AddFetchHint(typeof(PackLine), JobPackLinesSchema.JL_JS, BusinessObject.PK);
			}

			if (requireJobContainer)
			{
				Factory.AddFetchHint(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink, BusinessObject.PK);
			}

			if (requireCusEntryNum)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			}

			if (requireStmNote)
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			}
		}
	}
}


