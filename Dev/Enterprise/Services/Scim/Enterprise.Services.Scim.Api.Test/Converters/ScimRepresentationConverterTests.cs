using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enterprise.Services.Scim.Business;
using Enterprise.Services.Scim.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Persistence.InMemory;

namespace Enterprise.Services.Scim.Tests.Converters
{
	[TestFixture]
	public class ScimRepresentationConverterTests
	{
		DefaultSchemaQueryRepository schemaQueryRepository;
		SCIMRepresentationHelper scimRepresentationHelper;

		[Test]
		public void TestConvertWithGroups()
		{
			var scimHelper = new ScimToScimRepresentation(schemaQueryRepository, scimRepresentationHelper);
			var pk1 = Guid.NewGuid().ToString();
			var pk2 = Guid.NewGuid().ToString();
			var scimUser = new ScimUser
			{
				UserName = "t@g.com",
				Active = true,
				Id = Guid.NewGuid(),
				ExternalId = "extStr",
				NameFormatted = "Kevin Grace",
				FamilyName = "Grace",
				GivenName = "Kevin",
				MiddleName = "Kevin",
				Email = "kevin@wtg.com",
				Groups = new[]
				{
					new GroupMemberWithDisplay()
					{
						Display = "display 1",
						Value = pk1,
						Ref = "../Groups/" + pk1,
						Type = SCIMResourceTypes.Group
					},
					new GroupMemberWithDisplay()
					{
						Display = "display 2",
						Value = pk2,
						Ref = "../Groups/" + pk2,
						Type = SCIMResourceTypes.Group
					}
				}
			};

			var scimRepresentation = scimHelper.ScimToSCIMRepresentation(scimUser, true).Result;
			var groups = scimRepresentation.GetAttributesByPath(AttributeNames.Groups);
			Assert.That(groups.Count(), Is.EqualTo(2));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.Groups + "." + AttributeNames.Display).Count(), Is.EqualTo(2));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.Groups + "." + AttributeNames.Value).Count(), Is.EqualTo(2));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.Groups + "." + AttributeNames.Ref).Count(), Is.EqualTo(2));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.Groups + "." + AttributeNames.Type).Count(), Is.EqualTo(2));

			scimRepresentation = scimHelper.ScimToSCIMRepresentation(scimUser, false).Result;
			groups = scimRepresentation.GetAttributesByPath(AttributeNames.Groups).ToArray();
			Assert.That(groups.Count(), Is.EqualTo(0));
		}

		[Test]
		public void TestScimRepresentationConverter()
		{
			var scimHelper = new ScimToScimRepresentation(schemaQueryRepository, scimRepresentationHelper);
			var scimUser = new ScimUser
			{
				UserName = "t@g.com",
				Active = true,
				Id = Guid.NewGuid(),
				ExternalId = "extStr",
				NameFormatted = "Kevin Grace",
				FamilyName = "Grace",
				GivenName = "Kevin",
				MiddleName = "Kevin",
				PhoneNumbersMobile = "042123",
				PhoneNumbersWork = "23423",
				AddressesStreetAddress = "100 Delos street",
				AddressesLocality = "Alexandria",
				AddressesPostalCode = "2015",
				AddressesCountry = "Australia",
				Email = "kevin@wtg.com"
			};

			var scimRepresentation = scimHelper.ScimToSCIMRepresentation(scimUser).Result;

			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.UserName).First().ValueString, Is.EqualTo(scimUser.UserName));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.Active).First().ValueBoolean, Is.EqualTo(scimUser.Active));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.Id).Count(), Is.EqualTo(0));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.ExternalId).Count(), Is.EqualTo(0));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.NameFormatted).First().ValueString, Is.EqualTo(scimUser.NameFormatted));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.FamilyName).First().ValueString, Is.EqualTo(scimUser.FamilyName));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.GivenName).First().ValueString, Is.EqualTo(scimUser.GivenName));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.MiddleName).First().ValueString, Is.EqualTo(scimUser.MiddleName));
			Assert.That(scimRepresentation.GetAttributesByPath("emails.value").First().ValueString, Is.EqualTo(scimUser.Email));
			Assert.That(scimRepresentation.GetAttributesByPath("emails.type").First().ValueString, Is.EqualTo(AttributeNames.Work));

			var phoneNo = scimRepresentation.GetAttributesByPath("phoneNumbers.type").FirstOrDefault(a => a.ValueString.Equals("mobile"));
			var phoneNoVal = scimRepresentation.GetAttributesByPath("phoneNumbers.value").FirstOrDefault(a => a.ValueString.Equals(scimUser.PhoneNumbersMobile));
			Assert.That(phoneNo, Is.Not.Null);
			Assert.That(phoneNoVal, Is.Not.Null);
			Assert.That(phoneNo.ParentAttributeId, Is.EqualTo(phoneNoVal.ParentAttributeId));

			Assert.That(scimRepresentation.GetAttributesByPath("addresses.type").First().ValueString, Is.EqualTo(AttributeNames.Home));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.AddressesStreetAddress).First().ValueString, Is.EqualTo(scimUser.AddressesStreetAddress));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.AddressesLocality).First().ValueString, Is.EqualTo(scimUser.AddressesLocality));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.AddressesRegion).FirstOrDefault().ValueString, Is.Empty);
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.AddressesPostalCode).First().ValueString, Is.EqualTo(scimUser.AddressesPostalCode));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.AddressesCountry).First().ValueString, Is.EqualTo(scimUser.AddressesCountry));
		}

		[Test]
		public void TestScimRepresentationConverter_ExistingCw1User_WithoutNames()
		{
			var scimHelper = new ScimToScimRepresentation(schemaQueryRepository, scimRepresentationHelper);
			var scimUser = new ScimUser
			{
				UserName = "t@g.com",
				Active = true,
				Id = Guid.NewGuid(),
				ExternalId = "extStr",
				NameFormatted = "Kevin Grace",
				FamilyName = "",
				GivenName = "",
				MiddleName = "",
				PhoneNumbersMobile = "042123",
				PhoneNumbersWork = "23423",
				AddressesStreetAddress = "100 Delos street",
				AddressesLocality = "Alexandria",
				AddressesPostalCode = "2015",
				AddressesCountry = "Australia",
				Email = "kevin@wtg.com"
			};

			var scimRepresentation = scimHelper.ScimToSCIMRepresentation(scimUser).Result;

			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.NameFormatted).First().ValueString, Is.EqualTo(scimUser.NameFormatted));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.FamilyName).First().ValueString, Is.EqualTo(AttributeNames.NotProvided));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.GivenName).First().ValueString, Is.EqualTo(AttributeNames.NotProvided));
			Assert.That(scimRepresentation.GetAttributesByPath(AttributeNames.MiddleName).First().ValueString, Is.EqualTo(scimUser.MiddleName));
		}

		[Test]
		public void TestJsonToScimRepresentation()
		{
			var json = /*lang=json,strict*/ @"
{
""schemas"": [
	""urn:ietf:params:scim:schemas:core: 2.0:User""
],
""userName"": ""bjen"",
""externalId"": ""externalid"",
""name"": {
	""formatted"": ""formatted"",
	""familyName"": ""familyName"",
	""givenName"": ""givenName""
},
""phoneNumbers"": [
	{
		""value"": ""555-555-5555"",
		""type"": ""work""
	}
],
""addresses"": [
	{
		""type"": ""home"",
		""streetAddress"": ""100 Universal City Plaza"",
		""locality"": ""Hollywood"",
		""region"": ""CA"",
		""postalCode"": ""91608"",
		""country"": ""USA"",
		""formatted"": ""100 Universal City Plaza\nHollywood, CA 91608 USA"",
		""primary"": true
	}
],
""emails"": [
	{
        ""value"":""babs@jensen.org"",
        ""type"":""work""
    }
],
""employeeNumber"": ""number""
}
";
			var res = JsonConvert.DeserializeObject<ScimUser>(json, new ScimUserJsonConverter());

			Assert.That(res, Is.Not.Null);
			Assert.That(res.UserName, Is.EqualTo("bjen"));
			Assert.That(res.ExternalId, Is.EqualTo("externalid"));
			Assert.That(res.NameFormatted, Is.EqualTo("formatted"));
			Assert.That(res.FamilyName, Is.EqualTo("familyName"));
			Assert.That(res.AddressesStreetAddress, Is.EqualTo("100 Universal City Plaza"));
			Assert.That(res.AddressesCountry, Is.EqualTo("USA"));
			Assert.That(res.AddressesPostalCode, Is.EqualTo("91608"));
			Assert.That(res.AddressesRegion, Is.EqualTo("CA"));
			Assert.That(res.AddressesLocality, Is.EqualTo("Hollywood"));
			Assert.That(res.Email, Is.EqualTo("babs@jensen.org"));
		}

		[Test]
		public void TestJsonToScimRepresentation_WorkAddress()
		{
			var json = /*lang=json,strict*/ @"
{
""schemas"": [
	""urn:ietf:params:scim:schemas:core: 2.0:User""
],
""userName"": ""bjen"",
""externalId"": ""externalid"",
""name"": {
	""formatted"": ""formatted"",
	""familyName"": ""familyName"",
	""givenName"": ""givenName""
},
""phoneNumbers"": [
	{
		""value"": ""555-555-5555"",
		""type"": ""work""
	}
],
""addresses"": [
	{
		""type"": ""work"",
		""streetAddress"": ""100 Universal City Plaza"",
		""locality"": ""Hollywood"",
		""region"": ""CA"",
		""postalCode"": ""91608"",
		""country"": ""USA"",
		""formatted"": ""100 Universal City Plaza\nHollywood, CA 91608 USA"",
		""primary"": true
	}
],
""emails"": [
	{
        ""value"":""babs@jensen.org"",
        ""type"":""work""
    }
],
""employeeNumber"": ""number""
}
";
			var res = JsonConvert.DeserializeObject<ScimUser>(json, new ScimUserJsonConverter());

			Assert.That(res, Is.Not.Null);
			Assert.That(res.UserName, Is.EqualTo("bjen"));
			Assert.That(res.ExternalId, Is.EqualTo("externalid"));
			Assert.That(res.NameFormatted, Is.EqualTo("formatted"));
			Assert.That(res.FamilyName, Is.EqualTo("familyName"));
			Assert.That(res.AddressesStreetAddress, Is.EqualTo(""));
			Assert.That(res.AddressesCountry, Is.EqualTo(""));
			Assert.That(res.AddressesPostalCode, Is.EqualTo(""));
			Assert.That(res.AddressesRegion, Is.EqualTo(""));
			Assert.That(res.AddressesLocality, Is.EqualTo(""));
			Assert.That(res.Email, Is.EqualTo("babs@jensen.org"));
		}

		[Test]
		public void TestJsonToScimRepresentation_WorkAndHomeAddress()
		{
			var json = /*lang=json,strict*/ @"
{
""schemas"": [
	""urn:ietf:params:scim:schemas:core: 2.0:User""
],
""userName"": ""bjen"",
""externalId"": ""externalid"",
""name"": {
	""formatted"": ""formatted"",
	""familyName"": ""familyName"",
	""givenName"": ""givenName""
},
""phoneNumbers"": [
	{
		""value"": ""555-555-5555"",
		""type"": ""work""
	}
],
""addresses"": [
	{
		""type"": ""work"",
		""streetAddress"": ""Dreamworld Pkwy"",
		""locality"": ""Coomera"",
		""region"": ""QLD"",
		""postalCode"": ""4209"",
		""country"": ""Australia"",
		""formatted"": ""Dreamworld Pkwy\nCoomera, QLD 4209 Australia"",
		""primary"": true
	}
],
""addresses"": [
	{
		""type"": ""home"",
		""streetAddress"": ""100 Universal City Plaza"",
		""locality"": ""Hollywood"",
		""region"": ""CA"",
		""postalCode"": ""91608"",
		""country"": ""USA"",
		""formatted"": ""100 Universal City Plaza\nHollywood, CA 91608 USA"",
		""primary"": false
	}
],
""emails"": [
	{
        ""value"":""babs@jensen.org"",
        ""type"":""work""
    }
],
""employeeNumber"": ""number""
}
";
			var res = JsonConvert.DeserializeObject<ScimUser>(json, new ScimUserJsonConverter());

			Assert.That(res, Is.Not.Null);
			Assert.That(res.UserName, Is.EqualTo("bjen"));
			Assert.That(res.ExternalId, Is.EqualTo("externalid"));
			Assert.That(res.NameFormatted, Is.EqualTo("formatted"));
			Assert.That(res.FamilyName, Is.EqualTo("familyName"));
			Assert.That(res.AddressesStreetAddress, Is.EqualTo("100 Universal City Plaza"));
			Assert.That(res.AddressesCountry, Is.EqualTo("USA"));
			Assert.That(res.AddressesPostalCode, Is.EqualTo("91608"));
			Assert.That(res.AddressesRegion, Is.EqualTo("CA"));
			Assert.That(res.AddressesLocality, Is.EqualTo("Hollywood"));
			Assert.That(res.Email, Is.EqualTo("babs@jensen.org"));
		}

		[SetUp]
		public void Init()
		{
			var basePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Schemas");
			var userSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "UserSchema.json"), SCIMResourceTypes.User, true);
			var groupSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "GroupSchema.json"), SCIMResourceTypes.Group, true);
			var cwUserSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "CargoWiseUserSchema.json"), SCIMResourceTypes.User, true);
			var cwGroupSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "CargoWiseGroupSchema.json"), SCIMResourceTypes.Group, true);

			userSchema.SchemaExtensions.Add(new SCIMSchemaExtension
			{
				Id = Guid.NewGuid().ToString(),
				Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:User"
			});

			groupSchema.SchemaExtensions.Add(new SCIMSchemaExtension
			{
				Id = Guid.NewGuid().ToString(),
				Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:Group"
			});

			var schemas = new List<SCIMSchema>();
			schemas.AddRange(new List<SCIMSchema>
			{
				userSchema,
				groupSchema,
				cwUserSchema,
				cwGroupSchema
			});

			schemaQueryRepository = new DefaultSchemaQueryRepository(schemas);
			scimRepresentationHelper = new SCIMRepresentationHelper(new SCIMHostOptions());
		}
	}
}
