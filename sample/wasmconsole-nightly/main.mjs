// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from "./dotnet.js";
import { getTypedAssemblyExports, setTypedModuleImports } from "dotnet-webassembly-type-helper";

const is_node =
    typeof process === "object" && typeof process.versions === "object" && typeof process.versions.node === "string";
if (!is_node) throw new Error(`This file only supports nodejs`);

const { setModuleImports, getAssemblyExports, getConfig, runMainAndExit } = await dotnet
    .withDiagnosticTracing(false)
    .create();

setTypedModuleImports(setModuleImports, "main.mjs", {
    node: {
        process: {
            version: () => globalThis.process.version,
        },
    },
});

const config = getConfig();
const exports = await getTypedAssemblyExports(getAssemblyExports(config.mainAssemblyName));
const text = exports.MyClass.Greeting();
console.log(text);

await runMainAndExit(config.mainAssemblyName, ["dotnet", "is", "great!"]);
