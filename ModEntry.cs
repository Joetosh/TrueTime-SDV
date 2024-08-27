using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace TrueTime {
public sealed class ModConfig {

  public Dictionary<ulong, DateTime> Data { get; set; }

  public ModConfig() { this.Data = new Dictionary<ulong, DateTime>(); }
}

internal sealed class ModEntry : Mod {
  private ModConfig Config = new();
  // public methods
  public override void Entry(IModHelper helper) {
    helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdate;
    helper.Events.GameLoop.SaveLoaded += OnSaveload;
    helper.Events.GameLoop.Saved += OnSaved;
  }

  // private methods
  private void OnUpdate(object? sender, OneSecondUpdateTickedEventArgs e) {
    if (!Context.IsWorldReady)
      return;
    Game1.timeOfDay = (DateTime.Now.Hour * 100) + DateTime.Now.Minute;
    // TODO fix it later
    if (Game1.timeOfDay >= 0200 || Game1.timeOfDay < 0600)
      Game1.PassOutNewDay();
  }
  private void OnSaveload(object? sender, SaveLoadedEventArgs e) {
    if (Game1.gameTimeInterval != 0)
      Game1.gameTimeInterval = 0;

    this.Config = this.Helper.ReadConfig<ModConfig>();
    if (Config.Data.ContainsKey(Game1.uniqueIDForThisGame)) {
      var TimeInterval =
          DateTime.Now.Subtract(Config.Data[Game1.uniqueIDForThisGame]).Days;
      if (TimeInterval > 0) {
        Game1.year = TimeInterval / 112;
        Game1.dayOfMonth = TimeInterval % 112;

        Game1.addHUDMessage(new HUDMessage(
            message: $"It's been {Game1.year} years and {Game1.dayOfMonth} days since you left the {Game1.player.farmName} farm."));

        switch (TimeInterval % 112) {
        case int n when (n <= 28):
          Game1.season = Season.Spring;
          break;
        case int n when (n <= 56):
          Game1.season = Season.Summer;
          Game1.dayOfMonth -= 28;
          break;
        case int n when (n <= 84):
          Game1.season = Season.Fall;
          Game1.dayOfMonth -= 56;
          break;
        case int n when (n <= 112):
          Game1.season = Season.Winter;
          Game1.dayOfMonth -= 84;
          break;
        }
        Game1.setGraphicsForSeason();
      }
    }
  }
  private void OnSaved(object? sender, SavedEventArgs e) {
    Config.Data.Add(Game1.uniqueIDForThisGame, DateTime.Today.Date);
    this.Helper.WriteConfig(this.Config);
  }
}
}
