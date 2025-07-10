using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class DetentionAdviceDelivery : AutoDetentionAdviceDelivery
	{
		public DetentionAdviceDelivery() { }

		public DetentionAdviceDelivery(BusinessObjectFactory factory)
			: base(factory) { }

		[List("Lookups.Modes")]
		public override ZString Mode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Mode; }
			set
			{
				base.Mode = value;
				PrinterInfo.RefreshBinding();
				NotificationGroupInfo.RefreshBinding();
				SendToNotificationGroupInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidatePrinter();
					ValidateNotificationGroup();
				}
			}
		}
		public override void ValidateMode()
		{
			base.ValidateMode();
			MandatoryValidation.CheckEntered(ModeInfo);
			ListValidation.ErrorIfInvalidCode(ModeInfo, Lookups.Modes);
		}

		[List("Lookups.Printers")]
		public override ZGuid Printer
		{
			get { return Printer_ReadOnly ? ZGuid.Empty : base.Printer; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Printer = value; }
		}
		protected override bool Printer_ReadOnly
		{
			get { return Mode == DetentionAdviceDeliveryMode.Codes.Notify; }
		}
		public override void ValidatePrinter()
		{
			base.ValidatePrinter();
			if (Mode != DetentionAdviceDeliveryMode.Codes.Notify)
			{
				MandatoryValidation.CheckEntered(PrinterInfo);
				ListValidation.ErrorIfInvalidPK(PrinterInfo, Lookups.Printers);
			}
		}

		[List("Lookups.Groups")]
		public override ZGuid NotificationGroup
		{
			get { return NotificationGroup_ReadOnly ? ZGuid.Empty : base.NotificationGroup; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.NotificationGroup = value; }
		}
		protected override bool NotificationGroup_ReadOnly
		{
			get { return Mode != DetentionAdviceDeliveryMode.Codes.Notify && !SendToNotificationGroup; }
		}
		public override void ValidateNotificationGroup()
		{
			base.ValidateNotificationGroup();
			if (Mode == DetentionAdviceDeliveryMode.Codes.Notify || SendToNotificationGroup)
			{
				MandatoryValidation.CheckEntered(NotificationGroupInfo);
				ListValidation.ErrorIfInvalidPK(NotificationGroupInfo, Lookups.Groups);
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool SendToNotificationGroup
		{
			get { return SendToNotificationGroup_ReadOnly || base.SendToNotificationGroup; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.SendToNotificationGroup = value; }
		}
		protected override bool SendToNotificationGroup_ReadOnly
		{
			get { return Mode == DetentionAdviceDeliveryMode.Codes.Notify; }
		}

		public new BusinessObjectFactory CurrentFactory
		{
			get { return base.CurrentFactory; }
		}

		public DetentionAdviceDeliveryLookups Lookups
		{
			get { return lookups ?? (lookups = new DetentionAdviceDeliveryLookups(this)); }
		}
		DetentionAdviceDeliveryLookups lookups;

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			NotificationGroup = Constants.Groups.AllPK;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DetentionAdviceDelivery(factory);
		}
	}
}


