using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business
{
	public class DocumentWebSecurityRights : IWebSecurityRightProvider
	{
		readonly BusinessObjectFactory factory;

		public DocumentWebSecurityRights(BusinessObjectFactory factory = null)
		{
			this.factory = factory ?? new BusinessObjectFactory();
		}

		public int Count => factory.GetDatabaseCount(typeof(RefDocType), new ZQuery());

		readonly Dictionary<string, WebSecurityRight> securityRightsCache = new Dictionary<string, WebSecurityRight>();
		public bool TryGetValue(string code, out WebSecurityRight right)
		{
			if (securityRightsCache.TryGetValue(code, out right))
			{
				return true;
			}

			var docType = GetRefDocTypeFromSecurityCode(code);
			if (docType != null)
			{
				right = GetSecurityRight(docType, checkCache: false);
				return true;
			}

			return false;
		}

		public WebSecurityRight GetSecurityRight(RefDocType docType)
		{
			return GetSecurityRight(Argument.NotNull(docType, nameof(docType)), checkCache: true);
		}

		WebSecurityRight GetSecurityRight(RefDocType docType, bool checkCache)
		{
			WebSecurityRight right;
			if (checkCache && securityRightsCache.TryGetValue(GetSecurityCodeFromDocType(docType), out right))
			{
				return right;
			}

			right = CreateRightForDocType(docType);
			securityRightsCache.Add(right.Code, right);
			return right;
		}

		RefDocType GetRefDocTypeFromSecurityCode(string code)
		{
			var match = Regex.Match(code, @"^RefDocType:([^:]{0,3}):([^:]{0,4})$");
			if (match.Success)
			{
				var query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, match.Groups[1].Value)
					.AddToFilter(RefDocTypeSchema.RT_DocType, match.Groups[2].Value);

				return factory.LoadTop1<RefDocType>(query);
			}
			if (code.StartsWith("RefDocType:", StringComparison.Ordinal)) // Code For security right name
			{
				ErrorReporter.ReportOnce("NullGetRefDocTypeFromSecurityCode", string.Format(CultureInfo.InvariantCulture, "Could not find RefDocType in database for code {0}.", code));
			}
			return null;
		}

		public IEnumerator<WebSecurityRight> GetEnumerator()
		{
			foreach (var docType in factory.Load<RefDocType>(new ZQuery()))
			{
				yield return CreateRightForDocType(docType);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		static WebSecurityRight CreateRightForDocType(RefDocType docType)
		{
			return new WebSecurityRight(GetSecurityCodeFromDocType(docType), GetDescriptionForDocType(docType), WebSecurityApplication.EdiWebTracker, isGrantedByDefault: true);
		}

		internal static string GetSecurityCodeFromDocType(RefDocType docType)
		{
			return "RefDocType:" + docType.RT_ReferenceType + ":" + docType.RT_DocType; // Code For security right name
		}

		static MultilingualString GetDescriptionForDocType(RefDocType docType)
		{
			return ResString.GetMultilingualString("313fd42b-b5cc-4adb-9570-95e2ae1ac525", "Document {0} for {1} ({2})", docType.RT_DocType, docType.RT_ReferenceType, docType.RT_DescMultilingual);
		}
	}
}
