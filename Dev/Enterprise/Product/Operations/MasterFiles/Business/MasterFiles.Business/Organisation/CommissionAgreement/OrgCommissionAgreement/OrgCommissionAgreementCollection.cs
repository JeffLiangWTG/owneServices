using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.OrgCommissionAgreement)]
	public class OrgCommissionAgreementCollection : ActiveBusinessObjectCollection<OrgCommissionAgreement>
	{
		#region Constructors

		public OrgCommissionAgreementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCommissionAgreementCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public OrgCommissionAgreementCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public OrgCommissionAgreementCollection(OrgOpportunity master)
			: base(master)
		{
		}

		public OrgCommissionAgreementCollection(OrgOpportunity master, ZQuery filter)
			: base(master, filter)
		{
		}

		#endregion

		#region New

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewElementCore(OrgCommissionAgreement newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var highestUsedSuffixIndex = 0;
			var suffixIndexRegex = new Regex(@"^#(?<Index>[0-9]+)$");
			foreach (var agreement in this)
			{
				var suffixIndexMatch = suffixIndexRegex.Match(agreement.CA0_Name);
				if (suffixIndexMatch.Success)
				{
					var index = int.Parse(suffixIndexMatch.Groups["Index"].Value);
					if (highestUsedSuffixIndex < index)
					{
						highestUsedSuffixIndex = index;
					}
				}
			}

			newElement.CA0_Name = "#" + (highestUsedSuffixIndex + 1);
		}

		#endregion
	}
}
