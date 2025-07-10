using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using IImporterNumberRequester = Enterprise.Integration.Customs.US.IImporterNumberRequester;
using IMQEDIMessage = Enterprise.Integration.Customs.US.IMQEDIMessage;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ImporterNumberRequester : IImporterNumberRequester
	{
		public ImporterNumberRequester(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		public ImporterNumberRequester() : this(new BusinessObjectFactory())
		{
		}

		readonly BusinessObjectFactory factory;

		public MQEDIMessage RequestImporterBond(BusinessObject parent, string importerNumber)
		{
			var qibk = new AQIBK();
			qibk.ImporterNumber1 = importerNumber;
			qibk.AddressRequestCode1 = "1"; // Transmit K1, K2, K3, K4, K5, and K6 records.

			var portCode = ProcessingPortCodeAndFilerFinder.GetPrcessingPortCodeFromRegistryForCompany(GlbCompany.CurrentCompany);
			var officeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var blockControlGenerator = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), portCode, officeCode);
			blockControlGenerator.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.QueryImporterBond;
			blockControlGenerator.AddMessageBlock(qibk);
			var message = blockControlGenerator.CreateMessage<MQEDIMessage>(factory);
			message.EM_LinkedObject = parent;
			factory.Save();
			return message;
		}

		public bool IsValidImporterBondNumber(ZString importerBondNumber)
		{
			return SocialSecurityNumberValidator.IsValidSSN(importerBondNumber) ||
								EmployerIdentificationNumberValidator.IsValidEINForStandAloneInBondMessage(importerBondNumber) ||
								CBPAssignedNumberValidator.IsValidCBPAssignedNumber(importerBondNumber);
		}

		public bool HasPermissionToSendImporterBondNumber(ZString importerBondNumber)
		{
			return !SocialSecurityNumberValidator.IsValidSSN(importerBondNumber) || Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
		}

		public ZString CurrentCompanyRegistryItemValidationForRequestImporterBond()
		{
			var result = ZString.Empty;
			var filerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
			var processingPortCode = ProcessingPortCodeAndFilerFinder.GetPrcessingPortCodeFromRegistryForCompany(GlbCompany.CurrentCompany);
			if (filerCode.IsEmpty || processingPortCode.IsEmpty)
			{
				result = "The Processing Port Code or Entry Filer Code has not been set up in the Registry for the current login company.";

				var usCompanies = factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Common.US.USCustomsJurisdiction.Countries));
				foreach (GlbCompany usCompany in usCompanies)
				{
					if (usCompany != GlbCompany.CurrentCompany)
					{
						var otherFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(usCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
						var otherProcessingPortCode = ProcessingPortCodeAndFilerFinder.GetPrcessingPortCodeFromRegistryForCompany(usCompany);
						if (!(otherFilerCode.IsEmpty || otherProcessingPortCode.IsEmpty))
						{
							result = result + ZString.Format(" Please log in to '{0} - {1}' to send this query message. ", usCompany.GC_Code, usCompany.GC_Name);
							return result;
						}
					}
				}
			}
			return result;
		}

		#region IImporterNumberRequester
		ZString IImporterNumberRequester.CurrentCompanyRegistryItemValidationForRequestImporterBond()
		{
			return CurrentCompanyRegistryItemValidationForRequestImporterBond();
		}

		IMQEDIMessage IImporterNumberRequester.RequestImporterBond(BusinessObject parent, string importerNumber)
		{
			return RequestImporterBond(parent, importerNumber);
		}

		bool IImporterNumberRequester.IsValidImporterBondNumber(ZString importerBondNumber)
		{
			return IsValidImporterBondNumber(importerBondNumber);
		}

		bool IImporterNumberRequester.HasPermissionToSendImporterBondNumber(ZString importerBondNumber)
		{
			return HasPermissionToSendImporterBondNumber(importerBondNumber);
		}
		#endregion

	}
}
