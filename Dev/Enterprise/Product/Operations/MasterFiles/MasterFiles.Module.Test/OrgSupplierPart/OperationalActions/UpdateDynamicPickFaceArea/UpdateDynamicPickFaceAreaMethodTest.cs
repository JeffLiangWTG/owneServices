using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UpdateDynamicPickFaceAreaMethod))]
	sealed class UpdateDynamicPickFaceAreaMethodTest : OperationalActionMethodTest<UpdateDynamicPickFaceAreaMethod>
	{
		#region TestNewApplicator_RealTest

		public void TestNewApplicator_RealTest()
		{
			AssertType<UpdateDynamicPickFaceAreaMethodApplicator>(NewMethod().NewApplicator(Factory, null));
		}

		#endregion

		#region TestNewGuiControl_RealTest

		public void TestNewGuiControl_RealTest()
		{
			var method = NewMethod();
			AssertEquals(true, method.HasControl);
			using (var control = method.NewGuiControl())
			{
				AssertType<UpdateDynamicPickFaceAreaControl>(control);
			}
		}

		#endregion

		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			AssertEquals("Update Dynamic Pick Face Area", NewMethod().Name);
			AssertEquals("Update Dynamic Pick Face Area", NewMethod().Description);
		}

		#endregion

		#region Implementation

		protected override UpdateDynamicPickFaceAreaMethod NewMethod() => new UpdateDynamicPickFaceAreaMethod();

		#endregion
	}
}
