using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration.DocumentEngine;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Business.Test;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	[TestedType(typeof(ValidationRuleForm))]
	class ValidationRuleFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestCriteriaPopup_ParentTypeIsFromDataContext()
		{
			var helper = new RuleHelper(Factory);
			var ruleSet = helper.CreateRuleSet(DataContextType.ForwardingConsol);
			var expectedType = DataContextType.ForwardingConsol.GetUniversalDataContextManager().TopLevelBusinessObjectType;

			var mockTreeView = new Moq.Mock<IMapTreePresentationManager>();

			using (ObjectFactory.Substitute(mockTreeView.Object))
			using (var form = new ValidationRuleForm(ruleSet))
			{
				form.CriteriaPopupButton_Click(form.criteriaPopupButton, EventArgs.Empty);
				mockTreeView.VerifySet(x => x.ParentTypes = new[] { expectedType });
			}
		}

		[ExpectNoExceptions]
		public void TestCriteriaPopup_ParentTypeDefaultIsForwardingShipment()
		{
			var ruleSet = Factory.NewWithValidTestData<UniversalValidationRuleSet>();
			ruleSet.VRS_DataContext = "";
			var expectedType = DataContextType.ForwardingShipment.GetUniversalDataContextManager().TopLevelBusinessObjectType;

			var mockTreeView = new Moq.Mock<IMapTreePresentationManager>();

			using (ObjectFactory.Substitute(mockTreeView.Object))
			using (var form = new ValidationRuleForm(ruleSet))
			{
				form.CriteriaPopupButton_Click(form.criteriaPopupButton, EventArgs.Empty);
				mockTreeView.VerifySet(x => x.ParentTypes = new[] { expectedType });
			}
		}

		protected override Form GetFormToBashCore()
		{
			var bizo = Factory.New<UniversalValidationRuleSet>();
			return new ValidationRuleForm(bizo);
		}
	}
}
