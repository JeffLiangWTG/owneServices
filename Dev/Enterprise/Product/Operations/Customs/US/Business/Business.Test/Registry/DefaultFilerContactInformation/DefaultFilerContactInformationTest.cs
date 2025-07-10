using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(DefaultFilerContactInformation))]
	sealed class DefaultFilerContactInformationTest : RegistryBusinessObjectTemplateTestCase<DefaultFilerContactInformation>
	{
		public void TestSetOverlengthValue()
		{
			BusinessObject staff = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbStaff)));
			staff[GlbStaffSchema.Constants.GS_WorkPhone] = "+886602134391422";
			staff[GlbStaffSchema.Constants.GS_LoginName] = "Test4User";
			staff[GlbStaffSchema.Constants.GS_FullName] = "Crazy";
			Factory.Save();
			using (Env.SetTemporaryUserContext(((IUser)staff).LoginName, Guid.Empty, Guid.Empty))
			{
				AssertNoExceptionThrown("Should not be throwing the MaxLength Exception in SetCustomDefaultValuesCore().", () => { new DefaultFilerContactInformation(); });
				AssertNoExceptionThrown("Should not throw any exception in CopyValuesToClone()", () => new DefaultFilerContactInformation().Clone(null, Factory));
			}
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DefaultFilerContactInformation GetBusinessObjectToClone()
		{
			DefaultFilerContactInformation result = new DefaultFilerContactInformation();
			result.ContactName = Env.CurrentUser.FullName;
			result.ContactPhone = Env.CurrentUser.WorkPhone;
			return result;
		}

		protected override DefaultFilerContactInformation GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
		#endregion
	}
}
