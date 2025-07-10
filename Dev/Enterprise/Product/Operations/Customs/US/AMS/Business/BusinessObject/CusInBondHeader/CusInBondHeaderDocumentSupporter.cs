using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondHeaderDocumentSupporter : DocumentSupporter
	{
		public CusInBondHeaderDocumentSupporter(CusInBondHeader header)
			: base(header)
		{
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			var result = "";

			switch (filterName)
			{
				case DocumentFilters.MOD:
					result = Header.BH_ImportTransportMode;
					break;
				case DocumentFilters.MSC:
					result = "OTH";
					break;
				case DocumentFilters.CTY:
					result = Header.Company.GC_RN_NKCountryCode;
					break;
				case DocumentFilters.BKRCTY:
					result = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Header.Company.GC_RN_NKCountryCode);
					break;
				default:
					result = base.GetFilterValue(filterName);
					break;
			}

			return result;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ConsolAMSReportingCustomiseDocuments; }
		}

		public override string TransportMode
		{
			get { return Header.BH_ImportTransportMode; }
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CBPForm1302DataContextValue));
			return result;
		}

		public const string CBPForm1302TemplateName = "CBPForm1302";
		public const string CBPForm1302DataContextValue = ".CBPForm1302";

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.ToString() == CBPForm1302DataContextValue)
			{
				return GetBODocDataFor1302Print();
			}
			else
			{
				return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		IBODocDataProvider[] GetBODocDataFor1302Print()
		{
			var result = new List<IBODocDataProvider>();
			Header.DocumentLines.Initialise1302();
			result.Add(BODocDataProvider.Get(Header));

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
			var result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

			if (result == null)
			{
				switch (dataContext)
				{
					case DataContext.CusInBondHeader:
					case DataContext.Notes:
						var wrapper = DocumentWrapperFactory.CreateWrapper(DataContext.CusInBondHeader, Header);
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
			if (dataContextValue.DataContext == DataContext.CusInBondHeader ||
				dataContextValue.DataContext == DataContext.Notes)
			{
				return Res.GetString("186EE53A-9705-4539-B7B7-2482F9EBF7B3", "AMS Header cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return (dataContext == DataContext.CusInBondHeader || dataContext == DataContext.Notes) &&
				base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}
	}
}

