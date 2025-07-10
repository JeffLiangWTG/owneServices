using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderDocumentSupporter : DocumentSupporter
	{
		public CusISFHeaderDocumentSupporter(CusISFHeader header)
			: base(header)
		{
		}

		protected CusISFHeader Header
		{
			get { return (CusISFHeader)BusinessObject; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.GenericFreightJob };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusISFHeader; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			string result = "";

			switch (filterName)
			{
				case DocumentFilters.MOD:
					result = Header.BF_TransportMode;
					break;
				case DocumentFilters.MSC:
					result = "OTH";
					break;
				case DocumentFilters.CTY:
					result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					break;
				case DocumentFilters.BKRCTY:
					result = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					break;
				default:
					result = base.GetFilterValue(filterName);
					break;
			}

			return result;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ImporterSecurityFilingCustomiseDocuments; }
		}

		public override string TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.Consignee)
			{
				return new OrgHeaderContact(Header.Importer, null);
			}
			return base.GetContactOrganisation(menuName, contact, direction);
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.GenericFreightJob)
			{
				return Res.GetString("638AB3BD-225C-4D80-9CED-338ECD911C14", "ISF Header or Line Details cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}
	}
}
