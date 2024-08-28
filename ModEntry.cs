using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace TrueTime;

public sealed class ModConfig
{
    public Dictionary<ulong, DateTime> LogTimes { get; set; } = new();
}

internal sealed class ModEntry : Mod
{
    private ModConfig? _config;

    // Public methods
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdate;
        helper.Events.GameLoop.SaveLoaded += OnLoadSave;
        helper.Events.GameLoop.Saved += OnSave;
    }

    // Private methods
    private void OnUpdate(object? sender,
        OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || Context.IsMultiplayer)
            return;
        // A fix to be able to play the game after midnight
        Game1.timeOfDay = DateTime.Now.Hour < 6 ? 2400 : DateTime.Now.Hour * 100 + DateTime.Now.Minute;
    }

    private void OnLoadSave(object? sender, SaveLoadedEventArgs e)
    {
        Game1.gameTimeInterval = 0;

        _config = Helper.ReadConfig<ModConfig>();
        // checks if save has time data
        if (!_config.LogTimes.TryGetValue(Game1.uniqueIDForThisGame, out var time))
            return;
        var timeInterval =
            DateTime.Now.Subtract(time).Days;
        Monitor.Log($"{timeInterval}");
        // check if at least a day has passed
        if (timeInterval < 1)
            return;
        TimeTravel(timeInterval);
        Game1.addHUDMessage(new HUDMessage(
            $"It's been {Game1.year} years and {Game1.dayOfMonth} days since you left the {Game1.player.farmName} farm."));
    }

    private void OnSave(object? sender, SavedEventArgs e)
    {
        _config ??= new ModConfig();
        _config.LogTimes[Game1.uniqueIDForThisGame] = DateTime.Now.Date;
        Helper.WriteConfig(_config);
    }

    private static void TimeTravel(int interval)
    {
        Game1.year = interval / 112;
        var seasonIndex = Game1.dayOfMonth = interval % 112;

        Game1.season = seasonIndex switch
        {
            <= 28 => Season.Spring,
            <= 56 => Season.Summer,
            <= 84 => Season.Fall,
            <= 112 => Season.Winter,
            _ => Game1.season
        };
        Game1.dayOfMonth -= 28 * (int)Game1.season;
        Game1.setGraphicsForSeason();
    }
}