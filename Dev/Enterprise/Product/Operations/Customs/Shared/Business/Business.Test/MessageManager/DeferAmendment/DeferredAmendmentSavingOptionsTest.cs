using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DeferredAmendmentSavingOptions))]
	sealed class DeferredAmendmentSavingOptionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSettingAnOptionToTrueMakesTheOtherTwoFalse()
		{
			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			SetThreeOptionsToTrue(savingOptions);

			savingOptions.SendAmendment = true;
			AssertEquals("SendAmendment", true, savingOptions.SendAmendment);
			AssertEquals("SaveWithEntryChanges", false, savingOptions.SaveWithEntryChanges);
			AssertEquals("SaveWithoutEntryChanges", false, savingOptions.SaveWithoutEntryChanges);

			SetThreeOptionsToTrue(savingOptions);
			savingOptions.SaveWithEntryChanges = true;
			AssertEquals("SendAmendment", false, savingOptions.SendAmendment);
			AssertEquals("SaveWithEntryChanges", true, savingOptions.SaveWithEntryChanges);
			AssertEquals("SaveWithoutEntryChanges", false, savingOptions.SaveWithoutEntryChanges);

			SetThreeOptionsToTrue(savingOptions);
			savingOptions.SaveWithoutEntryChanges = true;
			AssertEquals("SendAmendment", false, savingOptions.SendAmendment);
			AssertEquals("SaveWithEntryChanges", false, savingOptions.SaveWithEntryChanges);
			AssertEquals("SaveWithoutEntryChanges", true, savingOptions.SaveWithoutEntryChanges);
		}

		void SetThreeOptionsToTrue(DeferredAmendmentSavingOptions savingOptions)
		{
			savingOptions.fSendAmendment = true;
			savingOptions.fSaveWithEntryChanges = true;
			savingOptions.fSaveWithoutEntryChanges = true;

			AssertEquals("SendAmendment", true, savingOptions.SendAmendment);
			AssertEquals("SaveWithEntryChanges", true, savingOptions.SaveWithEntryChanges);
			AssertEquals("SaveWithoutEntryChanges", true, savingOptions.SaveWithoutEntryChanges);
		}

		public void TestSettingIsCancelledToTrue()
		{
			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			SetThreeOptionsToTrue(savingOptions);

			savingOptions.IsCancelled = true;
			AssertEquals("SendAmendment onCancelled", false, savingOptions.SendAmendment);
			AssertEquals("SaveWithEntryChanges onCancelled", false, savingOptions.SaveWithEntryChanges);
			AssertEquals("SaveWithoutEntryChanges onCancelled", false, savingOptions.SaveWithoutEntryChanges);
		}

		public void TestShouldSaveWithoutSendingAmendment()
		{
			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			savingOptions.SaveWithEntryChanges = true;
			AssertEquals("ShouldSaveWithoutSendingAmendment", true, savingOptions.ShouldSaveWithoutSendingAmendment);

			savingOptions.SendAmendment = true;
			AssertEquals("ShouldSaveWithoutSendingAmendment", false, savingOptions.ShouldSaveWithoutSendingAmendment);

			savingOptions.SaveWithoutEntryChanges = true;
			AssertEquals("ShouldSaveWithoutSendingAmendment", true, savingOptions.ShouldSaveWithoutSendingAmendment);
		}

		public void TestSetDefaultValues()
		{
			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			AssertEquals("SendAmendment should be defaulted", true, savingOptions.SendAmendment);
		}
	}
}
