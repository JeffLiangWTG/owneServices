using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Persistence.InMemory;
#if NET48
using System.Collections.Generic;
#endif

namespace Enterprise.Services.Scim.Business.Test
{
	static class ScimProcessorTestHelper
	{
		public static ScimUser CreateScimUser(string externalId, string userName, string fullName, string firstName, string middleName, string lastName, string address = "", string city = "", string mobile = "", string email = "", string title = "", string suffix = "", string prefix = "", string country = "", string language = "", string homePhone = "", string workPhone = "", string fax = "", string state = "", string branch = "", string department = "", string otherRef = "")
		{
			var scimUser = new ScimUser();
			scimUser.Active = true;
			scimUser.UserName = userName;
			scimUser.AddressesStreetAddress = address;
			scimUser.AddressesLocality = city;
			scimUser.AddressesRegion = state;
			scimUser.ExternalId = externalId;
			scimUser.PhoneNumbersMobile = mobile;
			scimUser.Email = email;
			scimUser.Title = title;
			scimUser.NameHonorificSuffix = suffix;
			scimUser.NameHonorificPrefix = prefix;
			scimUser.NameFormatted = fullName;
			scimUser.GivenName = firstName;
			scimUser.FamilyName = lastName;
			scimUser.MiddleName = middleName;
			scimUser.AddressesCountry = country;
			scimUser.PreferredLanguage = language;
			scimUser.PhoneNumbersHome = homePhone;
			scimUser.PhoneNumbersWork = workPhone;
			scimUser.PhoneNumbersFax = fax;
			scimUser.HomeBranch = branch;
			scimUser.HomeDepartment = department;
			scimUser.OtherReferences = otherRef;
			return scimUser;
		}

		public static ScimGroup CreateScimGroup(string externalId, string displayName, string category, params GroupMember[] members)
		{
			var scimGroup = new ScimGroup();
			scimGroup.DisplayName = displayName;
			scimGroup.ExternalId = externalId;
			scimGroup.Category = category;
			scimGroup.Members = members;

			return scimGroup;
		}

		public static ScimGroup CreateScimGroup(string externalId, string displayName, params GroupMember[] members)
		{
			return CreateScimGroup(externalId, displayName, "", members);
		}

		public static DefaultSchemaQueryRepository ReturnSchemaQueryRepository()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var userSchemaPath = resourceRetriever.SaveResourceToFile("Enterprise.Services.Scim.Business.Test.Schemas.UserSchema.json");
				var groupSchemaPath = resourceRetriever.SaveResourceToFile("Enterprise.Services.Scim.Business.Test.Schemas.GroupSchema.json");
				var cwUserSchemaPath = resourceRetriever.SaveResourceToFile("Enterprise.Services.Scim.Business.Test.Schemas.CargoWiseUserSchema.json");
				var cwGroupSchemaPath = resourceRetriever.SaveResourceToFile("Enterprise.Services.Scim.Business.Test.Schemas.CargoWiseGroupSchema.json");
				var userSchema = SCIMSchemaExtractor.Extract(userSchemaPath, SCIMResourceTypes.User, true);
				var groupSchema = SCIMSchemaExtractor.Extract(groupSchemaPath, SCIMResourceTypes.Group, true);
				var cwUserSchema = SCIMSchemaExtractor.Extract(cwUserSchemaPath, SCIMResourceTypes.User, true);
				var cwGroupSchema = SCIMSchemaExtractor.Extract(cwGroupSchemaPath, SCIMResourceTypes.User, true);

				userSchema.SchemaExtensions.Add(new SCIMSchemaExtension
				{
					Id = ZGuid.NewZGuid().ToString(),
					Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:User"
				});

				groupSchema.SchemaExtensions.Add(new SCIMSchemaExtension
				{
					Id = ZGuid.NewZGuid().ToString(),
					Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:Group"
				});

				var schemas = new List<SCIMSchema>
				{
					userSchema,
					groupSchema,
					cwUserSchema,
					cwGroupSchema
				};

				return new DefaultSchemaQueryRepository(schemas);
			}
		}
	}
}
