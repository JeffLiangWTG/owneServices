using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

class ProcessTemplateValidationActionSourceListTest : TestCase
{
	public void TestFieldSpecificValidationConditions()
	{
		using (TemporarilyEnableFSV())
		{
			AssertCollectionContains(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation, new ProcessTemplateValidationActionSourceList().GetAllCodes());
		}

		using (TemporarilyEnableFSV(isPilot: true))
		{
			AssertCollectionContains(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation, new ProcessTemplateValidationActionSourceList().GetAllCodes());
		}

		using (TemporarilyEnableFSV())
		using (TemporarilyEnableFSV(isPilot: true))
		{
			AssertCollectionContains(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation, new ProcessTemplateValidationActionSourceList().GetAllCodes());
		}

		AssertCollectionNotContains(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation, new ProcessTemplateValidationActionSourceList().GetAllCodes());
	}

	internal static IDisposable TemporarilyEnableFSV(bool isPilot = false) =>
		ObjectFactory.Get<IZZCustomsFunctionalityEffectiveDate>().TemporarilySetFunctionality(
			isPilot
				? ProcessTemplateValidationActionSourceList.FSVPilotFunctionCode
				: ProcessTemplateValidationActionSourceList.FSVFunctionalityCode,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
			ZDateTime.Now,
			value: true);
}
