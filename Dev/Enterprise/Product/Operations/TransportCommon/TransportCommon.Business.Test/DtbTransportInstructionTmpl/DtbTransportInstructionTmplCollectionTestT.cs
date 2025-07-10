using System.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionTmplCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<T>
			where T : DtbTransportInstructionTmplCollection
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			var template = GetNewTemplate();
			var iBindingList = ((IBindingList)template.Instructions);
			AssertEquals(true, iBindingList.AllowNew);

			template.KT_IsSystem = true;
			AssertEquals(false, iBindingList.AllowNew);
		}

		#endregion

		#region Implementation

		protected abstract DtbTransportTmpl GetNewTemplate();

		#endregion
	}
}
