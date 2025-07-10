using System.Data;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Blazor.Common.Data;
using Microsoft.Extensions.Options;

namespace CargoWise.Blazor.Common.Testing.Auth
{
	/// <summary>
	/// This should only be used for testing and local development.
	/// </summary>
	public class DebugClientTokenGenerator
	{
		readonly CargoWiseOptions cwOptions;
		static readonly IList<char> characterSet = new List<char>("ABCDEFHKMNPQRTWXY34578");

		public DebugClientTokenGenerator(IOptions<CargoWiseOptions> cwOptions)
		{
			this.cwOptions = cwOptions.Value;
		}

		public string GenerateClientToken(string type = "BLC", Guid? parentId = null, string parentTableCode = "GS", DateTime? expiresAtUtc = null, string scope = "")
		{
			var connectionString = ConnectionStringBuilder.GetConnectionString(cwOptions.DbServerName, cwOptions.DatabaseName, "CargoWiseBlazor.SessionBroker");

			try
			{
				var token = CreateToken(30);
				var tries = 0;
				parentId ??= GetParentId(connectionString);
#pragma warning disable CW1061 // We only care about the date in the context of the local machine here
				expiresAtUtc ??= DateTime.UtcNow.AddMinutes(5);
#pragma warning restore CW1061
				bool created;
				do
				{
					created = TryCreateToken(token, connectionString, type, parentId.Value, parentTableCode, expiresAtUtc.Value, scope);
					tries++;
				}
				while (!created && tries < 20);

				if (!created)
				{
					throw new TokenGenerationException("Failed to create unique test token");
				}
				return token;
			}
			catch (Exception ex)
			{
				throw new TokenGenerationException("Failed to generate token, connection string: " + connectionString, ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		Guid GetParentId(string connectionString)
		{
			using var connection = new SqlConnection(connectionString);
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "select GS_PK from dbo.GlbStaff where GS_Code = 'E'"; // this is the CWSupport user, I think
			return (Guid)command.ExecuteScalar();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		bool TryCreateToken(string token, string connectionString, string type, Guid parentId, string parentTableCode, DateTime expiresAtUtc, string scope)
		{
			using var connection = new SqlConnection(connectionString);
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "CreateAccessToken";
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue("@Token", token);
			command.Parameters.AddWithValue("@Type", type);
			command.Parameters.AddWithValue("@Scope", scope);
			command.Parameters.AddWithValue("@ParentId", parentId);
			command.Parameters.AddWithValue("@ParentTableCode", parentTableCode);
			command.Parameters.AddWithValue("@IsPermanent", false);
			command.Parameters.AddWithValue("@ExpiresAtUtc", expiresAtUtc);
			command.Parameters.AddWithValue("@UseCount", 1);
			command.Parameters.AddWithValue("@CreateUser", "ABC");
			command.Parameters.Add(new SqlParameter("@CATResult", SqlDbType.Bit, 0, ParameterDirection.Output, true, 0, 0, null, DataRowVersion.Default, DBNull.Value));

			command.ExecuteNonQuery();

			var result = (bool)command.Parameters["@CATResult"].Value;
			return result;
		}

		// token generation based on https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FArchitecture%2FGlowInterop%2FGlowInterop%2FAccessToken%2FTokenizedAccessControlExtensions.cs&version=GBmaster
		static string CreateToken(int length)
		{
			var sb = new StringBuilder(length);
			var numCharsToChooseFrom = characterSet.Count;

			for (var i = 0; i < length; i++)
			{
				var index = GetRandomNumber((uint)numCharsToChooseFrom);
				var randomChar = characterSet[index];
				sb.Append(randomChar);
			}

			return sb.ToString();
		}

		static int GetRandomNumber(uint maxNumber)
		{
			if (maxNumber <= 0 || maxNumber > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(maxNumber));
			}

			var randomData = RandomNumberGenerator.GetBytes(sizeof(ulong));
			var integerValue = BitConverter.ToUInt64(randomData, 0);
			var randomNumber = integerValue % maxNumber;
			return (int)randomNumber;
		}
	}
}
