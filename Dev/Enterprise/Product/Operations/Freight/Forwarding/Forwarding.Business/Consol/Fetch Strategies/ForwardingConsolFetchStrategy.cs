using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolFetchStrategy : CommonConsolFetchStrategy
	{
		public ForwardingConsolFetchStrategy(ForwardingConsol consol) : base(consol) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var consol = BusinessObject as ForwardingConsol;
			if (consol.JK_AgentType == Constants.AgentType.AWBMaster)
			{
				var colNames = new[]
				{
					ForwardingConsol.Schema.JK_TotalShipmentChargeable,
					ForwardingConsol.Schema.JK_TotalShipmentVolume,
					ForwardingConsol.Schema.JK_TotalShipmentWeight,
					ForwardingConsol.Schema.JK_TotalShipmentPackageCount,
					ForwardingConsol.Schema.JK_TotalShipmentQuantity,
					ForwardingConsol.Schema.JK_CorrectedConsolWeight,
					ForwardingConsol.Schema.JK_CorrectedConsolVolume,
					ForwardingConsol.Schema.JK_ConsolChargeable
				};

				if (columns.Any(col => colNames.Contains(col.ColumnName)))
				{
					Factory.AddFetchHint(JobConsolSchema.JK_JK_MasterConsol, consol.PK);

					var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
					var conShipLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
					var consolQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
					consolQuery.AddToFilter(JobConsolSchema.JK_JK_MasterConsol, consol.PK);
					conShipLinkQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, consolQuery, JoinCondition.And);
					shipmentQuery.AddSubQuery(conShipLinkQuery, JoinCondition.And);

					Factory.AddFetchHint(typeof(ForwardingShipment), shipmentQuery);
				}
			}

			var mawbRequired = false;
			var ediMessageRequired = false;
			var additionalReferenceNumbersRequired = false;
			var securityStatusCodeRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case ForwardingConsol.Schema.AWBCurrentStatus:
						ediMessageRequired = true;
						break;

					case ForwardingConsol.Schema.MasterBillNeutralMAWB:
					case ForwardingConsol.Schema.MAWBLabelDesc:
						mawbRequired = true;
						break;

					case nameof(ForwardingModuleConsol.NumbersAsString):
						additionalReferenceNumbersRequired = true;
						break;

					case nameof(ForwardingModuleConsol.SecurityStatusCode):
						securityStatusCodeRequired = true;
						break;

					case nameof(ForwardingConsol.Job) + "+" + nameof(JobHeader.JH_Status):
					case nameof(ForwardingConsol.Job) + "+" + nameof(JobHeader.JH_HoldReason):
						AddFetchHintJobHeader();
						break;
				}
			}

			if (mawbRequired)
			{
				Factory.AddFetchHint(JobMawbSchema.JM_ParentID, BusinessObject.PK);
			}

			if (ediMessageRequired)
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}

			if (additionalReferenceNumbersRequired)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			}

			if (securityStatusCodeRequired)
			{
				Factory.AddFetchHint(JobConsolAWBSpecialHandlingSchema.JKH_JK_Consol, BusinessObject.PK);
			}
		}

		void AddFetchHintJobHeader()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, BusinessObject.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}
	}
}
