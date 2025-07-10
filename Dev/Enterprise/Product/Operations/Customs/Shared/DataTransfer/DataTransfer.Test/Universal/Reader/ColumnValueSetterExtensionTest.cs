using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class ColumnValueSetterExtensionTest : TestCaseWithFactory
	{
		public void TestSetValueOnSetterSupenderParentInSpecificOrder()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			declaration.JE_GoodsDescriptionInfo.ValueChanged += (object s, EventArgs e) =>
			{
				declaration.JE_ContainerMode = "XXX";
			};

			var valueGetter = new StringColumnValueSetter
			(
				DataObjectReader.GetColumnIndexerFromRow(declaration)
				, JobDeclarationSchema.JE_ContainerMode
				, () => "CNT"
				, new TestErrorLogger()
			);

			var dictionary = new Dictionary<string, ValueSetter>();
			dictionary.Add(JobDeclarationSchema.Constants.JE_ContainerMode, valueGetter);

			declaration.JE_ContainerMode = "LCL";
			AssertEquals("Precondition.", "LCL", declaration.JE_ContainerMode);

			declaration.JE_GoodsDescription = "UPDATE CONTAINER MODE 000";
			AssertEquals("Should be updated from the event.", "XXX", declaration.JE_ContainerMode);

			var setterSuspender = declaration.SetterSuspender;

			dictionary.SetValueOnSetterSupenderParentInSpecificOrder(setterSuspender, null);
			AssertEquals("Should be changed as there is no any suspended actions on JE_ContainerMode.", "CNT", declaration.JE_ContainerMode);

			declaration.JE_ContainerMode = "LCL";

			using (setterSuspender.SuspendSetting(JobDeclarationSchema.Constants.JE_ContainerMode))
			{
				dictionary.SetValueOnSetterSupenderParentInSpecificOrder(setterSuspender, null);
				AssertEquals("Should ignore the suspended setting action for importing values.", "CNT", declaration.JE_ContainerMode);

				declaration.JE_GoodsDescription = "UPDATE CONTAINER MODE 001";
				AssertEquals("Should care the suspended setting action when the value is not from importing.", "CNT", declaration.JE_ContainerMode);
			}

			declaration.JE_ContainerMode = "LCL";

			using (setterSuspender.ResumeSetting(JobDeclarationSchema.Constants.JE_ContainerMode))
			{
				dictionary.SetValueOnSetterSupenderParentInSpecificOrder(setterSuspender, null);
				AssertEquals("Should be changed as there is no any suspended actions on JE_ContainerMode.", "CNT", declaration.JE_ContainerMode);

				declaration.JE_GoodsDescription = "UPDATE CONTAINER MODE 002";
				AssertEquals("Should be changed as there is no any suspended actions on JE_ContainerMode.", "XXX", declaration.JE_ContainerMode);
			}
		}
	}
}
