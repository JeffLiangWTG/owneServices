using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(ReferenceNumberForm))]
	sealed class ReferenceNumberFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new ReferenceNumberForm(Factory.New<DummyWithReferenceNumbers>());
		}

		class DummyWithReferenceNumbers : PackLine
		{
			public DummyWithReferenceNumbers(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new JobPackLineReferenceNumbersCollection CusEntryNums
			{
				get
				{
					if (cusEntryNums == null)
					{
						cusEntryNums = new JobPackLineReferenceNumbersCollection(this);
					}

					return cusEntryNums;
				}
			}
			JobPackLineReferenceNumbersCollection cusEntryNums;
		}
	}
}
