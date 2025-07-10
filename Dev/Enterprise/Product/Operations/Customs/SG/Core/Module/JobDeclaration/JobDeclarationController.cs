using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Module
{
	public class JobDeclarationController : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result;

			if (IsNewDeclaration(businessEntity))
			{
				result = new BrokeragePlugIn((ForwardingShipment)businessEntity);
			}
			else if (IsTradenet4OrITF(businessEntity))
			{
				result = new BrokeragePlugIn((ForwardingShipment)businessEntity);
			}
			else
			{
				result = new V3.GUI.V3BrokeragePlugin((ForwardingShipment)businessEntity);
			}

			return result;
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new JobDeclarationForm((JobDeclaration)businessEntity);
		}

		bool IsTradenet4OrITF(IBusiness businessEntity)
		{
			bool result = false;

			ForwardingShipment shipment = businessEntity as ForwardingShipment;
			if (shipment != null)
			{
				if (shipment.Declarations.Length > 0)
				{
					JobDeclaration jobDeclaration = Factory.LoadTop1<JobDeclaration>(SGDeclarations(shipment));
					if (jobDeclaration != null)
					{
						result = jobDeclaration.IsTradenet4OrITF;
					}
				}
			}

			return result;
		}

		bool IsNewDeclaration(IBusiness businessEntity)
		{
			bool result = false;

			ForwardingShipment shipment = businessEntity as ForwardingShipment;

			if (shipment != null)
			{
				result = !Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(JobDeclaration)), SGDeclarations(shipment));
			}

			return result;
		}

		ZQuery SGDeclarations(ForwardingShipment shipment)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_JS, shipment.PK);

			ZDBOnlySubQuery branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			ZDBOnlySubQuery countryQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), GlbBranchSchema.GB_RL_NKHomePort);
			countryQuery.AddToFilter(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.Singapore));
			branchQuery.AddSubQuery(GlbBranchSchema.GB_RL_NKHomePort, RefUNLOCOSchema.RL_Code, countryQuery, JoinCondition.And);

			result.AddSubQuery(branchQuery, JoinCondition.And);

			return result;
		}
	}
}
