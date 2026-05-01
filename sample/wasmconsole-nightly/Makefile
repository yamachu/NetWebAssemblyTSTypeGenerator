DOTNET:=$(shell which dotnet)

build:
	$(DOTNET) clean
	$(DOTNET) build /p:TypeOutputDir=$(shell pwd)/generated

test:
	rm -rf generated
	$(MAKE) build
	diff fixture/index.d.ts.template generated/index.d.ts
