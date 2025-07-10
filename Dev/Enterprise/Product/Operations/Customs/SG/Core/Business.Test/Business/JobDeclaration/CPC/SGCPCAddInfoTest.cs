using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Testing
{
	[TestedType(typeof(SGCPCAddInfo))]
	public class SGCPCAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SGCPCAddInfo(Declaration.CPCs.AddNew().B7_AddInfoDataInfo);
		}

		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
	}
}
