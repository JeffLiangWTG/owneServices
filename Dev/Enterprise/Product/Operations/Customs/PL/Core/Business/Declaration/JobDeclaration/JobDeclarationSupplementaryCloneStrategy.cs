using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobDeclarationSupplementaryCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn) : JobDeclarationDeepCloneStrategy(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
{
	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		JobDeclaration jobDeclaration = (JobDeclaration)base.CloneInternal(args);
		JobDeclaration oldDec = (JobDeclaration)bizObjToClone;
		JobDeclaration newDec = jobDeclaration;
		CopyItineraryCountries(oldDec, newDec, args);
		return jobDeclaration;
	}

	void CopyItineraryCountries(JobDeclaration oldDec, JobDeclaration newDec, BusinessObjectCloneArgs args)
	{
		foreach (var itineraryCountry in oldDec.ItineraryCountries)
		{
			newDec.ItineraryCountries.Add(itineraryCountry.Clone(args));
		}
	}
}
