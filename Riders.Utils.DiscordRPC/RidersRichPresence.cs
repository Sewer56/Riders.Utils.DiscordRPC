using System;
using DiscordRPC;
using Riders.Utils.DiscordRPC.Definitions;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.Fields;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Utils.DiscordRPC
{
    public class RidersRichPresence
    {
        private DiscordRpcClient _discordRpc;
        private System.Threading.Timer _timer;
        private bool _enableRpc = true;
        private GetRaceModeHook _getRaceModeHook;

        public RidersRichPresence(GetRaceModeHook getRaceModeHook)
        {
            _getRaceModeHook = getRaceModeHook;
            _discordRpc = new DiscordRpcClient("713075835653062666"); // Not like you could get this from decompiling anyway. Obfuscation? That sucks.
            _discordRpc.Initialize();
            _timer = new System.Threading.Timer(OnTick, null, 0, 5000);
        }

        private unsafe void OnTick(object state)
        {
            if (_enableRpc)
            {
                var richPresence = new RichPresence
                {
                    Details = GetCurrentDetails(), 
                    State = GetGameState(), 
                    Assets = new Assets()
                };

                
                // Get Image
                if (Variables.TryGetGameState(out var gameState))
                {
                    if (*gameState == GameState.Race || *gameState == GameState.LoadRace)
                    {
                        // Add timestamp
                        var timeStamps = new Timestamps();
                        if (!*Variables.IsPaused)
                        {
                            // Do not set timestamp if paused.
                            DateTime levelStartTime = DateTime.UtcNow.Subtract((*Variables.StageTimer).ToTimeSpan());
                            timeStamps.Start = levelStartTime;
                            richPresence.Timestamps = timeStamps;
                        }

                        if (DiscordDictionary.Images.TryGetValue(*Variables.Level, out string stageAssetName))
                        {
                            richPresence.Assets.LargeImageText = Utilities.GetLevelName(*Variables.Level);
                            richPresence.Assets.LargeImageKey = stageAssetName;
                        }
                    }
                }

                // Send to Discord
                _discordRpc.SetPresence(richPresence);
            }
        }


        /// <summary>
        /// Gets text set directly under game name on Discord.
        /// </summary>
        private unsafe string GetCurrentDetails()
        {
            if (!Variables.TryGetGameState(out var state)) 
                return String.Empty;

            if (*state != GameState.Race && *state != GameState.LoadRace)
                return String.Empty;

            switch (_getRaceModeHook.RaceMode)
            {
                case RaceMode.FreeRace: return "Free Race";
                case RaceMode.TimeTrial: return "Time Trial";
                case RaceMode.GrandPrix: return "Grand Prix";
                case RaceMode.StoryMode: return "Story Mode";
                case RaceMode.RaceStage: return "Race Stage";
                case RaceMode.BattleStage: return "Battle Stage";
                case RaceMode.MissionMode: return "Mission Mode";
                case RaceMode.TagMode: return "Tag Mode";
                case RaceMode.Demo: return "Demo";
                default: return String.Empty;
            }
        }

        /// <summary>
        /// Retrieves the current state of the game.
        /// </summary>
        private unsafe string GetGameState()
        {
            const string UnknownState = "Unknown State";
            if (!Variables.TryGetGameState(out var state)) 
                return UnknownState;
            
            switch (*state)
            {
                case GameState.LoadRace: return String.Empty;
                case GameState.Race: return String.Empty;
                case GameState.TitleScreen: return "Title Screen";
                case GameState.MainMenu: return "Main Menu";
                case GameState.NormalRaceSubmenu: return "Selecting a Race Mode";
                case GameState.StorySubmenu: return "Selecting a Story";
                case GameState.MissionSubmenu: return "Selecting a Mission";
                case GameState.LoadTagSubmenu: return "Setting up Tag Race";
                case GameState.SurvivalSubmenu: return "Setting up Survival Mode";
                case GameState.StageSelect: return "Stage Select";
                case GameState.CharacterSelect: return "Character Select";
                case GameState.Shop: return "Shopping";
                case GameState.ExtrasSubmenus: return "Extras";
                case GameState.OptionsSubmenus: return "Options";
                case GameState.MissionSelect: return "Selecting a Mission";
            }

            return UnknownState;
        }

        public void Suspend()
        {
            _getRaceModeHook.Suspend();
            _enableRpc = false;
        }

        public void Resume()
        {
            _getRaceModeHook.Resume();
            _enableRpc = true;
        }
    }
}
