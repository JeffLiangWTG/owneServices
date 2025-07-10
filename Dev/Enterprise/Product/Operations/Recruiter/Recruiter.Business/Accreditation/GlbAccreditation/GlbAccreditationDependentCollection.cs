using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationDependentCollection : BusinessObjectCollection<GlbAccreditation>, IGlbAccreditationDependantCollection
	{
		readonly BusinessObject baseBusinessObject;

		public GlbAccreditationDependentCollection(GlbAccreditation accreditation, ZQuery filter)
			: base(accreditation.Factory, filter)
		{
			baseBusinessObject = accreditation;
			CountChanged += GlbAccreditationDependentCollection_CountChanged;
		}

		public GlbAccreditationDependentCollection(GlbAccreditationGroup accreditationGroup, ZQuery filter)
			: base(accreditationGroup.Factory, filter)
		{
			baseBusinessObject = accreditationGroup;
			CountChanged += GlbAccreditationDependentCollection_CountChanged;
		}

		public override void Load(ZQuery filter)
		{
			base.Load(filter);

			if (baseBusinessObject is GlbAccreditationGroup)
			{
				foreach (var accreditation in this.Cast<GlbAccreditation>().ToArray())
				{
					AddRefresherForAccreditationIfExists(accreditation);
				}
			}
		}

		void AddRefresherForAccreditationIfExists(GlbAccreditation accreditation)
		{
			var refresher = accreditation.RefresherAccreditation;

			if (refresher != null)
			{
				Add(refresher);
			}
		}

		void GlbAccreditationDependentCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && !IsLoading)
			{
				var accreditationToAdd = e.BizObject as GlbAccreditation;
				if (accreditationToAdd != null)
				{
					switch (baseBusinessObject)
					{
						case GlbAccreditation accreditation:
							var newRequirementPivot = accreditation.RequirementPivotCollection.AddNew();
							newRequirementPivot.HAR_HAC_Parent = accreditationToAdd.PK;
							break;
						case GlbAccreditationGroup accreditationGroup:
							if (!accreditationToAdd.HAC_IsRefresher)
							{
								var newGroupPivot = accreditationGroup.AccreditationPivotCollection.AddNew();
								newGroupPivot.HAP_HAC = accreditationToAdd.PK;

								AddRefresherForAccreditationIfExists(accreditationToAdd);
							}

							break;
					}
				}
			}
			else if (e.ItemRemoved)
			{
				var accreditationToRemove = e.BizObject as GlbAccreditation;
				if (accreditationToRemove != null)
				{
					switch (baseBusinessObject)
					{
						case GlbAccreditation accreditation:
							var requirementPivotToDelete = accreditation.RequirementPivotCollection.Find(p => p.HAR_HAC_Parent == accreditationToRemove.PK);
							requirementPivotToDelete?.DeleteAll();
							break;
						case GlbAccreditationGroup accreditationGroup:
							if (!accreditationToRemove.HAC_IsRefresher)
							{
								var groupPivotToDelete = accreditationGroup.AccreditationPivotCollection.Find(p => p.HAP_HAC == accreditationToRemove.PK);
								groupPivotToDelete?.DeleteAll();

								var refresher = accreditationToRemove.RefresherAccreditation;

								if (refresher != null)
								{
									Remove(refresher);
								}
							}

							break;
					}
				}
			}
		}

		IGlbAccreditation IGlbAccreditationDependantCollection.this[int i] => this[i];
	}
}
