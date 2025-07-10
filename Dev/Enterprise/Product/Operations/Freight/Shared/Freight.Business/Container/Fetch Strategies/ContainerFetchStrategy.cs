using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ContainerFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ContainerFetchStrategy(CommonContainer container) : base(container)
		{
		}

		protected CommonContainer Container
		{
			get { return BusinessObject as CommonContainer; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC, BusinessObject.PK);
			Factory.AddFetchHint(typeof(ContainerPenalty), JobContainerPenaltySchema.CPY_JC_Container, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			if (Env.CurrentCompany.Country.Code == Enterprise.Core.Constants.CountryCodes.Australia)
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, Container.PK);
			}

			if (FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
			{
				//Test in CartageLegPlannerFilterControl
				Factory.AddFetchHint(JobBookedCtgMoveSchema.EW_JC_Container, Container.PK);//GetAllBookedMovesForContainer in CartageContainerHelper
				Factory.AddFetchHint(CusContainerSchema.CO_JC, Container.PK);//Declaration in CommonContainer
			}
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, Container.PK);
			Factory.AddFetchHint(JobServiceSchema.ES_ParentID, Container.PK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, Container.PK);

			if (FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
			{
				//Test in CartageLegPlannerFilterControl
				Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JC, Container.PK);  //Packlines In CommonContainer
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var containerPackPivotRequired = false;
			var jobDocAddressRequired = false;
			var cusContainerRequired = false;
			var jobBookedCtgMoveRequired = false;
			var ediMessageRequired = true;
			var confirmRequired = false;
			var additionalReferenceNumbersRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case JobContainerPackPivotSchema.Constants.J6_JC:
						containerPackPivotRequired = true;
						break;

					case JobPickupDeliveryConfirmSchema.Constants.EU_JC:
						confirmRequired = true;
						break;

					case CusContainerSchema.Constants.CO_JC:
						cusContainerRequired = true;
						break;

					case JobBookedCtgMoveSchema.Constants.EW_JC_Container:
						jobBookedCtgMoveRequired = true;
						break;

					case CommonContainer.Schema.CurrentPRAStatus:
						ediMessageRequired = true;
						break;

					case CommonContainer.Schema.GoodsWeightForBinding:
					case CommonContainer.Schema.JC_Calc_ActualGrossWeightInKgs:
					case CommonContainer.Schema.JC_Calc_TotalPackages:
					case CommonContainer.Schema.JC_Calc_TotalPackagesUnit:
					case CommonContainer.Schema.JC_Calc_TotalVolume:
					case CommonContainer.Schema.JC_Calc_TotalVolumeInM3:
					case CommonContainer.Schema.JC_Calc_TotalVolumeUnit:
					case CommonContainer.Schema.JC_Calc_TotalWeight:
					case CommonContainer.Schema.JC_Calc_TotalWeightInKgs:
					case CommonContainer.Schema.JC_Calc_TotalWeightUnit:
						containerPackPivotRequired = true;
						break;

					case nameof(CommonContainer.GrossWeightVerifiedByFieldType):
					case CommonContainer.Schema.GrossWeightVerifiedByNameOrPK:
						jobDocAddressRequired = true;
						break;

					case CommonContainer.Schema.JC_LastFreeDay:
						cusContainerRequired = true;
						jobBookedCtgMoveRequired = true;
						break;

					case nameof(CommonContainer.AdditionalReferenceNumbersAsString):
						additionalReferenceNumbersRequired = true;
						break;
				}
			}

			if (cusContainerRequired)
			{
				Factory.AddFetchHint(CusContainerSchema.CO_JC, Container.PK);
			}

			if (containerPackPivotRequired)
			{
				Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JC, Container.PK);
			}

			if (jobDocAddressRequired)
			{
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, Container.PK);
			}

			if (jobBookedCtgMoveRequired)
			{
				Factory.AddFetchHint(JobBookedCtgMoveSchema.EW_JC_Container, Container.PK);
			}

			if (ediMessageRequired)
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, Container.PK);
			}

			if (confirmRequired)
			{
				Factory.AddFetchHint(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_JC, Container.PK);
			}

			if (additionalReferenceNumbersRequired)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, Container.PK);
			}

			base.FetchForViewCore(columns);
		}
	}
}
