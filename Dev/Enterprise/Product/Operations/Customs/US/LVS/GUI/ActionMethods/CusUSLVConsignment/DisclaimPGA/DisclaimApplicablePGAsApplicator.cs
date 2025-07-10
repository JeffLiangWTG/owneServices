using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.LVS.GUI
{
	public class DisclaimApplicablePGAsApplicator : OperationalActionMethodApplicator
	{
		public DisclaimApplicablePGAsApplicator(BusinessObjectFactory factory)
			: base("DisclaimApplicablePGAsApplicator", factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			DisclaimApplicablePGAsHelper.ApplyDisclaimReasonsToConsignments(log, DisclaimOptions, targets);
		}

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			base.BuildCore(selectItemPKs);
			LoadSelectedConsignments(selectItemPKs);
		}

		void LoadSelectedConsignments(ZGuid[] selectItemPKs)
		{
			selectItemPKs.ForEach(pk =>
			{
				var consignment = Factory.Load<CusUSLVConsignment>(pk);
				if (consignment != null)
				{
					SelectedConsignments.Add(consignment);
				}
			});
		}

		public void PopulateConsignments(CusUSLVConsignment[] consignments)
		{
			SelectedConsignments.AddRange(consignments);
			disclaimOptions = null;
		}

		public CusUSLVItemPGADisclaimOptionCollection DisclaimOptions
		{
			get
			{
				if (disclaimOptions == null)
				{
					disclaimOptions = new CusUSLVItemPGADisclaimOptionCollection(Factory);
					disclaimOptions.Populate(SelectedConsignments);
				}
				return disclaimOptions;
			}
		}

		CusUSLVItemPGADisclaimOptionCollection disclaimOptions;

		List<CusUSLVConsignment> SelectedConsignments => selectedConsignments ?? (selectedConsignments = new List<CusUSLVConsignment>());

		List<CusUSLVConsignment> selectedConsignments;
	}
}
