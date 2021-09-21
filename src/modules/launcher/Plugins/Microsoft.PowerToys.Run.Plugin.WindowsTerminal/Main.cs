// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using ManagedCommon;
using Microsoft.PowerToys.Run.Plugin.WindowsTerminal.Helpers;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.SharedCommands;

namespace Microsoft.PowerToys.Run.Plugin.WindowsTerminal
{
    public class Main : IPlugin, IContextMenu
    {
        private readonly ITerminalQuery _terminalQuery = new TerminalQuery();
        private PluginInitContext _context;
        private string _warningIconPath;

        public string Name => "Windows Terminal";

        public string Description => "Windows Terminal Description";

        public void Init(PluginInitContext context)
        {
            _context = context;
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query)
        {
            var search = query?.Search ?? string.Empty;
            var profiles = _terminalQuery.GetTerminals();

            if (!profiles.Any())
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Warning: Windows Terminal",
                        SubTitle = "Failed to retrieve profiles from Windows Terminal",
                        IcoPath = _warningIconPath,
                    },
                };
            }

            var result = new List<Result>();

            foreach (var profile in profiles)
            {
                if ((!string.IsNullOrWhiteSpace(query.ActionKeyword) && string.IsNullOrWhiteSpace(search)) || StringMatcher.FuzzySearch(search, profile.Name).Success)
                {
                    result.Add(new Result
                    {
                        Title = profile.Name,
                        SubTitle = profile.Terminal.DisplayName,
                        Action = _ =>
                        {
                            Launch(profile.Terminal.AppUserModelId, $"-w 0 nt -p \"{profile.Name}\"");
                            return true;
                        },
                        ContextData = profile,
                    });
                }
            }

            return result;
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (!(selectedResult?.ContextData is TerminalProfile))
            {
                return new List<ContextMenuResult>();
            }

            var result = new List<ContextMenuResult>();

            if (selectedResult.ContextData is TerminalProfile profile)
            {
                result.Add(new ContextMenuResult
                {
                    Glyph = "\xE7EF",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    Action = _ =>
                    {
                        string command = "shell:AppsFolder\\" + profile.Terminal.AppUserModelId;
                        command = Environment.ExpandEnvironmentVariables(command.Trim());

                        var info = ShellCommand.SetProcessStartInfo(command, verb: "runas");
                        info.UseShellExecute = true;
                        info.Arguments = $"-w 0 nt -p \"{profile.Name}\"";
                        Process.Start(info);
                        return true;
                    },
                });
            }

            return result;
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _warningIconPath = "Images/Warning.light.png";
            }
            else
            {
                _warningIconPath = "Images/Warning.dark.png";
            }
        }

        private static void Launch(string id, string queryArguments)
        {
            var appManager = new ApplicationActivationManager();
            const ActivateOptions noFlags = ActivateOptions.None;
            appManager.ActivateApplication(id, queryArguments, noFlags, out var unusedPid);
        }
    }
}
