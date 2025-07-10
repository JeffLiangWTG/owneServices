using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(PersonAssociationsTreeModelView))]
	sealed class PersonAssociationsTreeModelViewTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			var person = Factory.New<GlbPerson>();
			var model = new PersonAssociationsTreeModel(person);
			return new PersonAssociationsTreeModelView(model);
		}
	}
}
