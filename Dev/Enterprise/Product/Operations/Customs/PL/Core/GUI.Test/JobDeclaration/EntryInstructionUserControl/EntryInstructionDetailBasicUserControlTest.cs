using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new EntryInstructionDetailBasicUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNotNull("OtherPartiesDetailsPanel", control.FindSingleOrDefault<ZPanel>("OtherPartiesDetailsPanel"));
				AssertNotNull("OtherPartiesGroupBox", control.FindSingleOrDefault<ZGroupBox>("OtherPartiesGroupBox"));
				AssertNotNull("FromWarehouseGroupBox", control.FindSingleOrDefault<ZGroupBox>("FromWarehouseGroupBox"));
				AssertNotNull("FromWarehouseCodeTextBox", control.FindSingleOrDefault<ZTextBox>("FromWarehouseCodeTextBox"));
				AssertNotNull("FromWarehouseAddressControl", control.FindSingleOrDefault<ZAddressControl>("FromWarehouseAddressControl"));
				AssertNotNull("ToWarehouseGroupBox", control.FindSingleOrDefault<ZGroupBox>("ToWarehouseGroupBox"));
				AssertNotNull("ToWarehouseCodeTextBox", control.FindSingleOrDefault<ZTextBox>("ToWarehouseCodeTextBox"));
				AssertNotNull("ToWarehouseAddressControl", control.FindSingleOrDefault<ZAddressControl>("ToWarehouseAddressControl"));
				AssertNotNull("DetailsPanel", control.FindSingleOrDefault<ZPanel>("DetailsPanel"));
			});
		}
	}

	public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		var entryInstr = declaration.CustomsEntryInstructions.AddNew();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		line.JI_CEI = entryInstr.PK;

		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailBasicUserControl())
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
					Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is EntryInstructionDetailBasicUserControl));
				}
				else
				{
					Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is EntryInstructionDetailBasicUserControl));
				}
			}
		}
	}
}
