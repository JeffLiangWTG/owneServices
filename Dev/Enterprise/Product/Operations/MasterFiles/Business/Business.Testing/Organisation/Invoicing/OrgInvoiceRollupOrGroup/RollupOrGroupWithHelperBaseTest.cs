using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RollupOrGroupWithHelperBaseTest : TestCaseWithFactory
	{
		public void TestForDisallowedInvoiceLineGroupingsForGroupOrSubTotal()
		{
			var helper = new InvoiceRollupOrGroupHelper(InvoiceRollupOrGroup);
			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			Assert("Should contain style", helper.GroupOrSubTotalStyleList.ContainsCode(OrgConstants.InvoiceLineGroupings.Code.None));
			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence;
			Assert("Should not contain style", !helper.GroupOrSubTotalStyleList.ContainsCode(OrgConstants.InvoiceLineGroupings.Code.None));
		}

		public void TestGroupOrSubTotalStyleListWhenGroupOrSubTotalOnlyForCLCAndNOG()
		{
			var helper = new InvoiceRollupOrGroupHelper(InvoiceRollupOrGroup);
			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Sequence;
			var groupOrSubTotalOnlyForCLCAndNOGList = new List<ZString>()
			{ OrgConstants.GroupOrSubTotalCharges.Code.Sequence, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical, OrgConstants.GroupOrSubTotalCharges.Code.User };

			foreach (var groupOrSubTotal in groupOrSubTotalOnlyForCLCAndNOGList)
			{
				Assert("IsGroupOrSubTotalOnlyForCLCAndNOG", helper.IsGroupOrSubTotalOnlyForCLCAndNOG(groupOrSubTotal));
				Assert("Should contain CLC", helper.GroupOrSubTotalStyleList.ContainsCode(OrgConstants.InvoiceLineGroupings.Code.CLC));
				Assert("Should contain NOG", helper.GroupOrSubTotalStyleList.ContainsCode(OrgConstants.InvoiceLineGroupings.Code.None));
			}
		}

		public void TestStyleShouldNotBeReadonlyWhenDisplayIsRolOrSubOrSSQ()
		{
			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			AssertEquals("Style Should Not Be Readonly", false, InvoiceRollupOrGroup.GroupOrSubtotalStyleInfo.ReadOnly);

			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			AssertEquals("Style Should Not Be Readonly", false, InvoiceRollupOrGroup.GroupOrSubtotalStyleInfo.ReadOnly);

			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence;
			AssertEquals("Style Should Not Be Readonly", false, InvoiceRollupOrGroup.GroupOrSubtotalStyleInfo.ReadOnly);

			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence;
			AssertEquals("Style Should Not Be Readonly", false, InvoiceRollupOrGroup.GroupOrSubtotalStyleInfo.ReadOnly);

			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical;
			AssertEquals("Style Should Not Be Readonly", false, InvoiceRollupOrGroup.GroupOrSubtotalStyleInfo.ReadOnly);

			InvoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol;
			AssertEquals("Style Should Not Be Readonly", false, InvoiceRollupOrGroup.GroupOrSubtotalStyleInfo.ReadOnly);
		}

		public void TestJobType()
		{
			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			Assert("TransportMode should be editable", !InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);
			Assert("ServiceDirection should be editable", !InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			Assert("TransportMode should be editable", !InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);
			Assert("ServiceDirection should be editable", !InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			Assert("TransportMode should be editable", !InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);
			Assert("ServiceDirection should be editable", !InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);

			InvoiceRollupOrGroup.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			InvoiceRollupOrGroup.ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			AssertEquals("Pre-Condition: TransportMode Air", OrgConstants.ModesForGroupOrSubTotal.Codes.Air, InvoiceRollupOrGroup.TransportMode);
			AssertEquals("Pre-Condition: ServiceDirection Export", OrgConstants.ServiceDirection.Code.Export, InvoiceRollupOrGroup.ServiceDirection);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.CFSLoadList.Code;
			AssertEquals("TransportMode should be ALL", OrgConstants.ModesForGroupOrSubTotal.Codes.All, InvoiceRollupOrGroup.TransportMode);
			AssertEquals("ServiceDirection should be ALL", OrgConstants.ServiceDirection.Code.All, InvoiceRollupOrGroup.ServiceDirection);
			Assert("TransportMode should be ReadOnly", InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);
			Assert("ServiceDirection should be ReadOnly", InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);

			InvoiceRollupOrGroup.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			InvoiceRollupOrGroup.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			AssertEquals("InvoiceLineDisplayOption should be NON", InvoiceDescriptionOptionsList.Codes.None, InvoiceRollupOrGroup.InvoiceLineDisplayOption);
			AssertEquals("InvoicePostingStyle should be empty", "", InvoiceRollupOrGroup.InvoicePostingStyle);
			Assert("InvoiceLineDisplayOption should be ReadOnly", InvoiceRollupOrGroup.InvoiceLineDisplayOptionInfo.ReadOnly);
			Assert("InvoicePostingStyle should be ReadOnly", InvoiceRollupOrGroup.InvoicePostingStyleInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			Assert("InvoiceLineDisplayOption should be not ReadOnly", !InvoiceRollupOrGroup.InvoiceLineDisplayOptionInfo.ReadOnly);
			Assert("InvoicePostingStyle should be not ReadOnly", !InvoiceRollupOrGroup.InvoicePostingStyleInfo.ReadOnly);
		}

		public void TestTransportModeReadOnly()
		{
			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			Assert("Should be not readonly, JobType is Brokerage type", !InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			Assert("Should be not readonly, JobType is Shipment type", !InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			Assert("Should be not readonly, JobType is ShipmentAndBrokerage type", !InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.CTOCusMAWB.Code;
			Assert("Should be readonly, JobType is CTOCusMawb type", InvoiceRollupOrGroup.TransportModeInfo.ReadOnly);
		}

		public void TestServiceDirectionReadOnly()
		{
			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			Assert("Should be not readonly, JobType is Brokerage type", !InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			Assert("Should be not readonly, JobType is Shipment type", !InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			Assert("Should be not readonly, JobType is ShipmentAndBrokerage type", !InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.CTOCusMAWB.Code;
			Assert("Should be readonly, JobType is CTOCusMawb type", InvoiceRollupOrGroup.ServiceDirectionInfo.ReadOnly);
		}

		#region Lookups

		public void TestJobTypeList()
		{
			int originalCount = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes().Count;
			AssertEquals("JobTypeList.Count should be 3 more than standard list", originalCount + 3, InvoiceRollupOrGroup.JobTypeList.Count);
		}

		public void TestTransportModeList()
		{
			AssertEquals("TransportModeList.Count", 8, InvoiceRollupOrGroup.TransportModeList.Count);
		}

		public void TestServiceDirectionList()
		{
			AssertEquals("ServiceDirectionList.Count", 5, InvoiceRollupOrGroup.ServiceDirectionList.Count);
		}

		public void TestGroupOrSubTotalList()
		{
			int additionalElement = IsDefaultCodeShouldBeInLookups ? 1 : 0;

			AssertEquals("GroupOrSubTotalList.Count", 7 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalList.Count);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("GroupOrSubTotalList.Count", 7 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalList.Count);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			AssertEquals("GroupOrSubTotalList.Count", 8 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalList.Count);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.GatewayConsol.Code;
			AssertEquals("GroupOrSubTotalList.Count", 8 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalList.Count);
		}

		public void TestInvoicePostingOptionsList()
		{
			int additionalElement = IsDefaultCodeShouldBeInLookups ? 1 : 0;

			AssertEquals("InvoicePostingOptionsList.Count", 14 + additionalElement, InvoiceRollupOrGroup.InvoicePostingOptionsList.Count);
		}

		public void TestInvoiceLineDisplayOptionsList()
		{
			int additionalElement = IsDefaultCodeShouldBeInLookups ? 1 : 0;

			AssertEquals("InvoiceLineDisplayOptionsList.Count", 8 + additionalElement, InvoiceRollupOrGroup.InvoiceLineDisplayOptionsList.Count);
		}

		public void TestGroupOrSubTotalStyleList()
		{
			int additionalElement = IsDefaultCodeShouldBeInLookups ? 1 : 0;

			InvoiceRollupOrGroup.JobType = "";
			AssertEquals("GroupOrSubTotalStyleList.Count", 5 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.CusMAWB.Code;
			AssertEquals("When GroupOrRollup is not a shipment or brokerage", 5 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			AssertEquals("When GroupOrRollup is brokerage type", 14 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("When GroupOrRollup is Shipment type", 14 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			AssertEquals("When GroupOrRollup is ShipmentAndBrokerage type", 14 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			AssertEquals("When GroupOrRollup is ALL type", 14 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.LocalCartage.Code;
			AssertEquals("When GroupOrRollup is LocalTransport", 5 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			AssertEquals("When GroupOrRollup is NonJobRelated", 4 + additionalElement, InvoiceRollupOrGroup.GroupOrSubTotalStyleList.Count);
		}

		public void TestInvoicePostingCurrencies()
		{
			AssertNotNull(InvoiceRollupOrGroup.InvoicePostingCurrencies);
			Assert("Is RefCurrencyCollection", InvoiceRollupOrGroup.InvoicePostingCurrencies is RefCurrencyCollection);
			Assert("Should not be loaded", !((IBusinessObjectCollection)InvoiceRollupOrGroup.InvoicePostingCurrencies).IsLoaded);
		}

		#endregion

		#region Validation

		public virtual void TestValidateInvoicePostingStyle()
		{
			InvoiceRollupOrGroup.InvoicePostingStyle = "RRR";
			AssertHasNotifications("Invalid code, has errors", InvoiceRollupOrGroup.InvoicePostingStyleInfo);

			InvoiceRollupOrGroup.InvoicePostingStyle = ZString.Empty;
			AssertHasNotifications("No code, has errors", InvoiceRollupOrGroup.InvoicePostingStyleInfo);

			InvoiceRollupOrGroup.InvoicePostingStyle = InvoiceRollupOrGroup.InvoicePostingOptionsList[0].Code;
			AssertNoNotifications("Valid code, no errors", InvoiceRollupOrGroup.InvoicePostingStyleInfo);

			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			InvoiceRollupOrGroup.InvoicePostingStyle = ZString.Empty;
			AssertNoNotifications("Valid code, no errors", InvoiceRollupOrGroup.InvoicePostingStyleInfo);
		}

		public virtual void TestValidateInvoiceLineDisplayOption()
		{
			InvoiceRollupOrGroup.InvoiceLineDisplayOption = "RRR";
			AssertHasNotifications("Error code", InvoiceRollupOrGroup.InvoiceLineDisplayOptionInfo);

			InvoiceRollupOrGroup.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			AssertNoNotifications("Valid code", InvoiceRollupOrGroup.InvoiceLineDisplayOptionInfo);
		}

		public virtual void TestValidateJobType()
		{
			InvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			InvoiceRollupOrGroup.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			InvoiceRollupOrGroup.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;

			SecondInvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			SecondInvoiceRollupOrGroup.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			SecondInvoiceRollupOrGroup.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;

			RunPreSaveValidation();
			AssertNoErrors("Should be no errors, code should be valid", InvoiceRollupOrGroup.JobTypeInfo);
			AssertNoErrors("Should be no errors, code should be valid", SecondInvoiceRollupOrGroup.JobTypeInfo);

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			SecondInvoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;

			RunPreSaveValidation();
			AssertNoErrors("Should be no errors, code should be valid", InvoiceRollupOrGroup.JobTypeInfo);
			AssertNoErrors("Should be no errors, code should be valid", SecondInvoiceRollupOrGroup.JobTypeInfo);

			AssertProperty(InvoiceRollupOrGroup.JobTypeInfo, JobInvoicingConsumerTypes.Shipment.Code);
		}

		public void TestRollupOrGroupWithTheSameJobTypeDirectionAndMode()
		{
			SecondInvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			SecondInvoiceRollupOrGroup.ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			SecondInvoiceRollupOrGroup.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			SecondInvoiceRollupOrGroup.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightFOB;
			SecondInvoiceRollupOrGroup.InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal;

			InvoiceRollupOrGroup.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			InvoiceRollupOrGroup.ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			InvoiceRollupOrGroup.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			InvoiceRollupOrGroup.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			InvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertHasErrors("JobType should have errors", InvoiceRollupOrGroup.JobTypeInfo);

			string errorMessage = "Can NOT have more than one Invoice Group or Sub Total Charge with the same Job Type, Direction and Mode";
			AssertHasError("ErrorMessage", InvoiceRollupOrGroup.JobTypeInfo, errorMessage);
		}

		public virtual void TestValidateServiceDirection()
		{
			AssertProperty(InvoiceRollupOrGroup.ServiceDirectionInfo, OrgConstants.ServiceDirection.Code.CrossTrade);
		}

		public virtual void TestValidateTransportMode()
		{
			AssertProperty(InvoiceRollupOrGroup.TransportModeInfo, OrgConstants.ModesForGroupOrSubTotal.Codes.Rail);
		}

		public virtual void TestValidateGroupOrSubTotal()
		{
			AssertProperty(InvoiceRollupOrGroup.GroupOrSubTotalInfo, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);
		}

		public virtual void TestValidateGroupOrSubTotalStyle()
		{
			AssertProperty(InvoiceRollupOrGroup.GroupOrSubtotalStyleInfo, OrgConstants.InvoiceLineGroupings.Code.All);
		}

		public void TestValidateInvoicePostingCurrency()
		{
			AssertNoErrors("Precondition - no error with default value", InvoiceRollupOrGroup.InvoicePostingCurrencyInfo);

			InvoiceRollupOrGroup.InvoicePostingCurrency = "***";
			AssertHasError(InvoiceRollupOrGroup.InvoicePostingCurrencyInfo, "Enter a valid selection.");

			InvoiceRollupOrGroup.InvoicePostingCurrency = "USD";
			AssertNoErrors("Should be no errors, USD is a valid currency code", InvoiceRollupOrGroup.InvoicePostingCurrencyInfo);

			InvoiceRollupOrGroup.InvoicePostingCurrency = ZString.Empty;
			AssertNoErrors("Should be no errors as empty value is ", InvoiceRollupOrGroup.InvoicePostingCurrencyInfo);
		}

		#endregion

		#region Implementation

		protected abstract IInvoiceRollupOrGroup InvoiceRollupOrGroup { get; }
		protected abstract IInvoiceRollupOrGroup SecondInvoiceRollupOrGroup { get; }
		protected abstract void RunPreSaveValidation();

		protected virtual bool IsDefaultCodeShouldBeInLookups
		{
			get { return false; }
		}

		void AssertProperty(ZPropertyInfo propertyInfo, ZString validCode)
		{
			propertyInfo.Value = validCode;
			AssertNoErrors("Should be no errors, code should be valid", propertyInfo);

			propertyInfo.Value = (ZString)"ZZZ";
			AssertHasErrors("Should be errors, ZZZ is not a valid code", propertyInfo);

			propertyInfo.Value = ZString.Empty;
			AssertHasErrors("Should be errors, " + propertyInfo.Name + " is mandatory", propertyInfo);
		}

		#endregion
	}
}
