using System;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public abstract class RerouteProcessorTest<TQRBlock, TQXBlock, T> : ABIProcessorTest<T, APLA, APLB, APLY>
			where TQRBlock : MessageBlock, IStatementReroute, new()
			where TQXBlock : MessageBlock, IStatementRerouteResponse, new()
			where T : RerouteProcessor<TQRBlock, TQXBlock>
	{
		protected override void EndToEndCore()
		{
			var image1 = new Bitmap(1, 2);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = applicationIdentifier;

			generator.AddMessageBlock(CreateQRBlock());
			generator.AddMessageBlock(CreateQXBlock(transmitDateBeyond14Days, "Date no good", 2));
			generator.AddMessageBlock(CreateQXBlock("", "Good job mate.", 6));
			generator.AddMessageBlock(CreateQXBlock(noReroutesRequested, "where is my request?", 3));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ProcessMessage(generator);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == rerouteSubject; }));
			Assert(email.Body.Contains(expectedBody));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		protected abstract string applicationIdentifier { get; }
		protected abstract string transmitDateBeyond14Days { get; }
		protected abstract string noReroutesRequested { get; }
		protected abstract string rerouteSubject { get; }
		protected abstract string expectedBody { get; }
		protected abstract GroupNotificationRegistryItem<GroupNotification> StatementRerouteGroup { get; }
		protected abstract bool IsPeriodicMonthly { get; }
		protected abstract TQRBlock CreateQRBlock();

		protected override void SetUp()
		{
			base.SetUp();
			StatementRerouteGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new GroupNotification(GroupNotification.StaffMemberOrNominatedGroup, groupZZ1.PK));
		}

		TQXBlock CreateQXBlock(ZString code, ZString message, int total)
		{
			var qx = new TQXBlock();
			qx.Deserialise("QX" + code.Left(3).PadRight(3) + message.Left(34).PadRight(34) + total.ToString().PadRight(41));
			return qx;
		}
	}
}
