using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupBankDefault : AutoOrgDebtorGroupBankDefault
	{
		public OrgDebtorGroupBankDefault(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			P6_GC = GlbCompany.CurrentCompany.PK;
		}

		#endregion

		#region Unique Index 

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new OrgDebtorGroupBankDefaultUniqueIndexFailureHandler(this); }
		}

		class OrgDebtorGroupBankDefaultUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public OrgDebtorGroupBankDefaultUniqueIndexFailureHandler(OrgDebtorGroupBankDefault bankDefault)
			{
				this.BankDefault = bankDefault;
			}

			readonly OrgDebtorGroupBankDefault BankDefault;

			#region IUniqueIndexFailureHandler Members

			void IUniqueIndexFailureHandler.NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				ZQuery filter = new ZQuery(OrgDebtorGroupBankDefaultSchema.P6_GC, BankDefault.P6_GC);
				filter.AddToFilter(OrgDebtorGroupBankDefaultSchema.P6_OJ, BankDefault.P6_OJ);
				filter.AddToFilter(OrgDebtorGroupBankDefaultSchema.PK, SQLComparisonOperator.NotEqual, BankDefault.PK);
				OrgDebtorGroupBankDefault otherGroupDefault = BankDefault.Factory.LoadTop1<OrgDebtorGroupBankDefault>(filter);
				if (otherGroupDefault != null)
				{
					otherGroupDefault.Delete();
				}
				notifier.ReportInformation(Res.GetString("9c7cf076-ba84-420d-a954-dcc9ef840c73", "While you were working with this Debtor Group, another user has changed the bank account. Press the Save button to try and save your changes again"), Res.GetString("f28b21a4-69fb-4c11-a1d8-041c1d8bca7a", "Debtor Group"));
			}

			IEnumerable<string> IUniqueIndexFailureHandler.HandledUniqueIndexNames
			{
				get { yield return OrgDebtorGroupBankDefaultSchema.Constants.Indexes.FK_UX__P6_OJ_P6_GC; }
			}

			#endregion
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			if (P6_AB == Guid.Empty)
			{
				Delete();
			}
		}
	}
}
