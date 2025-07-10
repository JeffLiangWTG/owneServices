using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.Business.OrgMergeBillingConstants;
using BillingAction = Enterprise.MasterFiles.Business.OrgMergeBillingConstants.Action;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressBindingMemberBashingTest] // for SetToInactive / DeleteOldOrg
	public partial class MergeOrgHeaderForm : ZChildForm
	{
		public MergeOrgHeaderForm(MergeOrgHeader businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		MergeOrgHeader OrgMerge
		{
			get { return (MergeOrgHeader)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return Res.GetString("MergeOrgHeaderForm|FormVerb", "Merge"); }
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("e7695d95-e9e3-42d2-af87-a5a9048709fb", "organization"), Res.GetString("b7ffaa34-232e-4a2d-9ffe-4fa355d04085", "merge"), Res.GetString("9fc893fc-cbdd-45e1-9584-b2834a730fdb", "merged"), includeIgnoreOption);
		}

		#region Process Transfer

		void ProcessButton_Click(object sender, EventArgs e)
		{
			if (OrgMerge.NewOrganisationPk.IsValid)
			{
				SyncOldOrgContactCollection();

				var mergeResult = GetMergeResult(out var elapsedMilliseconds);

				Save(new[]
				{
					new SaveInTransactionDelegateAction(Db.Connection, () =>
					{
						var sourceForm = Source.MergeOrgHeaderForm;
						var retainedOrgPK = OrgMerge.NewOrganisation.PK.ToString();
						var dissolvedOrgPK = OrgMerge.OldOrganisation.PK.ToString();
						switch (mergeResult)
						{
							case MergeResult.Success:
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, BillingAction.MergeSuccess, sourceForm, new[] { retainedOrgPK, dissolvedOrgPK, $"{elapsedMilliseconds}ms" });
								break;
							case MergeResult.Failed:
							case MergeResult.MergedWithErrors:
							case MergeResult.FailedWithCriticalError:
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, BillingAction.MergeFailure, sourceForm, new[] { retainedOrgPK, dissolvedOrgPK, mergeResult.ToString() });
								break;
							case MergeResult.Cancelled:
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, BillingAction.MergeCancel, sourceForm, new[] { retainedOrgPK, dissolvedOrgPK });
								break;
						}
						return ChangedTableNames.Empty;
					})
				});

				if (mergeResult == MergeResult.Success || mergeResult == MergeResult.FailedWithCriticalError)
				{
					Close();
				}
			}
			else
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("4d49669b-0835-4297-8273-8ef98ce00507", "You must specify a retained organization"));
			}
		}

		void SyncOldOrgContactCollection()
		{
			foreach (MergeOrgContact dummyContact in OrgMerge.ContactCollectionWithoutDummyContact)
			{
				foreach (MergeOrgContact oldContact in OrgMerge.OldOrgContactCollection)
				{
					if (oldContact.OldContactPK == dummyContact.OldContactPK)
					{
						oldContact.Action = dummyContact.Action;
						if (dummyContact.Action == MergeOrgContact.ActionMerge)
						{
							oldContact.NewObjectPK = dummyContact.NewContactPK;
						}
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1021: AvoidOutParameters")]
		protected virtual MergeResult GetMergeResult(out long elapsedMilliseconds) => OrganizationMergeHelper.Merge(OrgMerge, out elapsedMilliseconds);

		#endregion
	}
}
