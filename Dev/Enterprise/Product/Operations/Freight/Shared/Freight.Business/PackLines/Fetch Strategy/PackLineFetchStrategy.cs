using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PackLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PackLineFetchStrategy(PackLine packLine)
			: base(packLine)
		{
		}

		PackLine PackLine
		{
			get { return BusinessObject as PackLine; }
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobPackProductSchema.D2_JL, PackLine.PK);
			Factory.AddFetchHint(JobPackLocSchema.JQ_JL, PackLine.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var containerPackPivotRequired = false;
			var commonConfirmDivotRequired = false;
			var cusEntryNumRequired = false;
			var undgDataItemRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case JobContainerPackPivotSchema.Constants.J6_JL:
					case nameof(PackLine.JL_Calc_FirstImportContainerNum):
					case PackLine.Schema.JL_Calc_JV_Vessel:
					case PackLine.Schema.JL_Calc_JV_VoyageNo:
					case PackLine.Schema.JL_Calc_JX_ETD:
					case PackLine.Schema.JL_Calc_JX_NKDischPort:
					case PackLine.Schema.JL_Calc_JX_NKLoadPort:
						containerPackPivotRequired = true;
						break;

					case JobTransportLegPackLineDivotSchema.Constants.J8_JL:
					case PackLine.Schema.JL_Calc_OutturnedInStock:
						commonConfirmDivotRequired = true;
						break;

					case PackLine.Schema.JL_InspectionTypeCode:
						cusEntryNumRequired = true;
						break;
					case PackLine.Schema.JL_Calc_DGClass:
					case PackLine.Schema.JL_Calc_DGSubstance:
						undgDataItemRequired = true;
						break;
				}
			}

			if (commonConfirmDivotRequired)
			{
				Factory.AddFetchHint(typeof(CommonConfirmDivot), JobTransportLegPackLineDivotSchema.J8_JL, PackLine.PK);
			}

			if (containerPackPivotRequired)
			{
				Factory.AddFetchHint(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL, PackLine.PK);
			}

			if (cusEntryNumRequired)
			{
				Factory.AddFetchHint(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, PackLine.PK);
			}

			if (undgDataItemRequired)
			{
				Factory.AddFetchHint(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID, PackLine.PK);
			}

			base.FetchForViewCore(columns);
		}
	}
}
