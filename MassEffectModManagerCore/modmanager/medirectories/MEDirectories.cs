﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MassEffectModManagerCore.modmanager;
using MassEffectModManagerCore.modmanager.objects;

namespace MassEffectModManagerCore.GameDirectories

{
    public static class MEDirectories
    {
        public static string CookedPath(Mod.MEGame game)
        {
            switch (game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.cookedPath;
                case Mod.MEGame.ME2:
                    return ME2Directory.cookedPath;
                case Mod.MEGame.ME3:
                    return ME3Directory.cookedPath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }
        public static string CookedPath(GameTarget target)
        {
            switch (target.Game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.CookedPath(target);
                case Mod.MEGame.ME2:
                    return ME2Directory.CookedPath(target);
                case Mod.MEGame.ME3:
                    return ME3Directory.CookedPath(target);
                default:
                    throw new ArgumentOutOfRangeException(nameof(target.Game), target.Game, null);
            }
        }

        public static string ASIPath(GameTarget target)
        {
            switch (target.Game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.ASIPath(target);
                case Mod.MEGame.ME2:
                    return ME2Directory.ASIPath(target);
                case Mod.MEGame.ME3:
                    return ME3Directory.ASIPath(target);
                default:
                    throw new ArgumentOutOfRangeException(nameof(target.Game), target.Game, null);
            }
        }

        public static string GamePath(Mod.MEGame game)
        {
            switch (game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.gamePath;
                case Mod.MEGame.ME2:
                    return ME2Directory.gamePath;
                case Mod.MEGame.ME3:
                    return ME3Directory.gamePath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }
        public static string BioGamePath(Mod.MEGame game)
        {
            switch (game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.bioGamePath;
                case Mod.MEGame.ME2:
                    return ME2Directory.bioGamePath;
                case Mod.MEGame.ME3:
                    return ME3Directory.biogamePath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        public static string BioGamePath(GameTarget target) => Path.Combine(target.TargetPath, "BioGame"); //all games use same biogame path.

        public static Dictionary<string, string> OfficialDLCNames(Mod.MEGame game)
        {
            switch (game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.OfficialDLCNames;
                case Mod.MEGame.ME2:
                    return ME2Directory.OfficialDLCNames;
                case Mod.MEGame.ME3:
                    return ME3Directory.OfficialDLCNames;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Gets path to executable for the specified Game Target
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string ExecutablePath(GameTarget game)
        {
            switch (game.Game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.ExecutablePath(game.TargetPath);
                case Mod.MEGame.ME2:
                    return ME2Directory.ExecutablePath(game.TargetPath);
                case Mod.MEGame.ME3:
                    return ME3Directory.ExecutablePath(game.TargetPath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        /// <summary>
        /// Refreshes the registry active paths for all three games
        /// </summary>
        public static void ReloadGamePaths()
        {
            ME1Directory.ReloadActivePath();
            ME2Directory.ReloadActivePath();
            ME3Directory.ReloadActivePath();
        }

        public static string DLCPath(Mod.MEGame game)
        {
            switch (game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.DLCPath;
                case Mod.MEGame.ME2:
                    return ME2Directory.DLCPath;
                case Mod.MEGame.ME3:
                    return ME3Directory.DLCPath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        public static string DLCPath(GameTarget target)
        {
            switch (target.Game)
            {
                case Mod.MEGame.ME1:
                    return Path.Combine(target.TargetPath, "DLC");
                case Mod.MEGame.ME2:
                case Mod.MEGame.ME3:
                    return Path.Combine(target.TargetPath, "BIOGame", "DLC");
                default:
                    throw new ArgumentOutOfRangeException(nameof(target.Game), target.Game, null);
            }
        }

        public static List<string> OfficialDLC(Mod.MEGame game)
        {
            switch (game)
            {
                case Mod.MEGame.ME1:
                    return ME1Directory.OfficialDLC;
                case Mod.MEGame.ME2:
                    return ME2Directory.OfficialDLC;
                case Mod.MEGame.ME3:
                    return ME3Directory.OfficialDLC;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        //public static bool IsInBasegame(this IMEPackage pcc) => IsInBasegame(pcc.FilePath, pcc.Game);

        public static bool IsInBasegame(string path, Mod.MEGame game) => path.StartsWith(CookedPath(game));

        public static bool IsInBasegame(string path, GameTarget target) => path.StartsWith(CookedPath(target), StringComparison.InvariantCultureIgnoreCase);

        //public static bool IsInOfficialDLC(this IMEPackage pcc) => IsInOfficialDLC(pcc.FilePath, pcc.Game);

        public static bool IsInOfficialDLC(string path, Mod.MEGame game)
        {
            if (game == Mod.MEGame.Unknown)
            {
                return false;
            }
            string dlcPath = DLCPath(game);

            return OfficialDLC(game).Any(dlcFolder => path.StartsWith(Path.Combine(dlcPath, dlcFolder)));
        }

        public static bool IsInOfficialDLC(string path, GameTarget target)
        {
            if (target.Game == Mod.MEGame.Unknown)
            {
                return false;
            }
            string dlcPath = DLCPath(target);

            return OfficialDLC(target.Game).Any(dlcFolder => path.StartsWith(Path.Combine(dlcPath, dlcFolder), StringComparison.CurrentCultureIgnoreCase));
        }

        public static List<string> EnumerateGameFiles(Mod.MEGame GameVersion, string searchPath, bool recurse = true, Predicate<string> predicate = null)
        {
            List<string> files = Directory.EnumerateFiles(searchPath, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

            files = EnumerateGameFiles(GameVersion, files, predicate);

            return files;
        }

        public static List<string> EnumerateGameFiles(Mod.MEGame GameVersion, List<string> files, Predicate<string> predicate = null)
        {
            if (predicate == null)
            {
                // KFreon: Set default search predicate.
                switch (GameVersion)
                {
                    case Mod.MEGame.ME1:
                        predicate = s => s.ToLowerInvariant().EndsWith(".upk", true, null) || s.ToLowerInvariant().EndsWith(".u", true, null) || s.ToLowerInvariant().EndsWith(".sfm", true, null);
                        break;
                    case Mod.MEGame.ME2:
                    case Mod.MEGame.ME3:
                        predicate = s => s.ToLowerInvariant().EndsWith(".pcc", true, null) || s.ToLowerInvariant().EndsWith(".tfc", true, null) || s.ToLowerInvariant().EndsWith(".afc", true, null);
                        break;
                }
            }

            return files.Where(t => predicate(t)).ToList();
        }

        internal static List<string> GetInstalledDLC(GameTarget target)
        {
            var dlcDirectory = MEDirectories.DLCPath(target);
            if (Directory.Exists(dlcDirectory))
            {
                return Directory.GetDirectories(dlcDirectory).Where(x => Path.GetFileName(x).StartsWith("DLC_")).Select(x => Path.GetFileName(x)).ToList();
            }

            return new List<string>();
        }

        internal static bool IsOfficialDLCInstalled(ModJob.JobHeader header, GameTarget gameTarget)
        {
            if (header == ModJob.JobHeader.BALANCE_CHANGES) return true; //Don't check balance changes
            if (header == ModJob.JobHeader.ME2_RCWMOD) return true; //Don't check
            if (header == ModJob.JobHeader.ME1_CONFIG) return true; //Don't check
            if (header == ModJob.JobHeader.BASEGAME) return true; //Don't check basegame
            if (header == ModJob.JobHeader.CUSTOMDLC) return true; //Don't check custom dlc

            if (header == ModJob.JobHeader.TESTPATCH)
            {
                return File.Exists(Utilities.GetTestPatchPath(gameTarget));
            }
            else
            {
                return MEDirectories.GetInstalledDLC(gameTarget).Contains(ModJob.GetHeadersToDLCNamesMap(gameTarget.Game)[header]);
            }
        }

        /// <summary>
        /// Gets DLC path based on specified game root and game.
        /// </summary>
        /// <param name="gameRoot"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static string DLCPath(string gameRoot, Mod.MEGame game)
        {
            if (game == Mod.MEGame.ME1) return Path.Combine(gameRoot, @"DLC");
            if (game == Mod.MEGame.ME2 || game == Mod.MEGame.ME3) return Path.Combine(gameRoot, "BioGame", @"DLC");
            return null;
        }
    }
}
