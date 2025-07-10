using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class AutoSendStatementDateChangeRequest : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string OverrideAllOrByOrganisation = "OverrideAllOrByOrganisation";
			public const int OverrideAllOrByOrganisation_MaxLength = 3;
		}
		#endregion

		#region Bound Properties

		#region OverrideAllOrByOrganisation

		[List(nameof(OverrideAllOrByOrganisationList))]
		[MaxLength(Schema.OverrideAllOrByOrganisation_MaxLength)]
		public ZString OverrideAllOrByOrganisation
		{
			get { return overrideAllOrByOrganisation; }
			set
			{
				SetNonPersistentPropertyValue(OverrideAllOrByOrganisationInfo, ref overrideAllOrByOrganisation, value);
				CheckMaximumLength(OverrideAllOrByOrganisationInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOverrideAllOrByOrganisation();
				}
			}
		}
		ZString overrideAllOrByOrganisation;

		public ZPropertyInfo OverrideAllOrByOrganisationInfo
		{
			get { return GetZPropertyInfo(Schema.OverrideAllOrByOrganisation); }
		}

		public void ValidateOverrideAllOrByOrganisation()
		{
			OverrideAllOrByOrganisationInfo.ClearAllNotifications();
			if (OverrideAllOrByOrganisation != OverrideAllOrByOrganisationList.Codes.ALL && OverrideAllOrByOrganisation != OverrideAllOrByOrganisationList.Codes.ORG)
			{
				OverrideAllOrByOrganisationInfo.AddError(OverrideValues);
			}
		}
		internal const string OverrideValues = "If overriding the default behaviour, you must select to override for ALL organizations or based on an individual organization indicator.";

		#region Lookups

		public CodeDescriptionPairList OverrideOrganisationList
		{
			get { return new OverrideAllOrByOrganisationList(); }
		}

		#endregion

		#endregion

		#endregion

		#region Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			OverrideAllOrByOrganisation = ZString.Empty;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoSendStatementDateChangeRequest();
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			base.RunPreSaveValidationCore();
			ValidateOverrideAllOrByOrganisation();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OverrideAllOrByOrganisation, OverrideAllOrByOrganisation);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OverrideAllOrByOrganisation = reader.ReadElementString(Schema.OverrideAllOrByOrganisation);
		}
		#endregion
	}
}
