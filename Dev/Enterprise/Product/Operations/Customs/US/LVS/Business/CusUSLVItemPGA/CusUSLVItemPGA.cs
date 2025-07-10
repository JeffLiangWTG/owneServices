using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	[DependentBusinessObject(typeof(CusUSLVConsignment), "CusUSLVItemPGA")]
	public class CusUSLVItemPGA : AutoCusUSLVItemPGA
	{
		public CusUSLVItemPGA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override

		public CusUSLVItem ParentItem
		{
			get { return Factory.Load<CusUSLVItem>(ULP_ULI); }
		}

		[RelatedBusinessObject("ParentItem")]
		public override ZGuid ULP_ULI
		{
			get { return base.ULP_ULI; }
			set
			{
				base.ULP_ULI = value;

				if (ParentItem != null)
				{
					ULP_ClusterKey = ParentItem.ULI_ClusterKey;
				}
			}
		}

		public string AgencyCode { get; set; }

		[MaxLength(1)]
		[List(nameof(PGADisclaimReasonList))]
		public override ZString ULP_DisclaimReason { get => base.ULP_DisclaimReason; set => base.ULP_DisclaimReason = value; }

		public CodeDescriptionPairList PGADisclaimReasonList => ParentItem.RequirementsProvider.GetDisclaimReasonList(AgencyCode);

		#endregion
	}
}
