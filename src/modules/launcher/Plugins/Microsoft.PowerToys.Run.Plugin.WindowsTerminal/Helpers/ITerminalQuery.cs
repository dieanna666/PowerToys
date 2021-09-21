﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.PowerToys.Run.Plugin.WindowsTerminal.Helpers
{
    public interface ITerminalQuery
    {
        IEnumerable<TerminalProfile> GetTerminals();
    }
}
