using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class WHSPackLineFilteredCollection : Customs.Business.FilteredCollection<WHSPackLine>
	{
		public WHSPackLineFilteredCollection(JobDeclaration declaration)
			: base(declaration.WHSPackLines)
		{
			this.declaration = declaration;
			Rebuild();
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return declaration.PackableInvoiceLines.Count > 0; }
		}

		protected override void RebuildOnConstruction()
		{
			// nope
		}

		readonly JobDeclaration declaration;

		protected override bool IsThisPartOfTheCollection(BusinessObject bObject)
		{
			var line = bObject as WHSPackLine;
			return line != null && IsMatchingFilter(line);
		}

		protected override bool IsFilterEmpty
		{
			get
			{
				return
					declaration.WHSInvLineFilter.IsEmpty &&
					declaration.WHSProductFilter.IsEmpty &&
					declaration.WHSPackageFilter.IsEmpty;
			}
		}

		protected override void ClearFilterCore()
		{
			declaration.WHSInvLineFilter = ZGuid.Empty;
			declaration.WHSProductFilter = ZString.Empty;
			declaration.WHSPackageFilter = ZGuid.Empty;
		}

		bool IsMatchingFilter(WHSPackLine line)
		{
			bool result = IsFilterEmpty;

			if (!result)
			{
				result = true;
				if (!declaration.WHSInvLineFilter.IsEmpty)
				{
					result = line.US_JI_InvoiceLine == declaration.WHSInvLineFilter;
				}
				if (result && !declaration.WHSPackageFilter.IsEmpty)
				{
					result = line.US_B7_WHSPack == declaration.WHSPackageFilter;
				}
				if (result && !declaration.WHSProductFilter.IsEmpty)
				{
					result = line.B7_Calc_PartNo == declaration.WHSProductFilter;
				}
			}

			return result;
		}

		#endregion Implementation
	}
}
