using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business
{
	public class DeclarationExportStatementCreator : ExportStatementCreator
	{
		public DeclarationExportStatementCreator(JobDeclaration declaration, ExportStatementSetting exportStatementSetting)
			: base(exportStatementSetting)
		{
			if (declaration == null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}
			this.declaration = declaration;
		}

		protected override ZString ExportStatementCore
		{
			get
			{
				StringCollectionX collection = new StringCollectionX();
				ZStringBuilder builder = new ZStringBuilder();
				foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
				{
					ZString entryHeaderExportStatement = new CusEntryHeaderExportStatementCreator(entryHeader, ExportStatementSetting).ExportStatement;
					if (!entryHeaderExportStatement.IsEmpty && !collection.Contains(entryHeaderExportStatement))
					{
						collection.Add(entryHeaderExportStatement);
						builder.Append(entryHeaderExportStatement);
					}
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		readonly JobDeclaration declaration;
	}
}
