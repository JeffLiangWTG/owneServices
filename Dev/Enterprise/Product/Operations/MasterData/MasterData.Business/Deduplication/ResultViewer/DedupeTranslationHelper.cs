using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Business
{
	public static class DedupeTranslationHelper
	{
		[ThreadSafe]
		static IReadOnlyDictionary<string, ResourceString> confidenceDescriptionDictionary;
		[ThreadSafe]
		static IReadOnlyDictionary<string, ResourceString> modelSourceDictionary;
		[ThreadSafe]
		static IReadOnlyDictionary<string, ResourceString> modelHeaderCaptionDictionary;

		static DedupeTranslationHelper()
		{
			InitialiseConfidenceDescriptionDictionary();
			InitialiseModelSourceDictionary();
			InitialiseModelHeaderDictionary();
		}

		public static string GetConfidenceDescription(string key)
		{
			if (key is null)
			{
				return string.Empty;
			}

			confidenceDescriptionDictionary.TryGetValue(key, out var translation);
			return translation ?? key;
		}

		public static string GetModelSource(string key)
		{
			if (key is null)
			{
				return string.Empty;
			}

			modelSourceDictionary.TryGetValue(key, out var translation);
			return translation ?? key;
		}

		public static string GetModelHeaderCaption(string key)
		{
			if (key is null)
			{
				return string.Empty;
			}

			modelHeaderCaptionDictionary.TryGetValue(key, out var translation);
			return translation ?? key;
		}

		static void InitialiseConfidenceDescriptionDictionary()
		{
			confidenceDescriptionDictionary = new Dictionary<string, ResourceString>
			{
				[nameof(ConfidenceRating.None)] = ResString.GetMultilingualString("c2aeb1be-48e0-4024-84b5-ec64d863772c", "None"),
				[nameof(ConfidenceRating.Low)] = ResString.GetMultilingualString("8f3738dd-8926-4a0e-a618-7a0a0c0d281b", "Low"),
				[nameof(ConfidenceRating.Medium)] = ResString.GetMultilingualString("e65959a1-f30b-4c8a-9e51-42b89a88b4cb", "Medium"),
				[nameof(ConfidenceRating.High)] = ResString.GetMultilingualString("f502751e-f8d4-4ec2-aa7a-9dfd6c397c6a", "High"),
				[nameof(ConfidenceRating.Exact)] = ResString.GetMultilingualString("33b94ffa-4d12-4e33-999a-90b0349cea28", "Exact"),
				[nameof(ConfidenceRating.Undefined)] = ResString.GetMultilingualString("08677880-d604-4e9d-b786-7a8d51f0a8a4", "Undefined")
			};
		}

		static void InitialiseModelSourceDictionary()
		{
			modelSourceDictionary = new Dictionary<string, ResourceString>
			{
				[DeduplicationProvider.Constants.Name] = ResString.GetMultilingualString("4c7b6013-fbd9-429c-9009-d4c200b8636a", "Name"),
				[DeduplicationProvider.Constants.Number] = ResString.GetMultilingualString("93ba2498-e887-4f3c-b71d-a95acac1b98a", "Number"),
				[DeduplicationProvider.Constants.Website] = ResString.GetMultilingualString("f3c9046c-6b1b-42ce-821f-f4a5e679ac99", "Website"),
				[DeduplicationProvider.Constants.Domain] = ResString.GetMultilingualString("388d958b-84d0-4d88-b96a-e89ac2182519", "Domain"),
				[DeduplicationProvider.Constants.Email] = ResString.GetMultilingualString("c4029daa-c64c-4dae-8565-c033e2faa57a", "E-mail"),
				[DeduplicationProvider.Constants.Birthday] = ResString.GetMultilingualString("cda3500b-f860-45d2-bf9f-8a626c5e3841", "Birthday")
			};
		}

		static void InitialiseModelHeaderDictionary()
		{
			modelHeaderCaptionDictionary = new Dictionary<string, ResourceString>
			{
				[DeduplicationProvider.Constants.Emails] = ResString.GetMultilingualString("f992b2ce-bc33-4e4d-8272-893325eb340c", "E-mails"),
				[DeduplicationProvider.Constants.Birthdays] = ResString.GetMultilingualString("e3072475-ae85-40d7-863d-088f08a8bd72", "Birthdays"),
				[DeduplicationProvider.Constants.Organisations] = ResString.GetMultilingualString("eaac42c1-7e5b-41d4-909e-52f40764271c", "Organizations"),
				[DeduplicationProvider.Constants.Addresses] = ResString.GetMultilingualString("53a91e22-5343-46f0-b8cd-fc64496e0067", "Addresses"),
				[DeduplicationProvider.Constants.Contacts] = ResString.GetMultilingualString("86c4d4b6-5145-4a50-8981-6fdd97765720", "Contacts"),
				[DeduplicationProvider.Constants.RegistrationCodes] = ResString.GetMultilingualString("453c973e-6ff6-442c-b106-e4d3b14da58f", "Registration Codes"),
				[DeduplicationProvider.Constants.Websites] = ResString.GetMultilingualString("35a9b204-9143-4437-b0a9-9e1f937a3c57", "Websites"),
				[DeduplicationProvider.Constants.Domains] = ResString.GetMultilingualString("858e7403-9a11-4484-a29f-a01d8104e5c2", "Domains"),
				[DeduplicationProvider.Constants.PhoneNumbers] = ResString.GetMultilingualString("982b1710-6bcf-4070-89b3-e0fa2b411068", "Phone Numbers"),
				[DeduplicationProvider.Constants.OrganisationNames] = ResString.GetMultilingualString("20890b8a-f668-456d-9bcd-f74ae0940e63", "Organization Names"),
				[DeduplicationProvider.Constants.PersonNames] = ResString.GetMultilingualString("8596850b-ef71-469f-83d4-7e1cf7010b90", "Person Names"),
				[DeduplicationProvider.Constants.Person] = ResString.GetMultilingualString("3b567063-a539-4526-8803-e7ac8c971199", "Person"),
				[DeduplicationProvider.Constants.Staff] = ResString.GetMultilingualString("507d10fc-1a70-4841-a6c9-cd2122f461dc", "Staff"),
				[DeduplicationProvider.Constants.Applicant] = ResString.GetMultilingualString("9e68a990-99a3-4c77-8078-abfa63300f5e", "Applicant"),
				[DeduplicationProvider.Constants.ActiveAssociations] = ResString.GetMultilingualString("24961066-636e-4d2a-8868-86c3d7831932", "Active Associations")
			};
		}
	}
}
