using LageFreeze.Models;

namespace LageFreeze.Services;

/// <summary>Pure monitor-selection matching logic, kept separate for focused tests.</summary>
public static class MonitorMatcher
{
    public static MonitorInfo? FindBestMatch(
        MonitorSelection? selection,
        IEnumerable<MonitorInfo> availableMonitors)
    {
        ArgumentNullException.ThrowIfNull(availableMonitors);

        if (selection is null)
        {
            return null;
        }

        var monitors = availableMonitors.ToArray();
        if (monitors.Length == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(selection.StableId))
        {
            var stableMatches = monitors
                .Where(monitor => EqualsIdentity(monitor.StableId, selection.StableId))
                .ToArray();

            if (stableMatches.Length == 1)
            {
                return stableMatches[0];
            }

            if (stableMatches.Length > 1)
            {
                return ChooseByLastBounds(selection, stableMatches);
            }
        }

        var ranked = monitors
            .Select(monitor => new Candidate(monitor, Score(selection, monitor)))
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Monitor.DisplayNumber)
            .ToArray();

        // Requiring multiple agreeing hints prevents selecting an unrelated
        // monitor after the actual target was disconnected.
        if (ranked[0].Score < 60)
        {
            return null;
        }

        if (ranked.Length > 1 && ranked[0].Score == ranked[1].Score)
        {
            return null;
        }

        return ranked[0].Monitor;
    }

    private static int Score(MonitorSelection selection, MonitorInfo monitor)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(selection.DeviceName)
            && EqualsIdentity(selection.DeviceName, monitor.DeviceName))
        {
            score += 60;
        }

        if (!string.IsNullOrWhiteSpace(selection.DisplayName)
            && string.Equals(
                selection.DisplayName.Trim(),
                monitor.DisplayName.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
        }

        var previous = selection.LastKnownBounds;
        if (!previous.IsEmpty)
        {
            if (previous.Width == monitor.Bounds.Width
                && previous.Height == monitor.Bounds.Height)
            {
                score += 25;
            }

            if (previous.Left == monitor.Bounds.Left && previous.Top == monitor.Bounds.Top)
            {
                score += 15;
            }
        }

        if (selection.WasPrimary == monitor.IsPrimary)
        {
            score += 5;
        }

        return score;
    }

    private static MonitorInfo? ChooseByLastBounds(
        MonitorSelection selection,
        IReadOnlyList<MonitorInfo> monitors)
    {
        var exactBounds = monitors.Where(monitor => monitor.Bounds == selection.LastKnownBounds).ToArray();
        return exactBounds.Length == 1 ? exactBounds[0] : null;
    }

    private static bool EqualsIdentity(string? first, string? second)
    {
        return !string.IsNullOrWhiteSpace(first)
               && !string.IsNullOrWhiteSpace(second)
               && string.Equals(first.Trim(), second.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed record Candidate(MonitorInfo Monitor, int Score);
}
