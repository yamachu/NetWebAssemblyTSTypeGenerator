DOTNET:=$(shell which dotnet)

build:
	$(DOTNET) clean
	$(DOTNET) build /p:TypeOutputDir=$(shell pwd)/generated

publish:
	$(DOTNET) build /p:TypeOutputDir=$(shell pwd)/generated

test:
	rm -rf generated
	$(MAKE) build
	diff fixture/index.d.ts.template generated/index.d.ts
	$(MAKE) publish
	cd bin/Debug/net10.0/browser-wasm/AppBundle && npm i dotnet-webassembly-type-helper@file:../../../../../generated && node main.mjs
