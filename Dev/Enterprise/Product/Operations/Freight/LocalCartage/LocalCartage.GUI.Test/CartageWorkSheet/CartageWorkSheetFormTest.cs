using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(CartageWorkSheetForm))]
	public class CartageWorkSheetFormTest : ZFormBasherTest
	{
		public void TestShowCartageLegDetails()
		{
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				form.jobCartageRunSheetMainControl1.ShowCartageLegDetails = true;
				Assert(form.jobCartageRunSheetMainControl1.CartageLegsControlWithDetails.Visible);
				form.Size = form.MinimumSize;
				Assert(!form.jobCartageRunSheetMainControl1.CartageLegsControlWithDetails.Visible);
				form.jobCartageRunSheetMainControl1.ShowCartageLegDetails = true;
				Assert(form.jobCartageRunSheetMainControl1.CartageLegsControlWithDetails.Visible);
				Assert(form.Size.Height > form.MinimumSize.Height);
				Assert(form.Size.Width >= form.MinimumSize.Width);
			}
		}

		public void TestRunSheetSecurityGUIProvider_Register()
		{
			using (var form = new CartageWorkSheetForm(workSheet))
			{
				var provider = RunSheetSecurityProvider.GetProvider(Factory);
				AssertNotNull(provider);
				AssertEquals(typeof(RunSheetSecurityGUIProvider), provider.GetType());
			}
		}

		public void TestUserNoticationMember()
		{
			using (var form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				((INotifications)form).Add(new NotificationTest());
				AssertEquals("Message should be shown to the user.", "Test Message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class NotificationTest : INotification
		{
			string INotification.Message
			{
				get
				{
					return "Test Message";
				}
			}

			INotification INotification.ReplaceMessage(string message)
			{
				throw new NotImplementedException();
			}

			INotificationType INotification.Type
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}

		public void TestApportionmentPlugIn()
		{
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.ApportionmentForCommonWorkSheet));
				AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.Apportionment));
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new CartageWorkSheetForm(workSheet);
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			workSheet = Factory.New<CommonWorkSheet>();
			base.SetUp();
		}

		CommonWorkSheet workSheet;
	}
}
