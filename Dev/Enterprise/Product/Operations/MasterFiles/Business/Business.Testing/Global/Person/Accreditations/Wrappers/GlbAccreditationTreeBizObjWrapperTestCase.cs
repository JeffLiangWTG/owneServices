using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class GlbAccreditationTreeBizObjWrapperTestCase<T> : NonPersistentBusinessObjectTestCase
			where T : GlbAccreditationTreeBizObjWrapperBase
	{
		protected abstract GlbAccreditationTreeBizObjWrapperBase GetWrapper(GlbAccreditationTreeModel model, GlbPerson person);

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetWrapper(model, person);
		}

		protected override void SetUp()
		{
			base.SetUp();
			accreditation = Factory.New<IGlbAccreditation>();
			accreditation.HAC_Code = "z";
			person = Factory.New<GlbPerson>();
			model = new GlbAccreditationTreeModel(accreditation);
		}

		protected IGlbAccreditation accreditation;
		protected GlbPerson person;
		protected GlbAccreditationTreeModel model;
	}
}
