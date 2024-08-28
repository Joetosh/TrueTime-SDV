using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace TrueTime;

public sealed class ModConfig {
  public Dictionary<ulong, DateTime> LogTimes { get; set; } = new();
}

internal sealed class ModEntry : Mod {
  private ModConfig? _config = new();

  // public methods
  public override void Entry(IModHelper helper) {
    helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdate;
    helper.Events.GameLoop.SaveLoaded += OnLoadSave;
    helper.Events.GameLoop.Saved += OnSave;
  }

  // private methods
  private static void OnUpdate(object? sender,
                               OneSecondUpdateTickedEventArgs e) {
    if (!Context.IsWorldReady)
      return;

    Game1.timeOfDay = DateTime.Now.Hour * 100 + DateTime.Now.Minute;
  }

  private void OnLoadSave(object? sender, SaveLoadedEventArgs e) {
    Game1.gameTimeInterval = 0;

    _config = Helper.ReadConfig<ModConfig>();
    // checks if save hasn't time data
    if (!_config.LogTimes.ContainsKey(Game1.uniqueIDForThisGame))
      return;
    var timeInterval =
        DateTime.Now.Subtract(_config.LogTimes[Game1.uniqueIDForThisGame]).Days;
    // check if at least a day hasn't passed
    if (timeInterval < 0)
      return;
    TimeTravel(timeInterval);
    Game1.addHUDMessage(new HUDMessage(
        $"It's been {Game1.year} years and {Game1.dayOfMonth} days since you left the {Game1.player.farmName} farm."));
  }

  private void OnSave(object? sender, SavedEventArgs e) {
    _config.LogTimes.Add(Game1.uniqueIDForThisGame, DateTime.Today.Date);
    Helper.WriteConfig(_config);
  }

  private void TimeTravel(int interval) {
    Game1.year = interval / 112;
    var seasonIndex = Game1.dayOfMonth = interval % 112;

    switch (seasonIndex) {
    case int n when n <= 28:
      Game1.season = Season.Spring;
      break;
    case int n when n <= 56:
      Game1.season = Season.Summer;
      Game1.dayOfMonth -= 28;
      break;
    case int n when n <= 84:
      Game1.season = Season.Fall;
      Game1.dayOfMonth -= 56;
      break;
    case int n when n <= 112:
      Game1.season = Season.Winter;
      Game1.dayOfMonth -= 84;
      break;
    }

    Game1.setGraphicsForSeason();
  }
}
