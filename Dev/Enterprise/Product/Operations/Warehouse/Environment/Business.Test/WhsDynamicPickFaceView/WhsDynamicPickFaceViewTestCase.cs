using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsDynamicPickFaceView))]
	class WhsDynamicPickFaceViewTestCase : WhsEnvBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete DynamicPickFaceView", expected: false, GetNewBusinessObject().CanDelete);
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		public void TestHumanReadableName()
		{
			var view = Factory.New<WhsDynamicPickFaceView>();
			AssertEquals("Dynamic Pick Face", view.HumanReadableName);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			Helper.CreateDynamicPF(data.Whs1, location);
			Factory.Save();
			return Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject bizObj, BusinessObjectFactory newFactory)
		{
			var result = base.GetBusinessObjectInNewFactory(bizObj, newFactory);
			_ = ((WhsDynamicPickFaceView)result).Location; // consume WhsLocationView FetchHint as we are ok with this FetchHint being in FetchForLoad
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			Helper.CreateDynamicPF(data.Whs1, location);
			Factory.Save();
			return Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
		}

		public void TestSetProductDescriptionToMaxLengthAndReadback()
		{
			var view = Factory.New<WhsDynamicPickFaceView>();
			var maxLength = WhsDynamicPickFaceView.Schema.WDP_ProductDescMaxLength;

			AssertEquals("Max length of description should be 128", 128, maxLength);
			AssertNoExceptionThrown(() => view.WDP_ProductDesc = new string('a', maxLength));

			var description = view.WDP_ProductDesc;

			AssertEquals("Description can be set to max length and read back as max number of chars", maxLength, description.Length);
		}
	}
}
