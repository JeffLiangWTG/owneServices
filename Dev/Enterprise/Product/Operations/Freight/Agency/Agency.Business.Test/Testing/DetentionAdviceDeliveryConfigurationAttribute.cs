using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public sealed class DetentionAdviceDeliveryConfigurationAttribute : TestSetupAttribute
	{
		public DetentionAdviceDeliveryConfigurationAttribute()
		{
			DeliveryMode = DetentionAdviceDeliveryMode.Codes.Notify;
			PrinterName = "Printer";
			GroupName = "GRP";
			SendToGroup = false;
		}

		public string DeliveryMode { get; set; }

		public string PrinterName { get; set; }

		public string GroupName { get; set; }

		public bool SendToGroup { get; set; }

		public override void SetUp(TestCase testCase)
		{
			ZGuid printerPK = ZGuid.Empty;
			ZGuid groupPK = ZGuid.Empty;
			ZBool sendToGroup = false;
			switch (DeliveryMode)
			{
				case DetentionAdviceDeliveryMode.Codes.Notify:
					groupPK = GetGroup(GroupName);
					break;
				case DetentionAdviceDeliveryMode.Codes.Auto:
				case DetentionAdviceDeliveryMode.Codes.Print:
					printerPK = GetPrinter(PrinterName);
					if (SendToGroup)
					{
						sendToGroup = true;
						groupPK = GetGroup(GroupName);
					}

					break;
			}

			DetentionAdviceDelivery delivery = new DetentionAdviceDelivery();
			delivery.Mode = DeliveryMode;
			delivery.SendToNotificationGroup = sendToGroup;
			delivery.NotificationGroup = groupPK;
			delivery.Printer = printerPK;
			AgencyRegistry.Instance.DetentionAdviceBehaviour.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, delivery);
		}

		public override void TearDown(TestCase testCase)
		{
		}

		static ZGuid GetPrinter(string printerName)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(StmPrintQueueSchema.SQ_DisplayName, printerName);
			StmPrintQueue printer = factory.LoadTop1<StmPrintQueue>(filter);
			if (printer == null)
			{
				printer = factory.New<StmPrintQueue>();
				printer.SQ_DisplayName = printerName;
				printer.SQ_AllowPrinting = true;
				factory.Save();
			}

			return printer.PK;
		}

		static ZGuid GetGroup(string code)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(GlbGroupSchema.GG_Code, code);
			GlbGroup group = factory.LoadTop1<GlbGroup>(filter);
			if (group == null)
			{
				group = factory.New<GlbGroup>();
				group.GG_Code = code;
				GlbStaff staff = group.Staff.AddNew();
				staff.GS_LoginName = "blaticus";
				staff.GS_Code = "BLT";
				staff.GS_FullName = "Blaticus";
				staff.GS_EmailAddress = "blaticus@cargowise.com";
				factory.Save();
			}

			return group.PK;
		}
	}
}
