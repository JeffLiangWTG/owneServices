using System.Collections.Generic;
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

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderDocumentSupporter : DocumentSupporter
	{
		public CusInBondHeaderDocumentSupporter(CusInBondHeader header)
			: base(header)
		{
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.MOD:
					return Header.BH_ImportTransportMode;

				case DocumentFilters.MSC:
					return "OTH";

				case DocumentFilters.CTY:
					return Header.Company.GC_RN_NKCountryCode;

				case DocumentFilters.BKRCTY:
					return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Header.Company.GC_RN_NKCountryCode);

				default:
					return base.GetFilterValue(filterName);
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.USInBondEditCustomiseDocuments; }
		}

		public override string TransportMode
		{
			get { return Header.BH_ImportTransportMode; }
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CBPForm7512DataContextValue));
			return result;
		}

		public const string CBPForm7512TemplateName = "CBPForm7512";
		public const string CBPForm7512DataContextValue = ".CBPForm7512";

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.ToString() == CBPForm7512DataContextValue)
			{
				return GetBODocDataFor7512Print();
			}
			else
			{
				return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		protected virtual IBODocDataProvider[] GetBODocDataFor7512Print()
		{
			List<IBODocDataProvider> result = new List<IBODocDataProvider>();

			foreach (CusInBondMoveHeader moveHeader in Header.MovementHeaders)
			{
				result.Add(BODocDataProvider.Get(new CBP7512Document(moveHeader)));
			}

			return result.ToArray();
		}

		protected CusInBondHeader Header
		{
			get { return (CusInBondHeader)BusinessObject; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.Notes, DataContext.CusInBondHeader };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusInBondHeader; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

			if (result == null)
			{
				switch (dataContext)
				{
					case DataContext.CusInBondHeader:
					case DataContext.Notes:
						DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(DataContext.CusInBondHeader, Header);
						if (wrapper != null)
						{
							result = new DocumentWrapper[] { wrapper };
						}
						break;
				}
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.CusInBondHeader || dataContextValue.DataContext == DataContext.Notes)
			{
				return Res.GetString("8F8F6A1D-4854-4E63-8ACC-AE2DECCB04B2", "In-bond Header cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return (dataContext == DataContext.CusInBondHeader || dataContext == DataContext.Notes) &&
				base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			var inbondHeader = Header;
			if (contactType == ContactType.ShippingLine && GetMovementForShippingLine() is CusInBondMoveHeader movement)
			{
				return new OrgHeaderContact(movement.InBondCarrierOrg, movement.InBondCarrier);
			}
			else if (contactType == ContactType.Consignee)
			{
				var importer = inbondHeader.Importer;
				return new OrgHeaderContact(importer.Header, inbondHeader.Supplier, importer);
			}
			else if (contactType == ContactType.Consignor)
			{
				return new OrgHeaderContact(inbondHeader.Supplier, inbondHeader.ImporterOrg, null);
			}
			else
			{
				return base.GetContactOrganisation(menuName, contactType, direction);
			}
		}

		protected virtual CusInBondMoveHeader GetMovementForShippingLine()
		{
			return Header?.MovementHeader;
		}
	}
}
