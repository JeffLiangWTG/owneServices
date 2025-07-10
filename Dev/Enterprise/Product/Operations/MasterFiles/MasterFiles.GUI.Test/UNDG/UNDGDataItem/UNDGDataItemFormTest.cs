using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UNDGDataItemForm))]
	sealed class UNDGDataItemFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new UNDGDataItemForm(Factory.New<DummyWithUNDGs>());
		}

		class DummyWithUNDGs : DummyBusinessObject, IUNDGDataItemProvider
		{
			public DummyWithUNDGs(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public UNDGDataItemCollection UNDGs
			{
				get
				{
					if (fUNDGs == null)
					{
						fUNDGs = new UNDGDataItemCollection(this);
					}
					return fUNDGs;
				}
			}
			UNDGDataItemCollection fUNDGs;

			bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;
		}
	}
}
