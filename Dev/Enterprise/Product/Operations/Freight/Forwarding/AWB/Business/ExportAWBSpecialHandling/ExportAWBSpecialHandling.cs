using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[DependentBusinessObject(typeof(ExportAWBHeader), "AWBSpecialHandlingItems")]
	public class ExportAWBSpecialHandling : AutoExportAWBSpecialHandling, IAWBSpecialHandlingMessageDetailsProvider
	{
		public ExportAWBSpecialHandling(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ExportAWBHeader Master
		{
			get { return Factory.Load<ExportAWBHeader>(EP_EH); }
		}

		[List("Lookups.SpecialHandlingCodeDescriptionList")]
		public override ZString EP_SpecialHandling
		{
			get { return base.EP_SpecialHandling; }
			set
			{
				if (EP_SpecialHandling != value)
				{
					base.EP_SpecialHandling = value;

					var master = Master;

					if (master != null)
					{
						master.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZString SpecialHandlingDescription
		{
			get
			{
				var list = Lookups.SpecialHandlingCodeDescriptionList;
				return list.ContainsCode(EP_SpecialHandling) ? (ZString)list[EP_SpecialHandling, System.StringComparison.OrdinalIgnoreCase].Description : ZString.Empty;
			}
		}

		public bool IsSecurityStatus
		{
			get
			{
				return (EP_SpecialHandling == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft
				|| EP_SpecialHandling == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft
				|| EP_SpecialHandling == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly
				|| EP_SpecialHandling == AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements);
			}
		}

		#region IAWBSpecialHandlingMessageDetailsProvider Members

		public ZString SpecialHandling => EP_SpecialHandling;

		#endregion
	}
}
