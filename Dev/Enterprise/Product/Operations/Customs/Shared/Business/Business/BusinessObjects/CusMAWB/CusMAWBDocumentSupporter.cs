using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.Business
{
	public class CusMAWBDocumentSupporter : DocumentSupporter
	{
		public CusMAWBDocumentSupporter(CusMAWB parent)
			: base(parent)
		{
			mAWB = parent;
		}
		readonly CusMAWB mAWB;

		const string CusHAWBDataContext = ".CusHAWB";

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusMAWB; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { Core.Constants.DataContext.GenericFreightJob };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, mAWB);
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CusHAWBDataContext));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.Equals(new DataContextValue(CusHAWBDataContext)))
			{
				List<IBODocDataProvider> result = new List<IBODocDataProvider>();
				foreach (CusHAWB hawb in mAWB.ChildBills)
				{
					result.Add(BODocDataProvider.Get(hawb));
				}
				return result.ToArray();
			}
			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == Core.Constants.DataContext.GenericFreightJob)
			{
				return Res.GetString("89B69A31-67E8-4154-A202-AA31E76D8938", "This shipment is not associated with a MAWB data.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext == Core.Constants.DataContext.GenericFreightJob &&
				base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}
	}
}
