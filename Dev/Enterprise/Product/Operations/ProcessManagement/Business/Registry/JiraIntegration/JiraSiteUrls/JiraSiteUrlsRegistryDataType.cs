using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraSiteUrlsRegistryDataType : CodeDescriptionPairListRegistryDataType
	{
		public JiraSiteUrlsRegistryDataType(int codeMaxLength)
			: base(codeMaxLength)
		{
		}

		ReadOnlyCodeDescriptionPairList deserializedList;

		protected override ReadOnlyCodeDescriptionPairList DeserialiseCore(byte[] value)
		{
			deserializedList = base.DeserialiseCore(value);

			return deserializedList;
		}

		protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			ValidateUrlShouldBeWellFormed(proposedValue);
			ValidateOriginalItemsWithExistingLinksAreUnchanged(proposedValue);
		}

		static void ValidateUrlShouldBeWellFormed(ReadOnlyCodeDescriptionPairList proposedValue)
		{
			foreach (CodeDescriptionPair pair in proposedValue)
			{
				UriRegistryTypeValidator.ValidateUri(pair.Description, Uri.UriSchemeHttps, shouldAllowAutoProtocolPrefixing: false);
			}
		}

		void ValidateOriginalItemsWithExistingLinksAreUnchanged(ReadOnlyCodeDescriptionPairList proposedValue)
		{
			if (deserializedList != null && deserializedList.Count > 0)
			{
				var codesWithExistingLinks = GetExistingLinksForOriginalSystemCodes();
				var errors = new List<string>();

				foreach (var codeWithExistingLinks in codesWithExistingLinks)
				{
					if (!proposedValue.ContainsCode(codeWithExistingLinks))
					{
						errors.Add(GetChangedSystemExceptionMessage(codeWithExistingLinks));
					}
					else
					{
						var originalUrl = deserializedList.GetDescriptionFromCode(codeWithExistingLinks);
						var newUrl = proposedValue.GetDescriptionFromCode(codeWithExistingLinks);

						if (!originalUrl.Equals(newUrl, StringComparison.Ordinal))
						{
							errors.Add(GetChangedSystemExceptionMessage(codeWithExistingLinks));
						}
					}
				}

				if (errors.Any())
				{
					throw new RegistryValidationException(string.Join(System.Environment.NewLine, errors));
				}
			}
		}

		static string GetChangedSystemExceptionMessage(string code)
		{
			return ResString.GetMultilingualString("92627b36-2816-42f2-b6e6-50aef8375035", "There are imported items linked to the [{0}] system. Changes cannot be made to the Code or URL for systems that have imported links already.", code);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<string> GetExistingLinksForOriginalSystemCodes()
		{
			var codes = string.Join(",", deserializedList.GetAllCodes().Select(x => $"'{x}'")); // Just building up a sql statement here
			var sql = $"SELECT DISTINCT EEL_SystemCode FROM dbo.ExternalEntityLink WHERE EEL_SystemCode IN ({codes})";
			var results = new List<string>();

			using (var reader = Db.Connection.Command(sql).ExecuteReader()) // We don't want the business objects, just the list of codes.
			{
				while (reader.Read())
				{
					results.Add(reader.GetString(0));
				}
			}

			return results;
		}

		#region For Test
#if DEBUG

		public void SetDeserializedList_ForTest(CodeDescriptionPairList list)
		{
			deserializedList = list;
		}

#endif
		#endregion
	}
}
