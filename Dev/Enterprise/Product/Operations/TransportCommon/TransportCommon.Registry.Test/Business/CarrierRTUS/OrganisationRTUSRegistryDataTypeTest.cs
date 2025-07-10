using System;
using System.Text;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(OrganisationRTUSRegistryDataType))]
	class OrganisationRTUSRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OrganisationRTUSRegistryDataType>
	{
		#region Implementation

		protected override OrganisationRTUSRegistryDataType GetNewDataType()
		{
			return new OrganisationRTUSRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "OrganisationRTUSEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var organisationRTUSCollection = new OrganisationRTUSCollection();
			SetupOption(organisationRTUSCollection.AddNew(), "BEEBF4DF-FB6A-4696-A457-80C6E465B691", CBAList.Codes.SaaS, "https://wtg.saas.app.com/");
			SetupOption(organisationRTUSCollection.AddNew(), "BEEBF4DF-FB6A-4696-A457-80C6E465B692", CBAList.Codes.SmartFreight, "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE");
			SetupOption(organisationRTUSCollection.AddNew(), "BEEBF4DF-FB6A-4696-A457-80C6E465B693", CBAList.Codes.Teknowlogi, "https://test");

			var organisationRTUSCollection2 = new OrganisationRTUSCollection();
			SetupOption(organisationRTUSCollection2.AddNew(), "BEEBF4DF-FB6A-4696-A457-80C6E465B694", CBAList.Codes.TMS3G, "https://testwtg1.p1.app.testing.com/DontOverride");
			SetupOption(organisationRTUSCollection2.AddNew(), "BEEBF4DF-FB6A-4696-A457-80C6E465B695", CBAList.Codes.Transtream, "https://wtg.transtream.app.com/");
			SetupOption(organisationRTUSCollection2.AddNew(), "BEEBF4DF-FB6A-4696-A457-80C6E465B696", CBAList.Codes.Trinium, "https://twtg1.p1.app.testing.com/DontOverride4");

			#region Setup XML and Byte Arrays
			var rawXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ArrayOfOrganisationRTUSOption xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<OrganisationRTUSOption>
<OrganisationPK>BEEBF4DF-FB6A-4696-A457-80C6E465B691</OrganisationPK>
<CBACode>SAA</CBACode>
<Url>https://wtg.saas.app.com/</Url>
</OrganisationRTUSOption>
<OrganisationRTUSOption>
<OrganisationPK>BEEBF4DF-FB6A-4696-A457-80C6E465B692</OrganisationPK>
<CBACode>SMA</CBACode>
<Url>https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE</Url>
</OrganisationRTUSOption>
<OrganisationRTUSOption>
<OrganisationPK>BEEBF4DF-FB6A-4696-A457-80C6E465B693</OrganisationPK>
<CBACode>TEK</CBACode>
<Url>https://test</Url>
</OrganisationRTUSOption>
</ArrayOfOrganisationRTUSOption>";

			var rawXml2 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ArrayOfOrganisationRTUSOption xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<OrganisationRTUSOption>
<OrganisationPK>BEEBF4DF-FB6A-4696-A457-80C6E465B694</OrganisationPK>
<CBACode>3GT</CBACode>
<Url>https://testwtg1.p1.app.testing.com/DontOverride</Url>
</OrganisationRTUSOption>
<OrganisationRTUSOption>
<OrganisationPK>BEEBF4DF-FB6A-4696-A457-80C6E465B695</OrganisationPK>
<CBACode>TRA</CBACode>
<Url>https://wtg.transtream.app.com/</Url>
</OrganisationRTUSOption>
<OrganisationRTUSOption>
<OrganisationPK>BEEBF4DF-FB6A-4696-A457-80C6E465B696</OrganisationPK>
<CBACode>TRI</CBACode>
<Url>https://twtg1.p1.app.testing.com/DontOverride4</Url>
</OrganisationRTUSOption>
</ArrayOfOrganisationRTUSOption>";

			byte[] byteArrayValue = Encoding.UTF8.GetBytes(rawXml.Replace(System.Environment.NewLine, ""));
			byte[] byteArrayValue2 = Encoding.UTF8.GetBytes(rawXml2.Replace(System.Environment.NewLine, ""));

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(organisationRTUSCollection, byteArrayValue),
				new ValidSampleAndBinaryValueInDB(organisationRTUSCollection2, byteArrayValue2)
			};
		}

		void SetupOption(OrganisationRTUSOption option, ZString organisationPK, ZString cbaCode, ZString url)
		{
			ZGuid pk = new ZGuid(new Guid(organisationPK));
			option.OrganisationPK = pk;
			option.CBACode = cbaCode;
			option.Url = url;
		}

		#endregion
	}
}
