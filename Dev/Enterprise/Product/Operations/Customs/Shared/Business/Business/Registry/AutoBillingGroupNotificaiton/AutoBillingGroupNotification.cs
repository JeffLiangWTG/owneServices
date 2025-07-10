using System;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class AutoBillingGroupNotification : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string SendGroupPK = "SendGroupPK";
			public const string SuppressUnpostARNotificaiton = "SuppressUnpostARNotificaiton";
		}

		#region Properties

		[List(nameof(SendGroupList))]
		public ZGuid SendGroupPK
		{
			get { return fSendGroupPK; }
			set
			{
				SetNonPersistentPropertyValue(SendGroupPKInfo, ref fSendGroupPK, value);
				if (!IsValidationSuspended)
				{
					ValidateSendGroupPK();
				}
			}
		}
		ZGuid fSendGroupPK;

		public ZPropertyInfo SendGroupPKInfo
		{
			get { return GetZPropertyInfo(Schema.SendGroupPK); }
		}

		public ZBool SuppressUnpostARNotificaiton
		{
			get { return fSuppressUnpostARNotificaiton; }
			set
			{
				SetNonPersistentPropertyValue(SuppressUnpostARNotificaitonInfo, ref fSuppressUnpostARNotificaiton, value);
			}
		}
		ZBool fSuppressUnpostARNotificaiton;

		public ZPropertyInfo SuppressUnpostARNotificaitonInfo
		{
			get { return GetZPropertyInfo(Schema.SuppressUnpostARNotificaiton); }
		}

		#endregion

		#region Lookups

		public IBusinessObjectCollection SendGroupList => fSendGroupList ??= (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IGlbGroupCollection>(), new object[] { CurrentFactory });
		IBusinessObjectCollection fSendGroupList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateSendGroupPK();
		}

		void ValidateSendGroupPK()
		{
			SendGroupPKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(SendGroupPKInfo);
		}

		#endregion

		#region Implementation

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SendGroupPK, SendGroupPK.ToString());
			writer.WriteElementString(Schema.SuppressUnpostARNotificaiton, SuppressUnpostARNotificaiton ? "Y" : "N");
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SendGroupPK = new Guid(reader.ReadElementString(Schema.SendGroupPK));
			SuppressUnpostARNotificaiton = reader.ReadElementString(Schema.SuppressUnpostARNotificaiton) == "Y";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new AutoBillingGroupNotification();
			result.SendGroupPK = SendGroupPK;
			result.SuppressUnpostARNotificaiton = SuppressUnpostARNotificaiton;
			return result;
		}

		#endregion
	}
}
