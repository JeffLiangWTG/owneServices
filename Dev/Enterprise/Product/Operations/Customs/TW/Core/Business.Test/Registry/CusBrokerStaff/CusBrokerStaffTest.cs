using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusBrokerStaff))]
	sealed class CusBrokerStaffTest : RegistryBusinessObjectTemplateTestCase<CusBrokerStaff>
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestBrokerStaffCode()
		{
			NUnit.Framework.Assert.That(currentElement.BrokerStaffCode, NUnit.Framework.Is.EqualTo("CYO").Using(CustomComparers.TypeComparison));
			var brokerStaff = currentElement.BrokerStaff;
			NUnit.Framework.Assert.That(brokerStaff, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.MasterFiles.Business.GlbStaff)));
			NUnit.Framework.Assert.That(brokerStaff, NUnit.Framework.Is.TypeOf<GlbStaff>());
			var typeToCheck = currentElement.GetType();
			NUnit.Framework.Assert.That(typeToCheck, CustomConstraints.HasCustomAttribute<ListAttribute>(CusBrokerStaff.Schema.BrokerStaffCode, false, attrib => attrib.ListDataSourceMember == "Lookups.BrokerStaffList"));
			NUnit.Framework.Assert.That(typeToCheck, CustomConstraints.HasCustomAttribute<RelatedBusinessObjectAttribute>(CusBrokerStaff.Schema.BrokerStaffCode, false, attrib => attrib.RelatedBizObjName == "BrokerStaff"));
			NUnit.Framework.Assert.That(currentElement.BrokerStaffCodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(3));
			currentElement.BrokerStaffCode = ZString.Empty;
			NUnit.Framework.Assert.That(currentElement.BrokerStaff, NUnit.Framework.Is.EqualTo(default(Enterprise.MasterFiles.Business.GlbStaff)));
		}

		[ExpectNoExceptions]
		public void TestMailbox()
		{
			NUnit.Framework.Assert.That(currentElement.Mailbox, NUnit.Framework.Is.EqualTo("TBK0461-0").Using(CustomComparers.TypeComparison));
			var typeToCheck = currentElement.GetType();
			NUnit.Framework.Assert.That(typeToCheck, CustomConstraints.HasCustomAttribute<ListAttribute>(CusBrokerStaff.Schema.Mailbox, false, attrib => attrib.ListDataSourceMember == "Lookups.MailboxList"));
			NUnit.Framework.Assert.That(typeToCheck, CustomConstraints.HasCustomAttribute<ReadOnlyMemberAttribute>(CusBrokerStaff.Schema.Mailbox, false, attrib => attrib.Member == "MailboxReadOnly"));
			NUnit.Framework.Assert.That(currentElement.MailboxInfo.MaxLength, NUnit.Framework.Is.EqualTo(16));
			currentElement.BrokerStaffCode = "JLU";
			NUnit.Framework.Assert.That(currentElement.Mailbox, NUnit.Framework.Is.EqualTo(ZString.Empty));
			currentElement.Mailbox = "TBK0461-0";
			currentElement.BrokerStaffCode = ZString.Empty;
			NUnit.Framework.Assert.That(currentElement.Mailbox, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestMailboxReadOnly()
		{
			NUnit.Framework.Assert.That(currentElement.MailboxReadOnly, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			currentElement.BrokerStaffCode = ZString.Empty;
			NUnit.Framework.Assert.That(currentElement.MailboxReadOnly, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestValidationType()
		{
			var validation = currentElement.Validation;
			NUnit.Framework.Assert.That(validation, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusBrokerStaffValidation)));
			NUnit.Framework.Assert.That(validation, NUnit.Framework.Is.TypeOf<CusBrokerStaffValidation>());
		}

		[ExpectNoExceptions]
		public void TestLookupsType()
		{
			var lookups = currentElement.Lookups;
			NUnit.Framework.Assert.That(lookups, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusBrokerStaffLookups)));
			NUnit.Framework.Assert.That(lookups, NUnit.Framework.Is.TypeOf<CusBrokerStaffLookups>());
		}

		#endregion
		#region override
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override BusinessObject GetNewBusinessObject() => new CusBrokerStaff(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		protected override CusBrokerStaff GetBusinessObjectToClone() => (CusBrokerStaff)GetNewBusinessObject();
		protected override CusBrokerStaff GetBusinessObjectToSerialise() => (CusBrokerStaff)GetNewBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateBrokerStaff();
			currentElement = (CusBrokerStaff)GetNewBusinessObject();
			currentElement.BrokerStaffCode = "CYO";
			currentElement.Mailbox = "TBK0461-0";
		}

		CusBrokerStaff currentElement;
		#endregion
	}
}
