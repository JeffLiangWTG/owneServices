using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(CertificationCodeMappingRegistryItemEditor))]
	class CertificationCodeMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new CertificationCodeMappingRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CertificationCodeMappingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CertificationCodeMappingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CertificationCodeMappingRegistryItem("", (NoResString)"");
		}

		protected override object[] GetValidRegistryValues()
		{
			CertificationCodeMappingCollection collection = new CertificationCodeMappingCollection();
			CertificationCodeMapping item = collection.AddNew();
			item.MainCode = "XXX";
			item.SpecialisationCode = (NoResString)"ZZZ";
			return new object[] { collection };
		}

		protected override void SetUp()
		{
			base.SetUp();
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			certCodes.Add("ZZZ", (NoResString)"ZZZ");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
		#endregion
	}
}
