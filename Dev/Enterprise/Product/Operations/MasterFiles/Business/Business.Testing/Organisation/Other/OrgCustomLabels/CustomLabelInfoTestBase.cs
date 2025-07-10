using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomLabelInfoTestBase : TestCaseWithFactory
	{
		public void TestCreateCustomLabel()
		{
			CustomLabelInfoBaseTest info = new CustomLabelInfoBaseTest("prop", typeof(ZString), "defaultcaption", "defaulthint", CustomLabelStyles.None, GlbCompany.CurrentCompany.OrgProxy, Factory);
			AssertEquals("prop", info.PropertyName);
			AssertEquals("defaultcaption", info.Caption);
			AssertEquals("defaulthint", info.Hint);
			AssertEquals(CustomLabelStyles.None, info.Styles);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy, info.Org);
		}

		public class CustomLabelInfoBaseTest : CustomLabelInfoBase
		{
			public CustomLabelInfoBaseTest(
				string propertyName, Type propertyType, string defaultCaption, string defaultHint, CustomLabelStyles styles, OrgHeader org, BusinessObjectFactory factory)
				: base(propertyName, propertyType, (NoResString)defaultCaption, (NoResString)defaultHint, styles, org, factory)
			{
			}

			public override string Hint
			{
				get { return DefaultHint; }
			}

			public override MultilingualString Caption
			{
				get { return DefaultCaption; }
			}

			public OrgHeader Org
			{
				get { return base.Organisation; }
			}

			public override bool IsEnabled
			{
				get { return false; }
			}

			public override bool IsMandatory
			{
				get { return false; }
			}

			public override int Position
			{
				get { return 0; }
			}
		}
	}
}
