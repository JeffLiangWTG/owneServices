using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionAgreementConflictsTextProvider
	{
		#region New

		public static CommissionAgreementConflictsTextProvider New()
		{
			var type = TypeDecider.GetTypeForBinding(typeof(CommissionAgreementConflictsTextProvider));
			var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			return (CommissionAgreementConflictsTextProvider)constructor.Invoke(null);
		}

		protected CommissionAgreementConflictsTextProvider()
		{
		}

		#endregion

		#region ToDisplayList

		public virtual string ToDisplayList(IEnumerable<ICommissionAgreementConflict> conflicts, int indentLevel, bool html)
		{
			var result = new ZStringBuilder();
			var itemConflicts = conflicts.OfType<ICommissionAgreementItemConflict>();
			result.Append(GetLeafList(itemConflicts, indentLevel, html));

			return result.ToString();
		}

		protected static string GetLeafList(IEnumerable<ICommissionAgreementItemConflict> conflicts, int indentLevel, bool html)
		{
			if (html)
			{
				return string.Join("", conflicts.Select(x => string.Format(CultureInfo.InvariantCulture, (NoResString)"<li>{0}</li>", WebUtility.HtmlEncode(x.AgreementItem.GetItemPathDisplayText()))).OrderBy(x => x));
			}
			else
			{
				var prefix = new string(' ', indentLevel * 3);
				return string.Join(System.Environment.NewLine, conflicts.Select(x =>
					prefix +
					string.Format(CultureInfo.InvariantCulture, "{0}", x.AgreementItem.GetItemPathDisplayText().Replace("\r\n ", "\r\n" + prefix))).OrderBy(x => x));
			}
		}

		public ZString ToDisplayListGroupedByAgreement(IEnumerable<ICommissionAgreementConflict> conflicts, bool withWinnerAgreement)
		{
			var conflictsGroupedByCommissionAgreements = conflicts.GroupBy(x => Tuple.Create(x.WinnerCommissionAgreement, x.LoserCommissionAgreement));
			return string.Join(System.Environment.NewLine + System.Environment.NewLine, conflictsGroupedByCommissionAgreements.Select(x => ToDisplayListGroupedByAgreement(x, withWinnerAgreement)));
		}

		ZString ToDisplayListGroupedByAgreement(IGrouping<Tuple<OrgCommissionAgreement, OrgCommissionAgreement>, ICommissionAgreementConflict> conflictsGrouping, bool withWinnerAgreementText)
		{
			var conflictsList = ToDisplayList(conflictsGrouping, 1, false);
			var header = withWinnerAgreementText
				? Res.GetString("fdeaa79b-0e61-4fc2-9cae-f5e931b7eb37", @"{0} will take commission from {1}",
					conflictsGrouping.Key.Item1.AgreementId,
					conflictsGrouping.Key.Item2.AgreementId)
				: (string)conflictsGrouping.Key.Item1.AgreementId;

			return header +
				System.Environment.NewLine +
				conflictsList;
		}

		#endregion

		#region ToErrorMessage

		public virtual string ToErrorMessage(IEnumerable<ICommissionAgreementDuplication> duplications)
		{
			return Res.GetString("c207fab6-865f-4cfc-b20a-c20c71cc7699", "Other agreement(s) have a duplicate item. ({0})", string.Join(", ", duplications.Select(x => x.ToDisplayText()).Distinct()));
		}

		public virtual string ToConflictWarningMessage(IEnumerable<ICommissionAgreementConflict> conflicts)
		{
			var builder = new ZStringBuilder();
			var itemConflicts = conflicts.OfType<ICommissionAgreementItemConflict>();
			foreach (var groupedConflicts in itemConflicts.Where(x => x.LoserAgreementItem != null).GroupBy(x => x.LoserAgreementItem))
			{
				builder.AppendLine(groupedConflicts.Key.GetItemPathDisplayText()).AppendLine();
				builder.AppendLine(ToDisplayListGroupedByAgreement(groupedConflicts, false)).AppendLine().AppendLine();
			}

			var conflictDispalyList = builder.ToString();
			return Res.GetString("404A5857-52EF-43AC-8664-B2125F0D1A3A", "This will not include the following subset of commissions as more specific commission agreements already exist:\r\n\r\n{0}", conflictDispalyList);
		}

		#endregion
	}
}
