/** Generated: for wasmconsole-nightly.dll */
export const getTypedAssemblyExports: (originalGetAssemblyExports: Promise<any>) => Promise<
/* AutoGeneratedExportsHelperStart */
{
  "MyClass": {
    "Greeting": () => any /* TODO */
  }
}
/* AutoGeneratedExportsHelperEnd */
>;
/* AutoGeneratedImportsHelperStart */
type ImportModuleNames = 'main.mjs'
type ImportModuleValues<T extends ImportModuleNames> = T extends 'main.mjs'
? {
  "node": {
    "process": {
      "version": () => any /* TODO */
    }
  }
}
: never;

export const setTypedModuleImports: <T extends ImportModuleNames>(
    originalSetModuleImports: (moduleName: string, moduleImports: any) => void,
    moduleName: T,
    moduleImports: ImportModuleValues<T>
) => void;
/* AutoGeneratedImportsHelperEnd */