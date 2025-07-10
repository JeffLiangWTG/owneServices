using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(ProcessTaskTemplateForm))]
	sealed class SystemDefinedTemplateFormTest : ZFormBasherTest
	{
		public void TestSystemDefinedTemplate_EverythingShouldBeReadonly_ExceptValidExemptions_ForUniversalTemplate()
		{
			BashForm(GetUniversalTemplateForm());
			ThrowFailureException();
		}

		public void TestSystemDefinedTemplate_EverythingShouldBeReadonly_ExceptValidExemptions_ForNonUniversalTemplate()
		{
			BashForm(GetRegularTemplateForm());
			ThrowFailureException();
		}

		#region Implementation

		public override void TestBashingForm()
		{
			// This class can't figure out if it's an API or a base-class test case. What's the point of allowing customisable bashers if it's just going to go ahead and run the base class tests always anyway???
			Assert(true);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override IEnumerable<Form> FormsToBash // Sometimes it's fun to bash multiple forms.
		{
			get
			{
				yield return GetUniversalTemplateForm();
				yield return GetRegularTemplateForm();
			}
		}

		protected override Form GetFormToBashCore()
		{
			return GetUniversalTemplateForm(); // Sometimes it's fun to bash just one form.
		}

		protected override IControlBasher[] GetControlBashers(Control control) => new[] { new EnsureFieldIsReadonlyBasher() };

		ProcessTaskTemplate universalTemplate;
		ProcessTaskTemplate regularTemplate;

		protected override void SetUp()
		{
			base.SetUp();

			var bmHelper = ObjectFactory.Get<IBMTestHelper>();
			bmHelper.EnableBMSInRegistry();
			bmHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			universalTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, DummyWorkflowDescriptor.Instance.Code, name: "Universal Template", isSystemDefined: true, isUniversal: true);
			regularTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, DummyWorkflowDescriptor.Instance.Code, name: "Regular Template", isSystemDefined: true);

			Factory.Save();
		}

		ProcessTaskTemplateForm GetUniversalTemplateForm() => new ProcessTaskTemplateForm(universalTemplate);
		ProcessTaskTemplateForm GetRegularTemplateForm() => new ProcessTaskTemplateForm(regularTemplate);

		#endregion

		#region Readonly Basher

		class EnsureFieldIsReadonlyBasher : IControlBasher
		{
			void IControlBasher.Bash(Control control, INotifications notifications)
			{
				var bindingMember = control.GetBindingMember();

				if (!string.IsNullOrEmpty(bindingMember) && control.Visible)
				{
					var readOnlyPropertyDescriptor = control.GetReadOnlyPropertyDescriptor();

					if (readOnlyPropertyDescriptor?.GetValue(control) is bool value && !value)
					{
						var hierarchy = control.SelectRecursive(c => c != null ? new[] { c.Parent } : Array.Empty<Control>()).WhereNotNull();

						if (!ParentHierarchyIncludeTabPageThatIsAllowedToBeEditable(hierarchy) && !IsBoundPropertyAllowedToBeEditableOnSystemDefinedTemplate(bindingMember))
						{
							var ancestorControlNames = string.Join(", ", hierarchy.Select(c => c.Name));

							var message = FormattableString.Invariant(
		$@"Control named [{control.Name}], bound to property [{bindingMember}], is not marked as ReadOnly, even though its parent template is system-defined.
If this property should be editable, add it to the list of exemptions in this test. Most of the time, properties on a system-defined template should be readonly though, so it's more likely you'll need to make the bound business objects or collections readonly instead.
The control is located in this hierarchy: {ancestorControlNames}");
							notifications.AddError(message);
						}
					}
				}
			}

			static bool ParentHierarchyIncludeTabPageThatIsAllowedToBeEditable(IEnumerable<Control> parentHierarchy)
			{
				return parentHierarchy.Any(c => c.Name.In("NotesTabPage", "ChangeLogsTabPage"));
			}

			static bool IsBoundPropertyAllowedToBeEditableOnSystemDefinedTemplate(string bindingMember)
			{
				return bindingMember.In(ProcessTaskTemplateSchema.Constants.P0_IsActive);
			}
		}

		#endregion
	}
}
