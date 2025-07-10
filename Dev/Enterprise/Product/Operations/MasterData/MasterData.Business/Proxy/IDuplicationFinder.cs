using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	public interface IDuplicationFinder<TMaster, TTarget> : ISupportDuplicationFinder
		where TMaster : BusinessObject
		where TTarget : BusinessObject
	{
		DeduplicationResponseStatus CompareBizOs(TTarget targetBizO, ZString staffCode);
		IEnumerable<DeduplicationResponseStatus> GetPotentialTargets(ZString staffCode);
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an async method.")]
		Task<IEnumerable<DeduplicationResponseStatus>> GetPotentialDuplicatesAsync(ZString staffCode);
		DeduplicationResponseStatus AddExclusion(ZString staffCode);
		DeduplicationResponseStatus AddIgnore(TTarget targetBizO, UserIgnoreStatus ignoreStatus, ZString staffCode);
		DeduplicationResponseStatus RemoveExclusion();
		DeduplicationResponseStatus RemoveTemporaryIgnore(TTarget targetBizO);
		DeduplicationResponseStatus RemovePermanentIgnore(TTarget targetBizO);
		CancellationTokenSource TokenSource { get; }
	}
}
