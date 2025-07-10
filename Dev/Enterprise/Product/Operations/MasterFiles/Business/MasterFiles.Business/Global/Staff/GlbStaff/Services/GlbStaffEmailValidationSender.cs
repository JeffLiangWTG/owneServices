using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEmailValidationSender : IGlbStaffEmailValidationSender
	{
		EmailDef CreateEmailDef(GlbStaff staff, string email)
		{
			var emailToVerify = staff.EmailAddresses.FirstOrDefault(x => x.GSE_EmailAddress == email) ?? throw new Exception($"Could not find email {email} on staff {staff.GS_Code}");

			var token = CreateNonExistingToken();
			var emailDef = new EmailDef();
			emailDef.AddRecipientForUserCommunication(email);
			emailDef.Body = $"Click link to verify email: https://stubportal.com?token={token}";
			emailToVerify.GSE_VerifyToken = token;
			emailToVerify.GSE_VerifyTokenCreateTimeUtc = ZDateTime.UtcNow;
			return emailDef;
		}

		public void GenerateValidationEmail(GlbStaff staff, string email)
		{
			var emailDef = CreateEmailDef(staff, email);
			staff.Factory?.Save();
			Env.OutgoingMailManager.CreateAndSave(emailDef);
		}

		const int MaxTokenGenerationAttempts = 3;

		string CreateNonExistingToken()
		{
			var factory = CreateFactory();
			for (var i = 0; i < 3; i++)
			{
				var token = GenerateToken();
				var query = new ZQuery(GlbStaffEmailAddressSchema.GSE_VerifyToken, token);
				var existingTokens = factory.ExistsInDatabase(GlbStaffEmailAddressSchema.Constants.TableName, query);
				if (!existingTokens)
				{
					return token;
				}
			}
			throw new Exception($"Could not generate a unique token within {MaxTokenGenerationAttempts} tries");
		}

		BusinessObjectFactory CreateFactory()
		{
			return new BusinessObjectFactory { NameForDebugging = nameof(GlbStaffEmailValidationSender) };
		}

		const string CharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

		string GenerateToken()
		{
			using var random = RandomNumberGenerator.Create();
			var length = 26 + GetRandomNumber(5, random);
			var sb = new StringBuilder(length);
			for (var i = 0; i < length; i++)
			{
				sb.Append(CharacterSet[GetRandomNumber(CharacterSet.Length, random)]);
			}
			return sb.ToString();
		}

		int GetRandomNumber(int maxNumberExclusive, RandomNumberGenerator random)
		{
			var data = new byte[sizeof(ulong)];
			random.GetBytes(data);
			var integerValue = BitConverter.ToUInt64(data, 0);
			var randomNumber = integerValue % (uint)maxNumberExclusive;
			return (int)randomNumber;
		}
	}
}
