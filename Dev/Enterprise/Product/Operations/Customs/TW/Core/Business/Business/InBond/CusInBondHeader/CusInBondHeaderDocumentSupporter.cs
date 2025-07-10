using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondHeaderDocumentSupporter : DocumentSupporter
	{
		public CusInBondHeaderDocumentSupporter(CusInBondHeader header)
			: base(header)
		{
		}

		public const string N5301Declaration = ".N5301Declaration";

		public override string TransportMode
		{
			get { return Header.BH_ImportTransportMode; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.TWTranshipmentCustomiseDocuments; }
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(N5301Declaration));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var wrapperList = new List<IBODocDataProvider>();
			switch (dataContextValue.FullDataContext)
			{
				case N5301Declaration:
					{
						wrapperList.Add(new N5301EDIMessageDocumentWrapper(Header));
						break;
					}
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return wrapperList.Count != 0 ? wrapperList.ToArray() : null;
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

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.Notes, DataContext.CusInBondHeader };
		}

		protected CusInBondHeader Header
		{
			get { return (CusInBondHeader)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusInBondHeader; }
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.CusInBondHeader || dataContextValue.DataContext == DataContext.Notes)
			{
				return Res.GetString("38CDA5A2-93FF-48EA-8303-CC40EF85DEF3", "TW In-bond Header cannot be found.");
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
