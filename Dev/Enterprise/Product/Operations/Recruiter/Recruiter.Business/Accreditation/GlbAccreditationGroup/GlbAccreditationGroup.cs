using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(GlbAccreditationGroupSchema.Constants.HAG_Description)]
	public class GlbAccreditationGroup : AutoGlbAccreditationGroup
	{
		public GlbAccreditationGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("f16c1d86-0693-4794-b5a4-1f69e1a42794", "Accreditation Group");

		[ChildEditable]
		public GlbAccreditationGroupPivotCollection AccreditationPivotCollection
		{
			get
			{
				if (accreditationPivotCollection == null)
				{
					accreditationPivotCollection = new GlbAccreditationGroupPivotCollection(this);
					RegisterEditableChildObject(accreditationPivotCollection);
				}

				return accreditationPivotCollection;
			}
		}

		GlbAccreditationGroupPivotCollection accreditationPivotCollection;

		[ChildEditable]
		public GlbAccreditationDependentCollection Accreditations
		{
			get
			{
				if (accreditations == null)
				{
					var query = new ZDBOnlyQuery(typeof(GlbAccreditation));

					var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationGroupPivot), GlbAccreditationGroupPivotSchema.HAP_HAC);
					pivotSubQuery.AddToFilter(GlbAccreditationGroupPivotSchema.HAP_HAG, PK);

					query.AddSubQuery(pivotSubQuery, JoinCondition.And);

					accreditations = new GlbAccreditationDependentCollection(this, query);
					accreditations.Load();
					RegisterEditableChildObject(accreditations);
				}

				return accreditations;
			}
		}

		GlbAccreditationDependentCollection accreditations;

		public override void Delete()
		{
			AccreditationPivotCollection.DeleteAll();
			base.Delete();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;
	}
}
