namespace TravelTogether.Application.Settlements;

/// <param name="ParticipantId">The participant.</param>
/// <param name="Name">Participant name.</param>
/// <param name="Paid">Total of the expenses this participant paid for.</param>
/// <param name="Share">This participant's part of all expenses.</param>
/// <param name="Net">Paid minus Share: positive means others owe them, negative means they owe others.</param>
public sealed record BalanceDto(Guid ParticipantId, string Name, decimal Paid, decimal Share, decimal Net);

public sealed record BalancesDto(string Currency, IReadOnlyList<BalanceDto> Balances);

public sealed record TransferDto(
    Guid FromParticipantId,
    string FromName,
    Guid ToParticipantId,
    string ToName,
    decimal Amount);

public sealed record SettlementDto(string Currency, IReadOnlyList<TransferDto> Transfers);
