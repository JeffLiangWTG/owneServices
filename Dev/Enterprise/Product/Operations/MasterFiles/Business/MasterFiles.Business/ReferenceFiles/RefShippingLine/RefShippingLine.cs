using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[CodeProperty(Schema.RSL_CargoWiseOneCode), DescriptionProperty(Schema.RSL_CarrierName)]
	public class RefShippingLine : AutoRefShippingLine
	{
		public RefShippingLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void Delete()
		{
			DeleteMessagingRequirements();
			DeleteEBLProviders();
			base.Delete();
		}

		void DeleteMessagingRequirements()
		{
			ShippingLineMessagingRequirements.DeleteAll();
		}

		void DeleteEBLProviders()
		{
			ShippingLineEBLProviders.DeleteAll();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("358b7901-51a4-4c1b-8008-e0a82733602c", "Shipping Line");

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result;

			if (property.Name == RefShippingLineSchema.RSL_IsSystem.Name || property.Name == RefShippingLineSchema.RSL_OceanCarrierMessagingAvailable.Name || property.Name == RefShippingLineSchema.RSL_GlobalSailingScheduleAvailable.Name ||
				property.Name == RefShippingLineSchema.RSL_ContainerAutomationAvailable.Name || property.Name == RefShippingLineSchema.RSL_CargoSphereRatesAvailable.Name || property.Name == RefShippingLineSchema.RSL_InvoiceAvailable.Name ||
				property.Name == RefShippingLineSchema.RSL_BookingRequestAvailable.Name || property.Name == RefShippingLineSchema.RSL_ShippingInstructionAvailable.Name || property.Name == RefShippingLineSchema.RSL_VerifiedGrossContainerWeightAvailable.Name ||
				property.Name == RefShippingLineSchema.RSL_ShippingOrderAvailable.Name || property.Name == RefShippingLineSchema.RSL_EManifestAvailable.Name)
			{
				result = true;
			}
			else
			{
				result = RSL_IsSystem;
			}

			return result;
		}

		#endregion

		public string ShippingLineIntegration
		{
			get
			{
				var result = string.Empty;
				var fieldsNeedCheck = new[]
				{
					RSL_OceanCarrierMessagingAvailableInfo,
					RSL_BookingRequestAvailableInfo,
					RSL_ShippingInstructionAvailableInfo,
					RSL_VerifiedGrossContainerWeightAvailableInfo,
					RSL_ShippingOrderAvailableInfo,
					RSL_EManifestAvailableInfo,
					RSL_GlobalSailingScheduleAvailableInfo,
					RSL_ContainerAutomationAvailableInfo,
					RSL_InvoiceAvailableInfo
				};

				var availableIntegration = fieldsNeedCheck.Where(o => new ZBool(o.Value)).ToList();
				if (availableIntegration.Any())
				{
					var messageBuilder = new StringBuilder();
					foreach (ZPropertyInfo info in availableIntegration)
					{
						if (info.Name == RefShippingLineSchema.RSL_BookingRequestAvailable.Name ||
							info.Name == RefShippingLineSchema.RSL_ShippingInstructionAvailable.Name ||
							info.Name == RefShippingLineSchema.RSL_VerifiedGrossContainerWeightAvailable.Name ||
							info.Name == RefShippingLineSchema.RSL_ShippingOrderAvailable.Name ||
							info.Name == RefShippingLineSchema.RSL_EManifestAvailable.Name)
						{
							messageBuilder.Append(string.Empty.PadLeft(4));
						}
						messageBuilder.AppendLine(info.HumanReadableName);
					}
					result = messageBuilder.ToString();
				}
				else
				{
					result = Res.GetString("A43D27DF-6D5C-4A29-BB68-6374DAFD4877", "No Integrations Specified");
				}

				return result;
			}
		}

		[ChildEditable(true)]
		public RefShippingLineMessagingRequirementCollection ShippingLineMessagingRequirements
		{
			get
			{
				if (messagingRequirements == null)
				{
					messagingRequirements = new RefShippingLineMessagingRequirementCollection(this);
					RegisterEditableChildObject(messagingRequirements);
				}

				return messagingRequirements;
			}
		}

		RefShippingLineMessagingRequirementCollection messagingRequirements;

		[ChildEditable(true)]
		public RefShippingLineEBLProviderCollection ShippingLineEBLProviders
		{
			get
			{
				if (eBLProviders == null)
				{
					eBLProviders = new RefShippingLineEBLProviderCollection(this);
					RegisterEditableChildObject(eBLProviders);
				}

				return eBLProviders;
			}
		}

		RefShippingLineEBLProviderCollection eBLProviders;

		public ZString AvailableEBLProviders
		{
			get
			{
				var availabEBLProviders = ShippingLineEBLProviders.Where(p => p.RSE_IsAvailable);
				if (availabEBLProviders.Any())
				{
					return string.Join(", ", availabEBLProviders.Select(p => p.RSE_Name));
				}

				return ZString.Empty;
			}
		}
	}
}
