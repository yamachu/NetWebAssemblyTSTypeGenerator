import { App } from "./app-support.mjs";

/**
 * @typedef { import("@microsoft/dotnet-runtime").ExportsHelper } ExportsHelper
 */

async function main(applicationArguments) {
    App.runtime.setModuleImports("main.mjs", {
        node: {
            process: {
                version: () => globalThis.process.version,
            },
        },
    });

    /** @type {ExportsHelper} */
    const getAssemblyExports = await App.runtime.getAssemblyExports;
    const exports = await getAssemblyExports("wasmconsole-nightly.dll");
    const text = exports.MyClass.Greeting();
    console.log(text);

    return await App.runtime.runMain("wasmconsole-nightly.dll", applicationArguments);
}

App.run(main);
