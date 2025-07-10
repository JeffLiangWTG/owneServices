using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class CustomsMiscOptionUserControlTest : TestCaseWithFactory
	{
		#region TestControlVisibilitySwitchingBetweenECIAndFormalEntry
		public void TestControlVisibilitySwitchingBetweenECIAndFormalEntry()
		{
			using (CustomsMiscOptionUserControl userControl = new CustomsMiscOptionUserControl())
			{
				userControl.JobDeclaration = Declaration;
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				AssertEquals("PaymentPartyDropEdit.Visible", true, (userControl.FindSingle<ZDropEdit>("PaymentPartyDropEdit")).Visible);
				AssertEquals("MergeByDropEdit.Visible", true, (userControl.FindSingle<ZDropEdit>("MergeByDropEdit")).Visible);
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				AssertEquals("PaymentPartyDropEdit.Visible", false, (userControl.FindSingle<ZDropEdit>("PaymentPartyDropEdit")).Visible);
				AssertEquals("MergeByDropEdit.Visible", false, (userControl.FindSingle<ZDropEdit>("MergeByDropEdit")).Visible);
			}
		}

		#endregion
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}

				return fDeclaration;
			}
		}

		JobDeclaration fDeclaration;
		#endregion
	}
}
