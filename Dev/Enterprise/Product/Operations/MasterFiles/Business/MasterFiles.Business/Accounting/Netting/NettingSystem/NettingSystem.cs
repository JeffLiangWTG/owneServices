using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

#if DEBUG
using Enterprise.MasterFiles.Business.Testing;
#endif

namespace Enterprise.MasterFiles.Business
{
	public class NettingSystem : AutoNettingSystem
	{
		public NettingSystem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NS_Code = MasterFilesTestHelper.GetRandomString(10);
			NS_Description = "Blah blah blah";
		}
#endif
	}
}
