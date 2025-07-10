using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.Registry
{
	[XmlSerializerAssembly("Enterprise.Recruitment.Registry.XmlSerializers")]
	public class WorkItemTemplateProperties : RegistryBusinessObjectTemplate
	{
		abstract class Schema
		{
			public const string FriendlyName = "FriendlyName";
			public const string WKI_PK = "WKI_PK";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new WorkItemTemplateProperties();

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone1)
		{
			base.CopyValuesToClone(clone1);

			var clone = (WorkItemTemplateProperties)clone1;
			clone.FriendlyName = FriendlyName;
			clone.WKI_PK = WKI_PK;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateFriendlyName();
			ValidateWKI_PK();
		}

		#region Properties

		public BusinessObject BoundWorkItem
		{
			get
			{
				if (boundWorkItem?.PK != WKI_PK)
				{
					boundWorkItem = CurrentFactory.Load(WorkItemSchema.Constants.Prefix, WKI_PK);
				}
				return boundWorkItem;
			}
		}
		BusinessObject boundWorkItem;

		#region Friendly Name

		[MaxLength(256)]
		public ZString FriendlyName
		{
			get => friendlyName;
			set
			{
				if (value != friendlyName)
				{
					//this doesnt work for some reason, capped at 3
					CheckMaximumLength(FriendlyNameInfo, value);
					_ = SetNonPersistentPropertyValue(FriendlyNameInfo, ref friendlyName, value);
					if (!IsValidationSuspended)
					{
						ValidateFriendlyName();
					}
				}
			}
		}
		ZString friendlyName;

		public ZPropertyInfo FriendlyNameInfo => GetZPropertyInfo(Schema.FriendlyName);

		#endregion

		public ZString Type => (ZString)(BoundWorkItem?[WorkItemSchema.WKI_WorkItemType] ?? ZString.Empty);

		public ZString Area => (ZString)(BoundWorkItem?[WorkItemSchema.WKI_WorkItemArea] ?? ZString.Empty);

		public ZString ActivityType => (ZString)(BoundWorkItem?[WorkItemSchema.WKI_ActivityType] ?? ZString.Empty);

		public ZString ActivitySubType => (ZString)(BoundWorkItem?[WorkItemSchema.WKI_ActivitySubtype] ?? ZString.Empty);

		public ZString Priority => (ZString)(BoundWorkItem?[WorkItemSchema.WKI_Priority] ?? ZString.Empty);

		#region WKI_PK

		[List("WorkflowTemplateTypes")]
		public ZGuid WKI_PK
		{
			get => wki_PK;
			set
			{
				_ = SetNonPersistentPropertyValue(WKI_PKInfo, ref wki_PK, value);
				if (!IsValidationSuspended)
				{
					ValidateWKI_PK();
				}
			}
		}
		ZGuid wki_PK;

		public ZPropertyInfo WKI_PKInfo => GetZPropertyInfo(Schema.WKI_PK);

		public IActiveBusinessObjectCollection WorkflowTemplateTypes
		{
			get
			{
				if (workflowTemplateTypes == null)
				{
					var workItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(WorkItemSchema.Constants.Prefix);
					var createCollection = GetType().GetMethod(nameof(CreateCollection), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(workItemType);

					workflowTemplateTypes = (IActiveBusinessObjectCollection)createCollection.Invoke(null, [CurrentFactory]);
				}
				return workflowTemplateTypes;
			}
		}
		IActiveBusinessObjectCollection workflowTemplateTypes;

		static ActiveBusinessObjectCollection<T> CreateCollection<T>(BusinessObjectFactory factory) where T : BusinessObject
			=> new ActiveBusinessObjectCollection<T>(factory);

		#endregion

		#endregion

		#region Validation

		public void ValidateFriendlyName()
		{
			FriendlyNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FriendlyNameInfo);
		}

		public void ValidateWKI_PK()
		{
			WKI_PKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(WKI_PKInfo);
			MandatoryValidation.CheckEntered(WKI_PKInfo);
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FriendlyName = reader.ReadElementString(Schema.FriendlyName);

			try
			{
				WKI_PK = new ZGuid(reader.ReadElementString(WorkItemSchema.Constants.PK));
			}
			catch (FormatException)
			{
				WKI_PK = ZGuid.Empty;
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.FriendlyName, FriendlyName.ToString());
			writer.WriteElementString(Schema.WKI_PK, WKI_PK.ToString());
		}

		#endregion
	}
}
