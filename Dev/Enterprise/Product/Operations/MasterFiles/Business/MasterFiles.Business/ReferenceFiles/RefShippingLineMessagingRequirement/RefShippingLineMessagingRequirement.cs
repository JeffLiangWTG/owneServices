using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineMessagingRequirement : AutoRefShippingLineMessagingRequirement
	{
		public RefShippingLineMessagingRequirement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("RequirementType")]
		[List("Lookups.RefShippingLineMessagingRequirementTypesList")]
		public override ZString RSR_RST_NKType
		{
			get
			{
				return base.RSR_RST_NKType;
			}
			set
			{
				base.RSR_RST_NKType = value;
			}
		}

		public RefShippingLineMessagingRequirementType RequirementType => Factory.LoadFromNaturalKey<RefShippingLineMessagingRequirementType>(RefShippingLineMessagingRequirementTypeSchema.RST_Code, RSR_RST_NKType);
	}
}
