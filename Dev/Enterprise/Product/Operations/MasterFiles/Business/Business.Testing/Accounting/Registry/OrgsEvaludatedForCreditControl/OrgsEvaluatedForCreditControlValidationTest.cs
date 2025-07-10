using System;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class OrgsEvaluatedForCreditControlValidationTest : JobConfigurationSelectorValidationTest
	{
		public void TestValidateOrganizationType()
		{
			AssertNoErrors("Precondition: Organization Type should not have errors.", BizObj.OrganizationTypeInfo);

			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.Shipment.Code, OrganizationTypePermittedForShipmentsList);
			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.Brokerage.Code, OrganizationTypePermittedForDeclarationsList);
			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.QuotedBooking.Code, OrganizationTypePermittedForBookingsList);
			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.CFSShipment.Code, OrganizationTypePermittedForCFSsList);
			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.MasterAWB.Code, OrganizationTypePermittedForOthersList);
			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.CFSLoadList.Code, OrganizationTypePermittedForOthersList);
			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.FCLStorage.Code, OrganizationTypePermittedForOthersList);
			AssertIsPermittedOrganizationType(JobInvoicingConsumerTypes.LocalCartage.Code, OrganizationTypePermittedForOthersList);
		}

		public void TestValidateINCOTerm()
		{
			var incoTermList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			var domesticINCOTermList = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);
			CodeDescriptionPairList allINCOTermList = new CodeDescriptionPairList();
			allINCOTermList.AddRange(incoTermList);
			allINCOTermList.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));

			AssertNoErrors("Precondition: INCOTerm should not have errors.", BizObj.INCOTermInfo);
			BizObj.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Import;
			foreach (var code in incoTermList.GetAllCodes())
			{
				BizObj.INCOTerm = code;
				AssertNoErrors(BizObj.INCOTermInfo);
			}

			foreach (var code in domesticINCOTermList.GetAllCodes())
			{
				BizObj.INCOTerm = code;
				AssertHasError(BizObj.INCOTermInfo, "Enter a valid selection.");
			}

			BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Domestic;
			foreach (var code in incoTermList.GetAllCodes())
			{
				BizObj.INCOTerm = code;
				AssertHasError(BizObj.INCOTermInfo, "Enter a valid selection.");
			}

			foreach (var code in domesticINCOTermList.GetAllCodes())
			{
				BizObj.INCOTerm = code;
				AssertNoErrors(BizObj.INCOTermInfo);
			}

			BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.All;
			foreach (var code in allINCOTermList.GetAllCodes())
			{
				BizObj.INCOTerm = code;
				AssertNoErrors(BizObj.INCOTermInfo);
			}
		}

		public void TestValidateFreightPaymentTerm()
		{
			AssertNoErrors("Precondition: Freight Payment Term should not have errors.", BizObj.FreightPaymentTermInfo);
			BizObj.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			foreach (var code in BizObj.FreightPaymentTermList.GetAllCodes())
			{
				BizObj.FreightPaymentTerm = code;
				AssertNoErrors(BizObj.FreightPaymentTermInfo);
			}

			BizObj.FreightPaymentTerm = "XXX";
			AssertHasError(BizObj.FreightPaymentTermInfo, "Enter a valid selection.");
		}

		#region Implementation

		protected override string ExpectedDuplicateJobParametersError
		{
			get { return "At least one more record already sets organization evaluation behavior for the same Job parameters."; }
		}

		protected new OrgsEvaluatedForCreditControl BizObj
		{
			get { return (OrgsEvaluatedForCreditControl)base.BizObj; }
			set { base.BizObj = value; }
		}

		#endregion

		CodeDescriptionPairList CompleteOrganizationTypeList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.CompleteOrganizationTypeList; }
		}

		void AssertIsPermittedOrganizationType(string jobType, CodeDescriptionPairList permittedOptionsList)
		{
			BizObj.JobType = jobType;
			foreach (ICodeDescription availableOrganizationType in CompleteOrganizationTypeList)
			{
				BizObj.OrganizationType = availableOrganizationType.Code;
				Assert(String.Format("Organization Type '{0}' is not permitted for job type {1}", availableOrganizationType.Code, jobType),
					permittedOptionsList.Contains(availableOrganizationType) ^ BizObj.OrganizationTypeInfo.HasErrors());
			}
		}

		CodeDescriptionPairList OrganizationTypePermittedForShipmentsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForShipmentsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForDeclarationsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForDeclarationsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForBookingsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForBookingsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForCFSsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForCFSsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForOthersList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForOthersList; }
		}
	}
}
