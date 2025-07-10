using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanSailingDataLookups : ZLookups
	{
		public StowPlanSailingDataLookups(StowPlanSailingData parent)
			: base(parent)
		{ }

		protected new StowPlanSailingData Parent
		{
			get { return (StowPlanSailingData)base.Parent; }
		}

		public ICodeDescriptionPairList USPorts
		{
			get
			{
				if (fUSPorts == null)
				{
					fUSPorts = new CodeDescriptionPairList();
					foreach (var voyagePort in Parent.VoyagePorts)
					{
						if (IsUSOrPR(voyagePort.Port))
						{
							var unLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, voyagePort.Port);
							if (unLOCO != null)
							{
								fUSPorts.Add(unLOCO);
							}
						}
					}
				}
				return fUSPorts;
			}
		}
		CodeDescriptionPairList fUSPorts;

		bool IsUSOrPR(ZString port)
		{
			return port.StartsWith(Core.Constants.CountryCodes.UnitedStates) || port.StartsWith(Core.Constants.CountryCodes.PuertoRico);
		}

		public RefUNLOCOCollection ForeignPorts
		{
			get
			{
				if (fForeignPorts == null)
				{
					fForeignPorts = new RefUNLOCOCollection(Factory);
					var query = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, Core.Constants.CountryCodes.UnitedStates);
					query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, Core.Constants.CountryCodes.PuertoRico);
					fForeignPorts.AdditionalFilter = query;
				}
				return fForeignPorts;
			}
		}
		RefUNLOCOCollection fForeignPorts;

		public STWIssueFilterList IssueTypes
		{
			get { return Factory.GetCachedValue<STWIssueFilterList>(); }
		}
	}
}
