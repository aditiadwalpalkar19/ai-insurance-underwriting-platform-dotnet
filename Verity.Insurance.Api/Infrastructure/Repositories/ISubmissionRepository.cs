using Verity.Insurance.Api.Contracts;

namespace Verity.Insurance.Api.Infrastructure.Repositories;

public interface ISubmissionRepository
{
    Task<List<SubmissionResponse>> SearchAsync(
        bool isBroker,
        string? userUid,
        CancellationToken cancellationToken = default);
}