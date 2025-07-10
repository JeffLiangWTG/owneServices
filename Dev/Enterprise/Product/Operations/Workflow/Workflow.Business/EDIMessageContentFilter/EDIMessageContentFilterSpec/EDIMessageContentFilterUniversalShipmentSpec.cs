using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterUniversalShipmentSpec : EDIMessageContentFilterSpec
	{
		public EDIMessageContentFilterUniversalShipmentSpec(BusinessObjectFactory factory, ZString schema)
			: base(factory, schema)
		{
		}

		[XmlColumnProperty]
		public EDIMessageContentFilterAdditionalConfiguration AdditionalConfiguration
		{
			get
			{
				if (additionalConfiguration == null)
				{
					additionalConfiguration = new EDIMessageContentFilterAdditionalConfiguration(this);
					RegisterEditableChildObject(additionalConfiguration);
				}
				return additionalConfiguration;
			}
		}

		EDIMessageContentFilterAdditionalConfiguration additionalConfiguration;
	}
}
