using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Server.ADT.Antag;

public sealed class AntagRollBonusSystem : EntitySystem
{
    [Dependency] private readonly AntagRollBonusManager _rollBonus = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public static readonly TimeSpan AbortedRoundCountsAfter = TimeSpan.FromHours(1);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        switch (ev.New)
        {
            case GameRunLevel.PostRound:
                FinishRound();
                break;

            case GameRunLevel.PreRoundLobby when ev.Old == GameRunLevel.InRound:
                if (_gameTicker.RoundDuration() < AbortedRoundCountsAfter)
                    break;

                FinishRound();
                break;
        }
    }

    private void FinishRound()
    {
        var participants = _gameTicker.PlayerGameStatuses
            .Where(x => x.Value == PlayerGameStatus.JoinedGame)
            .Select(x => x.Key)
            .ToList();

        _rollBonus.FinishRound(participants);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _rollBonus.ClearRoundState();
    }
}
