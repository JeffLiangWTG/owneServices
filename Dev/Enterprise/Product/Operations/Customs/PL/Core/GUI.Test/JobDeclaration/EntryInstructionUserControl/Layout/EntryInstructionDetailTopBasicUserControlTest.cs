using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EntryInstructionDetailTopBasicUserControlTest : TestCaseWithFactory
{
	public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		var entryInstr = declaration.CustomsEntryInstructions.AddNew();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		line.JI_CEI = entryInstr.PK;

		using (var form = new ZForm(entryInstr))
		using (var control = new EntryInstructionDetailTopBasicUserControl())
		{
			form.Controls.Add(control);
			form.Show();
		}

		var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
		var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);

		var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
		var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

		var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
		var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);

		foreach (var dictionaryOfSubcriber in dictionary)
		{
			foreach (var subcriber in dictionaryOfSubcriber.Value)
			{
				var subcriberTarget = subcriber.Value.Target;
				if (subcriberTarget is DeclarationValueChangedAnnouncer)
				{
					var onValueChangedField = subcriberTarget.GetType().BaseType.GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
					var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
					var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
					Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is EntryInstructionDetailTopBasicUserControl));
				}
				else
				{
					Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is EntryInstructionDetailTopBasicUserControl));
				}
			}
		}
	}

	public void TestControls()
	{
		using (var control = new EntryInstructionDetailTopBasicUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("ExportManifestCheckBox", () => control.FindSingle<ZCheckBox>("ExportManifestCheckBox"));
				AssertNoExceptionThrown("PostExportTransitCheckBox", () => control.FindSingle<ZCheckBox>("PostExportTransitCheckBox"));
				AssertNoExceptionThrown("EADPrintOutDropEdit", () => control.FindSingle<ZDropEdit>("EADPrintOutDropEdit"));
				AssertNoExceptionThrown("TemporaryLocationDropEdit", () => control.FindSingle<ZDropEdit>("TemporaryLocationDropEdit"));
				AssertNoExceptionThrown("TemporaryLocationTextBox", () => control.FindSingle<ZTextBox>("TemporaryLocationTextBox"));
				AssertNoExceptionThrown("OfficeOfExitArrivalTimeLimitDateEdit", () => control.FindSingle<ZDateEdit>("OfficeOfExitArrivalTimeLimitDateEdit"));
				AssertNoExceptionThrown("DeclarationDateDateEdit", () => control.FindSingle<ZDateEdit>("DeclarationDateDateEdit"));
			});
		}
	}
}
