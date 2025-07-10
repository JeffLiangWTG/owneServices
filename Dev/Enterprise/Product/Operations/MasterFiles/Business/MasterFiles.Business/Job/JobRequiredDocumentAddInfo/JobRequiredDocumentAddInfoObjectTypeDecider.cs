using System;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class JobRequiredDocumentAddInfoObjectTypeDecider
	{
		public Type GetTypeForNewAddInfo(JobRequiredDocumentAddInfo documentAddInfo)
		{
			var company = documentAddInfo.Company;
			var applicationCode = documentAddInfo.EX_ApplicationCode;

			return GetTypeForNewAddInfo(company, applicationCode);
		}

		//=============================================================================
		//WHEN a new application id is mapped for a new country, please do not forget to 
		//add an instruction on how users can delete dbo.JobRequiredDocumentAddInfo
		//in the method below, GetInstructionsToDeleteDISMessagingRecords
		//=============================================================================
		public Type GetTypeForNewAddInfo(GlbCompany company, ZString applicationCode)
		{
			var countryCodeOfCompany = company != null ? company.GC_RN_NKCountryCode : ZString.Empty;
			switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCodeOfCompany))
			{
				case Core.Constants.CountryCodes.UnitedStates:
					return GetTypeForUS(applicationCode);
				case Core.Constants.CountryCodes.Canada:
					return GetTypeForCA(applicationCode);

				default:
					return null;
			}
		}

		Type GetTypeForUS(string applicationCode)
		{
			switch (applicationCode)
			{
				case Core.Constants.Customs.DocumentImageSystemIDs.US_DIS:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.DIS.IDISDocument>();

				default:
					return null;
			}
		}

		Type GetTypeForCA(string applicationCode)
		{
			switch (applicationCode)
			{
				case Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CA.DIF.IDIFDocument>();

				default:
					return null;
			}
		}

		internal static string GetInstructionsToDeleteDISMessagingRecords(GlbCompany company, string applicationCode)
		{
			var countryCodeOfCompany = company != null ? company.GC_RN_NKCountryCode : ZString.Empty;
			switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCodeOfCompany))
			{
				case Core.Constants.CountryCodes.UnitedStates:
					return GetInstructionForUS(company, applicationCode);
				case Core.Constants.CountryCodes.Canada:
					return GetInstructionForCA(applicationCode);

				default:
					return null;
			}
		}

		static string GetInstructionForUS(GlbCompany company, string applicationCode)
		{
			switch (applicationCode)
			{
				case Core.Constants.Customs.DocumentImageSystemIDs.US_DIS:

					if (company.PK != GlbCompany.CurrentCompany.PK)
					{
						return Res.GetString("44b2f6dc-75de-468d-b9bf-8bf8aa0856f2", "go to eDocs and click 'View/Edit DIS Data'. Then delete the related records in the first grid.");
					}
					else
					{
						return Res.GetString("44b2f6dc-75de-468d-b9bf-8bf8aa0856f3", "Please click 'View/Edit DIS Data'. Then delete the related records in the first grid.");
					}
				default:
					return null;
			}
		}

		static string GetInstructionForCA(string applicationCode)
		{
			switch (applicationCode)
			{
				case Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF:
					return Res.GetString("44b2f6dc-75de-468d-b9bf-8bf8aa0856f3", "Please click 'View/Edit DIS Data'. Then delete the related records in the first grid.");
				default:
					return null;
			}
		}
	}
}
