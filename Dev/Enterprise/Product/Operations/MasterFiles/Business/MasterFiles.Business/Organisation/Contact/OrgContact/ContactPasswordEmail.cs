using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IPasswordEmailSource : IContactable, IBusiness
	{
		ZString Password { get; }
		ZString Url { get; }
		ZString OrgCode { get; }
		ZString ExtraInstruction { get; }
		ZString FromDisplayName { get; }
		ZString FromAddress { get; }
		ZString Salutation { get; }
		ZString Language { get; }
	}

	public class ContactPasswordEmail : HtmlFormatEmailToContactBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ContactPasswordEmail(IPasswordEmailSource emailSource)
			: base((BusinessObject)emailSource, emailSource.FromAddress, emailSource.FromDisplayName)
		{
			SetDefaultEmailContent();
		}

		public IPasswordEmailSource EmailInfo
		{
			get { return (IPasswordEmailSource)BusinessObjectSendingEmail; }
		}

		#region Random Password Generator

		static Random Random
		{
			get { return (Random)(random.Target ?? (random.Target = new Random())); }
		}

		[SuppressThreadStaticFieldMessage]
		static readonly WeakReference random = new WeakReference(null);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Character set for password")]
		public static string GenerateRandomWebPassword()
		{
			string result = string.Empty;
			const string lowerCaseCharacterSet = "abcdefghijklmnopqrstuvwxyz";
			const string upperCaseCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			const string numberCharacterSet = "0123456789";
			const string specialCharacterSet = " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

			var lowerCaseAmount = Random.Next(3, 5);
			var upperCaseAmount = Random.Next(3, 5);
			var numberAmount = Random.Next(3, 5);

			var minAdditionalLength = WebDataRegistry.Instance.WebPasswordMinLength.Value - lowerCaseAmount - upperCaseAmount - numberAmount;

			var specialCharMinAmount = Math.Max(minAdditionalLength, 3);
			var specialCharAmount = Random.Next(specialCharMinAmount, specialCharMinAmount + 2);

			result = AddRandomChars(result, lowerCaseCharacterSet, lowerCaseAmount);
			result = AddRandomChars(result, upperCaseCharacterSet, upperCaseAmount);
			result = AddRandomChars(result, numberCharacterSet, numberAmount);
			result = AddRandomChars(result, specialCharacterSet, specialCharAmount);

			result = ShuffleString(result);

			return result;

			string AddRandomChars(string root, string characterSet, int amountToAdd)
			{
				var characterSetLength = characterSet.Length;

				for (int i = 0; i < amountToAdd; i++)
				{
					root += characterSet[Random.Next(characterSetLength - 1)];
				}

				return root;
			}

			string ShuffleString(string root)
			{
				return new string(root.ToCharArray().OrderBy(s => (Random.Next(3) % 2) == 0).ToArray());
			}
		}

		#endregion

		void SetDefaultEmailContent()
		{
			IPasswordEmailSource contact = (IPasswordEmailSource)BusinessObjectSendingEmail;
			ToDisplayName = contact.Name;
			ToEmailAddress = contact.Email;

			using (WebDataRegistry.Instance.AllowedLanguages.Value.Any(allowed => ((CodeSelection)allowed).Code == contact.Language) ? Res.TemporarilySwitchLanguage(contact.Language) : null)
			{
				Subject = GetEmailSubject();
				Body = GetEmailBody();
			}
		}

		protected virtual string GetEmailSubject()
		{
			return Parser.Parse(BusinessObjectSendingEmail, EmailSubjectTemplate);
		}

		protected virtual string EmailSubjectTemplate => WebDataRegistry.Instance.EmailPasswordTemplate.Value.EmailSubject;

		protected virtual string GetEmailBody()
		{
			return Parser.Parse(BusinessObjectSendingEmail, EmailBodyTemplate);
		}

		protected virtual string EmailBodyTemplate => WebDataRegistry.Instance.EmailPasswordTemplate.Value.EmailBody;

		DocumentParser Parser
		{
			get
			{
				if (parser == null)
				{
					parser = DocumentParser.New(ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordEmail>(), Factory);
				}
				return parser;
			}
		}
		DocumentParser parser;
	}
}
