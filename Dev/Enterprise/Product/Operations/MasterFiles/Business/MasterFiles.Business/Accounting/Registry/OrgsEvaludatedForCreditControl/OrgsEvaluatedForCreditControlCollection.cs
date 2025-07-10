using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class OrgsEvaluatedForCreditControlCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new OrgsEvaluatedForCreditControl this[int i]
		{
			get { return (OrgsEvaluatedForCreditControl)Elements[i]; }
		}

		public new OrgsEvaluatedForCreditControl AddNew()
		{
			return (OrgsEvaluatedForCreditControl)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgsEvaluatedForCreditControlCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var control = new OrgsEvaluatedForCreditControl();
			control.CurrentFallbackLevel = this.CurrentFallbackLevel;
			return control;
		}

		public bool Exists(ZString jobType, ZString direction, ZString mode, ZString incoTerm, ZString paymentTerm, ZString organizationType)
		{
			return this.Cast<OrgsEvaluatedForCreditControl>().Any(c =>
				{
					return (c.JobType == jobType || c.JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All) &&
							(c.DirectionCode_ReadOnly || c.DirectionCode == direction || c.DirectionCode == Constants.FreightShipmentDirection.Code.All) &&
							(c.Mode_ReadOnly || c.Mode == mode || c.Mode == JobConfigurationSelectorLookups.ModeAdditionalCodes.All) &&
							(c.INCOTerm_ReadOnly || c.INCOTerm == incoTerm || c.INCOTerm == INCOTermCodes.All) &&
							(c.FreightPaymentTerm_ReadOnly || c.FreightPaymentTerm == paymentTerm || c.FreightPaymentTerm == FreightPaymentTermCodes.All) &&
							(c.OrganizationType_ReadOnly || c.OrganizationType == organizationType || c.OrganizationType == AccountingMasterFilesConstants.OrganisationTypeCodes.All);
				});
		}
	}
}
