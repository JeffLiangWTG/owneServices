using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefPackTypeForm))]
	sealed class RefPackTypeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefPackTypeForm(Factory.New<RefPackType>());
		}

		#region TestPackTypeIsReservedTypeLabel

		[RequiresSTA]
		public void TestPackTypeIsReservedTypeLabel()
		{
			var packTypeCNT = Factory.New<RefPackType>();
			packTypeCNT.F3_Code = RefPackTypeCollection.ReservedContainerType;
			AssertPackTypeIsReservedTypeLabelVisible(packTypeCNT, true);

			var packTypeUNT = Factory.New<RefPackType>();
			packTypeUNT.F3_Code = "UNT";
			AssertPackTypeIsReservedTypeLabelVisible(packTypeUNT, false);
		}

		void AssertPackTypeIsReservedTypeLabelVisible(RefPackType packType, bool visible)
		{
			using (var form = new RefPackTypeForm(packType))
			{
				form.Show();

				AssertEquals(visible, form.PackTypeIsReservedTypeLabel.Visible);
			}
		}

		#endregion
	}
}
