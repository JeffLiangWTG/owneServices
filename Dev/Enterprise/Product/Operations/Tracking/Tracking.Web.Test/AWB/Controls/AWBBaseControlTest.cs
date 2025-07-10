using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	abstract class AWBBaseControlTest : WebControlTest
	{
		#region Test Cases

		public void TestControlsBindTo()
		{
			AssertNotNull(TestControl);
			if (RequiresBindToCheck)
			{
				AssertBindToForChildControls(TestControl.Controls);
			}
		}

		void AssertBindToForChildControls(ControlCollection controls)
		{
			foreach (Control childControl in controls)
			{
				if (childControl is ISelfBindingWebControl && !(childControl is ZTextLabel))
				{
					if (childControl.Parent == null || (childControl != null && !(childControl.Parent is ZDropEditList)))
					{
						if (!GetControlTypesToIgnore().Contains(childControl.GetType()) && !GetControlIDsToIgnore().Contains(childControl.ClientID))
						{
							Assert("Web Control '" + childControl.ClientID + "' of type '" + childControl.GetType().ToString() + "' has no binding info", !string.IsNullOrEmpty(((ISelfBindingWebControl)childControl).BindTo));
						}
					}
				}
				AssertBindToForChildControls(childControl.Controls);
			}
		}

		#endregion

		#region Implementation

		protected virtual bool RequiresBindToCheck
		{
			get { return true; }
		}

		protected virtual List<Type> GetControlTypesToIgnore()
		{
			return new List<Type>();
		}

		protected virtual List<string> GetControlIDsToIgnore()
		{
			return new List<string>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull(TestControl);
			if (TestControl is AWBBaseControl)
			{
				((AWBBaseControl)TestControl).InitializeForTesting();
			}
			if (TestControl is AWBFieldControl)
			{
				((AWBFieldControl)TestControl).InitializeForTesting();
			}
		}

		protected CompositeControl TestControl
		{
			get { return Control as CompositeControl; }
		}

		#endregion
	}
}
