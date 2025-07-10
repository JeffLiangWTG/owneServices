using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Services.Scim.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Services.Scim.Business
{
	public class ScimUserJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ScimUser);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Json property name")]
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jObject = JObject.Load(reader);
			var res = jObject.Properties().FirstOrDefault(a => a.Name == AttributeNames.Name);
			if (res != null)
			{
				foreach (var item in res.Value)
				{
					if (item.First != null)
					{
						var jp = new JProperty(item.Path, item.First.Value<string>());
						jObject.Add(jp);
					}
				}
				jObject.Remove(AttributeNames.Name);
			}

			var res2 = jObject.Properties().FirstOrDefault(a => a.Name == AttributeNames.PhoneNumbers);
			if (res2 != null)
			{
				foreach (var item in res2.Value)
				{
					var jp = new JProperty($"{AttributeNames.PhoneNumbers}.{item.Value<string>(AttributeNames.Type)}", item.Value<string>(AttributeNames.Value));
					jObject.Add(jp);
				}
				jObject.Remove(AttributeNames.PhoneNumbers);
			}

			var res3 = jObject.Properties().FirstOrDefault(a => a.Name == AttributeNames.Emails);
			if (res3 != null)
			{
				foreach (var item in res3.Value)
				{
					if (item.Value<string>(AttributeNames.Type) == AttributeNames.Work)
					{
						var jp = new JProperty($"{AttributeNames.Emails}.{item.Value<string>(AttributeNames.Type)}", item.Value<string>(AttributeNames.Value));
						jObject.Add(jp);
					}
				}
				jObject.Remove(AttributeNames.Emails);
			}

			var res4 = jObject.Properties().FirstOrDefault(a => a.Name == AttributeNames.Addresses);
			if (res4 != null)
			{
				foreach (var item in res4.Value)
				{
					if (item == null)
					{
						continue;
					}

					var value = item.Value<string>(AttributeNames.Type);

					if (value != null && value.Equals(AttributeNames.Home))
					{
						jObject.Add(new JProperty(AttributeNames.AddressesStreetAddress, item.Value<string>("streetAddress")));
						jObject.Add(new JProperty(AttributeNames.AddressesLocality, item.Value<string>("locality")));
						jObject.Add(new JProperty(AttributeNames.AddressesRegion, item.Value<string>("region")));
						jObject.Add(new JProperty(AttributeNames.AddressesPostalCode, item.Value<string>("postalCode")));
						jObject.Add(new JProperty(AttributeNames.AddressesCountry, item.Value<string>("country")));
						break;
					}
				}
				jObject.Remove(AttributeNames.Addresses);
			}

			return jObject.ToObject<ScimUser>();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			var t = JToken.FromObject(value);
			var o = (JObject)t;
			var scimUser = (ScimUser)value;

			var propertyNames = o.Properties().Select(p => p.Name).ToList();
			var nameProps = propertyNames.Where(a => a.StartsWith($"{AttributeNames.Name}."));
			if (nameProps.Any())
			{
				var namePr = new List<JProperty>();
				foreach (var item in nameProps)
				{
					var strValue = o.GetValue(item).Value<string>();
					if ((item.Equals(AttributeNames.GivenName) || item.Equals(AttributeNames.FamilyName)) && string.IsNullOrEmpty(strValue))
					{
						strValue = AttributeNames.NotProvided;
					}
					namePr.Add(new JProperty(item.Split('.')[1], strValue));
					o.Remove(item);
				}
				o.Add(new JProperty(AttributeNames.Name, new JObject(namePr)));
			}
			else
			{
				return;
			}

			var phoneNumbersProps = propertyNames.Where(a => a.StartsWith($"{AttributeNames.PhoneNumbers}."));

			if (phoneNumbersProps.Any())
			{
				var phonePr = new List<JObject>();
				foreach (var item in phoneNumbersProps)
				{
					var jObj = new JObject
					{
						new JProperty(AttributeNames.Value, o.GetValue(item)),
						new JProperty(AttributeNames.Type, item.Split('.')[1])
					};
					phonePr.Add(jObj);
					o.Remove(item);
				}
				o.Add(new JProperty(AttributeNames.PhoneNumbers, new JArray(phonePr)));
			}

			var addressesProps = propertyNames.Where(a => a.StartsWith($"{AttributeNames.Addresses}."));
			if (addressesProps.Any())
			{
				var addressPr = new List<JObject>();
				var jObj = new JObject
				{
					new JProperty(AttributeNames.Type, AttributeNames.Home)
				};

				foreach (var item in addressesProps)
				{
					jObj.Add(new JProperty(item.Split('.')[1], o.GetValue(item)));
					o.Remove(item);
				}
				addressPr.Add(jObj);
				o.Add(new JProperty(AttributeNames.Addresses, new JArray(addressPr)));
			}

			var emailsProps = propertyNames.Where(a => a.StartsWith($"{AttributeNames.Emails}."));
			if (emailsProps.Any())
			{
				var emailPr = new List<JObject>();
				var jObj = new JObject
				{
					new JProperty(AttributeNames.Value, o.GetValue(emailsProps.First())),
					new JProperty(AttributeNames.Type, AttributeNames.Work)
				};
				emailPr.Add(jObj);
				o.Remove(emailsProps.First());
				o.Add(new JProperty(AttributeNames.Emails, new JArray(new JArray(emailPr))));
			}

			var groupProps = propertyNames.Where(a => a.StartsWith($"{AttributeNames.Groups}"));
			foreach (var group in groupProps)
			{
				o.Remove(group);
			}

			var groupPr = new List<JObject>();
			foreach (var group in scimUser.Groups)
			{
				var jObj = new JObject();

				if (!group.Value.IsNullOrEmpty())
				{
					jObj.Add(new JProperty(AttributeNames.Value, group.Value));
				}

				if (!group.Ref.IsNullOrEmpty())
				{
					jObj.Add(new JProperty(AttributeNames.Ref, group.Ref));
				}

				if (!group.Display.IsNullOrEmpty())
				{
					jObj.Add(new JProperty(AttributeNames.Display, group.Display));
				}

				if (!group.Type.IsNullOrEmpty())
				{
					jObj.Add(new JProperty(AttributeNames.Type, group.Type));
				}

				groupPr.Add(jObj);
			}

			o.Add(new JProperty(AttributeNames.Groups, new JArray(groupPr)));

			o.WriteTo(writer);
		}
	}
}
