using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AIMMessageChooser))]
	sealed class AIMMessageChooserBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			return new AIMMessageChooser(header, items, string.Empty);
		}
	}
}
