//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementLookups
//
//    This class should be used for overriding collections in AutoOrgCommissionAgreementLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementLookups : AutoOrgCommissionAgreementLookups
	{
		public OrgCommissionAgreementLookups(AutoOrgCommissionAgreement parent) : base(parent)
		{
		}

		new OrgCommissionAgreement Parent
		{
			get { return (OrgCommissionAgreement)base.Parent; }
		}

		#region Trigger Types

		public ReadOnlyCodeDescriptionPairList TriggerTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddRange(CommissionLookups.New(Factory).TriggerTypes);
				result.AddRange(new OrgCommissionAgreementTriggerTypes());

				return result;
			}
		}

		#endregion

		#region Commission Basis

		public ReadOnlyCodeDescriptionPairList CommissionBasisType
		{
			get { return CommissionLookups.New(Factory).CommissionBasisType; }
		}

		#endregion

		#region Commission Streams

		public ICodeDescriptionPairList CommissionBasisStreams_All
		{
			get { return CommissionLookups.New(Factory).CommissionStreams_All; }
		}

		public ICodeDescriptionPairList CommissionBasisStreams_Active
		{
			get { return CommissionLookups.New(Factory).CommissionStreams_Active; }
		}

		#endregion

		#region Customers

		public override OrgHeaderCollection Customers
		{
			get
			{
				var opportunity = Parent.Opportunity;
				var opportunityOrgPk = opportunity != null ? opportunity.P8_OH : ZGuid.Empty;

				return Factory.GetCachedValue("OrgCommissionAgreementLookups.Customers" + opportunityOrgPk + "-" + Parent.CA0_Name, () =>
				{
					var filter = GetCustomersFilter(opportunityOrgPk);
					var factory = new BusinessObjectFactory();
					var collection = new OrgHeaderCollection(factory, filter);
					collection.Load();

					if (Parent.IsApproved)
					{
						foreach (var bizO in collection)
						{
							bizO.ShowWarningIfCancelled = true;
						}
					}
					return collection;
				});
			}
		}

		protected virtual ZQuery GetCustomersFilter(ZGuid opportunityOrgPk)
		{
			if (opportunityOrgPk.IsEmpty)
			{
				return ZQuery.NoResultQuery;
			}

			var result = new ZDBOnlyQuery(typeof(OrgHeader));
			result.AddFilterAndZSQLParameterCollection(@"
OH_PK IN
(
	SELECT OrgPk
	FROM dbo.vw_OrgManagementGroupingNode TreeNode
	WHERE
		TreeNode.RootOrgPk IN (SELECT RootOrgPk FROM dbo.vw_OrgManagementGroupingNode WHERE OrgPk = @MasterPk)

	UNION ALL

	SELECT @MasterPk
)", new ZSqlParameterCollection(ZSqlParameter.New("@MasterPk", opportunityOrgPk, OrgHeaderSchema.PK)));

			return result;
		}

		#endregion
	}
}
