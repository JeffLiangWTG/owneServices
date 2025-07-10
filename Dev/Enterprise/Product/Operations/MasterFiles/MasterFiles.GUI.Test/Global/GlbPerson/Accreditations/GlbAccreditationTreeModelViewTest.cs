using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbAccreditationTreeModelView))]
	sealed class GlbAccreditationTreeModelViewTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var accreditation = Factory.New<IGlbAccreditation>();
			var model = new GlbAccreditationTreeModel(accreditation);

			return new GlbAccreditationTreeModelView(model);
		}
	}
}
